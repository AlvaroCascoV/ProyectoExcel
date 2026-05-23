namespace Attendance.Infrastructure.Configuration;

public class DatabaseSettings
{
    public const string SectionName = "Database";

    /// <summary>
    /// Name of the entry under ConnectionStrings (e.g. ProyectoExcelDBLocal, ProyectoExcelDBMac).
    /// </summary>
    public string ActiveConnection { get; set; } = "ProyectoExcelDBLocal";
}
