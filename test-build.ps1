# Build test script
param(
    [string]$Configuration = "Release"
)

Write-Host "Starting build test..." -ForegroundColor Green

# Check project file
if (!(Test-Path "JoyConfig.csproj")) {
    Write-Error "Project file not found"
    exit 1
}

Write-Host "Project file exists" -ForegroundColor Green

# Check icon file
if (Test-Path "public\Icon.ico") {
    Write-Host "ICO icon file exists" -ForegroundColor Green
} else {
    Write-Host "ICO icon file missing" -ForegroundColor Yellow
}

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Cyan
dotnet restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "Package restore failed"
    exit 1
}

Write-Host "Package restore successful" -ForegroundColor Green

# Build project
Write-Host "Building project..." -ForegroundColor Cyan
dotnet build --configuration $Configuration --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}

Write-Host "Build successful" -ForegroundColor Green
Write-Host "Build test completed!" -ForegroundColor Green