# run-integration-tests.ps1

Write-Host "Starting Varnex Auth Service Integration Tests..." -ForegroundColor Cyan

# Ensure we are in the root directory
$rootPath = Get-Location
if (-not (Test-Path "$rootPath\Varnex.AuthService.sln")) {
    Write-Error "Please run this script from the root of the repository."
    exit 1
}

# 1. Clean and Build
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build Varnex.AuthService.sln -c Debug
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed."
    exit $LASTEXITCODE
}

# 2. Run Unit Tests (Optional but recommended)
Write-Host "Running Unit Tests..." -ForegroundColor Yellow
dotnet test --filter "Category!=Integration" --no-build
if ($LASTEXITCODE -ne 0) {
    Write-Host "Unit tests failed. Proceeding with integration tests anyway..." -ForegroundColor Magenta
}

# 3. Run Integration Tests
Write-Host "Running Integration Tests..." -ForegroundColor Yellow
# We filter for the integration test project
dotnet test test/Varnex.AuthService.Integration.Test/Varnex.AuthService.Integration.Test.csproj --no-build --logger:"trx;LogFileName=integration-results.trx"

if ($LASTEXITCODE -eq 0) {
    Write-Host "Integration tests completed successfully!" -ForegroundColor Green
} else {
    Write-Host "Integration tests failed." -ForegroundColor Red
}

exit $LASTEXITCODE


