@echo off
echo Setting up PEP Scanner Application...

echo.
echo 1. Installing frontend dependencies...
cd "PEPScanner-master\src\pep-scanner-ui"
call npm install

echo.
echo 2. Building frontend...
call npm run build

echo.
echo 3. Running database migrations...
cd "..\backend\PEPScanner.API"
dotnet ef database update

echo.
echo 4. Starting backend API...
start "Backend API" cmd /k "dotnet run"

echo.
echo 5. Starting frontend...
cd "..\..\pep-scanner-ui"
start "Frontend" cmd /k "npm start"

echo.
echo Setup complete! 
echo Backend API: http://localhost:5098
echo Frontend: http://localhost:4200
echo.
pause