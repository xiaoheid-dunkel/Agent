@echo off
REM Troubleshooting script for XiaoHei

echo.
echo ========================================
echo  XiaoHei - Troubleshooting
echo ========================================
echo.

echo [1] Checking appsettings.json...
if exist appsettings.json (
    echo     ? Found in project root
) else (
    echo     ? NOT FOUND - Create it or copy from backup
)

if exist bin\Debug\net10.0\appsettings.json (
    echo     ? Found in output directory
) else (
    echo     ? NOT in output (may need rebuild)
)

echo.
echo [2] Checking project file...
if exist xiaohei.csproj (
    echo     ? Project file exists
    findstr /M "appsettings.json" xiaohei.csproj >nul
    if %errorlevel%==0 (
        echo     ? Configured to copy appsettings.json
    ) else (
        echo     ? NOT configured - may cause issues
    )
) else (
    echo     ? PROJECT FILE NOT FOUND
)

echo.
echo [3] Checking .NET...
dotnet --version >nul 2>&1
if %errorlevel%==0 (
    echo     ? .NET is installed
    dotnet --version
) else (
    echo     ? .NET not found - install .NET 10
)

echo.
echo [4] Attempting rebuild...
dotnet clean >nul 2>&1
dotnet build >nul 2>&1
if %errorlevel%==0 (
    echo     ? Build successful
) else (
    echo     ? Build failed - see error above
)

echo.
echo [5] Suggestions...
echo     - Check appsettings.json has valid API credentials
echo     - Run: dotnet clean && dotnet build
echo     - Run: dotnet run
echo.
echo For more help, see SETUP_GUIDE.md
echo.
pause
