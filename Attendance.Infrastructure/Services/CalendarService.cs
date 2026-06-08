using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure.Services;

public interface ICalendarService
{
    Task<IReadOnlyList<DateOnly>> GetLectiveDatesAsync(int courseId, CancellationToken ct = default);
    Task<bool> HasCalendarAsync(int courseId, CancellationToken ct = default);
    Task UploadCalendarAsync(int courseId, List<CourseCalendarEntry> entries, CancellationToken ct = default);
    Task<IReadOnlyList<CourseCalendarEntry>> GetCalendarEntriesAsync(int courseId, CancellationToken ct = default);
    Task<int> GetLectiveDaysCountAsync(int courseId, CancellationToken ct = default);
}

public class CalendarService : ICalendarService
{
    private readonly ApplicationDbContext _dbContext;

    public CalendarService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DateOnly>> GetLectiveDatesAsync(int courseId, CancellationToken ct = default)
    {
        var hasCal = await HasCalendarAsync(courseId, ct);
        if (hasCal)
        {
            var dates = await _dbContext.CourseCalendarEntries
                .Where(e => e.CourseId == courseId && e.IsLective)
                .OrderBy(e => e.Date)
                .Select(e => e.Date)
                .ToListAsync(ct);
            return dates;
        }

        // Fallback to LectiveDayCalendar
        var course = await _dbContext.Courses.FindAsync([courseId], ct);
        if (course == null)
        {
            return Array.Empty<DateOnly>();
        }

        var start = LectiveDayCalendar.GetCourseStartDate(course.StartDate);
        var end = LectiveDayCalendar.GetCourseEndDate(course.EndDate, start);
        return LectiveDayCalendar.GetLectiveDates(start, end);
    }

    public async Task<bool> HasCalendarAsync(int courseId, CancellationToken ct = default)
    {
        return await _dbContext.CourseCalendarEntries.AnyAsync(e => e.CourseId == courseId, ct);
    }

    public async Task UploadCalendarAsync(int courseId, List<CourseCalendarEntry> entries, CancellationToken ct = default)
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync(ct);
        try
        {
            // Remove existing calendar entries for this course
            var existing = await _dbContext.CourseCalendarEntries
                .Where(e => e.CourseId == courseId)
                .ToListAsync(ct);

            if (existing.Any())
            {
                _dbContext.CourseCalendarEntries.RemoveRange(existing);
            }

            if (entries != null && entries.Any())
            {
                await _dbContext.CourseCalendarEntries.AddRangeAsync(entries, ct);
            }

            await _dbContext.SaveChangesAsync(ct);
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<IReadOnlyList<CourseCalendarEntry>> GetCalendarEntriesAsync(int courseId, CancellationToken ct = default)
    {
        return await _dbContext.CourseCalendarEntries
            .Where(e => e.CourseId == courseId)
            .OrderBy(e => e.Date)
            .ToListAsync(ct);
    }

    public async Task<int> GetLectiveDaysCountAsync(int courseId, CancellationToken ct = default)
    {
        var hasCal = await HasCalendarAsync(courseId, ct);
        if (hasCal)
        {
            return await _dbContext.CourseCalendarEntries
                .CountAsync(e => e.CourseId == courseId && e.IsLective, ct);
        }

        return LectiveDayCalendar.LectiveDaysPerYear;
    }
}
