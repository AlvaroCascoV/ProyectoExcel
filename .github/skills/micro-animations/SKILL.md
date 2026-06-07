---
name: micro-animations
description: >
  Adds loading overlay HTML and JS micro-behaviors: form submit spinner,
  tooltip initialization, double-submit prevention. animate.css is already
  loaded. The .loading-overlay CSS already exists in site.css.
  Apply LAST — always after all other design skills.
  Activates on: "loading overlay", "spinner submit", "tooltip init",
  "double submit", "micro animaciones", "spinner formulario".
  Requires /design-system applied first.
---

# Micro-animations & UX Polish

## What already exists — DO NOT re-add
- `animate.css` CDN in `_Layout.cshtml`
- `.loading-overlay` CSS in `site.css`
- Dark mode toggle + alert auto-dismiss in `site.js`
- Page fade-in + card animations in design-system `base.css`

## Step 1 — Loading overlay HTML in _Layout.cshtml
Paste just before `</body>`:
```html
<div class="loading-overlay d-none" id="loadingOverlay" aria-hidden="true">
    <div class="text-center text-white">
        <div class="spinner-border mb-3" style="width:2.5rem;height:2.5rem;">
            <span class="visually-hidden">@SharedLocalizer["Loading"]</span>
        </div>
    </div>
</div>
```

## Step 2 — Append JS to site.js
Append INSIDE the existing `(function(){ ... })();` IIFE, before closing `})();`:
```javascript
// Show loading overlay on any form submit
document.addEventListener('submit', function () {
    var o = document.getElementById('loadingOverlay');
    if (o) o.classList.remove('d-none');
});
// Bootstrap tooltip initialization
document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(function(el){
        new bootstrap.Tooltip(el);
    });
});
// Prevent double submit
document.addEventListener('submit', function (e) {
    var btn = e.target.querySelector('[type="submit"]:not([data-allow-multi])');
    if (btn) setTimeout(function(){ btn.disabled = true; }, 0);
});
```

## Step 3 — Localization key (if not already added)
```xml
<!-- ES --> <data name="Loading"><value>Cargando...</value></data>
<!-- EN --> <data name="Loading"><value>Loading...</value></data>
```

## Rules
- Never add `animate.css` CDN again — already loaded
- Only add the loading overlay HTML — the CSS exists already
- Append JS inside existing IIFE — never create a new one
