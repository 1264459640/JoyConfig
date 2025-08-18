@echo off
setlocal

echo.
echo ========================================
echo    JoyConfig Project Build Tool
echo ========================================
echo.

if not exist "build.ps1" (
    echo [ERROR] Build script build.ps1 does not exist
    pause
    exit /b 1
)

echo Please select build option:
echo.
echo 1. Full build and package
echo 2. Build only, skip tests
echo 3. Build only, skip Squirrel packaging
echo 4. Clean and rebuild
echo 5. Show help
echo 0. Exit
echo.
set /p choice=Enter option (0-5): 

if "%choice%"=="1" goto full_build
if "%choice%"=="2" goto skip_tests
if "%choice%"=="3" goto skip_squirrel
if "%choice%"=="4" goto clean_build
if "%choice%"=="5" goto show_help
if "%choice%"=="0" goto exit

echo Invalid option
pause
goto exit

:full_build
echo Starting full build...
powershell -ExecutionPolicy Bypass -File "build.ps1"
goto check_result

:skip_tests
echo Starting build (skip tests)...
powershell -ExecutionPolicy Bypass -File "build.ps1" -SkipTests
goto check_result

:skip_squirrel
echo Starting build (skip Squirrel)...
powershell -ExecutionPolicy Bypass -File "build.ps1" -SkipSquirrel
goto check_result

:clean_build
echo Starting clean build...
powershell -ExecutionPolicy Bypass -File "build.ps1" -Clean
goto check_result

:show_help
powershell -ExecutionPolicy Bypass -File "build.ps1" -Help
pause
goto exit

:check_result
if errorlevel 1 (
    echo.
    echo [ERROR] Build failed!
) else (
    echo.
    echo [SUCCESS] Build completed!
)
echo.
pause

:exit
echo Goodbye!