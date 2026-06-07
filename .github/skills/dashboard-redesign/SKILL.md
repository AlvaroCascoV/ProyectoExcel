---
name: dashboard-redesign
description: >
  Redesigns Dashboard/Index.cshtml: adds diploma banner with progress bar
  showing three thresholds (75% danger, 80% diploma, 85% warning zone),
  and upgrades stat cards with ta-stat-card class and icons.
  Activates on: "redesign dashboard", "diploma banner", "progress bar",
  "student stats cards", "mejorar dashboard alumno", "banner diploma",
  "barra progreso tres umbrales", "85% aviso".
  Requires /design-system applied first. View-only.
---

# Dashboard Redesign — Student View

## Three thresholds (from Context_Alvaro.md business rules)
- < 75%: `AtRiskDrop` → red border + `badge-pulse`
- 75–80%: below diploma → warning/orange border
- 80–85%: warning zone (DiplomaWarningThreshold) → yellow border
- ≥ 85%: safe → success green border

## Files to modify
- `ProyectoExcel/Views/Dashboard/Index.cshtml`
- `ProyectoExcel/wwwroot/css/site.css` (ta-diploma-banner already in base.css)

## Instructions
1. Open Dashboard/Index.cshtml
2. Find the stat cards row (`<div class="row g-3 mb-4">`)
3. Replace with [stat-cards.cshtml](./references/stat-cards.cshtml). Show diff.

## Rules
- Use `Model.Summary.RealAttendancePercentage` directly
- `style="width: @pct%"` inline only for dynamic progress bar width
- Keep existing course selector form exactly as-is
- Keep existing history table — just improve container styling
