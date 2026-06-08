---
name: home-hero
description: >
  Redesigns Home/Index.cshtml with a hero section, four feature cards
  (attendance tracking, risk detection, statistics, calendar upload),
  and a stats strip (156 días, 80% diploma, 7 estados). Replaces the
  plain centered text. Activates on: "redesign home", "hero section",
  "landing page", "home page", "página de inicio", "mejorar home".
  Requires /design-system applied first. View-only — no controller changes.
---

# Home Hero Redesign

## Files to modify
- `ProyectoExcel/Views/Home/Index.cshtml` — replace entire content
- `ProyectoExcel/Resources/SharedResource.es.resx` — add keys
- `ProyectoExcel/Resources/SharedResource.en.resx` — add keys

## Localization keys

```xml
<!-- SharedResource.es.resx -->
<data name="FeatureAttendanceTitle"><value>Control de asistencia</value></data>
<data name="FeatureAttendanceDesc"><value>Registro diario con 7 estados diferenciados y exportación PDF y Excel.</value></data>
<data name="FeatureRiskTitle"><value>Detección de riesgo</value></data>
<data name="FeatureRiskDesc"><value>Alertas automáticas a los 75%, 80% y 85% de asistencia real.</value></data>
<data name="FeatureStatsTitle"><value>Estadísticas y gráficos</value></data>
<data name="FeatureStatsDesc"><value>Rankings, distribución por estado y exportación a Excel y PDF.</value></data>
<data name="FeatureCalendarTitle"><value>Calendario lectivo</value></data>
<data name="FeatureCalendarDesc"><value>Sube el calendario Excel del curso para días lectivos dinámicos.</value></data>
<data name="StatLectiveDays"><value>días lectivos</value></data>
<data name="StatDiplomaThreshold"><value>mínimo diploma</value></data>
<data name="StatStatusTypes"><value>estados de asistencia</value></data>
<!-- SharedResource.en.resx -->
<data name="FeatureAttendanceTitle"><value>Attendance tracking</value></data>
<data name="FeatureAttendanceDesc"><value>Daily record with 7 attendance statuses and PDF/Excel export.</value></data>
<data name="FeatureRiskTitle"><value>Risk detection</value></data>
<data name="FeatureRiskDesc"><value>Automatic alerts at 75%, 80% and 85% real attendance.</value></data>
<data name="FeatureStatsTitle"><value>Statistics and charts</value></data>
<data name="FeatureStatsDesc"><value>Rankings, status distribution and Excel/PDF export.</value></data>
<data name="FeatureCalendarTitle"><value>Academic calendar</value></data>
<data name="FeatureCalendarDesc"><value>Upload the course Excel calendar for dynamic lective days.</value></data>
<data name="StatLectiveDays"><value>lective days</value></data>
<data name="StatDiplomaThreshold"><value>diploma minimum</value></data>
<data name="StatStatusTypes"><value>attendance statuses</value></data>
```

## Instructions
1. Replace Home/Index.cshtml with [home.cshtml](./references/home.cshtml). Show file.
2. Add all keys to both .resx files. Show diffs.

## Rules
- CTA → `asp-controller="Account" asp-action="Login"` — do not change
- Use `ta-feature-card`, `ta-stats-strip`, `ta-hero` from design-system
- No new controller logic
