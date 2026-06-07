# SKILL: filter-validation
Use this skill when adding or fixing percent filter validation (min/max) and non-lective day guards.

## When to trigger
- "Validate minPercent / maxPercent bounds"
- "Block attendance on weekends"
- "Fix filter range 0-100"
- "Non-lective day check"

---

## Percent filter validation

### API layer — StatisticsController
```csharp
// Add at the top of GetCourseStatistics and ExportToExcel actions,
// before calling the service:

if (minPercent is < 0 or > 100)
    return BadRequest(new { message = "minPercent must be between 0 and 100." });

if (maxPercent is < 0 or > 100)
    return BadRequest(new { message = "maxPercent must be between 0 and 100." });

if (minPercent.HasValue && maxPercent.HasValue && minPercent > maxPercent)
    return BadRequest(new { message = "minPercent cannot be greater than maxPercent." });
```

### MVC layer — StatisticsDashboardViewModel
```csharp
// Add these annotations to the ViewModel properties:
[Range(0, 100, ErrorMessage = "Minimum must be between 0 and 100")]
public decimal? MinPercent { get; set; }

[Range(0, 100, ErrorMessage = "Maximum must be between 0 and 100")]
public decimal? MaxPercent { get; set; }
```

### MVC Razor — input constraints
```html
<!-- In Statistics/Index.cshtml filter form -->
<input asp-for="MinPercent"
       type="number" min="0" max="100" step="0.1"
       class="form-control" placeholder="0" />

<input asp-for="MaxPercent"
       type="number" min="0" max="100" step="0.1"
       class="form-control" placeholder="100" />
```

---

## Non-lective day guard

### What already exists — do not duplicate
`ICalendarService` manages course lective days in the database.
If a course has an uploaded calendar (Excel spreadsheet), `ICalendarService.GetLectiveDatesAsync` returns those entries.
If a course has NO custom calendar, it dynamically falls back to the static `LectiveDayCalendar.GetLectiveDates` weekday generation logic.

### Dynamic guards — API AttendanceController & AttendanceService
Weekend and holiday checks are evaluated dynamically. Do not hardcode weekend blocks in controllers, as some courses might run weekend classes.
Instead, let `AttendanceService.SaveSessionAsync` validate using the database calendar entries:

```csharp
// In AttendanceService.SaveSessionAsync, before saving:
var lectiveDatesList = await calendarService.GetLectiveDatesAsync(courseId, cancellationToken);
var lectiveDates = lectiveDatesList.ToHashSet();

if (!lectiveDates.Contains(date))
    return (false, $"{date:yyyy-MM-dd} is not a lective day for this course.");
```

### MVC — Interactive visual monthly calendar grid
Instead of a simple date picker input, render an interactive monthly calendar using Javascript.
1. Fetch detailed course calendar entries from `/api/courses/{courseId}/calendar/entries`.
2. Map each entry by date in a lookup dictionary.
3. Determine the start and end range of the course calendar if a custom calendar is loaded.
4. Render grid items for the selected month:
   - **Lective days**: highlighted green, clickable.
   - **Festivo / Holiday**: highlighted red/orange with details, unclickable.
   - **No Lectivo**: highlighted orange/gray, unclickable.
   - **Weekends**: highlighted gray, unclickable.
   - **Outside Range / Non-Calendar Days**: Any day completely outside the custom calendar date range, or weekdays with no calendar entries when a custom calendar is loaded, MUST be disabled and grayed out.
   - **Passed/Recorded**: A highly visible blue indicator dot under the day cell.
   - **Selected**: Blue focus ring.

---

## Flag-icons integration
```html
<!-- In ProyectoExcel/Views/Shared/_Layout.cshtml, inside <head> -->
<link rel="stylesheet"
      href="https://cdn.jsdelivr.net/npm/flag-icons@7.2.3/css/flag-icons.min.css" />
```

```html
<!-- Usage in any view: ISO 3166-1 alpha-2 country code in lowercase -->
<span class="fi fi-es" title="Spain"></span>
<span class="fi fi-gb" title="United Kingdom"></span>
```
