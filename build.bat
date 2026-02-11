@echo off
REM Build, test, and package the ReqStream project

echo === ReqStream Build Script ===
echo.

REM Restore dotnet tools
echo Restoring dotnet tools...
dotnet tool restore
if %errorlevel% neq 0 (
    echo X Failed to restore dotnet tools
    exit /b 1
)
echo + Dotnet tools restored
echo.

REM Restore dependencies
echo Restoring dependencies...
dotnet restore
if %errorlevel% neq 0 (
    echo X Failed to restore dependencies
    exit /b 1
)
echo + Dependencies restored
echo.

REM Build
echo Building...
dotnet build --no-restore --configuration Release
if %errorlevel% neq 0 (
    echo X Build failed
    exit /b 1
)
echo + Build succeeded
echo.

REM Test
echo Running tests...
dotnet test --no-build --configuration Release --verbosity normal
if %errorlevel% neq 0 (
    echo X Tests failed
    exit /b 1
)
echo + Tests passed
echo.

REM Package
echo Packaging...
dotnet pack --no-build --configuration Release
if %errorlevel% neq 0 (
    echo X Packaging failed
    exit /b 1
)
echo + Packaging succeeded
echo.

echo === Build Complete ===
