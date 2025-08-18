# Squirrel.Windows Packaging Script
# For creating JoyConfig application installer

param(
    [string]$Version,
    [string]$Configuration = "Release",
    [string]$OutputPath = "Releases",
    [string]$PublishPath = "publish",
    [string]$IconPath = "public\Icon.ico",
    [switch]$Help
)

# Set console encoding to UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Show help information
if ($Help) {
    Write-Host "Squirrel.Windows Packaging Script Usage:" -ForegroundColor Green
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -Version          Application version (required)"
    Write-Host "  -Configuration    Build configuration (default: Release)"
    Write-Host "  -OutputPath       Output path (default: Releases)"
    Write-Host "  -PublishPath      Published files path (default: publish)"
    Write-Host "  -IconPath         Application icon path (default: public\Icon.ico)"
    Write-Host "  -Help             Show this help information"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\build-squirrel.ps1 -Version 1.0.0"
    Write-Host "  .\build-squirrel.ps1 -Version 1.2.3 -OutputPath MyReleases"
    exit 0
}

# Check required parameters
if ([string]::IsNullOrEmpty($Version)) {
    Write-Host "Error: Version parameter is required" -ForegroundColor Red
    Write-Host "Use -Help to see usage information" -ForegroundColor Yellow
    exit 1
}

# Project configuration
$ProjectName = "JoyConfig"
$AppId = "JoyConfig"
$CompanyName = "JoyConfig Team"
$Description = "Attribute Database Editor for Game Development"
$Copyright = "Copyright © 2025 JoyConfig Team"

# Color output functions
function Write-Step {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Success: $Message" -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Error: $Message" -ForegroundColor Red
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[$(Get-Date -Format 'HH:mm:ss')] Warning: $Message" -ForegroundColor Yellow
}

# Error handling
$ErrorActionPreference = "Stop"

