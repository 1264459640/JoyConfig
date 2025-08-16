# Squirrel.Windows packaging script
# Creates installer and update packages

param(
    [string]$Version = "1.0.0",
    [string]$Configuration = "Release",
    [string]$OutputPath = "Releases"
)

# Set error handling
$ErrorActionPreference = "Stop"

Write-Host "Starting Squirrel.Windows packaging process..." -ForegroundColor Green
Write-Host "Version: $Version" -ForegroundColor Yellow
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow

# Clean output directory
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
    Write-Host "Cleaned output directory: $OutputPath" -ForegroundColor Yellow
}

# Create output directory
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Publish application
Write-Host "Publishing application..." -ForegroundColor Cyan
dotnet publish JoyConfig.csproj `
    --configuration $Configuration `
    --framework net9.0 `
    --runtime win-x64 `
    --self-contained true `
    --output "publish" `
    -p:PublishSingleFile=false `
    -p:PublishReadyToRun=true `
    -p:Version=$Version

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publishing failed"
    exit 1
}

Write-Host "Application published successfully" -ForegroundColor Green

# Create NuSpec file
$nuspecContent = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
    <id>JoyConfig</id>
    <version>$Version</version>
    <title>JoyConfig</title>
    <authors>JoyConfig Team</authors>
    <description>Attribute Database Editor for Game Development</description>
    <projectUrl>https://github.com/1264459640/JoyConfig</projectUrl>
    <iconUrl>https://raw.githubusercontent.com/1264459640/JoyConfig/main/public/Icon.png</iconUrl>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <copyright>Copyright © 2025</copyright>
  </metadata>
  <files>
    <file src="publish\**" target="lib\net9.0\" />
  </files>
</package>
"@

$nuspecPath = "JoyConfig.nuspec"
$nuspecContent | Out-File -FilePath $nuspecPath -Encoding UTF8
Write-Host "Created NuSpec file: $nuspecPath" -ForegroundColor Green

# Create NuGet package
Write-Host "Creating NuGet package..." -ForegroundColor Cyan
.\tools\nuget.exe pack $nuspecPath -OutputDirectory $OutputPath

if ($LASTEXITCODE -ne 0) {
    Write-Error "NuGet package creation failed"
    exit 1
}

$nupkgFile = Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | Select-Object -First 1
Write-Host "NuGet package created: $($nupkgFile.Name)" -ForegroundColor Green

# Use Squirrel to create installer
Write-Host "Creating installer with Squirrel..." -ForegroundColor Cyan

# Ensure Squirrel.Windows tools are installed
$squirrelExe = "tools\Squirrel.Windows\tools\squirrel.exe"
if (!(Test-Path $squirrelExe)) {
    Write-Host "Installing Squirrel.Windows tools..." -ForegroundColor Yellow
    
    # Create tools directory
    if (!(Test-Path "tools")) {
        New-Item -ItemType Directory -Path "tools" -Force | Out-Null
    }
    
    # Download NuGet.exe if not exists
    $nugetExe = "tools\nuget.exe"
    if (!(Test-Path $nugetExe)) {
        Write-Host "Downloading NuGet.exe..." -ForegroundColor Yellow
        Invoke-WebRequest -Uri "https://dist.nuget.org/win-x86-commandline/latest/nuget.exe" -OutFile $nugetExe
    }
    
    # Use NuGet to download Squirrel.Windows
    & $nugetExe install Squirrel.Windows -ExcludeVersion -OutputDirectory tools
    Write-Host "Squirrel.Windows tools installed" -ForegroundColor Green
}

# Create Squirrel release
& $squirrelExe --releasify $nupkgFile.FullName --releaseDir $OutputPath --setupIcon "public\Icon.ico"

if ($LASTEXITCODE -ne 0) {
    Write-Error "Squirrel packaging failed"
    exit 1
}

Write-Host "Squirrel packaging completed!" -ForegroundColor Green
Write-Host "Output directory: $OutputPath" -ForegroundColor Yellow

# List generated files
Write-Host "Generated files:" -ForegroundColor Cyan
Get-ChildItem -Path $OutputPath | ForEach-Object {
    Write-Host "  $($_.Name)" -ForegroundColor White
}

# Clean up temporary files
Remove-Item "publish" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $nuspecPath -Force -ErrorAction SilentlyContinue

Write-Host "Packaging process completed!" -ForegroundColor Green