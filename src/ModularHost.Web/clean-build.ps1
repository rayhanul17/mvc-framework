# PowerShell script to clean up processes and build the project
Write-Host "Cleaning up MRCMS project processes..." -ForegroundColor Yellow

# Kill all MRCMS.exe processes
$mrcmsProcesses = Get-Process -Name "MRCMS" -ErrorAction SilentlyContinue
if ($mrcmsProcesses) {
    Write-Host "Found $($mrcmsProcesses.Count) MRCMS processes. Killing..." -ForegroundColor Cyan
    $mrcmsProcesses | Stop-Process -Force
    Write-Host "MRCMS processes killed." -ForegroundColor Green
} else {
    Write-Host "No MRCMS processes found." -ForegroundColor Gray
}

# Kill all dotnet.exe processes that are running our project
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | 
    Where-Object { $_.CommandLine -like "*ModularHost.Web*" -or $_.CommandLine -like "*MRCMS*" }
    
if ($dotnetProcesses) {
    Write-Host "Found $($dotnetProcesses.Count) related dotnet processes. Killing..." -ForegroundColor Cyan
    $dotnetProcesses | Stop-Process -Force
    Write-Host "Dotnet processes killed." -ForegroundColor Green
} else {
    Write-Host "No related dotnet processes found." -ForegroundColor Gray
}

# Kill processes by checking who has the DLL locked
$dllPath = "D:\mvc-framework\src\ModularHost.Web\bin\Debug\net9.0\MRCMS.dll"
$exePath = "D:\mvc-framework\src\ModularHost.Web\bin\Debug\net9.0\MRCMS.exe"

# Function to find and kill processes locking a file
function Kill-LockingProcess {
    param([string]$FilePath)
    
    if (Test-Path $FilePath) {
        try {
            # Use handle.exe or similar if available, otherwise try to find locking processes
            $lockingProcesses = Get-Process | Where-Object {
                try {
                    $modules = $_.Modules | Where-Object { $_.FileName -eq $FilePath }
                    return $modules.Count -gt 0
                } catch {
                    return $false
                }
            }
            
            if ($lockingProcesses) {
                Write-Host "Found processes locking $FilePath. Killing..." -ForegroundColor Yellow
                $lockingProcesses | Stop-Process -Force
            }
        } catch {
            Write-Host "Could not check for locking processes on $FilePath" -ForegroundColor Gray
        }
    }
}

Kill-LockingProcess -FilePath $dllPath
Kill-LockingProcess -FilePath $exePath

# Wait a moment for processes to fully terminate
Start-Sleep -Seconds 2

# Clean the build output
Write-Host "`nCleaning build output..." -ForegroundColor Yellow
if (Test-Path "D:\mvc-framework\src\ModularHost.Web\bin") {
    Remove-Item -Path "D:\mvc-framework\src\ModularHost.Web\bin" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Bin folder cleaned." -ForegroundColor Green
}

if (Test-Path "D:\mvc-framework\src\ModularHost.Web\obj") {
    Remove-Item -Path "D:\mvc-framework\src\ModularHost.Web\obj" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "Obj folder cleaned." -ForegroundColor Green
}

# Now build the project
Write-Host "`nBuilding the project..." -ForegroundColor Yellow
Set-Location "D:\mvc-framework\src\ModularHost.Web"
dotnet build

if ($LASTEXITCODE -eq 0) {
    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
} else {
    Write-Host "`nBuild failed with exit code: $LASTEXITCODE" -ForegroundColor Red
}