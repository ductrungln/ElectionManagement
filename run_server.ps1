#!/usr/bin/env pwsh

Write-Host "Starting Election Management API Server..." -ForegroundColor Cyan

# Set EPPlus License environment variables
$env:EPPlus_LicenseContext = "NonCommercial"
$env:EPPlus_License = "EPPlus-NonCommercial"

# Navigate to project
cd "e:\Test\ElectionManagement"

# Run the application
Write-Host "Running: dotnet run --configuration Debug --no-build" -ForegroundColor Yellow
dotnet run --configuration Debug --no-build 2>&1
