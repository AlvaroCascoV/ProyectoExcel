using Attendance.Infrastructure.Entities;

namespace Attendance.Infrastructure.Services;

public static class LectiveDayCalendar
{
    public const int LectiveDaysPerYear = 156;

    public static DateOnly GetCourseStartDate(DateTime? courseStartDate) =>
        courseStartDate.HasValue
            ? DateOnly.FromDateTime(courseStartDate.Value)
            : new DateOnly(2025, 10, 1);

    public static DateOnly GetCourseEndDate(DateTime? courseEndDate, DateOnly courseStartDate) =>
        courseEndDate.HasValue
            ? DateOnly.FromDateTime(courseEndDate.Value)
            : courseStartDate.AddMonths(9);

    public static IReadOnlyList<DateOnly> GetWeekdaysInRange(DateOnly from, DateOnly to)
    {
        var dates = new List<DateOnly>();
        var current = from;

        while (current <= to)
        {
            if (current.DayOfWeek is not DayOfWeek.Saturday and not DayOfWeek.Sunday)
            {
                dates.Add(current);
            }

            current = current.AddDays(1);
        }

        return dates;
    }

    public static IReadOnlyList<DateOnly> GetLectiveDates(
        DateOnly courseStartDate,
        DateOnly courseEndDate,
        int count = LectiveDaysPerYear)
    {
        if (courseEndDate < courseStartDate)
        {
            courseEndDate = courseStartDate;
        }

        var allWeekdays = GetWeekdaysInRange(courseStartDate, courseEndDate);

        if (allWeekdays.Count == 0)
        {
            return allWeekdays;
        }

        if (allWeekdays.Count <= count)
        {
            return allWeekdays;
        }

        if (count == 1)
        {
            return [allWeekdays[0]];
        }

        var dates = new List<DateOnly>(count);
        for (var i = 0; i < count; i++)
        {
            var index = (int)((long)i * (allWeekdays.Count - 1) / (count - 1));
            dates.Add(allWeekdays[index]);
        }

        return dates;
    }

    public static IReadOnlyList<DateOnly> FilterLectiveDates(
        IReadOnlyList<DateOnly> lectiveDates,
        DateOnly? from = null,
        DateOnly? to = null,
        int? month = null,
        int? year = null)
    {
        IEnumerable<DateOnly> query = lectiveDates;

        if (from.HasValue)
        {
            query = query.Where(d => d >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(d => d <= to.Value);
        }

        if (month is >= 1 and <= 12)
        {
            query = query.Where(d => d.Month == month.Value);
        }

        if (year is > 0)
        {
            query = query.Where(d => d.Year == year.Value);
        }

        return query.ToList();
    }

    public static bool HasDateFilter(DateOnly? from, DateOnly? to, int? month, int? year) =>
        from.HasValue || to.HasValue || month is >= 1 and <= 12 || year is > 0;

    public static int GetDenominator(IReadOnlyList<DateOnly> lectiveDates, bool isFiltered) =>
        isFiltered ? lectiveDates.Count : LectiveDaysPerYear;

    public static IReadOnlyList<AttendanceStatus> BuildStatuses(
        IReadOnlyList<DateOnly> lectiveDates,
        IReadOnlyDictionary<DateOnly, AttendanceStatus>? recordsByDate,
        DateOnly today)
    {
        var statuses = new List<AttendanceStatus>(lectiveDates.Count);

        foreach (var date in lectiveDates)
        {
            if (date > today)
            {
                statuses.Add(AttendanceStatus.Present);
                continue;
            }

            if (recordsByDate is not null && recordsByDate.TryGetValue(date, out var status))
            {
                statuses.Add(status);
            }
            else
            {
                statuses.Add(AttendanceStatus.Present);
            }
        }

        return statuses;
    }
}
