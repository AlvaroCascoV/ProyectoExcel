using System.Net.Http;
using System.Text.Json;

namespace MvcProyectoExcel.Services;

internal static class ApiErrorHelper
{
    public static async Task EnsureSuccessOrThrowAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        throw new HttpRequestException(BuildErrorMessage(response, body));
    }

    public static string BuildErrorMessage(HttpResponseMessage response, string? body)
    {
        var message = ExtractMessage(body);
        if (!string.IsNullOrWhiteSpace(message))
        {
            return message;
        }

        if (!string.IsNullOrWhiteSpace(response.ReasonPhrase))
        {
            return response.ReasonPhrase;
        }

        return $"HTTP {(int)response.StatusCode} ({response.StatusCode})";
    }

    public static string ExtractMessage(string? body)
    {
        if (string.IsNullOrWhiteSpace(body))
        {
            return string.Empty;
        }

        var trimmed = body.Trim();
        if (!trimmed.StartsWith('{'))
        {
            return trimmed;
        }

        try
        {
            using var doc = JsonDocument.Parse(trimmed);
            if (doc.RootElement.TryGetProperty("message", out var message))
            {
                return message.GetString() ?? trimmed;
            }

            if (doc.RootElement.TryGetProperty("title", out var title))
            {
                return title.GetString() ?? trimmed;
            }
        }
        catch (JsonException)
        {
            // fall through to raw body
        }

        return trimmed;
    }
}
