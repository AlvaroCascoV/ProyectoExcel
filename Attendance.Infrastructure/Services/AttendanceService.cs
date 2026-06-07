using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure.Services;

public interface IAttendanceService
{
    Task<AttendanceSessionDto?> GetSessionSheetAsync(int courseId, DateOnly date, CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error)> SaveSessionAsync(
        int courseId,
        DateOnly date,
        IReadOnlyList<SaveAttendanceEntryRequest> entries,
        int recordedByUserId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DateOnly>> GetSessionDatesAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceRecordDto>> GetStudentAttendanceAsync(
        int studentId,
        int? courseId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default);
    Task<AttendanceSummaryDto?> GetStudentSummaryAsync(
        int studentId,
        int courseId,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceSummaryDto>> GetStudentSummariesAsync(
        int studentId,
        CancellationToken cancellationToken = default);
    Task<(bool Success, string? Error, SeedAttendanceResultDto? Result)> SeedPresentDaysAsync(
        int courseId,
        int recordedByUserId,
        int days = 7,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default);
}

public class AttendanceService(ApplicationDbContext dbContext, ICalendarService calendarService) : IAttendanceService
{
    public async Task<AttendanceSessionDto?> GetSessionSheetAsync(
        int courseId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.Id, Name = c.Name ?? string.Empty })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return null;
        }

