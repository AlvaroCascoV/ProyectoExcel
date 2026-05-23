using Attendance.Infrastructure.Data;
using Attendance.Infrastructure.Entities;
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Attendance.Infrastructure.Data;

public static class DbInitializer
{
    public const string DefaultDevPassword = "12345";

    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ApplicationDbContext>>();

        try
        {
            await context.Database.MigrateAsync();
            await EnsureRolesAsync(roleManager);
            await SeedIdentityUsersAsync(context, userManager, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Database initialization failed. Run script.sql against ProyectoExcel, then restart the API. " +
                "See ApiProyectoExcel/appsettings.json (ConnectionStrings + Database:ActiveConnection).");
            throw;
        }
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in new[] { AppRoles.Admin, AppRoles.Teacher, AppRoles.Student })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedIdentityUsersAsync(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        ILogger logger)
    {
        var tajamarUsers = await context.TajamarUsers
            .AsNoTracking()
            .Include(u => u.Role)
            .Where(u => u.IsActive == true && u.Email != null)
            .ToListAsync();

        var created = 0;
        foreach (var tajamarUser in tajamarUsers)
        {
            var existing = await userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.TajamarUserId == tajamarUser.Id);

            if (existing is not null)
            {
                continue;
            }

            var identityUser = new ApplicationUser
            {
                UserName = tajamarUser.Email,
                Email = tajamarUser.Email,
                EmailConfirmed = true,
                TajamarUserId = tajamarUser.Id,
                FirstName = tajamarUser.FirstName,
                LastName = tajamarUser.LastName
            };

            var createResult = await userManager.CreateAsync(identityUser, DefaultDevPassword);
            if (!createResult.Succeeded)
            {
                logger.LogWarning(
                    "Could not create identity user for {Email}: {Errors}",
                    tajamarUser.Email,
                    string.Join(", ", createResult.Errors.Select(e => e.Description)));
                continue;
            }

            var appRole = AppRoles.FromTajamarRole(tajamarUser.Role?.Name);
            await userManager.AddToRoleAsync(identityUser, appRole);
            created++;
        }

        logger.LogInformation(
            "Identity seed complete. {Created} new users. Dev password for new users: {Password}",
            created,
            DefaultDevPassword);
    }
}
