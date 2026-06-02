using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace Attendance.Infrastructure.Services;

public interface IStatisticsService
{
    Task<CourseStatisticsDto?> GetCourseStatisticsAsync(
        int courseId,
        DateOnly? from = null,
        DateOnly? to = null,
        int? month = null,
        int? year = null,
        decimal? minPercent = null,
        decimal? maxPercent = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RankingEntryDto>> GetCourseRankingsAsync(
        int courseId,
        bool ascending = false,
        int? top = null,
        DateOnly? from = null,
        DateOnly? to = null,
        int? month = null,
        int? year = null,
        CancellationToken cancellationToken = default);
}

public class StatisticsService(ApplicationDbContext dbContext, ICalendarService calendarService) : IStatisticsService
{
    public async Task<CourseStatisticsDto?> GetCourseStatisticsAsync(
        int courseId,
        DateOnly? from = null,
        DateOnly? to = null,
        int? month = null,
        int? year = null,
        decimal? minPercent = null,
        decimal? maxPercent = null,
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

        var allLectiveDates = await calendarService.GetLectiveDatesAsync(courseId, cancellationToken);
        var isFiltered = LectiveDayCalendar.HasDateFilter(from, to, month, year);
        var lectiveDates = LectiveDayCalendar.FilterLectiveDates(allLectiveDates, from, to, month, year);
        var totalClassDays = isFiltered ? lectiveDates.Count : allLectiveDates.Count;
        var today = DateOnly.FromDateTime(DateTime.Today);

        var recordsByStudent = await LoadRecordsByStudentAsync(courseId, cancellationToken);

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

        var studentStats = new List<StudentStatisticsDto>(students.Count);

        foreach (var student in students)
        {
            recordsByStudent.TryGetValue(student.Id, out var recordsByDate);
            var statuses = LectiveDayCalendar.BuildStatuses(lectiveDates, recordsByDate, today);
            var metrics = AttendanceMetricsCalculator.FromStatuses(statuses, totalClassDays);

            studentStats.Add(new StudentStatisticsDto(
                student.Id,
                student.FullName,
                metrics.PresentCount,
                metrics.AbsentCount,
                metrics.LateCount,
                metrics.JustifiedAbsentCount,
                metrics.JustifiedLateCount,
                metrics.EarlyLeaveCount,
                metrics.JustifiedEarlyLeaveCount,
                metrics.UnjustifiedWeighted,
                metrics.RealAttendancePercentage,
                metrics.AbsentFPercentage,
                metrics.AbsentFRPercentage,
                metrics.DiplomaEligible,
                metrics.AtRiskDrop,
                0));
        }

        if (minPercent.HasValue)
        {
            studentStats = studentStats
                .Where(s => s.RealAttendancePercentage >= minPercent.Value)
                .ToList();
        }

        if (maxPercent.HasValue)
        {
            studentStats = studentStats
                .Where(s => s.RealAttendancePercentage <= maxPercent.Value)
                .ToList();
        }

        var ranked = RankStudents(studentStats, ascending: false);
        var average = ranked.Count == 0
            ? 100m
            : Math.Round(ranked.Average(s => s.RealAttendancePercentage), 1);

        return new CourseStatisticsDto(
            course.Id,
            course.Name,
            totalClassDays,
            average,
            ranked.Count(s => !s.DiplomaEligible),
            ranked.Count(s => s.AtRiskDrop),
            ranked);
    }

    public async Task<IReadOnlyList<RankingEntryDto>> GetCourseRankingsAsync(
        int courseId,
        bool ascending = false,
        int? top = null,
        DateOnly? from = null,
        DateOnly? to = null,
        int? month = null,
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        var statistics = await GetCourseStatisticsAsync(
            courseId,
            from,
            to,
            month,
            year,
            cancellationToken: cancellationToken);

        if (statistics is null)
        {
            return [];
        }

        var ranked = RankStudents(statistics.Students.ToList(), ascending);
        var query = ranked.Select(s => new RankingEntryDto(
            s.Rank,
            s.StudentId,
            s.FullName,
            s.RealAttendancePercentage,
            s.UnjustifiedWeighted,
            s.DiplomaEligible));

        if (top.HasValue && top.Value > 0)
        {
            query = query.Take(top.Value);
        }

        return query.ToList();
    }

    private async Task<Dictionary<int, Dictionary<DateOnly, AttendanceStatus>>> LoadRecordsByStudentAsync(
        int courseId,
        CancellationToken cancellationToken)
    {
        var records = await dbContext.AttendanceRecords
            .AsNoTracking()
            .Where(r => r.CourseId == courseId)
            .Select(r => new { r.StudentId, r.Date, r.Status })
            .ToListAsync(cancellationToken);

        return records
            .GroupBy(r => r.StudentId)
            .ToDictionary(
                g => g.Key,
                g => g.ToDictionary(r => r.Date, r => r.Status));
    }

    private static List<StudentStatisticsDto> RankStudents(List<StudentStatisticsDto> students, bool ascending)
    {
        var ordered = ascending
            ? students.OrderBy(s => s.RealAttendancePercentage).ThenBy(s => s.FullName).ToList()
            : students.OrderByDescending(s => s.RealAttendancePercentage).ThenBy(s => s.FullName).ToList();

        var ranked = new List<StudentStatisticsDto>(ordered.Count);
        for (var i = 0; i < ordered.Count; i++)
        {
            var student = ordered[i];
            ranked.Add(student with { Rank = i + 1 });
        }

        return ranked;
    }
}
