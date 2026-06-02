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
