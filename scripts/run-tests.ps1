Write-Host "Running GarmentsERP Test Suite..." -ForegroundColor Cyan
Set-Location "E:\Garments Project\GarmentsERP\src\backend"
dotnet test GarmentsERP.slnx --logger "console;verbosity=normal"
