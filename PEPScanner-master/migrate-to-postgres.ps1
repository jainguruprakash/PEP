# PowerShell script to migrate PEP Scanner to PostgreSQL
Write-Host "Migrating PEP Scanner to PostgreSQL..." -ForegroundColor Green

# Change to API directory
Set-Location "d:\guru prakash\Repo\PEP\PEPScanner-master\src\backend\PEPScanner.API"

Write-Host "`nStep 1: Removing existing migrations..." -ForegroundColor Yellow
if (Test-Path "..\PEPScanner.Infrastructure\Migrations") {
    Remove-Item "..\PEPScanner.Infrastructure\Migrations" -Recurse -Force
    Write-Host "Existing migrations removed." -ForegroundColor Green
}

Write-Host "`nStep 2: Creating new PostgreSQL migration..." -ForegroundColor Yellow
dotnet ef migrations add InitialPostgreSQL --project ..\PEPScanner.Infrastructure --startup-project .

Write-Host "`nStep 3: Updating database..." -ForegroundColor Yellow
dotnet ef database update --project ..\PEPScanner.Infrastructure --startup-project .

Write-Host "`nMigration completed!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Update your connection strings with correct PostgreSQL credentials" -ForegroundColor White
Write-Host "2. Run the application to verify everything works" -ForegroundColor White

Read-Host "`nPress Enter to continue"