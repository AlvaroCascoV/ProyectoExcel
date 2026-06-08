using System.Text.Json;

namespace MvcProyectoExcel.Services;

internal static class ApiErrorHelper
{
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
