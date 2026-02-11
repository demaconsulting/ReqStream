#!/bin/bash
# Build, test, and package the ReqStream project

set -e

echo "=== ReqStream Build Script ==="
echo ""

# Restore dotnet tools
echo "Restoring dotnet tools..."
dotnet tool restore || { echo "✗ Failed to restore dotnet tools"; exit 1; }
echo "✓ Dotnet tools restored"
echo ""

# Restore dependencies
echo "Restoring dependencies..."
dotnet restore || { echo "✗ Failed to restore dependencies"; exit 1; }
echo "✓ Dependencies restored"
echo ""

# Build
echo "Building..."
dotnet build --no-restore --configuration Release || { echo "✗ Build failed"; exit 1; }
echo "✓ Build succeeded"
echo ""

# Test
echo "Running tests..."
dotnet test --no-build --configuration Release --verbosity normal || { echo "✗ Tests failed"; exit 1; }
echo "✓ Tests passed"
echo ""

# Package
echo "Packaging..."
dotnet pack --no-build --configuration Release || { echo "✗ Packaging failed"; exit 1; }
echo "✓ Packaging succeeded"
echo ""

echo "=== Build Complete ==="
