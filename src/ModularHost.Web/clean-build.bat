@echo off
echo Cleaning up MRCMS project processes...

REM Kill all MRCMS.exe processes
echo Killing MRCMS.exe processes...
taskkill /F /IM MRCMS.exe 2>nul
if %errorlevel%==0 (
    echo MRCMS.exe processes killed successfully.
) else (
    echo No MRCMS.exe processes found or already terminated.
)

REM Kill all dotnet.exe processes (be careful, this kills ALL dotnet processes)
echo Killing dotnet.exe processes...
taskkill /F /IM dotnet.exe 2>nul
if %errorlevel%==0 (
    echo dotnet.exe processes killed successfully.
) else (
    echo No dotnet.exe processes found or already terminated.
)

REM Wait for processes to fully terminate
echo Waiting for processes to terminate...
timeout /t 3 /nobreak >nul

REM Clean the build directories
echo.
echo Cleaning build directories...
if exist "bin" (
    rmdir /s /q bin 2>nul
    echo Bin folder cleaned.
)

if exist "obj" (
    rmdir /s /q obj 2>nul
    echo Obj folder cleaned.
)

REM Build the project
echo.
echo Building the project...
dotnet build

if %errorlevel%==0 (
    echo.
    echo ========================================
    echo Build completed successfully!
    echo ========================================
) else (
    echo.
    echo ========================================
    echo Build failed with exit code: %errorlevel%
    echo ========================================
)

pause