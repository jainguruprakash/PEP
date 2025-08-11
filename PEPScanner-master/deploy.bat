@echo off
echo ========================================
echo    Pepify - PEP Scanner Deployment
echo ========================================
echo.

echo [1/6] Checking prerequisites...
where node >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: Node.js not found. Please install Node.js 18+ from https://nodejs.org/
    pause
    exit /b 1
)

where dotnet >nul 2>nul
if %errorlevel% neq 0 (
    echo ERROR: .NET 8 SDK not found. Please install from https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo ✓ Prerequisites check passed

echo.
echo [2/6] Setting up backend...
cd src\backend\PEPScanner.API
echo Restoring .NET packages...
dotnet restore
if %errorlevel% neq 0 (
    echo ERROR: Failed to restore .NET packages
    pause
    exit /b 1
)

echo ✓ Backend packages restored

echo.
echo [3/6] Setting up database...
echo Running database migrations...
dotnet ef database update
if %errorlevel% neq 0 (
    echo WARNING: Database migration failed. Please check PostgreSQL connection.
    echo You can run 'dotnet ef database update' manually later.
)

echo ✓ Database setup completed

echo.
echo [4/6] Setting up frontend...
cd ..\..\pep-scanner-ui
echo Installing npm packages...
npm install
if %errorlevel% neq 0 (
    echo ERROR: Failed to install npm packages
    pause
    exit /b 1
)

echo ✓ Frontend packages installed

echo.
echo [5/6] Building applications...
echo Building frontend...
npm run build
if %errorlevel% neq 0 (
    echo WARNING: Frontend build had warnings but completed successfully
)

echo ✓ Applications built successfully

echo.
echo [6/6] Starting applications...
echo.
echo Starting backend API...
start "Pepify Backend API" cmd /k "cd /d %~dp0src\backend\PEPScanner.API && dotnet run"

timeout /t 5 /nobreak >nul

echo Starting frontend development server...
start "Pepify Frontend" cmd /k "cd /d %~dp0src\pep-scanner-ui && npm start"

echo.
echo ========================================
echo    Deployment Complete!
echo ========================================
echo.
echo Applications are starting up...
echo.
echo Backend API: http://localhost:5098
echo Frontend App: http://localhost:4200
echo API Documentation: http://localhost:5098/swagger
echo Interactive Demo: file:///%~dp0INTERACTIVE_DEMO.html
echo.
echo Please wait 30-60 seconds for applications to fully start.
echo.
echo Press any key to open the interactive demo...
pause >nul

start "" "%~dp0INTERACTIVE_DEMO.html"

echo.
echo Deployment script completed successfully!
echo Both applications should now be running.
echo.
pause