---
name: calendar-upload
description: >
  Reference skill for the academic calendar Excel upload feature.
  Status: DONE. Use only for bug fixes, new calendar endpoints, or
  UX improvements to the upload flow. Do NOT use to recreate from scratch.
  Activates on: "fix calendar", "calendar bug", "calendar endpoint",
  "calendario lectivo fix", "CourseCalendarEntry", "CalendarParserService",
  "CalendarService", "upload calendar improvement".
---

# Calendar Upload — DONE Feature Reference

## Status: Fully implemented (see Context_Alvaro.md changelog)

## Architecture
```
POST /api/courses/{id}/calendar/upload
POST /api/courses/{id}/calendar/preview
GET  /api/courses/{id}/calendar/dates
GET  /api/courses/{id}/calendar/entries
GET  /api/courses/{id}/calendar/status
  → CoursesController → CalendarParserService (ClosedXML) → CalendarService → DB
  → CourseCalendarEntry (CALENDARIOCURSO — EF-managed, has migrations)
```

## Key services (all in Attendance.Infrastructure/Services/)
- `CalendarParserService` — parses .xlsx, validates dates
- `CalendarService` — CRUD for `CourseCalendarEntry`
- `ICalendarService` — injected into `AttendanceService` for lective day validation

## Rules for any calendar change
- `LectiveDayCalendar` fallback stays — for courses without uploaded calendar
- Weekend override: custom calendar CAN mark a weekend day lective
- `CalendarParserService` lives in Infrastructure — never in API/MVC
- Upload accepts `.xlsx` only — validate content-type in controller
- Preview endpoint → does NOT write to DB
- `CourseCalendarEntry` uses EF migrations — never `ExcludeFromMigrations()`

## Files for fixes
- `Attendance.Infrastructure/Services/CalendarParserService.cs`
- `Attendance.Infrastructure/Services/CalendarService.cs`
- `ApiProyectoExcel/Controllers/CoursesController.cs`
- `ProyectoExcel/Services/AttendanceApiClient.cs` (calendar methods)
- `ProyectoExcel/Views/Attendance/Index.cshtml` (upload UX)
