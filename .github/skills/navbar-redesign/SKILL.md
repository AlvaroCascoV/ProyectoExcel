---
name: navbar-redesign
description: >
  Improves _Layout.cshtml navigation: adds active page link highlighting
  and a mortarboard icon to the navbar brand. The flag-icons library and
  language switcher already work correctly with fi fi-* classes — do NOT
  touch them. Activates on: "active nav link", "navbar brand icon",
  "highlight current page", "mejorar navbar", "enlace activo",
  "icono navbar brand". Requires /design-system applied first.
  Do NOT use to fix flags — they already work.
---

# Navbar Redesign

## What already works — DO NOT touch
- `fi fi-@currentFlag` language switcher — already correct
- `currentFlag` / `currentLabel` Razor variables — already correct  
- Dark mode toggle — fully working
- animate.css CDN — already loaded

## File to modify
`ProyectoExcel/Views/Shared/_Layout.cshtml`

## Step 1 — Add NavCss helper
In the top `@{ }` block, add:
```csharp
var ctrl = ViewContext.RouteData.Values["controller"]?.ToString() ?? "";
string NavCss(string c) => c == ctrl ? "nav-link active-page" : "nav-link";
```

## Step 2 — Apply to nav links
```html
<!-- Before -->
<a class="nav-link" asp-controller="Attendance" asp-action="Index">

<!-- After -->
<a class="@NavCss("Attendance")" asp-controller="Attendance" asp-action="Index">
```
Apply to: Attendance, Students, Statistics, Dashboard, and any other nav links.

## Step 3 — Brand icon
```html
<a class="navbar-brand d-flex align-items-center gap-2 fw-semibold"
   asp-controller="Home" asp-action="Index">
    <i class="bi bi-mortarboard-fill text-primary" aria-hidden="true"></i>
    @SharedLocalizer["AppName"]
</a>
```

## Step 4 — Show diff before applying

## Rules
- `active-page` CSS class is defined in design-system base.css
- Do NOT change flag-icons implementation
- Do NOT remove animate.css CDN
- Keep all `[ValidateAntiForgeryToken]` forms intact
