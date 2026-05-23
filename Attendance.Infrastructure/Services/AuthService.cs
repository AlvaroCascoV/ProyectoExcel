using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Attendance.Infrastructure.Configuration;
using Attendance.Infrastructure.DTOs;
using Attendance.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Attendance.Infrastructure.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(AuthUserDto user);
}

public class JwtTokenService(IOptions<JwtSettings> options) : IJwtTokenService
{
    private readonly JwtSettings _settings = options.Value;

    public (string Token, DateTime ExpiresAt) CreateToken(AuthUserDto user)
    {
        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Role, user.Role),
            new("tajamar_user_id", user.TajamarUserId.ToString()),
            new("full_name", user.FullName)
        };

        foreach (var courseId in user.CourseIds)
        {
            claims.Add(new Claim("course_id", courseId.ToString()));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}

public class AuthService(
    Microsoft.AspNetCore.Identity.UserManager<ApplicationUser> userManager,
    Microsoft.AspNetCore.Identity.SignInManager<ApplicationUser> signInManager,
    Attendance.Infrastructure.Data.ApplicationDbContext dbContext,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<LoginResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return null;
        }

        var result = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: false);
        if (!result.Succeeded)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        var role = roles.FirstOrDefault() ?? AppRoles.Student;

        var courseIds = await dbContext.CourseEnrollments
            .AsNoTracking()
            .Where(e => e.UserId == user.TajamarUserId)
            .Select(e => e.CourseId!.Value)
            .Distinct()
            .ToListAsync(cancellationToken);

        var authUser = new AuthUserDto(
            user.Id,
            user.Email ?? request.Email,
            role,
            user.TajamarUserId,
            $"{user.FirstName} {user.LastName}".Trim(),
            courseIds);

        var (token, expiresAt) = jwtTokenService.CreateToken(authUser);

        return new LoginResponse(
            token,
            authUser.Email,
            authUser.Role,
            authUser.TajamarUserId,
            authUser.FullName,
            authUser.CourseIds,
            expiresAt);
    }
}

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static string FromTajamarRole(string? tajamarRole) => tajamarRole switch
    {
        "ADMINISTRADOR" => Admin,
        "PROFESOR" => Teacher,
        "ALUMNO" => Student,
        _ => Student
    };
}
