# CONTEXT.md — Attendance Management System (Tajamar)
> Live document. Updated on every PR that changes architecture, endpoints, domain model or conventions.
> For setup instructions, test accounts and troubleshooting see [README.md](README.md).
> Coding and agent rules: [agents/AGENTS.md](agents/AGENTS.md)

---

## Solution layout

| Project | Folder | Role | Port |
|---|---|---|---|
| **ApiProyectoExcel** | `ApiProyectoExcel/` | REST API, JWT auth, OpenAPI + Scalar docs | `5180` |
| **MvcProyectoExcel** | `ProyectoExcel/` | ASP.NET Core MVC frontend, cookie auth | `5162` |
| **Attendance.Infrastructure** | `Attendance.Infrastructure/` | Shared library: EF Core, entities, DTOs, services | — |

Both web projects reference `Attendance.Infrastructure`. MVC never touches the DB directly — calls the API over HTTP.

---

## Architecture

```
[Browser]
    │ Cookie auth (ApiJwt)
    ▼
[MvcProyectoExcel :5162]   ASP.NET Core MVC
    │ JWT via ApiTokenHandler (DelegatingHandler)
    ▼
[ApiProyectoExcel :5180]   REST API
    │ EF Core
    ▼
[SQL Server]   LOCALHOST\DEVELOPER  /  DB: ProyectoExcel
```

**Auth flow:**
1. User logs in at `/Account/Login` → MVC calls `POST /api/auth/login`
2. API returns JWT → stored in HttpOnly cookie `ApiJwt` + cookie ClaimsIdentity created
3. `ApiTokenHandler` reads `ApiJwt` cookie and attaches it as `Bearer` on every outgoing API call

**Roles:** `Admin`, `Teacher`, `Student` — mapped from legacy Tajamar roles (`ADMINISTRADOR`, `PROFESOR`, `ALUMNO`)

---

## Domain model

| Entity | SQL Table | Notes |
|---|---|---|
| `TajamarUser` | `USUARIOSTAJAMAR` | Students, teachers, admins |
| `RoleTajamar` | `ROLESCHARLASTAJAMAR` | Legacy role lookup |
| `Course` | `CURSOSTAJAMAR` | Courses with start/end dates |
| `CourseEnrollment` | `CURSOSUSUARIOSTAJAMAR` | Many-to-many join |
| `AttendanceRecord` | `ASISTENCIATAJAMAR` | One record per student per course per day |
| `CourseCalendarEntry` | `CALENDARIOCURSO` | Dynamic course academic calendar dates |
| `AttendanceStatus` | *(enum)* | See enum table below |
| `ApplicationUser` | ASP.NET Identity tables | Extends `IdentityUser` with `TajamarUserId` FK |

All entities in `Attendance.Infrastructure/Entities/`. English property names, Spanish SQL column via `.HasColumnName()`.

**AttendanceStatus enum:**

| Value | Name | Display code |
|---|---|---|
| 0 | `Present` | — |
| 1 | `Absent` | F |
| 2 | `Late` | R |
| 3 | `JustifiedAbsent` | FJ |
| 4 | `JustifiedLate` | RJ |
| 5 | `EarlyLeave` | SAF |
| 6 | `JustifiedEarlyLeave` | SAFJ |

---

## Data access

- **DbContext:** `ApplicationDbContext` extends `IdentityDbContext<ApplicationUser>` — `Attendance.Infrastructure/Data/`
- **Legacy tables** use `ExcludeFromMigrations()` — EF reads/writes them but never alters schema
- **No repository layer** — services inject `ApplicationDbContext` directly with LINQ
- **DbInitializer** — runs migrations on API startup, seeds Identity roles and accounts from `USUARIOSTAJAMAR`

---

## Service layer (`Attendance.Infrastructure/Services/`)