try {
    Write-Step "Starting Squirrel packaging process..."
    Write-Host "Version: $Version" -ForegroundColor Gray
    Write-Host "Output path: $OutputPath" -ForegroundColor Gray
    Write-Host ""

    # Check published files
    Write-Step "Checking published files..."
    
    if (!(Test-Path $PublishPath)) {
        throw "Publish path $PublishPath does not exist. Please run build and publish steps first."
    }
    
    $exeFile = Get-ChildItem -Path $PublishPath -Filter "$ProjectName.exe" -ErrorAction SilentlyContinue
    if (!$exeFile) {
        throw "$ProjectName.exe file not found in $PublishPath"
    }
    
    Write-Success "Published files check completed"
    Write-Host ""

    # Check icon file
    if (!(Test-Path $IconPath)) {
        Write-Warning "Icon file $IconPath does not exist, will use default icon"
        $IconPath = $null
    }
    else {
        Write-Host "Using icon: $IconPath" -ForegroundColor Green
    }

    # Prepare output directory
    Write-Step "Preparing output directory..."
    
    if (Test-Path $OutputPath) {
        Remove-Item $OutputPath -Recurse -Force
    }
    New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
    
    Write-Success "Output directory prepared: $(Resolve-Path $OutputPath)"
    Write-Host ""

    # Check Squirrel tools
    Write-Step "Checking Squirrel.Windows tools..."
    
    $squirrelExe = $null
    
    # Method 1: Check Chocolatey installation
    $chocoSquirrel = "$env:ChocolateyInstall\lib\squirrel-windows\tools\Squirrel.exe"
    if (Test-Path $chocoSquirrel) {
        $squirrelExe = $chocoSquirrel
        Write-Host "Found Chocolatey Squirrel: $squirrelExe" -ForegroundColor Green
    }
    
    # Method 2: Check local tools directory
    if (!$squirrelExe) {
        $localSquirrel = "tools\squirrel.windows\tools\Squirrel.exe"
        if (Test-Path $localSquirrel) {
            $squirrelExe = $localSquirrel
            Write-Host "Found local Squirrel: $squirrelExe" -ForegroundColor Green
        }
    }
    
    # Method 3: Check PATH
    if (!$squirrelExe) {
        if (Get-Command "Squirrel" -ErrorAction SilentlyContinue) {
            $squirrelExe = "Squirrel"
            Write-Host "Found Squirrel in PATH" -ForegroundColor Green
        }
    }
    
    # If not found, try to download
    if (!$squirrelExe) {
        Write-Step "Downloading Squirrel.Windows..."
        
        $toolsDir = "tools"
        if (!(Test-Path $toolsDir)) {
            New-Item -ItemType Directory -Path $toolsDir -Force | Out-Null
        }
        
        # Use NuGet to download Squirrel.Windows
        if (Get-Command "nuget" -ErrorAction SilentlyContinue) {
            Push-Location $toolsDir
            try {
                nuget install Squirrel.Windows -ExcludeVersion -OutputDirectory .
                $squirrelExe = "squirrel.windows\tools\Squirrel.exe"
                if (Test-Path $squirrelExe) {
                    $squirrelExe = "..\tools\$squirrelExe"
                    Write-Success "Squirrel.Windows downloaded"
                }
                else {
                    throw "Squirrel.exe download failed"
                }
            }
            finally {
                Pop-Location
            }
        }
        else {
            throw "Squirrel.Windows tools not found and cannot auto-download. Please install manually: choco install squirrel-windows"
        }
    }
    
    Write-Host ""

    # Create NuSpec file
    Write-Step "Creating NuSpec file..."
    
    $nuspecPath = "$ProjectName.nuspec"
    $nuspecContent = @"
<?xml version="1.0" encoding="utf-8"?>
<package xmlns="http://schemas.microsoft.com/packaging/2010/07/nuspec.xsd">
  <metadata>
    <id>$AppId</id>
    <version>$Version</version>
    <title>$ProjectName</title>
    <authors>$CompanyName</authors>
    <owners>$CompanyName</owners>
    <description>$Description</description>
    <copyright>$Copyright</copyright>
    <requireLicenseAcceptance>false</requireLicenseAcceptance>
    <tags>game-development attribute-editor database</tags>
  </metadata>
  <files>
    <file src="$PublishPath\**" target="lib\net45\" />
  </files>
</package>
"@
    
    $nuspecContent | Out-File -FilePath $nuspecPath -Encoding UTF8
    Write-Success "NuSpec file created: $nuspecPath"
    Write-Host ""

    # Create NuGet package
    Write-Step "Creating NuGet package..."
    
    if (Get-Command "nuget" -ErrorAction SilentlyContinue) {
        nuget pack $nuspecPath -OutputDirectory $OutputPath
    }
    else {
        # If no nuget, try alternative method
        Write-Warning "nuget.exe not found, trying alternative method..."
        
        # Manual nupkg creation (simplified)
        $nupkgName = "$AppId.$Version.nupkg"
        $tempDir = "temp_nupkg"
        
        if (Test-Path $tempDir) {
            Remove-Item $tempDir -Recurse -Force
        }
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
        New-Item -ItemType Directory -Path "$tempDir\lib\net45" -Force | Out-Null
        
        # Copy files
        Copy-Item "$PublishPath\*" "$tempDir\lib\net45\" -Recurse -Force
        Copy-Item $nuspecPath "$tempDir\" -Force
        
        # Create ZIP file (rename to .nupkg)
        $zipPath = "$OutputPath\$nupkgName"
        Compress-Archive -Path "$tempDir\*" -DestinationPath $zipPath -Force
        
        # Clean temp directory
        Remove-Item $tempDir -Recurse -Force
    }
    
    $nupkgFile = Get-ChildItem -Path $OutputPath -Filter "*.nupkg" | Select-Object -First 1
    if (!$nupkgFile) {
        throw "NuGet package creation failed"
    }
    
    Write-Success "NuGet package created: $($nupkgFile.Name)"
    Write-Host ""

    # Use Squirrel to create installer
    Write-Step "Creating Squirrel installer..."
    
    # Wait a moment to ensure file handles are released
    Start-Sleep -Seconds 2
    
    # Create a temporary directory to avoid file locking issues
    $tempDir = "temp_squirrel_$(Get-Random)"
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    
    try {
        # Copy nupkg to temporary directory
        $tempNupkg = Join-Path $tempDir $nupkgFile.Name
        Copy-Item $nupkgFile.FullName $tempNupkg -Force
        
        # Create a separate output directory for Squirrel
        $squirrelOutputDir = "squirrel_output_$(Get-Random)"
        New-Item -ItemType Directory -Path $squirrelOutputDir -Force | Out-Null
        
        $squirrelArgs = @(
            "--releasify", $tempNupkg,
            "--releaseDir", (Resolve-Path $squirrelOutputDir).Path
        )
        
        if ($IconPath) {
            $squirrelArgs += "--setupIcon", (Resolve-Path $IconPath).Path
            $squirrelArgs += "--icon", (Resolve-Path $IconPath).Path
        }
        
        Write-Host "Executing: $squirrelExe $($squirrelArgs -join ' ')" -ForegroundColor Gray
        
        & $squirrelExe $squirrelArgs
        
        if ($LASTEXITCODE -ne 0) {
            throw "Squirrel packaging failed with exit code: $LASTEXITCODE"
        }
        
        # Copy generated files to final output directory
        if (Test-Path $squirrelOutputDir) {
            Copy-Item "$squirrelOutputDir\*" $OutputPath -Force -ErrorAction SilentlyContinue
        }
        
        # Clean up temporary directories
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item $squirrelOutputDir -Recurse -Force -ErrorAction SilentlyContinue
    }
    catch {
        # Clean up on error
        Remove-Item $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        Remove-Item $squirrelOutputDir -Recurse -Force -ErrorAction SilentlyContinue
        throw
    }
    
    Write-Success "Squirrel installer created"
    
    # Generate MSI package manually if WiX files exist
    Write-Step "Generating MSI package..."
    
    $wxsFile = Join-Path $OutputPath "Setup.wxs"
    $wixobjFile = Join-Path $OutputPath "Setup.wixobj"
    $msiFile = Join-Path $OutputPath "Setup.msi"
    
    if (Test-Path $wxsFile) {
        try {
            # Compile WXS to WIXOBJ
             $candleExe = "tools\squirrel.windows\tools\candle.exe"
             if (Test-Path $candleExe) {
                 & $candleExe -out $wixobjFile $wxsFile
                 
                 if ($LASTEXITCODE -eq 0 -and (Test-Path $wixobjFile)) {
                     # Link WIXOBJ to MSI with WiX NetFx extension
                     $lightExe = "tools\squirrel.windows\tools\light.exe"
                     & $lightExe -ext WixNetFxExtension -out $msiFile $wixobjFile
                     
                     if ($LASTEXITCODE -eq 0 -and (Test-Path $msiFile)) {
                         Write-Success "MSI package created: Setup.msi"
                     }
                     else {
                         Write-Warning "MSI linking failed (this is normal for some configurations)"
                     }
                 }
                 else {
                     Write-Warning "WXS compilation failed"
                 }
             }
             else {
                 Write-Warning "WiX tools not found, MSI generation skipped"
             }
        }
        catch {
            Write-Warning "MSI generation failed: $($_.Exception.Message)"
        }
    }
    else {
        Write-Warning "WXS file not found, MSI generation skipped"
    }
    
    Write-Host ""

    # Clean temporary files
    Write-Step "Cleaning temporary files..."
    
    if (Test-Path $nuspecPath) {
        Remove-Item $nuspecPath -Force
    }
    
    Write-Success "Temporary files cleaned"
    Write-Host ""

    # Show results
    Write-Success "Squirrel packaging completed!"
    Write-Host ""
    Write-Host "Packaging Summary:" -ForegroundColor Green
    Write-Host "  Application: $ProjectName" -ForegroundColor Gray
    Write-Host "  Version: $Version" -ForegroundColor Gray
    Write-Host "  Output path: $(Resolve-Path $OutputPath)" -ForegroundColor Gray
    
    # Show generated files
    $outputFiles = Get-ChildItem -Path $OutputPath -File | Sort-Object Name
    if ($outputFiles.Count -gt 0) {
        Write-Host "  Generated files:" -ForegroundColor Gray
        foreach ($file in $outputFiles) {
            $size = [math]::Round($file.Length / 1MB, 2)
            $description = switch ($file.Extension) {
                ".exe" { "Installer" }
                ".nupkg" { "Update Package" }
                default { "Other File" }
            }
            Write-Host "    $($file.Name) ($size MB) - $description" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "Installer package created successfully!" -ForegroundColor Green
    Write-Host "Users can run Setup.exe to install the application" -ForegroundColor Yellow
    
}
catch {
    Write-Error "Squirrel packaging failed: $($_.Exception.Message)"
    Write-Host ""
    Write-Host "Troubleshooting Tips:" -ForegroundColor Yellow
    Write-Host "  - Ensure application is published to $PublishPath directory"
    Write-Host "  - Check if Squirrel.Windows is properly installed"
    Write-Host "  - Try manual installation: choco install squirrel-windows"
    Write-Host "  - Ensure all file paths are correct and accessible"
    exit 1
}
finally {
    # Clean any remaining temporary files
    $tempFiles = @("$ProjectName.nuspec", "temp_nupkg", "*_temp.nupkg")
    foreach ($tempFile in $tempFiles) {
        if ($tempFile.Contains("*")) {
            # Handle wildcard patterns
            Get-ChildItem -Path . -Filter $tempFile -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
        }
        elseif (Test-Path $tempFile) {
            Remove-Item $tempFile -Recurse -Force -ErrorAction SilentlyContinue
        }
    }
}