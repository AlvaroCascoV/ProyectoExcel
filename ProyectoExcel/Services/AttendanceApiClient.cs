using System.Net;
using System.Net.Http.Json;
using System.Globalization;
using Attendance.Infrastructure.DTOs;

namespace MvcProyectoExcel.Services;

public interface IAttendanceApiClient
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseDto>> GetCoursesAsync(bool activeOnly = true, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StudentDto>> GetStudentsByCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<AttendanceSessionDto?> GetAttendanceSessionAsync(int courseId, DateOnly date, CancellationToken cancellationToken = default);
    Task SaveAttendanceSessionAsync(int courseId, DateOnly date, SaveAttendanceSessionRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DateOnly>> GetAttendanceDatesAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AttendanceRecordDto>> GetMyAttendanceAsync(int? courseId = null, DateOnly? from = null, DateOnly? to = null, CancellationToken cancellationToken = default);
    Task<AttendanceSummaryDto?> GetMySummaryAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseDto>> GetMyCoursesAsync(CancellationToken cancellationToken = default);
    Task<CourseStatisticsDto?> GetCourseStatisticsAsync(
        int courseId,
        int? month = null,
        int? year = null,
        decimal? minPercent = null,
        decimal? maxPercent = null,
        CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RankingEntryDto>> GetCourseRankingsAsync(
        int courseId,
        bool ascending = false,
        int? top = null,
        int? month = null,
        int? year = null,
        CancellationToken cancellationToken = default);
    Task<SeedAttendanceResultDto?> SeedPresentDaysAsync(
        int courseId,
        int days = 7,
        CancellationToken cancellationToken = default);
    Task<byte[]?> ExportCourseStatisticsPdfAsync(int courseId, int? month = null, int? year = null, decimal? minPercent = null, decimal? maxPercent = null, CancellationToken cancellationToken = default);
    Task<byte[]?> ExportAttendanceSessionPdfAsync(int courseId, DateOnly date, CancellationToken cancellationToken = default);
    Task<byte[]?> ExportStatisticsToExcelAsync(int courseId, int? month = null, int? year = null, decimal? minPercent = null, decimal? maxPercent = null, CancellationToken cancellationToken = default);
    Task<CalendarUploadResultDto?> UploadCourseCalendarAsync(int courseId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<CalendarUploadResultDto?> PreviewCourseCalendarAsync(int courseId, Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DateOnly>> GetCourseLectiveDatesAsync(int courseId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CourseCalendarEntryDto>> GetCourseCalendarEntriesAsync(int courseId, CancellationToken cancellationToken = default);
    Task<CourseCalendarStatusDto?> GetCourseCalendarStatusAsync(int courseId, CancellationToken cancellationToken = default);
}

public class AttendanceApiClient(HttpClient httpClient) : IAttendanceApiClient
{
    private static string GetSupportedCultureQuery()
    {
        // MVC sets CurrentUICulture via request localization middleware (cookie-based).
        // We keep the API contract simple: pass `culture=es|en` for export endpoints.
        var culture = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName.ToLowerInvariant();
        return culture is "es" or "en" ? culture : "es";
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("api/auth/login", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<LoginResponse>(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync(
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/courses?activeOnly={activeOnly.ToString().ToLowerInvariant()}", cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<CourseDto>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<StudentDto>> GetStudentsByCourseAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/courses/{courseId}/students", cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<StudentDto>>(cancellationToken) ?? [];
    }

    public async Task<AttendanceSessionDto?> GetAttendanceSessionAsync(
        int courseId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync(
            $"api/courses/{courseId}/attendance?date={date:yyyy-MM-dd}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<AttendanceSessionDto>(cancellationToken);
    }

    public async Task SaveAttendanceSessionAsync(
        int courseId,
        DateOnly date,
        SaveAttendanceSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PutAsJsonAsync(
            $"api/courses/{courseId}/attendance?date={date:yyyy-MM-dd}",
            request,
            cancellationToken);

        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
    }

    public async Task<IReadOnlyList<DateOnly>> GetAttendanceDatesAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/courses/{courseId}/attendance/dates", cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<DateOnly>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<AttendanceRecordDto>> GetMyAttendanceAsync(
        int? courseId = null,
        DateOnly? from = null,
        DateOnly? to = null,
        CancellationToken cancellationToken = default)
    {
        var query = new List<string>();
        if (courseId.HasValue)
        {
            query.Add($"courseId={courseId.Value}");
        }

        if (from.HasValue)
        {
            query.Add($"from={from.Value:yyyy-MM-dd}");
        }

        if (to.HasValue)
        {
            query.Add($"to={to.Value:yyyy-MM-dd}");
        }

        var url = "api/attendance/me";
        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        var response = await httpClient.GetAsync(url, cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<AttendanceRecordDto>>(cancellationToken) ?? [];
    }

    public async Task<AttendanceSummaryDto?> GetMySummaryAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/attendance/me/summary?courseId={courseId}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<AttendanceSummaryDto>(cancellationToken);
    }

    public async Task<IReadOnlyList<CourseDto>> GetMyCoursesAsync(CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync("api/attendance/me/courses", cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<CourseDto>>(cancellationToken) ?? [];
    }

    public async Task<CourseStatisticsDto?> GetCourseStatisticsAsync(
        int courseId,
        int? month = null,
        int? year = null,
        decimal? minPercent = null,
        decimal? maxPercent = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildStatisticsQuery(month, year, minPercent, maxPercent);
        var response = await httpClient.GetAsync($"api/statistics/course/{courseId}{query}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CourseStatisticsDto>(cancellationToken);
    }

    public async Task<IReadOnlyList<RankingEntryDto>> GetCourseRankingsAsync(
        int courseId,
        bool ascending = false,
        int? top = null,
        int? month = null,
        int? year = null,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string> { $"ascending={ascending.ToString().ToLowerInvariant()}" };
        if (top.HasValue)
        {
            queryParts.Add($"top={top.Value}");
        }

        if (month.HasValue)
        {
            queryParts.Add($"month={month.Value}");
        }

        if (year.HasValue)
        {
            queryParts.Add($"year={year.Value}");
        }

        var query = "?" + string.Join("&", queryParts);
        var response = await httpClient.GetAsync($"api/statistics/course/{courseId}/rankings{query}", cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<RankingEntryDto>>(cancellationToken) ?? [];
    }

    private static string BuildStatisticsQuery(int? month, int? year, decimal? minPercent, decimal? maxPercent)
    {
        var queryParts = new List<string>();
        if (month.HasValue)
        {
            queryParts.Add($"month={month.Value}");
        }

        if (year.HasValue)
        {
            queryParts.Add($"year={year.Value}");
        }

        if (minPercent.HasValue)
        {
            queryParts.Add($"minPercent={minPercent.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        if (maxPercent.HasValue)
        {
            queryParts.Add($"maxPercent={maxPercent.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)}");
        }

        return queryParts.Count == 0 ? string.Empty : "?" + string.Join("&", queryParts);
    }

    public async Task<SeedAttendanceResultDto?> SeedPresentDaysAsync(
        int courseId,
        int days = 7,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsync(
            $"api/courses/{courseId}/attendance/seed-present?days={days}",
            null,
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<SeedAttendanceResultDto>(cancellationToken);
    }

    public async Task<byte[]?> ExportCourseStatisticsPdfAsync(
        int courseId,
        int? month = null,
        int? year = null,
        decimal? minPercent = null,
        decimal? maxPercent = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildStatisticsQuery(month, year, minPercent, maxPercent);
        var culture = GetSupportedCultureQuery();
        var separator = string.IsNullOrEmpty(query) ? "?" : "&";
        var response = await httpClient.GetAsync(
            $"api/statistics/course/{courseId}/export/pdf{query}{separator}culture={culture}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<byte[]?> ExportAttendanceSessionPdfAsync(
        int courseId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var culture = GetSupportedCultureQuery();
        var response = await httpClient.GetAsync(
            $"api/courses/{courseId}/attendance/export/pdf?date={date:yyyy-MM-dd}&culture={culture}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<byte[]?> ExportStatisticsToExcelAsync(
        int courseId,
        int? month = null,
        int? year = null,
        decimal? minPercent = null,
        decimal? maxPercent = null,
        CancellationToken cancellationToken = default)
    {
        var query = BuildStatisticsQuery(month, year, minPercent, maxPercent);
        var culture = GetSupportedCultureQuery();
        var separator = string.IsNullOrEmpty(query) ? "?" : "&";
        var response = await httpClient.GetAsync(
            $"api/statistics/course/{courseId}/export{query}{separator}culture={culture}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound) return null;
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<CalendarUploadResultDto?> UploadCourseCalendarAsync(
        int courseId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(streamContent, "file", fileName);

        var response = await httpClient.PostAsync($"api/courses/{courseId}/calendar/upload", content, cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<CalendarUploadResultDto>(cancellationToken);
    }

    public async Task<CalendarUploadResultDto?> PreviewCourseCalendarAsync(
        int courseId,
        Stream fileStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
        content.Add(streamContent, "file", fileName);

        var response = await httpClient.PostAsync($"api/courses/{courseId}/calendar/preview", content, cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);

        return await response.Content.ReadFromJsonAsync<CalendarUploadResultDto>(cancellationToken);
    }

    public async Task<IReadOnlyList<DateOnly>> GetCourseLectiveDatesAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/courses/{courseId}/calendar/dates", cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<DateOnly>>(cancellationToken) ?? [];
    }

    public async Task<IReadOnlyList<CourseCalendarEntryDto>> GetCourseCalendarEntriesAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/courses/{courseId}/calendar/entries", cancellationToken);
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<List<CourseCalendarEntryDto>>(cancellationToken) ?? [];
    }

    public async Task<CourseCalendarStatusDto?> GetCourseCalendarStatusAsync(
        int courseId,
        CancellationToken cancellationToken = default)
    {
        var response = await httpClient.GetAsync($"api/courses/{courseId}/calendar/status", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        await ApiErrorHelper.EnsureSuccessOrThrowAsync(response, cancellationToken);
        return await response.Content.ReadFromJsonAsync<CourseCalendarStatusDto>(cancellationToken);
    }
}

public record CalendarUploadResultDto(
    string Message,
    int TotalDays,
    int LectiveDays,
    int Festivos,
    int NoLectivos);

public record CourseCalendarEntryDto(
    DateOnly Date,
    bool IsLective,
    string? DayType,
    string? Module,
    string? Teacher,
    string? Room);

public record CourseCalendarStatusDto(
    bool HasCalendar,
    int LectiveCount);
