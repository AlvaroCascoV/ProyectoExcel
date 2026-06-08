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

- Never add NuGet packages to the wrong project. The Infrastructure library owns all data-access and business-logic packages (`ClosedXML`, `QuestPDF`, EF Core, Identity, JWT). The API and MVC projects should stay thin.
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
- New EF-managed tables (e.g., `CourseCalendarEntry` / `CALENDARIOCURSO`) are fine and will be handled by migrations.
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
- Export endpoints accept optional `?culture=es|en` — culture middleware in `Program.cs` sets thread culture for localized file output.

---

## MVC conventions (ProyectoExcel)

- Controllers inherit `Controller` and return `View(viewModel)` with a strongly-typed ViewModel.
- All API communication goes through `IAttendanceApiClient`. Never use raw `HttpClient` in controllers.
- Never call `ApplicationDbContext` from the MVC project.
- Use `IHtmlLocalizer<SharedResource>` (injected as `SharedLocalizer` in `_ViewImports.cshtml`) for all user-facing strings.
- Role checks: `User.IsInRole("Teacher")`, `User.IsInRole("Student")`, `User.IsInRole("Admin")`.
- Export actions return `File(bytes, contentType, fileName)` — no export generation logic in MVC controllers.
- Login form collects `EmailLocalPart` only; controller builds `{localPart}@tajamar365.com` before calling the API.

---

## Frontend (Views and static assets)

- Use **Bootstrap 5** utility classes. No inline styles.
- Respect dark mode: the layout uses `data-bs-theme` on `<html>`. Custom CSS must work in both light and dark themes.
- Use **jQuery** for DOM manipulation. Use **Chart.js** for charts.
- Icons: Bootstrap Icons (`bi bi-*` classes).
- Flags: **flag-icons** CSS library (`fi fi-xx` classes) — CDN link in `_Layout.cshtml`.
- Custom styles go in `wwwroot/css/site.css`. Custom JS goes in `wwwroot/js/site.js`.
- No CSS preprocessors (Sass/LESS). No JS bundlers (Webpack/Vite).

---

## Export UI convention

- Use the shared partial `Views/Shared/_ExportMenu.cshtml` with `ExportMenuViewModel` (PdfUrl, ExcelUrl).
- Do not duplicate export button/dropdown markup in individual views — pass URLs via `Url.Action("ExportPdf", ...)` and `Url.Action("Export", ...)`.
- Used in Statistics and Attendance views; extend the same pattern for new export surfaces.

---

## Localization

- Default culture: **`es`** (Spanish). Supported: `es`, `en`.
- **MVC UI strings** must have entries in both `ProyectoExcel/Resources/SharedResource.es.resx` and `SharedResource.en.resx`.
- In Razor views, use `@SharedLocalizer["KeyName"]`.
- Never hardcode user-visible text in views or controllers.

### Export localization (PDF / Excel file content)

- Export-facing strings live in `Attendance.Infrastructure/Resources/ExportResource.{es,en}.resx` with marker class `ExportResource.cs`.
- Never put export file content strings in `SharedResource` — keep MVC UI and export output separate.
- `PdfExportService` and `ExcelExportService` inject `IStringLocalizer<ExportResource>`.
- `AttendanceApiClient` must append `culture=es|en` (from `CultureInfo.CurrentCulture`) on all export HTTP calls.
- API: do **not** set `ResourcesPath` in `AddLocalization()` — it breaks embedded resources from referenced class libraries. Use the query-param culture middleware in `ApiProyectoExcel/Program.cs` instead.

---

## AttendanceStatus enum

Always use the named enum values. Never use magic numbers.

| Value | Name | Display code |
|-------|------|--------------|
| 0 | `Present` | — |
| 1 | `Absent` | F |
| 2 | `Late` | R |
| 3 | `JustifiedAbsent` | FJ |
| 4 | `JustifiedLate` | RJ |
| 5 | `EarlyLeave` | SAF |
| 6 | `JustifiedEarlyLeave` | SAFJ |

---

## Business rules — do not change without team consensus

- **Diploma eligibility:** `RealAttendancePercentage >= 80%` (`AttendanceMetricsCalculator.DiplomaThreshold`)
- **Diploma warning (UI):** `RealAttendancePercentage < 85%` (`BelowDiplomaWarning` — early alert before official 80% breach)
- **Drop risk:** `RealAttendancePercentage < 75%` (`DropThreshold`)
- **Lective days per year:** 156 (`LectiveDayCalendar.LectiveDaysPerYear`, or custom calendar count)
- **Non-lective days:** validated via `ICalendarService` when a custom calendar is uploaded; falls back to `LectiveDayCalendar` otherwise
- **Weekend days:** Sat & Sun never lective by default — overridden if marked lective in custom calendar entries
- **Attendance filter bounds:** `minPercent` must be ≥ 0, `maxPercent` must be ≤ 100. Enforce at both API and MVC level.

---

## Skills reference

Use these skill files for repeatable patterns — do not reinvent from scratch:

| Skill | Path | When to use |
|-------|------|-------------|
| Student live search | [Skills/student-search.md](Skills/student-search.md) | Client-side name filter on student tables (column index 1, no API calls) |
| Excel export | [Skills/excel-export.md](Skills/excel-export.md) | ClosedXML statistics export endpoint and service |
| Risk alerts | [Skills/risk-alerts.md](Skills/risk-alerts.md) | 85% warning rows, pulse badges, dismissible banners |
| Filter validation | [Skills/filter-validation.md](Skills/filter-validation.md) | Percent filter bounds at API + MVC |
| UI improvements | [Skills/UI_improvements.md](Skills/UI_improvements.md) | Calendar grid, dark mode, AJAX patterns |

---

## Testing

No test project exists yet. If adding tests:

- Use **xUnit** as the test framework.
- Place the test project at the solution root alongside the other projects.
- Name it `ProyectoExcel.Tests` or `Attendance.Tests`.

---

## Git rules

- **Never push to `main` or `develop` directly.**
- Branch naming: `feature/name`, `fix/name`, `docs/name`.
- Commit messages in English, semantic format:
  ```
  feat:     new functionality
  fix:      bug correction
  refactor: cleanup without behavior change
  docs:     Context_Alvaro.md, AGENTS_Alvaro.md, README only
  chore:    config, packages
  ```
- `[ValidateAntiForgeryToken]` on every MVC POST.
- Do **not** commit `appsettings.Development.json` with real credentials.
- Do **not** commit connection strings containing real passwords.
- `bin/` and `obj/` folders are already gitignored.

---

## Context_Alvaro.md update rule — mandatory on every PR

Any PR that touches the items below **must** include a `Context_Alvaro.md` update in the same commit:

| Change | Section to update |
|--------|-------------------|
| New API endpoint | API endpoints table |
| New DB table or entity | Domain model |
| New NuGet package | NuGet packages table |
| Feature completed | Feature status section |
| Feature cancelled | Feature status section |
| New business rule | Business rules section |
| New coding convention | This file (AGENTS_Alvaro.md) |

Add a line to the changelog at the bottom of `Context_Alvaro.md` on every PR:
```
| YYYY-MM-DD | [Name] | [What changed] |
```
