# AGENTS.md — Coding rules for AI agents
> Read [CONTEXT.md](../CONTEXT.md) first for project overview and architecture.
> This file governs ALL agents: Antigravity, Cursor, Copilot, any LLM.

---

## Token budget — read this first

Every agent call costs tokens. Follow this routing to avoid waste:

| Task type | Use | Reason |
|---|---|---|
| Inline autocomplete | Editor built-in / small model | Fast, no context needed |
| Generate a single method or small fix | Agent chat with **only the relevant file open** | Minimize context window |
| Generate a full controller or service | Agent chat with file + DTOs open | Medium context |
| Cross-file refactor or architecture decision | Claude cloud (this chat) | Complex reasoning |
| Security audit | Claude cloud on demand only | Never run automatically |

**Rules to save tokens:**
- Never attach the entire solution to a prompt. Open only files the task actually touches.
- Prefer editing an existing file over generating a new one from scratch — less to write.
- If the agent proposes something with 3+ new files, stop and discuss architecture first.
- Close agent context between unrelated tasks — don't let old context bleed.

---

## Language and framework

- **.NET 10**, C# 14, ASP.NET Core, EF Core 10, SQL Server.
- All projects target `net10.0`.
- Nullable reference types enabled (`<Nullable>enable</Nullable>`).
- Implicit usings enabled.

---

## Project boundaries

| What | Belongs in |
|---|---|
| Entities, DTOs, services, DbContext, EF config, migrations | `Attendance.Infrastructure/` |
| API controllers | `ApiProyectoExcel/` |
| MVC controllers, ViewModels, Views, frontend assets | `ProyectoExcel/` |

- Never add NuGet packages to the wrong project. Infrastructure owns all data-access packages. API and MVC stay thin.
- MVC **never** references `ApplicationDbContext` or queries the database directly. All data goes through `IAttendanceApiClient`.

---

## C# style

- **File-scoped namespaces** — `namespace Foo;` not `namespace Foo { }`
- **Primary constructors** for DI — `public class MyService(ApplicationDbContext db)`
- **Immutable DTOs** as `record` types — `public record CourseDto(int Id, string Name)`
- Return **`IReadOnlyList<T>`** from service methods, not `List<T>`
- **Propagate `CancellationToken`** through all async chains — last parameter, `= default`
- Prefer `var` when type is obvious from the right-hand side
- No `#region` blocks

---

## EF Core

- `AsNoTracking()` on all read-only queries
- All entity config via **Fluent API** in `ApplicationDbContext.OnModelCreating` — no data annotations on entities
- Legacy Tajamar tables mapped with `ExcludeFromMigrations()` — never generate migrations that alter them
- New EF-managed tables are fine and handled by migrations
- Use async LINQ: `ToListAsync`, `AnyAsync`, `ToDictionaryAsync` — never `.Result` or `.Wait()`

---

## Naming conventions

| Artifact | Convention | Example |
|---|---|---|
| Entity properties | English | `FirstName`, `StartDate` |
| SQL column mapping | Spanish via `.HasColumnName()` | `.HasColumnName("NOMBRE")` |
| DTOs | Suffix `Dto` | `CourseDto`, `AttendanceSessionDto` |
| Request DTOs | Suffix `Request` | `SaveAttendanceEntryRequest` |
| Response DTOs | Suffix `Response` | `LoginResponse` |
| ViewModels | Suffix `ViewModel` | `StudentListViewModel` |
| Services | `I{Name}Service` + `{Name}Service` | `ICourseService` / `CourseService` |
| Controllers | `{Name}Controller` | `AttendanceController` |

---

## API conventions (ApiProyectoExcel)

- All controllers: `[ApiController]` + inherit `ControllerBase`
- Route prefix: `api/` — e.g. `[Route("api/[controller]")]`
- Return `IActionResult` — not `ActionResult<T>`
- Auth: `[Authorize(Roles = $"{AppRoles.Teacher},{AppRoles.Admin}")]` — use `AppRoles` constants, never string literals
- Errors: `BadRequest(new { message = "..." })` or `NotFound(new { message = "..." })` — always `message` key
- Current user Tajamar ID: from `tajamar_user_id` claim

