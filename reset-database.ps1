# ===================================
# Database Reset Script for PowerShell
# ===================================

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  SoSuSaFsd Database Reset Script" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""

# Navigate to project directory
$projectPath = "C:\Users\louis\source\repos\SoSuSaFsdd\SoSuSaFsd"

Write-Host "Navigating to project directory..." -ForegroundColor Yellow
Set-Location $projectPath

Write-Host "Current directory: $(Get-Location)" -ForegroundColor Green
Write-Host ""

# Step 1: Drop Database
Write-Host "Step 1: Dropping existing database..." -ForegroundColor Yellow
try {
    dotnet ef database drop --force
    Write-Host "? Database dropped successfully!" -ForegroundColor Green
} catch {
    Write-Host "? Warning: Could not drop database (it may not exist yet)" -ForegroundColor Yellow
}
Write-Host ""

# Step 2: Apply Migrations
Write-Host "Step 2: Applying migrations to create fresh database..." -ForegroundColor Yellow
try {
    dotnet ef database update
    Write-Host "? Migrations applied successfully!" -ForegroundColor Green
} catch {
    Write-Host "? Error: Failed to apply migrations" -ForegroundColor Red
    Write-Host "Please check your connection string in appsettings.json" -ForegroundColor Red
    exit 1
}
Write-Host ""

# Step 3: Instructions
Write-Host "======================================" -ForegroundColor Cyan
Write-Host "  Database Reset Complete!" -ForegroundColor Cyan
Write-Host "======================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next Steps:" -ForegroundColor Yellow
Write-Host "1. Run the application with: dotnet run" -ForegroundColor White
Write-Host "2. Watch the console for seeding messages" -ForegroundColor White
Write-Host "3. Login with: john@example.com / User@123" -ForegroundColor White
Write-Host "4. Follow a category from the right sidebar" -ForegroundColor White
Write-Host "5. Go back to Home to see posts!" -ForegroundColor White
Write-Host ""
Write-Host "Do you want to start the application now? (Y/N): " -ForegroundColor Yellow -NoNewline
$response = Read-Host

if ($response -eq "Y" -or $response -eq "y") {
    Write-Host ""
    Write-Host "Starting application..." -ForegroundColor Green
    Write-Host "Watch for 'Database seeding completed successfully' message!" -ForegroundColor Cyan
    Write-Host ""
    dotnet run
} else {
    Write-Host ""
    Write-Host "You can start the application later with: dotnet run" -ForegroundColor White
    Write-Host ""
}
