#!/usr/bin/env pwsh

Write-Host ""
Write-Host "========================================"
Write-Host "  XiaoHei - Troubleshooting"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check 1: appsettings.json
Write-Host "[1] Checking appsettings.json..."
if (Test-Path "appsettings.json") {
    Write-Host "    ? Found in project root" -ForegroundColor Green
} else {
    Write-Host "    ? NOT FOUND in project root" -ForegroundColor Red
}

if (Test-Path "bin/Debug/net10.0/appsettings.json") {
    Write-Host "    ? Found in output directory" -ForegroundColor Green
} else {
    Write-Host "    ? NOT in output (may need rebuild)" -ForegroundColor Yellow
}

# Check 2: Project file
Write-Host ""
Write-Host "[2] Checking project file..."
if (Test-Path "xiaohei.csproj") {
    Write-Host "    ? Project file exists" -ForegroundColor Green
    
    $content = Get-Content "xiaohei.csproj"
    if ($content -match "appsettings.json") {
        Write-Host "    ? Configured to copy appsettings.json" -ForegroundColor Green
    } else {
        Write-Host "    ? NOT configured to copy appsettings.json" -ForegroundColor Red
    }
} else {
    Write-Host "    ? PROJECT FILE NOT FOUND" -ForegroundColor Red
}

# Check 3: .NET
Write-Host ""
Write-Host "[3] Checking .NET..."
try {
    $version = dotnet --version 2>$null
    Write-Host "    ? .NET is installed" -ForegroundColor Green
    Write-Host "    Version: $version"
} catch {
    Write-Host "    ? .NET not found - install .NET 10" -ForegroundColor Red
}

# Check 4: Build
Write-Host ""
Write-Host "[4] Attempting rebuild..."
Write-Host "    Cleaning..." -NoNewline
dotnet clean 2>$null | Out-Null
Write-Host " Done"
Write-Host "    Building..." -NoNewline
$buildOutput = dotnet build 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host " Success" -ForegroundColor Green
} else {
    Write-Host " Failed" -ForegroundColor Red
    Write-Host "    Output: $buildOutput"
}

# Check 5: Configuration
Write-Host ""
Write-Host "[5] Configuration check..."
if (Test-Path "appsettings.json") {
    try {
        $config = Get-Content "appsettings.json" | ConvertFrom-Json
        if ($config.Doubao.ApiKey -and $config.Doubao.EndpointId) {
            Write-Host "    ? appsettings.json has required fields" -ForegroundColor Green
            Write-Host "      ApiKey: $(($config.Doubao.ApiKey).Substring(0,10))..."
            Write-Host "      EndpointId: $($config.Doubao.EndpointId)"
        } else {
            Write-Host "    ? Missing required fields in appsettings.json" -ForegroundColor Red
        }
    } catch {
        Write-Host "    ? Error reading appsettings.json: $_" -ForegroundColor Red
    }
}

# Summary
Write-Host ""
Write-Host "========================================"
Write-Host "  Next Steps"
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "1. Fix any issues shown above"
Write-Host "2. Run: dotnet run"
Write-Host "3. Chat with the AI!"
Write-Host ""
Write-Host "For more help: SETUP_GUIDE.md"
Write-Host ""
