using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure.Services;

public interface ICheckInService
{
    Task<RegisterDeviceResponse> RegisterOrTouchDeviceAsync(
        string deviceIdentifier,
        string? observedIp,
        string? userAgent,
        CancellationToken cancellationToken = default);

    Task<CheckInContextResponse?> GetContextAsync(
        string deviceIdentifier,
        CancellationToken cancellationToken = default);

    Task<(bool Success, string? Error, CheckInResponse? Response)> CheckInAsync(
        int tajamarUserId,
        string deviceIdentifier,
        string? observedIp,
        CancellationToken cancellationToken = default);
}

public class CheckInService(ApplicationDbContext dbContext) : ICheckInService
{
    public async Task<RegisterDeviceResponse> RegisterOrTouchDeviceAsync(
        string deviceIdentifier,
        string? observedIp,
        string? userAgent,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var device = await dbContext.Devices
            .FirstOrDefaultAsync(d => d.DeviceIdentifier == deviceIdentifier, cancellationToken);

        if (device is null)
        {
            device = new Device
            {
                DeviceIdentifier = deviceIdentifier,
                FirstSeenAtUtc = now,
                LastSeenAtUtc = now,
                LastSeenIp = observedIp,
                LastSeenUserAgent = userAgent,
                IsActive = true
            };
            dbContext.Devices.Add(device);
        }
        else
        {
            device.LastSeenAtUtc = now;
            device.LastSeenIp = observedIp;
            device.LastSeenUserAgent = userAgent;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return new RegisterDeviceResponse(
            device.Id,
            device.DeviceIdentifier,
            device.LastSeenIp,
            device.LastSeenAtUtc);
    }

    public async Task<CheckInContextResponse?> GetContextAsync(
        string deviceIdentifier,
        CancellationToken cancellationToken = default)
    {
        var device = await dbContext.Devices
            .AsNoTracking()
            .Where(d => d.DeviceIdentifier == deviceIdentifier)
            .Select(d => new
            {
                d.Id,
                d.DeviceIdentifier,
                d.FriendlyName,
                d.LastSeenIp,
                d.LastSeenAtUtc
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (device is null)
        {
            return null;
        }

        var currentPosition = await dbContext.DevicePositionAssignments
            .AsNoTracking()
            .Where(a => a.DeviceId == device.Id && a.IsCurrent)
            .Select(a => new { a.PositionId, a.Position!.ClassCode, a.Position.DeviceCode })
            .FirstOrDefaultAsync(cancellationToken);

        int? assignedUserId = null;
        string? assignedStudentFullName = null;
        if (currentPosition is not null)
        {
            var assigned = await dbContext.PositionUserAssignments
                .AsNoTracking()
                .Where(a => a.PositionId == currentPosition.PositionId && a.IsCurrent)
                .Select(a => new
                {
                    a.TajamarUserId,
                    FullName = ((a.TajamarUser!.FirstName ?? string.Empty) + " " + (a.TajamarUser.LastName ?? string.Empty)).Trim()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (assigned is not null)
            {
                assignedUserId = assigned.TajamarUserId;
                assignedStudentFullName = assigned.FullName;
            }
        }

        return new CheckInContextResponse(
            device.Id,
            device.DeviceIdentifier,
            device.FriendlyName,
            device.LastSeenIp,
            device.LastSeenAtUtc,
            currentPosition?.PositionId,
            currentPosition?.ClassCode,
            currentPosition?.DeviceCode,
            assignedUserId,
            assignedStudentFullName);
    }

    public async Task<(bool Success, string? Error, CheckInResponse? Response)> CheckInAsync(
        int tajamarUserId,
        string deviceIdentifier,
        string? observedIp,
        CancellationToken cancellationToken = default)
    {
        var device = await dbContext.Devices
            .FirstOrDefaultAsync(d => d.DeviceIdentifier == deviceIdentifier, cancellationToken);

        if (device is null)
        {
            return (false, "Unknown device. Register the device first.", null);
        }

        var position = await dbContext.DevicePositionAssignments
            .AsNoTracking()
            .Where(a => a.DeviceId == device.Id && a.IsCurrent)
            .Select(a => new { a.PositionId, a.Position!.ClassCode, a.Position.DeviceCode })
            .FirstOrDefaultAsync(cancellationToken);

        if (position is null)
        {
            return (false, "This device is not assigned to a position yet.", null);
        }

        var assignedUserId = await dbContext.PositionUserAssignments
            .AsNoTracking()
            .Where(a => a.PositionId == position.PositionId && a.IsCurrent)
            .Select(a => (int?)a.TajamarUserId)
            .FirstOrDefaultAsync(cancellationToken);

        if (!assignedUserId.HasValue)
        {
            return (false, "This position is not assigned to a student yet.", null);
        }

        if (assignedUserId.Value != tajamarUserId)
        {
            return (false, "CHECKIN_WRONG_STUDENT", null);
        }

        var now = DateTime.UtcNow;

        // Also write into the existing attendance system so that teacher/student
        // history views show "Presente" for the day.
        var today = DateOnly.FromDateTime(DateTime.Today);
        var enrolledCourseIds = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.UserId == tajamarUserId && e.CourseId != null)
            .Select(e => e.CourseId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var todayCourseId = await ResolveLectiveCourseForDateAsync(
            tajamarUserId,
            enrolledCourseIds,
            today,
            cancellationToken);

        if (!todayCourseId.HasValue)
        {
            // Seat-to-student mapping is valid, but there is no matching course
            // where today's date is lective.
            return (false, "This is not a lective day for the student's enrolled courses.", null);
        }

        // If a teacher/admin has already marked the student as Absent ("Falta") for today
        // in any enrolled course, do not accept check-in. This avoids confusing states.
        var isMarkedAbsentToday = await dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(r =>
                r.StudentId == tajamarUserId
                && r.Date == today
                && enrolledCourseIds.Contains(r.CourseId)
                && r.Status == AttendanceStatus.Absent
                && r.RecordedByUserId != tajamarUserId)
            .AnyAsync(cancellationToken);

        if (isMarkedAbsentToday)
        {
            return (false, "CHECKIN_BLOCKED_ABSENT", null);
        }

        var checkIn = new CheckInRecord
        {
            TajamarUserId = tajamarUserId,
            DeviceId = device.Id,
            PositionId = position.PositionId,
            CheckedInAtUtc = now,
            ObservedIp = observedIp
        };

        dbContext.CheckInRecords.Add(checkIn);

        // Upsert attendance record for "Presente" (RecordedByUserId: using the
        // student id for now to satisfy the FK constraint; teacher auditing can be
        // improved later if/when we have a "recorded by" concept for check-ins).
        await UpsertAttendancePresentAsync(
            tajamarUserId,
            todayCourseId.Value,
            today,
            tajamarUserId,
            now,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        return (true, null, new CheckInResponse(
            checkIn.Id,
            tajamarUserId,
            device.Id,
            position.PositionId,
            position.ClassCode,
            position.DeviceCode,
            checkIn.CheckedInAtUtc));
    }

    private async Task<int?> ResolveLectiveCourseForDateAsync(
        int tajamarUserId,
        IReadOnlyList<int> enrolledCourseIds,
        DateOnly date,
        CancellationToken cancellationToken)
    {
        if (enrolledCourseIds.Count == 0)
        {
            return null;
        }

        // Pick the first enrolled course where `date` is lective.
        // (If multiple courses match, the order is by course id.)
        var courses = await dbContext.Courses
            .Where(c => enrolledCourseIds.Contains(c.Id))
            .OrderBy(c => c.Id)
            .Select(c => new { c.Id, c.StartDate, c.EndDate })
            .ToListAsync(cancellationToken);

        foreach (var course in courses)
        {
            var courseStart = LectiveDayCalendar.GetCourseStartDate(course.StartDate);
            var courseEnd = LectiveDayCalendar.GetCourseEndDate(course.EndDate, courseStart);
            var lectiveDates = LectiveDayCalendar.GetLectiveDates(courseStart, courseEnd).ToHashSet();

            if (lectiveDates.Contains(date))
            {
                return course.Id;
            }
        }

        return null;
    }

    private async Task UpsertAttendancePresentAsync(
        int studentId,
        int courseId,
        DateOnly date,
        int recordedByUserId,
        DateTime recordedAtUtc,
        CancellationToken cancellationToken)
    {
        var existing = await dbContext.AttendanceRecords
            .Where(r => r.StudentId == studentId && r.CourseId == courseId && r.Date == date)
            .FirstOrDefaultAsync(cancellationToken);

        if (existing is not null)
        {
            // If a teacher/admin already saved attendance for the day, do not let a student check-in override it.
            // Teacher saves should remain authoritative (they already overwrite in AttendanceService.SaveSessionAsync).
            if (existing.RecordedByUserId != recordedByUserId)
            {
                return;
            }

            existing.Status = AttendanceStatus.Present;
            existing.Comment = null;
            existing.RecordedByUserId = recordedByUserId;
            existing.RecordedAt = recordedAtUtc;
            return;
        }

        dbContext.AttendanceRecords.Add(new AttendanceRecord
        {
            StudentId = studentId,
            CourseId = courseId,
            Date = date,
            Status = AttendanceStatus.Present,
            Comment = null,
            RecordedByUserId = recordedByUserId,
            RecordedAt = recordedAtUtc
        });
    }
}

