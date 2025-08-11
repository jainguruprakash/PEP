@echo off
echo Migrating PEP Scanner to PostgreSQL...

cd /d "d:\guru prakash\Repo\PEP\PEPScanner-master\src\backend\PEPScanner.API"

echo.
echo Step 1: Removing existing migrations...
rmdir /s /q "..\PEPScanner.Infrastructure\Migrations" 2>nul

echo.
echo Step 2: Creating new PostgreSQL migration...
dotnet ef migrations add InitialPostgreSQL --project ..\PEPScanner.Infrastructure --startup-project .

echo.
echo Step 3: Updating database...
dotnet ef database update --project ..\PEPScanner.Infrastructure --startup-project .

echo.
echo Migration completed!
echo.
echo Next steps:
echo 1. Update your connection strings with correct PostgreSQL credentials
echo 2. Run the application to verify everything works
echo.
pause