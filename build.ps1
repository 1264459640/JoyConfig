# JoyConfig Project Build Script
# For local build, publish and package JoyConfig application

param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [switch]$SkipTests,
    [switch]$SkipPublish,
    [switch]$SkipSquirrel,
    [switch]$Clean,
    [string]$OutputPath = "publish",
    [string]$SquirrelOutputPath = "Releases",
    [switch]$Help
)

# Set console encoding to UTF-8
[Console]::OutputEncoding = [System.Text.Encoding]::UTF8

# Show help information
if ($Help) {
    Write-Host "JoyConfig Build Script Usage:" -ForegroundColor Green
    Write-Host ""
    Write-Host "Parameters:"
    Write-Host "  -Configuration    Build configuration (default: Release)"
    Write-Host "  -Runtime          Target runtime (default: win-x64)"
    Write-Host "  -SkipTests        Skip unit tests"
    Write-Host "  -SkipPublish      Skip publish step"
    Write-Host "  -SkipSquirrel     Skip Squirrel packaging"
    Write-Host "  -Clean            Clean output directories"
    Write-Host "  -OutputPath       Publish output path (default: publish)"
    Write-Host "  -SquirrelOutputPath Squirrel output path (default: Releases)"
    Write-Host "  -Help             Show this help information"
    Write-Host ""
    Write-Host "Examples:"
    Write-Host "  .\build.ps1                    # Full build and package"
    Write-Host "  .\build.ps1 -SkipTests         # Skip tests"
    Write-Host "  .\build.ps1 -SkipSquirrel      # Build only, no packaging"
    Write-Host "  .\build.ps1 -Clean             # Clean and rebuild"
    exit 0
}