| Service | Purpose |
|---|---|
| `AuthService` / `JwtTokenService` | Identity login, JWT creation |
| `CourseService` | Course listing, student roster |
| `AttendanceService` | Session CRUD, student records/summaries, dev seeding |
| `StatisticsService` | Course-level stats, rankings, filtering |
| `AttendanceMetricsCalculator` | Pure calc: attendance %, diploma warning threshold (<85%), drop risk (<75%) |
| `LectiveDayCalendar` | Weekday-only academic calendar, 156 lective days/year |
| `CalendarParserService` | Parses uploaded calendar Excel spreadsheets (.xlsx) using ClosedXML |
| `CalendarService` | Database operations for custom course calendars |

DI registration: `Attendance.Infrastructure/Extensions/ServiceCollectionExtensions.cs` → `AddAttendanceInfrastructure()`

---

## Business rules — do not change without team discussion

| Rule | Value | Where enforced |
|---|---|---|
| Diploma warning threshold | `RealAttendancePercentage < 85%` | `AttendanceMetricsCalculator` (warns early before dropping below official 80% threshold) |
| Drop risk threshold | `RealAttendancePercentage < 75%` | `AttendanceMetricsCalculator` |
| Lective days/year | 156 | `LectiveDayCalendar.LectiveDaysPerYear` (or custom calendar count) |
| Weekend exclusion | Sat & Sun never lective by default | Overridden if marked lective in custom calendar entries |
| Non-lective days | Public holidays excluded | Validated dynamically via custom calendar database |
| Default course | ID `3430` | `AttendanceController`, `StatisticsController` |
| Percent filter bounds | `minPercent` ≥ 0, `maxPercent` ≤ 100 | API + MVC validation |

---

## API endpoints

| Method | Route | Description | Roles |
|---|---|---|---|
| POST | `/api/auth/login` | Login, returns JWT | Public |
| GET | `/api/courses?activeOnly=` | List courses | Teacher, Admin |
| GET | `/api/courses/{id}/students` | Students by course | Teacher, Admin |
| GET | `/api/courses/{id}/attendance?date=` | Attendance session by date | Teacher, Admin |
| PUT | `/api/courses/{id}/attendance?date=` | Save attendance session | Teacher, Admin |
| GET | `/api/courses/{id}/attendance/dates` | Dates with recorded sessions | Teacher, Admin |
| POST | `/api/courses/{id}/attendance/seed-present?days=` | Seed data (Development only) | Teacher, Admin |
| GET | `/api/attendance/me` | My attendance history | Student |
| GET | `/api/attendance/me/summary?courseId=` | My attendance summary | Student |
| GET | `/api/attendance/me/courses` | My enrolled courses | Student |
| GET | `/api/statistics/course/{id}?month=&year=&minPercent=&maxPercent=` | Course statistics | Teacher, Admin |
| GET | `/api/statistics/course/{id}/rankings?ascending=&top=&month=&year=` | Attendance ranking | Teacher, Admin |
| GET | `/api/statistics/course/{id}/export?month=&year=&minPercent=&maxPercent=` | Export statistics to Excel (.xlsx) | Teacher, Admin |
| POST | `/api/courses/{id}/calendar/upload` | Upload custom calendar spreadsheet (.xlsx) | Teacher, Admin |
| POST | `/api/courses/{id}/calendar/preview` | Preview calendar stats from spreadsheet | Teacher, Admin |
| GET | `/api/courses/{id}/calendar/dates` | List of lective dates in calendar | Teacher, Admin, Student |
| GET | `/api/courses/{id}/calendar/entries` | List of detailed calendar entry list | Teacher, Admin |
| GET | `/api/courses/{id}/calendar/status` | Course calendar upload status | Teacher, Admin |

---

## MVC client (`ProyectoExcel/Services/AttendanceApiClient.cs`)

`IAttendanceApiClient` wraps every API endpoint as a typed method. `ApiTokenHandler` injects the JWT automatically. Controllers must never use raw `HttpClient` or call the DB directly.

---

## Frontend

