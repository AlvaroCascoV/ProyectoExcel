---
name: student-search
description: >
  Adds a live client-side search box to filter student tables by name.
  Use when adding a search input to Students/Index, Statistics/Index,
  or any view with a student table. Zero extra API calls — all filtering
  happens in the browser. Activates on: "search students", "filter by name",
  "buscar alumno", "filtrar alumnos", "add search box", "live search".
  Do not use for server-side filtering or API search endpoints.
---

# Student Search — Live Filter

## Approach
Pure client-side JavaScript. All student rows are already in the DOM.
No server round-trips. No extra tokens at runtime.

See [search.js](./scripts/search.js) for the standalone script.

## Instructions

### Step 1 — Add the search input HTML
Open the target view (e.g., `ProyectoExcel/Views/Students/Index.cshtml`).
Paste the search input block above the `<div class="table-responsive">` element.
See [search-input.html](./assets/search-input.html).

### Step 2 — Add the script
Paste the script content from [search.js](./scripts/search.js)
inside `@section Scripts { <script> ... </script> }` at the bottom of the view.

### Step 3 — Add localization keys
Open both `.resx` files:
- `ProyectoExcel/Resources/SharedResource.es.resx`
- `ProyectoExcel/Resources/SharedResource.en.resx`
Add key `SearchStudents`:
- ES: `Buscar alumno...`
- EN: `Search student...`

### Step 4 — Show diffs
Show diff for the view and both .resx files before applying.

## Rules
- Works on any table where student name is column index 1 (zero-based: second column)
- The search input does NOT submit a form — it filters in place
- Clear button (×) resets filter and returns focus to input
- Result count shown below input while filtering
- Does not affect the existing filter form (course selector, month, etc.)
