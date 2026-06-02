## Description
<!-- What does this PR do? One paragraph. -->


## Type of change
- [ ] `feat` — new feature
- [ ] `fix` — bug fix
- [ ] `refactor` — cleanup, no behavior change
- [ ] `docs` — documentation / Context.md only
- [ ] `chore` — config, packages

## Branch
- From: `feature/` or `fix/`
- Into: `develop` (never directly into `main`)

---

## Testing checklist
- [ ] API runs on `http://localhost:5180` without errors
- [ ] MVC runs on `http://localhost:5162` without errors
- [ ] Login works for Student, Teacher and Admin roles
- [ ] The specific feature I changed works end-to-end
- [ ] No `bin/` or `obj/` files committed
- [ ] No real credentials in `appsettings.json`

---

## CONTEXT.md checklist — required if any box below is checked

Did this PR...

- [ ] Add or modify an API endpoint? → Update **API endpoints table** in CONTEXT.md
- [ ] Add or modify a DB table or entity? → Update **Domain model** in CONTEXT.md
- [ ] Add a NuGet package? → Update **NuGet packages** in CONTEXT.md
- [ ] Add or change a business rule? → Update **Business rules** in CONTEXT.md
- [ ] Complete a feature? → Move from "In progress" to "Done" in CONTEXT.md
- [ ] Discard or defer a feature? → Add reason to "Discarded" in CONTEXT.md
- [ ] Change a coding convention? → Update **AGENTS.md**

**If any box above is checked, CONTEXT.md must be updated in this PR or it will not be merged.**

Changelog line added to CONTEXT.md:
```
| YYYY-MM-DD | [Name] | [Description] |
```

---

## Token hygiene (for AI-assisted PRs)
- [ ] Agent was given only the files it needed — not the entire solution
- [ ] Generated code was reviewed line by line before committing
- [ ] No unnecessary abstractions or new files were added by the agent
