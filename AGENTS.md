# GitHub Copilot Agents

This document provides comprehensive guidance for GitHub Copilot agents working on the ReqStream project.

## Overview

GitHub Copilot agents are AI-powered assistants that help with various development tasks. This document will be
updated as agents are configured for this repository.

## Project Overview

ReqStream is a .NET command-line tool for managing requirements written in YAML files. It provides functionality to
create, validate, and manage requirement documents in a structured and maintainable way.

### Technology Stack

- **Language**: C# 12
- **Framework**: .NET 8.0, 9.0, and 10.0
- **Testing Framework**: MSTest
- **Build System**: dotnet CLI
- **Package Manager**: NuGet

## Project Structure

```text
ReqStream/
├── .config/                      # Dotnet tools configuration
│   └── dotnet-tools.json         # Local tool manifest (SPDX Tool)
├── .github/                      # GitHub Actions workflows
│   └── workflows/
│       ├── build.yaml            # Reusable build workflow
│       └── build_on_push.yaml    # Main CI/CD pipeline
├── src/                          # Source code
│   └── DemaConsulting.ReqStream/ # Main application project
├── test/                         # Test projects
│   └── DemaConsulting.ReqStream.Tests/ # Test project
├── .cspell.json                  # Spell checking configuration
├── .editorconfig                 # Code style configuration
├── .markdownlint.json            # Markdown linting rules
├── .yamllint.yaml                # YAML linting rules
├── AGENTS.md                     # This file
├── LICENSE                       # MIT License
└── README.md                     # Project documentation
```

### Critical Files

- **`.editorconfig`**: Defines code style rules, naming conventions, and formatting standards
- **`.cspell.json`**: Contains spell-checking configuration and custom dictionary
- **`.markdownlint.json`**: Markdown linting rules
- **`.yamllint.yaml`**: YAML linting rules
- **`DemaConsulting.ReqStream.sln`**: Solution file containing all projects

## Testing Guidelines

### Test Structure

- **Test Framework**: MSTest (Microsoft.VisualStudio.TestTools.UnitTesting)
- **Test File Naming**: All test source files should follow the pattern `[Component]Tests.cs` (e.g., `BasicTests.cs`)
- **Test Class Naming**: Use descriptive names ending with `Tests` (e.g., `BasicTests`, `CommandTests`)
- **Test Method Pattern**: Use the AAA (Arrange, Act, Assert) pattern
- **Method Naming Convention**: Use `TestMethod_Scenario_ExpectedBehavior` format
  - Examples:
    - `Parse_ValidYaml_ReturnsDocument()`
    - `Validate_MissingRequiredField_ThrowsException()`
    - `Execute_WithHelpCommand_DisplaysHelpText()`

### Test Conventions

- **Test Coverage**: All new features must have corresponding tests
- **Code Coverage**: Strive for high code coverage (aim for >80%)
- **Test Paths**: Test both success and failure paths
- **Test Independence**: Tests should be isolated and not depend on execution order
- **Test Data**: Use descriptive test data that makes the test intent clear
- **Assertions**: Use clear, specific assertions with meaningful messages

### Test Requirements

- **All tests must pass** before merging any changes
- **No warnings allowed** in test builds
- **Tests should be isolated**: Each test should set up its own state and clean up after itself
- **Tests should be deterministic**: Tests should produce the same result every time they run
- **Tests should be fast**: Unit tests should run quickly to enable rapid feedback

### Running Tests

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal
```

## Code Style and Conventions

### Naming Conventions

Based on `.editorconfig` settings:

- **Interfaces**: Must begin with `I` (e.g., `IRequirementParser`)
- **Classes, Structs, Enums**: PascalCase (e.g., `RequirementDocument`)
- **Methods**: PascalCase (e.g., `ParseDocument`)
- **Properties**: PascalCase (e.g., `DocumentName`)
- **Parameters**: camelCase (e.g., `fileName`)
- **Local Variables**: camelCase (e.g., `documentPath`)

### Code Organization

- **Namespace Declarations**: Use file-scoped namespaces (C# 10+)
- **Using Directives**: Sort system directives first, no separation between groups
- **Braces**: Required for all control statements (enforced as warning)
- **Indentation**: 4 spaces for C#, 2 spaces for YAML/JSON/XML
- **Encoding**: UTF-8 with BOM
- **Line Endings**: Consistent (insert final newline, trim trailing whitespace)

### Best Practices

- **Nullable Reference Types**: Enabled by default - use nullable annotations appropriately
- **Implicit Usings**: Enabled - common namespaces are automatically imported
- **Expression-Bodied Members**: Use for properties, indexers, accessors, and lambdas; avoid for methods,
  constructors, and operators
- **Simple Using Statements**: Prefer simplified using syntax where appropriate
- **Code Quality**: Unused parameters trigger warnings
- **Documentation**: All public APIs should have XML documentation comments

### Code Quality

- **Copyright Headers**: All source files must include the MIT license header
- **XML Documentation**: Use triple-slash comments (`///`) for all members and classes (public, internal, and private)
- **Error Handling**: Use appropriate exception types and provide meaningful error messages
- **Resource Management**: Use `using` statements or `IDisposable` pattern for resource cleanup

## Quality Standards

### Static Analysis

The project uses built-in .NET analyzers configured in `.editorconfig`:

- **Code Style**: Enforces C# coding conventions
- **Naming Rules**: Enforces consistent naming patterns
- **Code Quality**: Detects unused parameters and code quality issues
- **Nullable Analysis**: Ensures proper nullable reference type usage

### Documentation

- **README.md**: Keep the main README up to date with features and usage
- **Code Comments**: Use XML documentation for public APIs
- **Inline Comments**: Use sparingly and only when necessary to explain complex logic
- **Commit Messages**: Write clear, descriptive commit messages