        var students = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId && e.User != null && e.User.RoleId == 2)
            .OrderBy(e => e.User!.LastName)
            .ThenBy(e => e.User!.FirstName)
            .Select(e => new
            {
                e.User!.Id,
                FullName = ((e.User.FirstName ?? string.Empty) + " " + (e.User.LastName ?? string.Empty)).Trim()
            })
            .ToListAsync(cancellationToken);

        var records = await dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.CourseId == courseId && r.Date == date)
            .ToDictionaryAsync(r => r.StudentId, cancellationToken);

        var entries = students
            .Select(s =>
            {
                records.TryGetValue(s.Id, out var record);
                return new AttendanceEntryDto(
                    record?.Id ?? 0,
                    s.Id,
                    s.FullName,
                    record?.Status ?? AttendanceStatus.Present,
                    record?.Comment);
            })
            .ToList();

        return new AttendanceSessionDto(course.Id, course.Name, date, entries);
    }

    public async Task<(bool Success, string? Error)> SaveSessionAsync(
        int courseId,
        DateOnly date,
        IReadOnlyList<SaveAttendanceEntryRequest> entries,
        int recordedByUserId,
        CancellationToken cancellationToken = default)
    {
        if (entries.Count == 0)
        {
            return (false, "At least one attendance entry is required.");
        }

        var today = DateOnly.FromDateTime(DateTime.Today);
        if (date > today)
        {
            return (false, "Attendance can only be recorded for today or past lective days.");
        }

        var course = await dbContext.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.StartDate, c.EndDate })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return (false, $"Course {courseId} not found.");
        }

        var lectiveDatesList = await calendarService.GetLectiveDatesAsync(courseId, cancellationToken);
        var lectiveDates = lectiveDatesList.ToHashSet();

        if (!lectiveDates.Contains(date))
            return (false, $"{date:yyyy-MM-dd} is not a lective day for this course.");

        var enrolledStudentIds = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId && e.UserId != null && e.User != null && e.User.RoleId == 2)
            .Select(e => e.UserId!.Value)
            .ToHashSetAsync(cancellationToken);

        foreach (var entry in entries)
        {
            if (!enrolledStudentIds.Contains(entry.StudentId))
            {
                return (false, $"Student {entry.StudentId} is not enrolled in course {courseId}.");
            }
        }

        var existingRecords = await dbContext.AttendanceRecords
            .Where(r => r.CourseId == courseId && r.Date == date)
            .ToDictionaryAsync(r => r.StudentId, cancellationToken);

        var now = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            if (existingRecords.TryGetValue(entry.StudentId, out var record))
            {
                record.Status = entry.Status;
                record.Comment = entry.Comment;
                record.RecordedByUserId = recordedByUserId;
                record.RecordedAt = now;
            }
            else
            {
                dbContext.AttendanceRecords.Add(new AttendanceRecord
                {
                    StudentId = entry.StudentId,
                    CourseId = courseId,
                    Date = date,
                    Status = entry.Status,
                    Comment = entry.Comment,
                    RecordedByUserId = recordedByUserId,
                    RecordedAt = now
                });
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public async Task<IReadOnlyList<DateOnly>> GetSessionDatesAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.CourseId == courseId)
            .Select(r => r.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> GetStudentAttendanceAsync(
        int studentId,
        int? courseId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.StudentId == studentId);

        if (courseId.HasValue)
        {
            query = query.Where(r => r.CourseId == courseId.Value);
        }

        if (from.HasValue)
        {
            query = query.Where(r => r.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(r => r.Date <= to.Value);
        }

        return await query
            .OrderByDescending(r => r.Date)
            .Select(r => new AttendanceRecordDto(
                r.Id,
                r.CourseId,
                r.Course!.Name ?? string.Empty,
                r.Date,
                r.Status,
                r.Comment,
                r.RecordedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<AttendanceSummaryDto?> GetStudentSummaryAsync(
        int studentId,
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var course = await dbContext.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.Id, Name = c.Name ?? string.Empty, c.StartDate, c.EndDate })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return null;
        }

        var isEnrolled = await dbContext.CourseEnrollments
            .AsNoTracking()
            .AnyAsync(e => e.CourseId == courseId && e.UserId == studentId, cancellationToken);

        if (!isEnrolled)
        {
            return null;
        }

        var lectiveDates = await calendarService.GetLectiveDatesAsync(courseId, cancellationToken);
        var today = DateOnly.FromDateTime(DateTime.Today);

        var recordsByDate = await dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.StudentId == studentId && r.CourseId == courseId)
            .ToDictionaryAsync(r => r.Date, r => r.Status, cancellationToken);

        var lectiveDaysCount = await calendarService.GetLectiveDaysCountAsync(courseId, cancellationToken);
        var statuses = LectiveDayCalendar.BuildStatuses(lectiveDates, recordsByDate, today);
        var metrics = AttendanceMetricsCalculator.FromStatuses(statuses, lectiveDaysCount);

        return new AttendanceSummaryDto(
            course.Id,
            course.Name,
            lectiveDaysCount,
            metrics.PresentCount,
            metrics.AbsentCount,
            metrics.LateCount,
            metrics.JustifiedAbsentCount,
            metrics.JustifiedLateCount,
            metrics.EarlyLeaveCount,
            metrics.JustifiedEarlyLeaveCount,
            metrics.AttendancePercentage,
            metrics.RealAttendancePercentage,
            metrics.DiplomaEligible,
            metrics.BelowDiplomaWarning,
            metrics.AtRiskDrop);
    }

    public async Task<IReadOnlyList<AttendanceSummaryDto>> GetStudentSummariesAsync(
        int studentId,
        CancellationToken cancellationToken = default)
    {
        var enrolledCourseIds = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.UserId == studentId && e.CourseId != null)
            .Select(e => e.CourseId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var summaries = new List<AttendanceSummaryDto>(enrolledCourseIds.Count);
        foreach (var courseId in enrolledCourseIds)
        {
            var summary = await GetStudentSummaryAsync(studentId, courseId, cancellationToken);
            if (summary is not null)
            {
                summaries.Add(summary);
            }
        }

        return summaries;
    }

    public async Task<(bool Success, string? Error, SeedAttendanceResultDto? Result)> SeedPresentDaysAsync(
        int courseId,
        int recordedByUserId,
        int days = 7,
        DateOnly? endDate = null,
        CancellationToken cancellationToken = default)
    {
        if (days is < 1 or > 31)
        {
            return (false, "Days must be between 1 and 31.", null);
        }

        var course = await dbContext.Courses
            .AsNoTracking()
            .Where(c => c.Id == courseId)
            .Select(c => new { c.StartDate, c.EndDate })
            .FirstOrDefaultAsync(cancellationToken);

        if (course is null)
        {
            return (false, $"Course {courseId} not found.", null);
        }

        var studentIds = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.CourseId == courseId && e.UserId != null && e.User != null && e.User.RoleId == 2)
            .Select(e => e.UserId!.Value)
            .ToListAsync(cancellationToken);

        if (studentIds.Count == 0)
        {
            return (false, "No students enrolled in this course.", null);
        }

        var end = endDate ?? DateOnly.FromDateTime(DateTime.Today);
        var lectiveDatesList = await calendarService.GetLectiveDatesAsync(courseId, cancellationToken);
        var lectiveDates = lectiveDatesList.ToHashSet();
        var dates = Enumerable.Range(0, days)
            .Select(offset => end.AddDays(-offset))
            .Where(d => d <= end && lectiveDates.Contains(d))
            .OrderBy(d => d)
            .ToList();

        if (dates.Count == 0)
        {
            return (false, "No lective days found in the selected range.", null);
        }

        var existingRecords = await dbContext.AttendanceRecords
            .Where(r => r.CourseId == courseId && dates.Contains(r.Date))
            .ToListAsync(cancellationToken);

        var existingLookup = existingRecords.ToDictionary(r => (r.StudentId, r.Date));
        var now = DateTime.UtcNow;
        var recordsUpserted = 0;

        foreach (var date in dates)
        {
            foreach (var studentId in studentIds)
            {
                if (existingLookup.TryGetValue((studentId, date), out var record))
                {
                    record.Status = AttendanceStatus.Present;
                    record.Comment = null;
                    record.RecordedByUserId = recordedByUserId;
                    record.RecordedAt = now;
                }
                else
                {
                    var newRecord = new AttendanceRecord
                    {
                        StudentId = studentId,
                        CourseId = courseId,
                        Date = date,
                        Status = AttendanceStatus.Present,
                        RecordedByUserId = recordedByUserId,
                        RecordedAt = now
                    };
                    dbContext.AttendanceRecords.Add(newRecord);
                    existingLookup[(studentId, date)] = newRecord;
                }

                recordsUpserted++;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return (true, null, new SeedAttendanceResultDto(
            courseId,
            dates.Count,
            studentIds.Count,
            recordsUpserted,
            dates));
    }
}
