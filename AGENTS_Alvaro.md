# AGENTS.md — Coding rules for AI agents

Read [Context_Alvaro.md](Context_Alvaro.md) first for project overview and architecture.

---

## Language and framework

- **.NET 10**, C# 14, ASP.NET Core, EF Core 10, SQL Server.
- All projects target `net10.0`.
- Nullable reference types are enabled (`<Nullable>enable</Nullable>`).
- Implicit usings are enabled.

---

## Project boundaries

| What | Belongs in |
|------|-----------|
| Entities, DTOs, services, DbContext, EF config, migrations | `Attendance.Infrastructure/` |
| API controllers | `ApiProyectoExcel/` |
| MVC controllers, ViewModels, Views, frontend assets | `ProyectoExcel/` |

- Never add NuGet packages to the wrong project. The Infrastructure library owns all data-access and business-logic packages. The API and MVC projects should stay thin.
- The MVC project must never reference `ApplicationDbContext` or query the database directly. All data access goes through `IAttendanceApiClient` (HTTP calls to the API).

---

## C# style

- **File-scoped namespaces** (`namespace Foo;` not `namespace Foo { }`).
- **Primary constructors** for dependency injection (e.g., `public class MyService(ApplicationDbContext db)`).
- **Immutable DTOs** as `record` types (e.g., `public record CourseDto(int Id, string Name)`).
- Return **`IReadOnlyList<T>`** from service methods, not `List<T>`.
- **Propagate `CancellationToken`** through all async method chains. Add `CancellationToken cancellationToken = default` as the last parameter.
- Prefer `var` when the type is obvious from the right-hand side.
- No `#region` blocks.

---

## EF Core

- Use **`AsNoTracking()`** for all read-only queries.
- All entity configuration uses **Fluent API** in `ApplicationDbContext.OnModelCreating` — no data annotations on entity classes.
- Legacy Tajamar tables (any table mapped with `ExcludeFromMigrations()`) must keep that flag. Never generate migrations that create or alter these tables.
- New EF-managed tables are fine and will be handled by migrations.
- Use **`ToDictionaryAsync`**, **`ToListAsync`**, **`AnyAsync`** etc. — never block with `.Result` or `.Wait()`.

---

## Naming conventions

| Artifact | Convention | Example |
|----------|-----------|---------|
| Entity properties | English names | `FirstName`, `StartDate` |
| SQL column mapping | Spanish legacy names via `.HasColumnName()` | `.HasColumnName("NOMBRE")` |
| DTOs | Suffix `Dto` | `CourseDto`, `AttendanceSessionDto` |
| Request DTOs | Suffix `Request` | `SaveAttendanceEntryRequest` |
| Response DTOs | Suffix `Response` | `LoginResponse` |
| ViewModels | Suffix `ViewModel` | `StudentListViewModel` |
| Services | `I{Name}Service` + `{Name}Service` | `ICourseService` / `CourseService` |
| Controllers | `{Name}Controller` | `AttendanceController` |

---

## API conventions (ApiProyectoExcel)

- All controllers use `[ApiController]` and inherit `ControllerBase`.
- Route prefix: `api/` (e.g., `[Route("api/[controller]")]` or `[Route("api")]` with explicit `[HttpGet("...")]`).
- Return `IActionResult` (not `ActionResult<T>`).
- Role-based auth: `[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]`. Use the `AppRoles` constants, not string literals.
- Error responses: return `BadRequest(new { message = "..." })` or `NotFound(new { message = "..." })`. Always use the `message` property key.
- Resolve the current user's Tajamar ID from the `tajamar_user_id` claim.

---

## MVC conventions (ProyectoExcel)

- Controllers inherit `Controller` and return `View(viewModel)` with a strongly-typed ViewModel.
- All API communication goes through `IAttendanceApiClient`. Never use raw `HttpClient` in controllers.
- Never call `ApplicationDbContext` from the MVC project.
- Use `IHtmlLocalizer<SharedResource>` (injected as `SharedLocalizer` in `_ViewImports.cshtml`) for all user-facing strings.
- Role checks: `User.IsInRole("Teacher")`, `User.IsInRole("Student")`, `User.IsInRole("Admin")`.

---

## Frontend (Views and static assets)

- Use **Bootstrap 5** utility classes. No inline styles.
- Respect dark mode: the layout uses `data-bs-theme` on `<html>`. Custom CSS must work in both light and dark themes.
- Use **jQuery** for DOM manipulation. Use **Chart.js** for charts.
- Icons: Bootstrap Icons (`bi bi-*` classes).
- Custom styles go in `wwwroot/css/site.css`. Custom JS goes in `wwwroot/js/site.js`.
- No CSS preprocessors (Sass/LESS). No JS bundlers (Webpack/Vite).

---

## Localization

- Default culture: **`es`** (Spanish). Supported: `es`, `en`.
- All UI-facing strings must have entries in both `Resources/SharedResource.es.resx` and `Resources/SharedResource.en.resx`.
- In Razor views, use `@SharedLocalizer["KeyName"]`.
- Never hardcode user-visible text in views or controllers.

---

## AttendanceStatus enum

Always use the named enum values. Never use magic numbers.

| Value | Name |
|-------|------|
| 0 | `Present` |
| 1 | `Absent` |
| 2 | `Late` |
| 3 | `JustifiedAbsent` |
| 4 | `JustifiedLate` |
| 5 | `EarlyLeave` |
| 6 | `JustifiedEarlyLeave` |

---

## Testing

No test project exists yet. If adding tests:

- Use **xUnit** as the test framework.
- Place the test project at the solution root alongside the other projects.
- Name it `ProyectoExcel.Tests` or `Attendance.Tests`.

---

## Git hygiene

- Do **not** commit `appsettings.Development.json` with real credentials.
- Do **not** commit connection strings containing real passwords.
- `bin/` and `obj/` folders are already gitignored.
- Commit messages in English.
