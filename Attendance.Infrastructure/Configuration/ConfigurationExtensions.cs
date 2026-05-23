using Microsoft.Extensions.Configuration;

namespace Attendance.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static string GetActiveSqlConnectionString(this IConfiguration configuration)
    {
        var settings = configuration.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>()
            ?? new DatabaseSettings();

        var connectionString = configuration.GetConnectionString(settings.ActiveConnection);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{settings.ActiveConnection}' is not configured. " +
                $"Set Database:{nameof(DatabaseSettings.ActiveConnection)} and ConnectionStrings:{settings.ActiveConnection} in appsettings.");
        }

        return connectionString;
    }
}
