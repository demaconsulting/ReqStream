# build.ps1
#
# Builds the ReqStream solution and runs all unit tests.

$buildError = $false

Write-Host "Restoring dependencies..."
dotnet restore
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Building..."
dotnet build --no-restore --configuration Release
if ($LASTEXITCODE -ne 0) { $buildError = $true }

Write-Host "Running tests..."
dotnet test --no-build --configuration Release --report-trx --results-directory artifacts/tests
if ($LASTEXITCODE -ne 0) { $buildError = $true }

# [PROJECT-SPECIFIC] Add additional build steps here (e.g., packaging, publishing).

exit ($buildError ? 1 : 0)
