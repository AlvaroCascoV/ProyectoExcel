# SKILL: mvc-controller
Use this skill when creating or modifying a controller in `ProyectoExcel/Controllers/`.

## When to trigger
- "Add a new MVC controller"
- "Create a view + controller for X"
- "Add an action to an existing MVC controller"

## Exact pattern — copy this, do not invent

```csharp
using Attendance.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MvcProyectoExcel.Services;
using MvcProyectoExcel.ViewModels;

namespace MvcProyectoExcel.Controllers;

[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]   // adjust role
public class ExampleController(IAttendanceApiClient apiClient) : Controller
{
    private const int DefaultCourseId = 3430;

    [HttpGet]
    public async Task<IActionResult> Index(int? courseId, CancellationToken cancellationToken)
    {
        try
        {
            // 1. Call API via IAttendanceApiClient — never raw HttpClient, never DbContext
            var courses = await apiClient.GetCoursesAsync(cancellationToken: cancellationToken);
            var selectedId = courseId ?? courses.FirstOrDefault(c => c.Id == DefaultCourseId)?.Id
                             ?? courses.FirstOrDefault()?.Id;

            var model = new ExampleViewModel
            {
                Courses = courses,
                SelectedCourseId = selectedId
            };

            // 2. Guard: return early if no course selected
            if (!selectedId.HasValue)
                return View(model);

            // 3. Load data for selected course
            model.SelectedCourseName = courses.FirstOrDefault(c => c.Id == selectedId)?.Name ?? string.Empty;
            // ... more API calls ...

            return View(model);
        }
        catch (HttpRequestException)
        {
            // 4. Always catch HttpRequestException with this exact message pattern
            return View(new ExampleViewModel
            {
                ErrorMessage = "Could not reach the API. Make sure ApiProyectoExcel is running on http://localhost:5180."
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]   // always on POST
    public async Task<IActionResult> Save(ExampleViewModel model, CancellationToken cancellationToken)
    {
        if (!model.SelectedCourseId.HasValue)
        {
            ModelState.AddModelError(string.Empty, "Select a course.");
            return View("Index", model);
        }

        try
        {
            await apiClient.SaveAttendanceSessionAsync(/* ... */, cancellationToken);
            TempData["SuccessMessage"] = "Saved successfully.";
            return RedirectToAction(nameof(Index), new { courseId = model.SelectedCourseId });
        }
        catch (HttpRequestException ex)
        {
            model.ErrorMessage = $"Could not save: {ex.Message}";
            return View("Index", model);
        }
    }
}
```

## Rules
- `[ValidateAntiForgeryToken]` on every POST — no exceptions
- Catch only `HttpRequestException` — do not swallow other exceptions
- Use `TempData["SuccessMessage"]` for success, `model.ErrorMessage` for errors
- RedirectToAction after successful POST (PRG pattern)
- Never call `ApplicationDbContext` — only `IAttendanceApiClient`
- ViewModels in `ProyectoExcel/ViewModels/`, inherit no base class, use `IReadOnlyList<T>` for collections
- String for user ID: `User.FindFirstValue("tajamar_user_id")`
- Role check: `User.IsInRole("Teacher")` not string comparison

## ViewModel pattern
```csharp
// ProyectoExcel/ViewModels/ExampleViewModel.cs
namespace MvcProyectoExcel.ViewModels;

public class ExampleViewModel
{
    public IReadOnlyList<CourseDto> Courses { get; set; } = [];
    public int? SelectedCourseId { get; set; }
    public string SelectedCourseName { get; set; } = string.Empty;
    // feature-specific properties...
    public string? ErrorMessage { get; set; }   // always nullable string
}
```
