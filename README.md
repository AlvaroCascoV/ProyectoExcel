# ProyectoExcel — Attendance Management System

Web application for tracking course attendance at Tajamar, with Excel-compatible metrics (present/absent/late, diploma eligibility, drop risk). The solution is split into three projects: a REST API, an ASP.NET Core MVC frontend, and a shared infrastructure layer.

## Solution structure

| Project | Folder | Role |
|---------|--------|------|
| **ApiProyectoExcel** | `ApiProyectoExcel/` | REST API with JWT authentication. OpenAPI + [Scalar](https://scalar.com/) docs in Development. Applies EF migrations and seeds ASP.NET Identity users on startup. |
| **ProyectoExcel** (MVC) | `ProyectoExcel/` | Browser UI. Cookie authentication for the web session; forwards JWT to the API via `ApiTokenHandler`. |
| **Attendance.Infrastructure** | `Attendance.Infrastructure/` | Shared data access (EF Core + SQL Server), domain entities, DTOs, and business services (auth, courses, attendance, statistics). |

```
ProyectoExcel.slnx
├── ApiProyectoExcel          → http://localhost:5180  (API + /scalar)
├── ProyectoExcel             → http://localhost:5162  (MVC UI)
└── Attendance.Infrastructure → class library (referenced by both apps)
```

**Data model:** Legacy Tajamar tables (`USUARIOSTAJAMAR`, `CURSOSTAJAMAR`, etc.) are created and seeded by `script.sql`. ASP.NET Identity tables (`AspNetUsers`, roles, etc.) are created by EF migrations when the API starts.

---

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (projects target `net10.0`)
- **SQL Server** (one of):
  - **Windows:** SQL Server Express / Developer / LocalDB
  - **macOS / Linux:** SQL Server in Docker (recommended)
- Optional: [Visual Studio 2022](https://visualstudio.microsoft.com/) or [VS Code](https://code.visualstudio.com/) with the C# extension
- Optional: [Azure Data Studio](https://azure.microsoft.com/products/data-studio/) or `sqlcmd` to run `script.sql`

---

## Local setup (all platforms)

### 1. Clone and restore

```bash
git clone <repository-url>
cd ProyectoExcel
dotnet restore
```

### 2. Create the database and seed Tajamar data

1. Create an empty database named **`ProyectoExcel`** on your SQL Server instance.
2. Run the root script against that database:

   ```bash
   # Example with sqlcmd (adjust server, user, and password)
   sqlcmd -S localhost,1433 -U sa -P "YourPassword" -d ProyectoExcel -i script.sql
   ```

   `script.sql` creates and fills:

   - Roles (Professor, Student, Administrator)
   - 5 courses, 115 users, 110 enrollments
   - Empty `ASISTENCIATAJAMAR` table for attendance records

### 3. Configure the API connection string

Connection strings live in **`ApiProyectoExcel/appsettings.json`**:

| Key | Use when |
|-----|----------|
| `ProyectoExcelDBLocal` | Windows — SQL Server instance `LOCALHOST\DEVELOPER` |
| `ProyectoExcelDBMac` | macOS / Docker — `127.0.0.1:1433` with SQL auth |

Pick the active profile with **`Database:ActiveConnection`**:

- **Windows (default):** `ProyectoExcelDBLocal` in `appsettings.json`
- **macOS (Development):** `ProyectoExcelDBMac` in `appsettings.Development.json`

Adjust server name, password, and catalog in the connection string that matches your machine. Do not commit real production credentials.

### 4. Start the API (required first)

```bash
cd ApiProyectoExcel
dotnet run --launch-profile http
```

On first run the API will:

- Apply EF Core migrations (Identity tables)
- Create roles: `Admin`, `Teacher`, `Student`
- Create Identity accounts for every active `USUARIOSTAJAMAR` row with a **dev password** (see [Test accounts](#test-accounts))

| URL | Purpose |
|-----|---------|
| http://localhost:5180 | API base URL |
| http://localhost:5180/scalar | Interactive API docs (Development only) |

### 5. Start the MVC app

In a **second terminal**:

```bash
cd ProyectoExcel
dotnet run --launch-profile http
```

Open **http://localhost:5162** and sign in at `/Account/Login`.

The MVC app reads the API URL from `ProyectoExcel/appsettings.json` → `ApiSettings:BaseUrl` (default `http://localhost:5180`). Keep the API running whenever you use the web UI.

---

## Platform-specific notes

### Windows

1. Install **.NET 10 SDK** and **SQL Server** (Express/Developer) or enable **LocalDB** with Visual Studio.
2. Create database `ProyectoExcel` in SSMS or:

   ```sql
   CREATE DATABASE ProyectoExcel;
   ```

3. Run `script.sql` (SSMS: open file → Execute, or use `sqlcmd`).
4. In `appsettings.json`, set `Database:ActiveConnection` to **`ProyectoExcelDBLocal`** and edit `ConnectionStrings:ProyectoExcelDBLocal` if your instance name or `sa` password differs.

5. Run API and MVC as in [steps 4–5](#4-start-the-api-required-first).

**Visual Studio:** Open `ProyectoExcel.slnx`, set multiple startup projects (Api + MVC), or run each project separately with the `http` profile.

### macOS

SQL Server does not run natively on Apple Silicon/Intel Macs for production use; use **Docker**:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=YourStrong!Passw0rd" \
  -p 1433:1433 --name sql-proyectoexcel \
  -d mcr.microsoft.com/mssql/server:2022-latest
```

Then create the database and run `script.sql`:

```bash
docker exec -it sql-proyectoexcel /opt/mssql-tools18/bin/sqlcmd \
  -S localhost -U sa -P "YourStrong!Passw0rd" -C \
  -Q "CREATE DATABASE ProyectoExcel"

sqlcmd -S localhost,1433 -U sa -P "YourStrong!Passw0rd" -C \
  -d ProyectoExcel -i script.sql
```

`appsettings.Development.json` already sets `Database:ActiveConnection` to **`ProyectoExcelDBMac`**. Update `ConnectionStrings:ProyectoExcelDBMac` in `appsettings.json` if your Docker `sa` password differs.

Install .NET 10 SDK if needed:

```bash
brew install --cask dotnet-sdk
# or download from https://dotnet.microsoft.com/download
dotnet --version   # should be 10.x
```

Run API and MVC from Terminal as above.

---

## Test accounts

After the API starts successfully, Identity users are created with password:

```text
12345
```

(`Attendance.Infrastructure/Data/DbInitializer.cs` — `DefaultDevPassword`)

| Role | Email (from seed data) | MVC landing page |
|------|------------------------|------------------|
| **Admin** | `admin@tajamar365.com` | Pass list (`/Attendance`) |
| **Teacher** | `paco.garcia.serrano@tajamar365.com` | Pass list (`/Attendance`) |
| **Student** | `sofia.martinez@tajamar365.com` | My dashboard (`/Dashboard`) |

Any other **active** user in `USUARIOSTAJAMAR` with an email can log in with the same dev password once Identity seeding has run.

**Default course for statistics UI:** course id `3430` (*Master Desarrollo Apps Cloud 2025-2026*) when none is selected.

---

## Main features (manual testing)

### Teacher / Admin (MVC)

| Area | Route | What to test |
|------|-------|----------------|
| **Pass list** | `/Attendance` | Select course and date; mark attendance (Present, Absent, Late, justified variants, early leave); save session. |
| **Students** | `/Students` | List enrolled students for a course. |
| **Statistics** | `/Statistics` | Course dashboard: attendance %, rankings, filters by month/year and percent range; export-oriented metrics aligned with Excel logic. |

### Student (MVC)

| Area | Route | What to test |
|------|-------|----------------|
| **My dashboard** | `/Dashboard` | Enrolled courses, attendance history, summary metrics (attendance %, diploma eligibility ≥ 80%, at-risk drop &lt; 75%). |

### REST API (Scalar or HTTP client)

Authenticate with `POST /api/auth/login`:

```json
{ "email": "paco.garcia.serrano@tajamar365.com", "password": "12345" }
```

Use the returned `token` as `Authorization: Bearer <token>`.

| Endpoint group | Examples |
|----------------|----------|
| **Auth** | `POST /api/auth/login`, `GET /api/auth/me` |
| **Courses** | `GET /api/courses`, `GET /api/courses/{id}`, `GET /api/courses/{id}/students` |
| **Attendance (staff)** | `GET/PUT /api/courses/{courseId}/attendance?date=YYYY-MM-DD`, `GET .../attendance/dates` |
| **Attendance (student)** | `GET /api/attendance/me`, `GET /api/attendance/me/summary`, `GET /api/attendance/me/courses` |
| **Statistics** | `GET /api/statistics/course/{courseId}`, `GET .../rankings` |
| **Dev only** | `POST /api/courses/{courseId}/attendance/seed-present?days=7` — seeds recent weekdays as Present (requires Development + Teacher/Admin token) |

### Attendance status codes

| Value | Meaning |
|-------|---------|
| 0 | Present |
| 1 | Absent |
| 2 | Late |
| 3 | Justified absent |
| 4 | Justified late |
| 5 | Early leave |
| 6 | Justified early leave |

Metrics (diploma / drop thresholds, weighted absences) are implemented in `AttendanceMetricsCalculator` and mirror the original Excel workbook rules.

---

## Suggested test checklist

1. **Database:** `script.sql` completed without errors; tables `USUARIOSTAJAMAR`, `CURSOSTAJAMAR`, etc. exist.
2. **API startup:** No migration/seed errors in console; log mentions Identity seed and dev password.
3. **Scalar:** Open http://localhost:5180/scalar → login → call `GET /api/courses` with Bearer token.
4. **MVC login:** Teacher → Pass list → pick course `3430` and a weekday → save attendance → reload and verify persistence.
5. **Student:** Login as `sofia.martinez@tajamar365.com` → Dashboard shows courses and summary.
6. **Statistics:** Teacher → Statistics → change course/month filters → rankings update.
7. **Dev seed (optional):** `POST .../attendance/seed-present` then refresh statistics/pass list.

---

## Configuration reference

| Setting | File | Description |
|---------|------|-------------|
| `ConnectionStrings:ProyectoExcelDBLocal` / `ProyectoExcelDBMac` | `ApiProyectoExcel/appsettings.json` | SQL Server profiles (Windows vs Mac/Docker) |
| `Database:ActiveConnection` | `ApiProyectoExcel/appsettings*.json` | Which connection string name to use |
| `Jwt:*` | `ApiProyectoExcel/appsettings.json` | JWT issuer, audience, signing key, expiry |
| `ApiSettings:BaseUrl` | `ProyectoExcel/appsettings.json` | MVC → API base URL (default `http://localhost:5180`) |

**Ports (http profiles):**

| App | HTTP |
|-----|------|
| API | 5180 |
| MVC | 5162 |

---

## Troubleshooting

| Problem | What to check |
|---------|----------------|
| API fails on startup with database error | Run `script.sql` first; verify connection string and that SQL Server is listening (Docker: `docker ps`, port `1433`). |
| `Login failed` / invalid password | Use dev password `12345` after API has run once (Identity seed). Restart API if you added users to SQL after first run. |
| MVC pages empty or 401 | API must be running; `ApiSettings:BaseUrl` must match API URL; log in again so JWT cookie is set. |
| No courses in UI | User must be enrolled in `CURSOSUSUARIOSTAJAMAR`; try teacher `paco.garcia.serrano@tajamar365.com` or course id `3430`. |
| Scalar not available | Only mapped when `ASPNETCORE_ENVIRONMENT=Development` (default for `dotnet run` with Development settings). |

### Apply migrations manually (optional)

Normally the API calls `MigrateAsync()` on startup. To update the database from CLI:

```bash
dotnet ef database update \
  --project Attendance.Infrastructure \
  --startup-project ApiProyectoExcel
```

---

## Related files

- **`script.sql`** — Tajamar schema + seed data (required before first API run).
- **`EXCEL_ASISTENCIA 2.xlsx`** — Original Excel reference for attendance formulas and layout.

---

## License / contributions

Internal Tajamar training project. For questions about seed users or course data, refer to `script.sql` or the team that maintains the Excel source.
