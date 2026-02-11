#!/bin/bash
# Run all linters on the ReqStream project

set -e

echo "=== ReqStream Lint Script ==="
echo ""

# Code format check
echo "Checking code format..."
dotnet format --verify-no-changes || { echo "✗ Code format check failed - run 'dotnet format' to fix"; exit 1; }
echo "✓ Code format check passed"
echo ""

# Markdown lint
echo "Checking markdown..."
if command -v markdownlint-cli2 &> /dev/null; then
    markdownlint-cli2 "**/*.md" || { echo "✗ Markdown lint failed"; exit 1; }
    echo "✓ Markdown lint passed"
else
    echo "⚠ markdownlint-cli2 not found - skipping markdown lint"
    echo "  Install: npm install -g markdownlint-cli2"
fi
echo ""

# Spell check
echo "Checking spelling..."
if command -v cspell &> /dev/null; then
    cspell --no-progress "**/*.md" "**/*.cs" || { echo "✗ Spell check failed"; exit 1; }
    echo "✓ Spell check passed"
else
    echo "⚠ cspell not found - skipping spell check"
    echo "  Install: npm install -g cspell"
fi
echo ""

# YAML lint
echo "Checking YAML..."
if command -v yamllint &> /dev/null; then
    yamllint . || { echo "✗ YAML lint failed"; exit 1; }
    echo "✓ YAML lint passed"
else
    echo "⚠ yamllint not found - skipping YAML lint"
    echo "  Install: pip install yamllint"
fi
echo ""

echo "=== Lint Complete ==="
