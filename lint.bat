@echo off
REM Run all linters on the ReqStream project

echo === ReqStream Lint Script ===
echo.

REM Code format check
echo Checking code format...
dotnet format --verify-no-changes
if %errorlevel% neq 0 (
    echo X Code format check failed - run 'dotnet format' to fix
    exit /b 1
)
echo + Code format check passed
echo.

REM Markdown lint
echo Checking markdown...
where markdownlint-cli2 >nul 2>nul
if %errorlevel% equ 0 (
    markdownlint-cli2 "**/*.md"
    if %errorlevel% neq 0 (
        echo X Markdown lint failed
        exit /b 1
    )
    echo + Markdown lint passed
) else (
    echo ! markdownlint-cli2 not found - skipping markdown lint
    echo   Install: npm install -g markdownlint-cli2
)
echo.

REM Spell check
echo Checking spelling...
where cspell >nul 2>nul
if %errorlevel% equ 0 (
    cspell --no-progress "**/*.md" "**/*.cs"
    if %errorlevel% neq 0 (
        echo X Spell check failed
        exit /b 1
    )
    echo + Spell check passed
) else (
    echo ! cspell not found - skipping spell check
    echo   Install: npm install -g cspell
)
echo.

REM YAML lint
echo Checking YAML...
where yamllint >nul 2>nul
if %errorlevel% equ 0 (
    yamllint .
    if %errorlevel% neq 0 (
        echo X YAML lint failed
        exit /b 1
    )
    echo + YAML lint passed
) else (
    echo ! yamllint not found - skipping YAML lint
    echo   Install: pip install yamllint
)
echo.

echo === Lint Complete ===
