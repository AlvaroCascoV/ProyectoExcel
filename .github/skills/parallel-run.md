# SKILL: parallel-run
Use this skill when the developer needs to run API and MVC simultaneously for local development.

## When to trigger
- "Run both projects at the same time"
- "Start the full stack locally"
- "How do I run API + MVC in parallel"

---

## Can an agent run both projects in parallel?

**Short answer: yes, but with important nuances.**

Antigravity and Cursor can open terminals and execute shell commands. They CAN:
- Run `dotnet run` in the API project in one terminal
- Run `dotnet run` in the MVC project in another terminal
- Watch output from both

They CANNOT:
- Guarantee both started successfully without reading terminal output
- Handle SQL Server connectivity automatically
- Manage process lifecycle (restart on crash)

**Recommended approach: let the agent start both, but YOU verify they are up.**

---

## Option 1 — Two terminals (simplest, most reliable)

### Terminal 1 — API
```bash
cd ApiProyectoExcel
dotnet run
# Wait for: Now listening on: http://localhost:5180
```

### Terminal 2 — MVC
```bash
cd ProyectoExcel
dotnet run
# Wait for: Now listening on: http://localhost:5162
```

**Prompt for Antigravity:**
```
Open two terminals.
In the first, run: cd ApiProyectoExcel && dotnet run
In the second, run: cd ProyectoExcel && dotnet run
Tell me when both show "Now listening on" in their output.
```

---

## Option 2 — Solution-level launch (recommended for daily use)

Create `launchSettings.json` or a compound launch in VS Code / Antigravity:

### `launch.json` — VS Code / Antigravity compound launch
```json
// .vscode/launch.json
{
  "version": "0.2.0",
  "configurations": [
    {
      "name": "API: ApiProyectoExcel",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build-api",
      "program": "${workspaceFolder}/ApiProyectoExcel/bin/Debug/net10.0/ApiProyectoExcel.dll",
      "args": [],
      "cwd": "${workspaceFolder}/ApiProyectoExcel",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    },
    {
      "name": "MVC: MvcProyectoExcel",
      "type": "coreclr",
      "request": "launch",
      "preLaunchTask": "build-mvc",
      "program": "${workspaceFolder}/ProyectoExcel/bin/Debug/net10.0/MvcProyectoExcel.dll",
      "args": [],
      "cwd": "${workspaceFolder}/ProyectoExcel",
      "stopAtEntry": false,
      "serverReadyAction": {
        "action": "openExternally",
        "pattern": "\\bNow listening on:\\s+(https?://\\S+)"
      },
      "env": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  ],
  "compounds": [
    {
      "name": "Full Stack: API + MVC",
      "configurations": ["API: ApiProyectoExcel", "MVC: MvcProyectoExcel"],
      "stopAll": true
    }
  ]
}
```

With this file in place, a single F5 or "Full Stack: API + MVC" launch starts both projects.

---

## Option 3 — PowerShell script (one command, Windows)

```powershell
# run-all.ps1 — place at solution root
$api = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd ApiProyectoExcel; dotnet run" -PassThru
$mvc = Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd ProyectoExcel; dotnet run" -PassThru

Write-Host "Started API (PID $($api.Id)) and MVC (PID $($mvc.Id))"
Write-Host "API:  http://localhost:5180"
Write-Host "MVC:  http://localhost:5162"
Write-Host "Press Enter to stop both..."
Read-Host
$api.Kill()
$mvc.Kill()
```

Run with: `.\run-all.ps1`

**Prompt for Antigravity:**
```
Create a file run-all.ps1 at the solution root that starts both projects
in separate PowerShell windows and stops them on Enter.
Use the parallel-run skill.
```

---

## Startup order — important

Always start **API first**, then MVC.
MVC calls the API on first page load. If the API is not ready, MVC shows:
`"Could not reach the API. Make sure ApiProyectoExcel is running on http://localhost:5180."`
This is normal — refresh the MVC page once the API is up.

## SQL Server prerequisite
SQL Server must be running before starting either project.
Check: `services.msc` → SQL Server (MSSQLSERVER or DEVELOPER instance) → Status: Running
Or: `Get-Service -Name 'MSSQL*' | Select-Object Name, Status` in PowerShell
