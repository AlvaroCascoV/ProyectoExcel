# SKILL: api-controller
Use this skill when creating or modifying a controller in `ApiProyectoExcel/Controllers/`.

## When to trigger
- "Add a new API endpoint"
- "Create a controller for X"
- "Add an action to an existing API controller"

## Exact pattern

```csharp
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiProyectoExcel.Controllers;

[ApiController]
[Route("api/[controller]")]                             // or [Route("api")] with explicit routes
[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]
public class ExampleController(IExampleService exampleService) : ControllerBase
{
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken = default)
    {
        var result = await exampleService.GetByIdAsync(id, cancellationToken);

        if (result is null)
            return NotFound(new { message = $"Item {id} not found." });

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateExampleRequest request,
        CancellationToken cancellationToken = default)
    {
        // Resolve current user when needed
        if (!TryGetTajamarUserId(out var userId))
            return Unauthorized(new { message = "Could not resolve the current user." });

        var (success, error) = await exampleService.CreateAsync(request, userId, cancellationToken);

        if (!success)
        {
            return error?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true
                ? NotFound(new { message = error })
                : BadRequest(new { message = error });
        }

        return Ok();
    }

    // Copy this helper into any controller that needs the current user
    private bool TryGetTajamarUserId(out int userId)
    {
        var claim = User.FindFirstValue("tajamar_user_id");
        return int.TryParse(claim, out userId);
    }
}
```

## Rules
- Always `[ApiController]` + `ControllerBase` — never `Controller`
- Return `IActionResult` — never `ActionResult<T>`
- Error responses always use `{ message = "..." }` — never bare strings
- Auth: use `AppRoles` constants — `AppRoles.Teacher`, `AppRoles.Admin`, `AppRoles.Student`
- Development-only endpoints: guard with `if (!environment.IsDevelopment()) return NotFound(new { message = "..." });`
- Inject `IWebHostEnvironment` only when needed for dev-only guards
- Query params: `[FromQuery]` — body params: `[FromBody]`
- Never inject `ApplicationDbContext` directly — always a service interface

## Validation pattern for percent/numeric filters
```csharp
// Enforce bounds before calling service
if (minPercent is < 0 or > 100)
    return BadRequest(new { message = "minPercent must be between 0 and 100." });

if (maxPercent is < 0 or > 100)
    return BadRequest(new { message = "maxPercent must be between 0 and 100." });

if (minPercent.HasValue && maxPercent.HasValue && minPercent > maxPercent)
    return BadRequest(new { message = "minPercent cannot be greater than maxPercent." });
```

## Non-lective day guard
```csharp
// Before saving attendance for a date:
var weekday = date.DayOfWeek;
if (weekday is DayOfWeek.Saturday or DayOfWeek.Sunday)
    return BadRequest(new { message = "Cannot save attendance on weekends." });
```