---

## MVC conventions (ProyectoExcel)

- Controllers inherit `Controller`, return `View(viewModel)` with strongly-typed ViewModel
- All API calls through `IAttendanceApiClient` — never raw `HttpClient` in controllers
- Never call `ApplicationDbContext` from MVC
- User-facing strings via `IHtmlLocalizer<SharedResource>` (`@SharedLocalizer["Key"]` in views)
- Role checks: `User.IsInRole("Teacher")`, `User.IsInRole("Student")`, `User.IsInRole("Admin")`

---

## Frontend (Views and static assets)

- **Bootstrap 5** utility classes — no inline styles
- Dark mode: layout uses `data-bs-theme` on `<html>` — custom CSS must work in both themes
- **jQuery** for DOM, **Chart.js** for charts
- **Bootstrap Icons** (`bi bi-*`)
- Flags: **flag-icons** CSS library (`fi fi-xx` classes) — already added in `feature/statistics-improvements`
- Custom styles: `wwwroot/css/site.css` — Custom JS: `wwwroot/js/site.js`
- No CSS preprocessors. No JS bundlers.

---

## Localization

- Default culture: **`es`** (Spanish). Supported: `es`, `en`
- All UI strings in `Resources/SharedResource.es.resx` and `Resources/SharedResource.en.resx`
- In Razor: `@SharedLocalizer["KeyName"]`
- Never hardcode user-visible text in views or controllers

---

## AttendanceStatus enum — use named values only

| Value | Name |
|---|---|
| 0 | `Present` |
| 1 | `Absent` |
| 2 | `Late` |
| 3 | `JustifiedAbsent` |
| 4 | `JustifiedLate` |
| 5 | `EarlyLeave` |
| 6 | `JustifiedEarlyLeave` |

---

## Business rules — do not change these without team consensus

- **Diploma eligibility:** `RealAttendancePercentage >= 80%`
- **Drop risk:** `RealAttendancePercentage < 75%`
- **Lective days per year:** 156 (`LectiveDayCalendar.LectiveDaysPerYear`)
- **Weekend days are never lective** — `LectiveDayCalendar.GetWeekdaysInRange` already filters them
- **Non-lective days:** defined in `LectiveDayCalendar` — agents must not add attendance records on these days
- **Attendance filter bounds:** `minPercent` must be ≥ 0, `maxPercent` must be ≤ 100. Enforce at both API and MVC level.

---

## Git rules

- **Never push to `main` or `develop` directly**
- Branch naming: `feature/name`, `fix/name`, `docs/name`
- Commit messages in English, semantic format:
  ```
  feat:     new functionality
  fix:      bug correction
  refactor: cleanup without behavior change
  docs:     CONTEXT.md, AGENTS.md, README only
  chore:    config, packages
  ```
- `[ValidateAntiForgeryToken]` on every MVC POST
- Do **not** commit `appsettings.Development.json` with real credentials
- `bin/` and `obj/` are gitignored

---

## CONTEXT.md update rule — mandatory on every PR

Any PR that touches the items below **must** include a `CONTEXT.md` update in the same commit:

| Change | Section to update |
|---|---|
| New API endpoint | Section: API endpoints table |
| New DB table or entity | Section: Domain model |
| New NuGet package | Section: Stack / dependencies |
| Feature completed | Move from "In progress" to "Done" in status section |
| Feature cancelled | Note reason in status section |
| New business rule | Section: Business rules |
| New coding convention | This file (AGENTS.md) |

**A PR without a required CONTEXT.md update will not be merged.**

Add a line to the changelog at the bottom of CONTEXT.md on every PR:
```
| YYYY-MM-DD | [Name] | [What changed] |
```

---

## Testing

No test project exists yet. If adding:
- Framework: **xUnit**
- Location: solution root
- Name: `ProyectoExcel.Tests` or `Attendance.Tests`
