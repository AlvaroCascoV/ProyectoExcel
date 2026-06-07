---
name: design-system
description: >
  Establishes CSS design tokens (--ta-* namespace) for ProyectoExcel.
  Apply FIRST before any other design skill. Adds tokens and base component
  classes to site.css. Works alongside existing calendar CSS variables
  (--color-lective, --bg-glass, etc.) — never overwrites them.
  Activates on: "design system", "CSS tokens", "set up styles",
  "base styles", "sistema de diseño", "tokens CSS", "componentes base".
  CRITICAL: calendar vars already exist — do not redefine them.
  Do not use for individual component redesigns.
---

# Design System — ProyectoExcel

## Already in site.css — DO NOT redefine
- `--bg-glass`, `--border-glass`, `--text-muted`, `--primary-glow`
- `--color-lective/festivo/no-lective/weekend/passed` + bg/border variants
- `--calendar-cell-color`, `--tooltip-bg`, `--tooltip-text`
- `.row-risk-danger`, `.row-risk-warning` (with dark mode)
- `.badge-pulse`, `.loading-overlay`, `.theme-card`
- All `.calendar-*`, `.legend-*`, `.upload-zone` classes

## Files to modify
`ProyectoExcel/wwwroot/css/site.css`

## Instructions

### Step 1 — Open and audit site.css
Confirm calendar variables exist. List any `--ta-*` conflicts. There should be none.

### Step 2 — Prepend tokens
Add content of [tokens.css](./assets/tokens.css) at the very top of site.css.

### Step 3 — Append base components
Add content of [base.css](./assets/base.css) at the very bottom of site.css.

### Step 4 — Show diff
Show complete diff before applying.

## Rules
- `--ta-*` namespace only — never `--bs-*`
- Dark overrides inside `[data-bs-theme="dark"] { }`
- Never touch the calendar CSS section
