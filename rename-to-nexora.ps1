# PowerShell script to rename DynamicRoleMenuSystem to Nexora
# This script will rename all files, folders, and update content

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Project Rename: DynamicRoleMenuSystem -> Nexora" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if git has uncommitted changes
$gitStatus = git status --porcelain
if ($gitStatus) {
    Write-Host "WARNING: You have uncommitted changes!" -ForegroundColor Yellow
    Write-Host "Please commit or stash your changes before proceeding." -ForegroundColor Yellow
    $confirm = Read-Host "Do you want to continue anyway? (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "Rename cancelled." -ForegroundColor Red
        exit
    }
}

Write-Host "This script will:" -ForegroundColor Yellow
Write-Host "1. Rename solution and project files" -ForegroundColor White
Write-Host "2. Rename project folders" -ForegroundColor White
Write-Host "3. Update all namespaces and references" -ForegroundColor White
Write-Host "4. Update configuration files" -ForegroundColor White
Write-Host ""

$confirm = Read-Host "Do you want to proceed? (yes/no)"
if ($confirm -ne "yes") {
    Write-Host "Rename cancelled." -ForegroundColor Red
    exit
}

$rootPath = "D:\mvc-framework"
Set-Location $rootPath

Write-Host ""
Write-Host "Starting rename process..." -ForegroundColor Green
Write-Host ""

# Step 1: Update content in all files BEFORE renaming folders
Write-Host "Step 1: Updating file contents..." -ForegroundColor Cyan

# Get all text files that need content updates
$extensions = @("*.cs", "*.csproj", "*.sln", "*.json", "*.cshtml", "*.css", "*.js", "*.md", "*.txt", "*.config", "*.xml")
$files = @()
foreach ($ext in $extensions) {
    $files += Get-ChildItem -Path $rootPath -Filter $ext -Recurse -File | Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\node_modules\\|\\\.git\\|\\packages\\" }
}

$totalFiles = $files.Count
$currentFile = 0

foreach ($file in $files) {
    $currentFile++
    $percent = [math]::Round(($currentFile / $totalFiles) * 100)
    Write-Progress -Activity "Updating file contents" -Status "$percent% Complete" -PercentComplete $percent
    
    $content = Get-Content $file.FullName -Raw -ErrorAction SilentlyContinue
    if ($content) {
        $newContent = $content -replace "DynamicRoleMenuSystem", "Nexora"
        if ($content -ne $newContent) {
            Set-Content -Path $file.FullName -Value $newContent -NoNewline
        }
    }
}

Write-Progress -Activity "Updating file contents" -Completed
Write-Host "File contents updated successfully!" -ForegroundColor Green
Write-Host ""

# Step 2: Rename solution files
Write-Host "Step 2: Renaming solution files..." -ForegroundColor Cyan

$oldSolutions = @("DynamicRoleMenuSystem.sln", "Mvc.Framework.sln")
foreach ($oldSln in $oldSolutions) {
    if (Test-Path "$rootPath\$oldSln") {
        $newName = $oldSln -replace "DynamicRoleMenuSystem", "Nexora"
        $newName = $newName -replace "Mvc.Framework", "Nexora"
        if ($oldSln -ne $newName) {
            Rename-Item -Path "$rootPath\$oldSln" -NewName $newName -Force
            Write-Host "  Renamed: $oldSln -> $newName" -ForegroundColor Gray
        }
    }
}

# Step 3: Rename project files (.csproj)
Write-Host "Step 3: Renaming project files..." -ForegroundColor Cyan

$projectFiles = Get-ChildItem -Path $rootPath -Filter "DynamicRoleMenuSystem*.csproj" -Recurse
foreach ($projFile in $projectFiles) {
    $newName = $projFile.Name -replace "DynamicRoleMenuSystem", "Nexora"
    if ($projFile.Name -ne $newName) {
        Rename-Item -Path $projFile.FullName -NewName $newName -Force
        Write-Host "  Renamed: $($projFile.Name) -> $newName" -ForegroundColor Gray
    }
}

# Step 4: Rename project folders
Write-Host "Step 4: Renaming project folders..." -ForegroundColor Cyan

# List folders to rename (in order from deepest to shallowest to avoid path issues)
$foldersToRename = @(
    "DynamicRoleMenuSystem.Web",
    "DynamicRoleMenuSystem.Application", 
    "DynamicRoleMenuSystem.Infrastructure",
    "DynamicRoleMenuSystem.Core"
)

foreach ($oldFolder in $foldersToRename) {
    $oldPath = Join-Path $rootPath $oldFolder
    if (Test-Path $oldPath) {
        $newFolder = $oldFolder -replace "DynamicRoleMenuSystem", "Nexora"
        $newPath = Join-Path $rootPath $newFolder
        
        # Close any file handles
        [System.GC]::Collect()
        [System.GC]::WaitForPendingFinalizers()
        
        Start-Sleep -Milliseconds 500
        
        try {
            Rename-Item -Path $oldPath -NewName $newFolder -Force -ErrorAction Stop
            Write-Host "  Renamed: $oldFolder -> $newFolder" -ForegroundColor Gray
        }
        catch {
            Write-Host "  WARNING: Could not rename $oldFolder - $($_.Exception.Message)" -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Rename process completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Open the solution in Visual Studio" -ForegroundColor White
Write-Host "2. Clean and rebuild the solution" -ForegroundColor White
Write-Host "3. Update database connection strings if needed" -ForegroundColor White
Write-Host "4. Run 'dotnet ef database update' if using migrations" -ForegroundColor White
Write-Host ""
Write-Host "The project has been renamed to: Nexora" -ForegroundColor Green