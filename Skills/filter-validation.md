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
`LectiveDayCalendar.GetWeekdaysInRange` already filters weekends from the lective calendar.
`LectiveDayCalendar.GetLectiveDates` produces the final list of valid days.

### Where to add weekend guard — API AttendanceController
```csharp
// In SaveSession action, before calling the service:
if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
    return BadRequest(new { message = "Cannot save attendance on weekends." });
```

### Where to add non-lective guard — AttendanceService
```csharp
// In SaveSessionAsync, before upserting records:
var lectiveDates = LectiveDayCalendar.GetLectiveDates(courseStart, courseEnd);
if (!lectiveDates.Contains(date))
    return (false, $"{date:yyyy-MM-dd} is not a lective day for this course.");
```

### MVC — disable weekend dates in the date picker
```html
<!-- In Attendance/Index.cshtml -->
<input asp-for="SelectedDate"
       type="date"
       class="form-control"
       id="attendanceDatePicker" />

<script>
// Disable weekends in native date input (visual hint, not a security control)
document.getElementById('attendanceDatePicker').addEventListener('input', function() {
    const d = new Date(this.value);
    const day = d.getUTCDay();
    if (day === 0 || day === 6) {
        this.setCustomValidity('Please select a weekday.');
        this.reportValidity();
    } else {
        this.setCustomValidity('');
    }
});
</script>
```

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
