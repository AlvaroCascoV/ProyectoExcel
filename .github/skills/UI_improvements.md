---
name: ui-polish
description: >
  Applies general UI improvements to any view in the ProyectoExcel MVC project.
  Use for adding empty states to tables, loading spinners on form submit,
  Bootstrap tooltips on badges, sticky table headers, progress bars for
  attendance percentage, breadcrumb navigation, or responsive table fixes.
  Activates on: "improve UI", "add empty state", "loading spinner",
  "tooltip", "progress bar", "breadcrumb", "sticky header",
  "mejorar visual", "estado vacío", "barra progreso".
  Do not use for risk-related colors (use /risk-alerts) or
  search functionality (use /student-search).
---

# UI Polish — ProyectoExcel

## Available patterns

See [patterns.md](./references/patterns.md) for all ready-to-use snippets:

| Pattern | Use when |
|---|---|
| Empty state | Table has no rows — replace plain "no data" text |
| Loading overlay | Any form submit that takes > 0.5s |
| Bootstrap tooltips | Any badge or icon needs explanation on hover |
| Sticky header | Table has > 15 rows |
| Attendance progress bar | Student profile or dashboard summary card |
| Breadcrumb | Any detail page (student profile, etc.) |

## Instructions

### Step 1 — Identify the pattern needed
Read the user's request and match it to one of the patterns above.
Read [patterns.md](./references/patterns.md) for the code.

### Step 2 — Open only the target file
Do not open more files than necessary.
For CSS additions: open `wwwroot/css/site.css`.
For JS additions: open `wwwroot/js/site.js`.
For view changes: open only the specific `.cshtml` file.

### Step 3 — Apply and show diff
Show diff before applying. One pattern at a time.

### Step 4 — Localization
Any new user-visible string goes in both `.resx` files.
Never hardcode display text in views or controllers.

## Rules
- Bootstrap 5 utilities only — no inline styles except dynamic `width` on progress bars
- Dark mode: every addition must work with `data-bs-theme="dark"` on `<html>`
- Tooltips require Bootstrap JS — it is already loaded via the CDN in `_Layout.cshtml`
- No new JS libraries — use only jQuery (already loaded) and Bootstrap JS (already loaded)
