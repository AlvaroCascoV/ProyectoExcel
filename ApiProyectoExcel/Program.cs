using System.Text.Json;
using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.Extensions;
using System.Globalization;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Do not set ResourcesPath here: it changes the resource base-name lookup and breaks
// localization for resources embedded in referenced class libraries (e.g. Attendance.Infrastructure).
builder.Services.AddLocalization();
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAttendanceInfrastructure(builder.Configuration);

var app = builder.Build();

var supportedCultures = new[] { "es", "en" };
app.Use(async (context, next) =>
{
    // Culture is propagated from MVC via a query param for export endpoints:
    //   ?culture=es|en
    var cultureParam = context.Request.Query["culture"].ToString();
    if (!string.IsNullOrWhiteSpace(cultureParam))
    {
        var normalized = cultureParam.Trim().ToLowerInvariant();
        if (supportedCultures.Contains(normalized, StringComparer.Ordinal))
        {
            var culture = CultureInfo.GetCultureInfo(normalized);
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
        }
    }

    await next();
});

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger("GlobalExceptionHandler");
        logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var payload = app.Environment.IsDevelopment()
            ? new { message = ex.Message, detail = ex.StackTrace }
            : new { message = "An internal server error occurred.", detail = (string?)null };

        await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
});

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ApiProyectoExcel — Attendance API");
    });

    app.MapGet("/", () => Results.Redirect("/scalar"))
       .ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await DbInitializer.InitializeAsync(app.Services);

app.Logger.LogInformation("API docs available at /scalar (Development only).");

app.Run();
