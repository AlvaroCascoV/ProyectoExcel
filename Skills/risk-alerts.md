---
name: risk-alerts
description: >
  Adds visual risk indicators to the statistics view: row color coding,
  animated danger badges, and alert banners for at-risk students.
  Use when highlighting students below attendance thresholds, adding
  warning banners for drop-risk or below-diploma cases, or color-coding
  table rows by attendance percentage. Activates on: "highlight at-risk",
  "color rows", "risk banner", "alerta riesgo", "colorear filas",
  "students below threshold", "diploma risk warning".
  Do not use for search/filter functionality (use /student-search).
---

# Risk Alerts — ProyectoExcel

## Thresholds (from business rules — do not change)
- `AtRiskDrop`: `RealAttendancePercentage < 75%` → red
- `!DiplomaEligible && !AtRiskDrop`: between 75% and 80% → amber
- `DiplomaEligible`: ≥ 80% → no color (default)

See [css-rules.md](./references/css-rules.md) for the CSS.
See [razor-patterns.md](./references/razor-patterns.md) for the Razor snippets.

## Instructions

### Step 1 — Add CSS
Open: `ProyectoExcel/wwwroot/css/site.css`
Append the risk row CSS from [css-rules.md](./references/css-rules.md).
Show diff before applying.

### Step 2 — Update Statistics table rows
Open: `ProyectoExcel/Views/Statistics/Index.cshtml`
Replace plain `<tr>` in the student foreach with the row class logic
from [razor-patterns.md](./references/razor-patterns.md).
Show diff before applying.

### Step 3 — Add alert banner
In the same file, add the alert banners above `<div class="table-responsive">`.
See [razor-patterns.md](./references/razor-patterns.md).
Show diff before applying.

### Step 4 — Add localization keys
Add to both `.resx` files. See [razor-patterns.md](./references/razor-patterns.md)
for the exact key names and values.

## Rules
- `badge-pulse` animation only on `AtRiskDrop` (< 75%) — not on below-diploma
- Dark mode overrides included — test in both `data-bs-theme="light"` and `"dark"`
- Alert banner is dismissible — use `alert-dismissible fade show` classes
- Never hardcode the 75% or 80% thresholds in the view — use the DTO properties
  `student.AtRiskDrop` and `student.DiplomaEligible` which are already computed