- **Bootstrap 5** with dark mode (`data-bs-theme` on `<html>`)
- **Bootstrap Icons** (CDN)
- **flag-icons** CSS library — added in `feature/statistics-improvements` for country flag display
- **jQuery** (DOM), **Chart.js** (statistics charts), jQuery Validation
- **Localization:** Spanish (default) + English via `.resx` files in `ProyectoExcel/Resources/`
- Custom: `wwwroot/css/site.css`, `wwwroot/js/site.js`
- No CSS preprocessors. No JS bundlers.

---

## NuGet packages

| Package | Project | Purpose |
|---|---|---|
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | Infrastructure | Identity |
| `Microsoft.EntityFrameworkCore.SqlServer` | Infrastructure | EF Core |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Infrastructure | JWT |
| `System.IdentityModel.Tokens.Jwt` | Infrastructure | JWT tokens |
| `Microsoft.AspNetCore.OpenApi` | API | OpenAPI |
| `Scalar.AspNetCore` | API | API docs UI |
| `ClosedXML` | Infrastructure | Excel export (.xlsx via `ExcelExportService`) |

---

## Feature status

### ✅ Done
- Full auth: login, logout, JWT + cookie, role-based redirect
- Attendance view for teachers (list, save session)
- Student dashboard (history + summary)
- Student list by course
- Statistics and rankings with charts
- Dev data seed (Development only)
- **Weekend / non-lective day filter** — `AttendanceController` rejects Sat/Sun; `AttendanceService.SaveSessionAsync` rejects non-lective days via `LectiveDayCalendar`
- **Percent filter validation** — `minPercent`/`maxPercent` bounds (0–100, min ≤ max) enforced at API (`StatisticsController`) and MVC (`[Range]` on ViewModel + `min`/`max` HTML attributes)
- **Export to Excel** — `GET /api/statistics/course/{id}/export` via `ExcelExportService` (ClosedXML); MVC `Export` action + button in `Statistics/Index.cshtml`
- **Flag-icons library** — CDN link added to `_Layout.cshtml`; `fi fi-xx` classes available in all views
- **Academic Excel Calendar Upload** — ClosedXML parsing and dynamic validation against weekend and holidays (`ICalendarService`).
- **Interactive Calendar Grid & AJAX load** — Full calendar grid with hover details and instant, flicker-free SPA-like AJAX loads (`fetch`).
- **Visual Risk alerts** — Custom color-coded rows (warning/danger), pulsing alert badges, and dismissible stats alert banners using the `85%` early warning margin.

### 🔄 In progress
*(nothing active — all feature/statistics-improvements items merged)*

### ⏳ Planned — Phase 2 (do not start until Phase 1 is stable in develop)
- Secure check-in tied to physical classroom device (TW17, TW18…)
- Seat assignment table in DB (student ↔ device ↔ date)
- Rotating seat assignment between students

### ❌ Discarded
- Excel import — data already in DB from legacy Tajamar tables

---

## Changelog

| Date | Author | Change |
|---|---|---|
| 2025-xx-xx | [Your name] | Initial CONTEXT.md + AGENTS.md setup |
| 2026-06-01 | Antigravity | feature/statistics-improvements: flag-icons CDN, percent filter validation (API + MVC), non-lective day guard (API + service), Excel export endpoint + ClosedXML service + MVC action + view button |
| 2026-06-02 | Antigravity | Dynamic Excel Calendar Upload: CourseCalendarEntry, CALENDARIOCURSO table, CalendarParserService, CalendarService, API upload/preview/dates/entries/status endpoints, MVC clients. Upgraded monthly calendar with localized circular dots and dynamic tooltips, timezone-safe local browser date checking to highlight past lective dates in blue, perfect HSL custom variables for legend dots in light/dark themes, robust Url.Action MVC AJAX day navigation, and dynamic AJAX PDF export button container updates without page refreshes. Custom risk-alerts upgrade (85% warning threshold, color-coded rows, pulse badges, dismissible banners). |

