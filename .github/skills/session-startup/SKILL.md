---
name: session-startup
description: >
  Initializes an Antigravity session with full project context.
  MUST run at the start of every session before any other task.
  Reads Context_Alvaro.md and Agents/AGENTS.md. Confirms architecture,
  all 20+ API endpoints, business rules (80% diploma, 85% warning,
  75% risk), export architecture (_ExportMenu partial, ExportResource.resx,
  culture param), and calendar feature status.
  Activates on: "start session", "load context", "initialize",
  "what is this project", "inicio sesión", "cargar contexto".
  Do not use for individual coding tasks.
---

# Session Startup — ProyectoExcel (Álvaro)

## Step 1 — Read Context_Alvaro.md
Extract and confirm:
- 3-project architecture: MVC :5162 → API :5180 → Infrastructure → SQL Server
- Auth: local-part email → controller appends `@tajamar365.com`
- All 20+ API endpoints — never duplicate
- Business rules: 80% diploma, 85% warning, 75% drop risk, 156 lective days
- Export: `_ExportMenu` partial, `ExportResource.resx`, `?culture=` param
- Calendar: `CourseCalendarEntry` EF-managed, `CalendarParserService`, `CalendarService`
- Feature status: Done vs Phase 2

## Step 2 — Read Agents/AGENTS.md
Confirm you will follow:
1. Primary constructors for all DI
2. `CancellationToken = default` as last param
3. `IReadOnlyList<T>` in public signatures
4. DTOs as `record` in Infrastructure/DTOs/ only
5. Export: always `_ExportMenu` partial — never duplicate buttons
6. No `ResourcesPath` in `AddLocalization()`
7. Never call `ApplicationDbContext` from MVC

## Step 3 — Confirm git branch
Current branch must be `feature/ui-redesign`.
If on `main` or `develop`, warn before proceeding.

## Step 4 — Respond with 3 bullets
- What the project does + 3-project architecture
- Done features vs Phase 2
- What you will NOT do (alter legacy tables, touch Chart.js, duplicate _ExportMenu)

---

## End-of-session checklist
```
1. Uncommitted changes? List them.
2. Changes affect: API endpoints / DB tables / NuGet / business rules / features?
3. If yes → update Context_Alvaro.md + changelog line.
4. Semantic commits used?
5. No credentials in committed files?
```

---

## Prompt templates

### Session start
```
Use the /session-startup skill.
```

### Single file task
```
Read Context_Alvaro.md and Agents/AGENTS.md.
Use the /[skill-name] skill.
Open only: [exact/path/file]
Task: [description]
Show diff before applying.
```

### Pre-commit review
```
Review changed files:
[list files]
Check: primary constructors, IReadOnlyList, CancellationToken,
ValidateAntiForgeryToken on POSTs, AppRoles constants,
no hardcoded strings, no _ExportMenu duplication,
no animate.css CDN re-added.
```
