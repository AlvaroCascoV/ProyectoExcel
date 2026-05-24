using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.Extensions;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddAttendanceInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("ApiProyectoExcel — Attendance API");
    });

    // Redirect root to Scalar docs in development
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
