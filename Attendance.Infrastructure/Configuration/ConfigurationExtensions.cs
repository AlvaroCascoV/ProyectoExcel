using System.Runtime.InteropServices;
using Microsoft.Extensions.Configuration;

namespace Attendance.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static string GetActiveSqlConnectionString(this IConfiguration configuration)
    {
        var connectionStringName = RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
            ? "ProyectoExcelDBLocal"
            : "ProyectoExcelDBMac";

        var connectionString = configuration.GetConnectionString(connectionStringName);
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                $"Connection string '{connectionStringName}' is not configured. " +
                $"Add ConnectionStrings:{connectionStringName} in appsettings.json.");
        }

        return connectionString;
    }
}
