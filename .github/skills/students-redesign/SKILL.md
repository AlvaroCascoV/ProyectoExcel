---
name: students-redesign
description: >
  Improves Students/Index.cshtml: avatar initials colored by name,
  inactive row dimming, live search box, and empty state with icon.
  Activates on: "redesign students", "avatar initials", "student search",
  "inactive students", "empty state students", "lista alumnos mejorar",
  "iniciales avatar alumnos", "buscar alumnos", "estado vacío lista".
  Requires /design-system applied first. View-only.
---

# Students List Redesign

## Files to modify
- `ProyectoExcel/Views/Students/Index.cshtml`

## Step 1 — Replace foreach <tr> with avatar version
See [student-row.cshtml](./references/student-row.cshtml).
`ta-avatar` and `ta-row-inactive` classes are defined in design-system base.css.

## Step 2 — Search input above table-responsive
```html
<div class="mb-3">
    <div class="ta-search-wrap">
        <i class="bi bi-search" aria-hidden="true"></i>
        <input type="text" id="studentsSearch" class="form-control form-control-sm"
               placeholder="@SharedLocalizer["SearchStudents"]" autocomplete="off" />
    </div>
</div>
```

## Step 3 — Empty state
Replace `<td colspan="4" class="text-center text-muted">No students</td>` with:
```html
<tr><td colspan="4">
    <div class="ta-empty">
        <i class="bi bi-inbox" aria-hidden="true"></i>
        @SharedLocalizer["NoStudentsEnrolled"]
    </div>
</td></tr>
```

## Step 4 — Search JS in @section Scripts
```javascript
(function(){
    var i=document.getElementById('studentsSearch');
    var t=document.querySelector('table tbody');
    if(!i||!t) return;
    i.addEventListener('input',function(){
        var q=i.value.trim().toLowerCase();
        t.querySelectorAll('tr').forEach(function(r){
            var n=r.cells[1]?r.cells[1].textContent.toLowerCase():'';
            r.style.display=!q||n.includes(q)?'':'none';
        });
    });
})();
```

## Step 5 — Localization keys
```xml
<!-- ES --> <data name="NoStudentsEnrolled"><value>No hay alumnos matriculados en este curso.</value></data>
<!-- EN --> <data name="NoStudentsEnrolled"><value>No students enrolled in this course.</value></data>
```

## Rules
- Avatar `data-hue` drives color — CSS handles it, no JS logic
- Inactive rows: `ta-row-inactive` class — never `display:none`
- `SearchStudents` key already added in statistics-redesign — reuse it
