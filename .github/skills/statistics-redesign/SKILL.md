---
name: statistics-redesign
description: >
  Improves Statistics/Index.cshtml with three-tier risk row highlighting
  (75% danger, 80% below-diploma, 85% warning), alert banners, live
  student search, and sticky table header. The 4 Chart.js charts and
  _ExportMenu partial must not be touched.
  Activates on: "redesign statistics", "risk rows", "alert banner stats",
  "search statistics", "tres umbrales estadísticas", "mejorar estadísticas",
  "colorear filas riesgo", "buscar alumno estadísticas", "sticky header".
  Requires /design-system applied first.
  NEVER modify: Chart.js code, _ExportMenu partial call, filter form.
---

# Statistics Redesign

## What already exists — DO NOT touch
- 4 Chart.js charts — do not modify initialization code
- `@await Html.PartialAsync("_ExportMenu", Model.ExportMenu)` — do not touch
- Filter form (course, month, year, minPercent, maxPercent)
- `.row-risk-danger`, `.row-risk-warning`, `.badge-pulse` in site.css

## Files to modify
- `ProyectoExcel/Views/Statistics/Index.cshtml`
- Both `.resx` files (alert keys + search key)

## Three risk tiers
- `AtRiskDrop` (< 75%) → `row-risk-danger` (existing CSS)
- `!DiplomaEligible` (75–80%) → `row-risk-warning` (existing CSS)
- Warning zone (80–85%) → `row-risk-warning85` (new — in base.css)

## Steps

### Step 1 — Alert banner
Paste above `<div class="table-responsive">`:
```html
@if (stats.BelowDropThresholdCount > 0)
{
    <div class="alert alert-danger alert-dismissible fade show d-flex align-items-center gap-2 mb-3" role="alert">
        <i class="bi bi-exclamation-triangle-fill fs-5 flex-shrink-0"></i>
        <div><strong>@stats.BelowDropThresholdCount @SharedLocalizer["StudentsAtRiskAlert"]</strong> @SharedLocalizer["StudentsAtRiskAlertDetail"]</div>
        <button type="button" class="btn-close ms-auto" data-bs-dismiss="alert"></button>
    </div>
}
else if (stats.AtRiskCount > 0)
{
    <div class="alert alert-warning alert-dismissible fade show d-flex align-items-center gap-2 mb-3" role="alert">
        <i class="bi bi-info-circle-fill fs-5 flex-shrink-0"></i>
        <div>@stats.AtRiskCount @SharedLocalizer["StudentsBelowDiplomaAlert"]</div>
        <button type="button" class="btn-close ms-auto" data-bs-dismiss="alert"></button>
    </div>
}
```

### Step 2 — Search input (between alert and table-responsive)
```html
<div class="mb-3 d-flex align-items-center justify-content-between flex-wrap gap-2">
    <div class="ta-search-wrap">
        <i class="bi bi-search" aria-hidden="true"></i>
        <input type="text" id="statsSearch" class="form-control form-control-sm"
               placeholder="@SharedLocalizer["SearchStudents"]" autocomplete="off" />
    </div>
    <span id="statsSearchCount" class="small text-muted" style="display:none"></span>
</div>
```

### Step 3 — Table row (replace existing foreach <tr>)
```csharp
@{ var rowCss = student.AtRiskDrop ? "row-risk-danger"
              : !student.DiplomaEligible ? "row-risk-warning"
              : student.RealAttendancePercentage < 85 ? "row-risk-warning85"
              : ""; }
<tr class="@rowCss">
```
Add `ta-table-sticky` to `<table>` tag.

### Step 4 — Search JS (inside @section Scripts, AFTER chart block)
See [stats-search.js](./scripts/stats-search.js).

### Step 5 — Localization keys
```xml
<!-- ES -->
<data name="StudentsAtRiskAlert"><value>alumnos en riesgo de abandono</value></data>
<data name="StudentsAtRiskAlertDetail"><value>— asistencia real por debajo del 75%.</value></data>
<data name="StudentsBelowDiplomaAlert"><value>alumnos por debajo del 80% — riesgo de perder el diploma.</value></data>
<data name="SearchStudents"><value>Buscar alumno...</value></data>
<!-- EN -->
<data name="StudentsAtRiskAlert"><value>students at drop risk</value></data>
<data name="StudentsAtRiskAlertDetail"><value>— real attendance below 75%.</value></data>
<data name="StudentsBelowDiplomaAlert"><value>students below 80% — at risk of losing the diploma.</value></data>
<data name="SearchStudents"><value>Search student...</value></data>
```