### Spelling and Markdown

- **Spell Checking**: Run cspell on all markdown and C# files
  - Custom dictionary maintained in `.cspell.json`
  - Add project-specific terms to the dictionary as needed
- **Markdown Linting**: All markdown files must pass markdownlint
  - Configuration in `.markdownlint.json`
  - Maximum line length: 120 characters
  - ATX-style headers required
- **YAML Linting**: All YAML files must pass yamllint
  - Configuration in `.yamllint.yaml`
  - Maximum line length: 120 characters
  - 2-space indentation

## CI/CD Pipelines

### Workflows

The project uses GitHub Actions for CI/CD:

1. **Build on Push** (`.github/workflows/build_on_push.yaml`)
   - Triggers: Push, manual dispatch, weekly schedule (Monday 5PM UTC)
   - Steps:
     - Quality checks (markdown lint, spell check, YAML lint)
     - Build on Windows (windows-latest)
     - Build on Linux (ubuntu-latest)

2. **Build** (`.github/workflows/build.yaml`)
   - Reusable workflow called by other workflows
   - Steps:
     - Checkout repository
     - Setup .NET (8.x, 9.x, 10.x)
     - Restore dotnet tools
     - Restore dependencies
     - Build (Release configuration)
     - Test (with normal verbosity)
     - Package (create NuGet packages)
     - Upload artifacts

### Build Commands

```bash
# Restore tools
dotnet tool restore

# Restore dependencies
dotnet restore

# Build
dotnet build --no-restore --configuration Release

# Test
dotnet test --no-build --configuration Release --verbosity normal

# Package
dotnet pack --no-build --configuration Release
```

## Pre-Finalization Quality Checks

Before completing any task, agents must perform these quality checks in order:

### 1. Build and Test Validation

```bash
# Restore and build
dotnet restore
dotnet build --configuration Release

# Run all tests
dotnet test --configuration Release --verbosity normal
```

- All tests must pass
- No build warnings or errors
- All target frameworks (net8.0, net9.0, net10.0) must build successfully

### 2. Code Review

- Use the `code_review` tool to get automated feedback
- Address all valid concerns raised by the review
- Ensure code follows established patterns and conventions
- Verify that changes are minimal and focused

### 3. Security Scanning

- Use the `codeql_checker` tool after code review
- Investigate all security alerts
- Fix any vulnerabilities related to your changes
- Document any unfixable issues in a Security Summary

### 4. Linting and Format Checks

```bash
# These are run automatically in CI, but you can check locally:

# Markdown linting (if markdownlint-cli2 is available)
# markdownlint-cli2 "**/*.md"

# Spell checking (if cspell is available)
# cspell "**/*.md" "**/*.cs"

# YAML linting (if yamllint is available)
# yamllint .
```

### 5. Final Verification

- Review all changed files
- Ensure no unintended changes were included
- Verify `.gitignore` excludes build artifacts (`bin/`, `obj/`, etc.)
- Confirm that commit messages are clear and descriptive
- Validate that documentation is updated if needed

## Boundaries and Guardrails

### What AI Agents Should NEVER Do

- **Delete or modify working code** unless absolutely necessary or fixing a security vulnerability
- **Remove or modify existing tests** unless they are directly related to changes being made
- **Change unrelated code** or fix unrelated bugs/issues
- **Commit build artifacts** (`bin/`, `obj/`, `node_modules/`, etc.)
- **Add unnecessary dependencies** or update library versions without explicit need
- **Modify CI/CD workflows** without understanding the full impact
- **Change licensing headers** or license files
- **Introduce breaking changes** without discussion
- **Add comments unnecessarily** unless they match existing style or explain complex logic
- **Use force push** (`git reset`, `git rebase`) as they are not supported
- **Create temporary files in the repository** - use `/tmp` instead
- **Violate security best practices** or introduce vulnerabilities
- **Make changes to files in `.github/agents/`** - these are for other agents

### What AI Agents Should ALWAYS Do

- **Make minimal changes** - only modify what's necessary to address the issue
- **Follow existing patterns** - match the style and structure of existing code
- **Run tests before and after changes** - understand baseline and verify no regressions
- **Use the `report_progress` tool frequently** - commit changes incrementally
- **Validate changes work** - test the actual behavior, not just that it compiles
- **Check for security vulnerabilities** - use `codeql_checker` before finalizing
- **Request code review** - use `code_review` tool before completing tasks
- **Update documentation** if changes affect usage or behavior
- **Add tests for new features** or bug fixes
- **Fix issues found by linters** - ensure code passes all quality checks
- **Use ecosystem tools** (e.g., `dotnet` CLI) rather than manual file editing
- **Store important codebase facts** - use `store_memory` for conventions learned
- **Review files committed** - ensure scope is minimal and expected

### What AI Agents Should ASK About

- **Breaking changes** - if a change might break existing functionality
- **Architecture decisions** - if unsure about the best approach
- **Unclear requirements** - if the issue description is ambiguous
- **Missing context** - if critical information is not available
- **Scope concerns** - if the requested change seems too large
- **Test coverage decisions** - if unsure what level of testing is appropriate
- **Third-party dependencies** - if adding new libraries or tools
- **Configuration changes** - if modifying build, CI/CD, or tool configurations
- **Design patterns** - if multiple valid approaches exist
- **Compatibility concerns** - if changes might affect multi-platform support

## Available Agents

Currently, no custom agents are configured for this repository. The default GitHub Copilot agent is available for
general assistance.

## Future Agents

As the project grows, we may add custom agents for:

- Code review and quality assurance
- Documentation generation
- Test generation
- Requirements validation
- YAML file parsing and validation

## Contributing

If you have suggestions for custom agents that would benefit this project, please open an issue or submit a pull
request.