# Project configuration
$ProjectName = "JoyConfig"
$SolutionFile = "JoyConfig.sln"
$ProjectFile = "JoyConfig.csproj"
$TargetFramework = "net9.0"

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
    Write-Step "Starting build for $ProjectName project..."
    Write-Host "Configuration: $Configuration" -ForegroundColor Gray
    Write-Host "Runtime: $Runtime" -ForegroundColor Gray
    Write-Host ""

    # Check required tools
    Write-Step "Checking build environment..."
    
    if (!(Get-Command "dotnet" -ErrorAction SilentlyContinue)) {
        throw ".NET SDK is not installed or not in PATH"
    }
    
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK version: $dotnetVersion" -ForegroundColor Green
    
    # Check project files
    if (!(Test-Path $SolutionFile)) {
        throw "Solution file $SolutionFile does not exist"
    }
    
    if (!(Test-Path $ProjectFile)) {
        throw "Project file $ProjectFile does not exist"
    }
    
    Write-Success "Build environment check completed"
    Write-Host ""

    # Clean output directories
    if ($Clean) {
        Write-Step "Cleaning output directories..."
        
        $dirsToClean = @("bin", "obj", $OutputPath, $SquirrelOutputPath)
        foreach ($dir in $dirsToClean) {
            if (Test-Path $dir) {
                Remove-Item $dir -Recurse -Force
                Write-Host "✓ Cleaned: $dir" -ForegroundColor Green
            }
        }
        
        Write-Success "Cleaning completed"
        Write-Host ""
    }

    # Get version information
    Write-Step "Getting version information..."
    
    $version = "1.0.0"
    $assemblyVersion = "1.0.0.0"
    $informationalVersion = "1.0.0"
    
    if (Get-Command "dotnet-gitversion" -ErrorAction SilentlyContinue) {
        try {
            $gitVersionOutput = dotnet gitversion | ConvertFrom-Json
            $version = $gitVersionOutput.SemVer
            $assemblyVersion = $gitVersionOutput.AssemblySemVer
            $informationalVersion = $gitVersionOutput.InformationalVersion
            Write-Host "✓ Using GitVersion: $version" -ForegroundColor Green
        }
        catch {
            Write-Warning "GitVersion execution failed, using default version: $version"
        }
    }
    else {
        Write-Warning "GitVersion not installed, using default version: $version"
    }
    
    Write-Host "Version: $version" -ForegroundColor Gray
    Write-Host "Assembly version: $assemblyVersion" -ForegroundColor Gray
    Write-Host "Informational version: $informationalVersion" -ForegroundColor Gray
    Write-Host ""

    # Restore dependencies
    Write-Step "Restoring NuGet packages..."
    dotnet restore $SolutionFile
    Write-Success "Dependencies restored"
    Write-Host ""

    # Build project
    Write-Step "Building project..."
    dotnet build $SolutionFile `
        --configuration $Configuration `
        --no-restore `
        -p:Version=$version `
        -p:AssemblyVersion=$assemblyVersion `
        -p:FileVersion=$assemblyVersion `
        -p:InformationalVersion=$informationalVersion
    
    Write-Success "Project build completed"
    Write-Host ""

    # Run tests
    if (!$SkipTests) {
        Write-Step "Running unit tests..."
        
        # Check for test projects
        $testProjects = Get-ChildItem -Path . -Filter "*.Tests.csproj" -Recurse
        if ($testProjects.Count -gt 0) {
            dotnet test $SolutionFile --configuration $Configuration --no-build --verbosity normal
            Write-Success "Unit tests completed"
        }
        else {
            Write-Warning "No test projects found, skipping tests"
        }
        Write-Host ""
    }
    else {
        Write-Warning "Skipping unit tests"
        Write-Host ""
    }

    # Publish application
    if (!$SkipPublish) {
        Write-Step "Publishing application..."
        
        # Ensure output directory exists
        if (!(Test-Path $OutputPath)) {
            New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null
        }
        
        dotnet publish $ProjectFile `
            --configuration $Configuration `
            --framework $TargetFramework `
            --runtime $Runtime `
            --self-contained true `
            --output $OutputPath `
            -p:PublishSingleFile=true `
            -p:PublishReadyToRun=true `
            -p:Version=$version
        
        Write-Success "Application published"
        Write-Host "Publish path: $(Resolve-Path $OutputPath)" -ForegroundColor Gray
        Write-Host ""
    }
    else {
        Write-Warning "Skipping application publish"
        Write-Host ""
    }

    # Squirrel packaging
    if (!$SkipSquirrel -and !$SkipPublish) {
        Write-Step "Creating Squirrel installer..."
        
        if (Test-Path ".\build-squirrel.ps1") {
            & ".\build-squirrel.ps1" -Version $version -Configuration $Configuration -OutputPath $SquirrelOutputPath
            Write-Success "Squirrel packaging completed"
            Write-Host "Installer path: $(Resolve-Path $SquirrelOutputPath)" -ForegroundColor Gray
        }
        else {
            Write-Warning "build-squirrel.ps1 script not found, skipping Squirrel packaging"
        }
        Write-Host ""
    }
    else {
        Write-Warning "Skipping Squirrel packaging"
        Write-Host ""
    }

    # Show build results
    Write-Success "Build completed!"
    Write-Host ""
    Write-Host "Build Summary:" -ForegroundColor Green
    Write-Host "  Project: $ProjectName" -ForegroundColor Gray
    Write-Host "  Version: $version" -ForegroundColor Gray
    Write-Host "  Configuration: $Configuration" -ForegroundColor Gray
    Write-Host "  Runtime: $Runtime" -ForegroundColor Gray
    
    if (!$SkipPublish) {
        Write-Host "  Publish path: $(Resolve-Path $OutputPath)" -ForegroundColor Gray
        
        # Show published files
        $publishedFiles = Get-ChildItem -Path $OutputPath -File | Select-Object -First 5
        if ($publishedFiles.Count -gt 0) {
            Write-Host "  Main files:" -ForegroundColor Gray
            foreach ($file in $publishedFiles) {
                $size = [math]::Round($file.Length / 1MB, 2)
                Write-Host "    $($file.Name) ($size MB)" -ForegroundColor Gray
            }
        }
    }
    
    if (!$SkipSquirrel -and !$SkipPublish -and (Test-Path $SquirrelOutputPath)) {
        Write-Host "  Installer path: $(Resolve-Path $SquirrelOutputPath)" -ForegroundColor Gray
        
        # Show installer files
        $setupFile = Get-ChildItem -Path $SquirrelOutputPath -Filter "Setup.exe" -ErrorAction SilentlyContinue
        if ($setupFile) {
            $size = [math]::Round($setupFile.Length / 1MB, 2)
            Write-Host "    Setup.exe ($size MB)" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "Build completed successfully!" -ForegroundColor Green
    
}
catch {
    Write-Error "Build failed: $($_.Exception.Message)"
    Write-Host ""
    Write-Host "Tips:" -ForegroundColor Yellow
    Write-Host "  - Ensure .NET 9.0 SDK is installed"
    Write-Host "  - Check project files for errors"
    Write-Host "  - Try using -Clean parameter to clean and rebuild"
    Write-Host "  - Use -Help parameter for detailed usage"
    exit 1
}