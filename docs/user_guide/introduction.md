# Introduction

## Purpose

This document serves as the comprehensive user guide for ReqStream, a .NET command-line tool for managing software
requirements. It provides complete instructions for installing, configuring, and using ReqStream to manage
requirements in a structured, version-controllable manner.

## Scope

This introduction covers the following topics:

- What ReqStream is and the problems it solves
- Key features and capabilities
- Primary use cases

This guide does not cover the internal implementation of ReqStream or advanced customization beyond the documented
command-line options and YAML format.

## What is ReqStream

ReqStream is a .NET command-line tool designed to help teams manage software requirements in a structured,
version-controllable, and maintainable way. By using YAML files to define requirements, ReqStream enables requirements
to be treated as code, stored in source control, and integrated into CI/CD pipelines.

## Key Features

- **YAML Format** - Manage requirements in human-readable YAML format that can be easily edited and reviewed
- **Command-Line Interface** - Automate requirement management with CLI tools that integrate with build systems
- **Multi-Platform** - Works on Windows, Linux, and macOS with .NET 8, 9, and 10
- **Hierarchical Structure** - Organize requirements with sections and subsections for better organization
- **Test Mapping** - Link requirements to test cases for traceability and verification
- **Source-Specific Test Matching** - Restrict coverage evidence to named result files using `filepart@testname` syntax
- **Justifications** - Document the rationale behind each requirement for better understanding
- **File Includes** - Modularize requirements across multiple YAML files for better maintainability
- **Linting** - Inspect requirements files for structural issues and reference errors, reporting all problems in one pass
- **Validation** - Run a built-in self-test suite to qualify the tool in its deployment environment
- **Tag Filtering** - Categorize and filter requirements using tags for focused reporting and enforcement
- **Export Capabilities** - Generate markdown reports for requirements, justifications, and test trace matrices
- **Continuous Compliance** - Automatically generate compliance evidence on every CI run, following the
  [Continuous Compliance][continuous-compliance] methodology

## Use Cases

ReqStream is ideal for:

- Software development projects requiring formal requirements documentation
- Teams practicing DevOps and want requirements versioned alongside code
- Projects needing traceability between requirements and test cases
- Organizations requiring compliance documentation
- Agile teams wanting lightweight, maintainable requirements management

# Continuous Compliance

ReqStream is a key component of the [Continuous Compliance][continuous-compliance] methodology by DEMA Consulting,
which ensures compliance evidence is generated automatically on every CI run.

## Key Practices

- **Requirements Traceability**: Every requirement is linked to passing tests, and a trace matrix is
  auto-generated on each CI run
- **Tag-Based Filtering**: Requirements can be tagged to generate focused compliance reports for specific
  categories (e.g., security, regulatory)
- **Enforcement Mode**: CI/CD pipelines fail if requirements lack passing tests, ensuring coverage is
  maintained on every commit
- **Automated Audit Documentation**: Each release ships with generated requirements, justifications, and
  trace matrix documents

# Prerequisites

To use ReqStream, you need:

- **[.NET SDK][dotnet-sdk]** version 8.0, 9.0, or 10.0
- A text editor for creating YAML files (any editor will work, but syntax highlighting for YAML is helpful)
- Basic familiarity with command-line tools
- Understanding of YAML syntax (or willingness to learn from examples)

To verify your .NET installation:

```bash
dotnet --version
```

This should display 8.0.x, 9.0.x, or 10.0.x.

# Installation

ReqStream is distributed as a .NET tool and can be installed globally for system-wide use or locally for specific
projects.

## Global Installation

For individual use or when you want ReqStream available system-wide:

```bash
dotnet tool install -g DemaConsulting.ReqStream
```

Verify the installation:

```bash
reqstream --version
```

## Local Installation

For team projects where you want to ensure everyone uses the same version:

First, create a tool manifest if you don't have one:

```bash
dotnet new tool-manifest
```

Then install ReqStream:

```bash
dotnet tool install DemaConsulting.ReqStream
```

Run the tool:

```bash
dotnet reqstream --version
```

The tool manifest (`.config/dotnet-tools.json`) should be committed to your repository so team members can restore
the exact version:

```bash
dotnet tool restore
```

## Updating

To update to the latest version:

```bash
# For global tools
dotnet tool update -g DemaConsulting.ReqStream

# For local tools
dotnet tool update DemaConsulting.ReqStream
```

# Requirements File Format

ReqStream uses YAML files to define requirements. The format is designed to be human-readable while providing
structure for tooling.

## Basic Structure

A requirements YAML file has a top-level `sections` array:

```yaml
---
sections:
  - title: My Section
    requirements:
      - id: Core-BasicRequirement
        title: My first requirement
```

## Sections and Subsections

Sections provide hierarchical organization. Sections can contain requirements and/or nested subsections:

```yaml
---
sections:
  - title: System Requirements
    requirements:
      - id: System-TopLevel
        title: Top-level system requirement
    
    sections:
      - title: Security
        requirements:
          - id: Security-AuthRequired
            title: Security requirement
      
      - title: Performance
        requirements:
          - id: Performance-ResponseTime
            title: Performance requirement
```

You can nest sections as deeply as needed to organize your requirements logically.

## Requirements

Each requirement must have:

- **id** - A unique identifier (can be any format, but must be unique across all files)
- **title** - A clear description of the requirement

Requirements can optionally include:

- **tests** - Array of test names that verify this requirement
- **children** - Array of requirement IDs that are children of this requirement
- **justification** - Explanation of why the requirement exists (recommended for better understanding)
- **tags** - Array of tag identifiers for categorizing and filtering requirements

Example:

```yaml
requirements:
  - id: Security-CredentialAuthentication
    title: The system shall support credentials authentication.
    justification: |
      Authentication is critical to ensure only authorized users can access the system.
      This requirement establishes the foundation for our security posture.
    tags:
      - security
      - critical
    children:
      - Auth-CredentialValidation
      - Auth-FailedAttemptLogging
```

## Test Mappings

Tests can be mapped to requirements in two ways:

**Inline with requirements:**

```yaml
requirements:
  - id: Auth-CredentialValidation
    title: All requests shall have their credentials authenticated before being processed.
    tests:
      - Credentials_Valid_Allowed
      - Credentials_Invalid_Refused
      - Credentials_Missing_Refused
```

**Separate mappings section:**

```yaml
sections:
  - title: Logging
    requirements:
      - id: Logging-RequestLogging
        title: All requests shall be logged.

mappings:
  - id: Logging-RequestLogging
    tests:
      - Logging_ValidRequest_Logged
      - Logging_InvalidRequest_Logged
```

The separate `mappings` section is useful when test mappings are maintained by a different team or in a different
file from the requirements.

## Test Source Linking

When testing requirements across multiple platforms or configurations, use test source linking to distinguish tests
from different sources. This is particularly useful for matrix testing scenarios.

**Pattern**: `[filepart@]testname`

- `filepart` (optional): A substring matching the base filename (without extension) of the test result file
- `testname`: The exact test name from the test result file

**Example - Platform-specific testing:**

```yaml
requirements:
  - id: Platform-Windows
    title: Shall support Windows operating systems
    tests:
      - "windows-latest@Test_PlatformBasic"
      - "windows-latest@Test_FileSystem"
  
  - id: Platform-Linux
    title: Shall support Linux operating systems
    tests:
      - "ubuntu-latest@Test_PlatformBasic"
      - "ubuntu-latest@Test_FileSystem"
  
  - id: Platform-CrossPlatform
    title: Shall support cross-platform APIs
    tests:
      - Test_CrossPlatformAPI  # Aggregates from all platforms
```

With test result files:

- `test-results-windows-latest.trx`
- `test-results-ubuntu-latest.trx`
- `test-results-macos-latest.trx`

The `windows-latest@Test_PlatformBasic` test will only match results from files containing "windows-latest" in their
base filename. The `Test_CrossPlatformAPI` test without a source specifier will aggregate results from all three
files.

**Key features:**

- Case-insensitive matching: `windows@Test` matches `test-results-WINDOWS-latest.trx`
- Partial matching: `ubuntu@Test` matches `test-results-ubuntu-22.04-latest.trx`
- Plain test names: Tests without `filepart@` prefix aggregate results from all test result files

## Tag Filtering

Requirements can be tagged for categorization and selective filtering. This is useful for organizing requirements by
themes (e.g., security, performance, compliance) and generating focused reports for specific requirement categories.

**Adding tags to requirements:**

```yaml
sections:
  - title: System Requirements
    requirements:
      - id: Security-CredentialAuthentication
        title: The system shall support credentials authentication.
        tags:
          - security
          - critical
      - id: Performance-ResponseTime
        title: The system shall respond within 100ms.
        tags:
          - performance
      - id: Security-DataEncryption
        title: The system shall encrypt data at rest.
        tags:
          - security
          - compliance
```

**Filtering by tags:**

Use the `--filter` option with comma-separated tags to export only requirements that match at least one of the
specified tags:

```bash
# Export only security-tagged requirements
reqstream --requirements "docs/**/*.yaml" \
          --filter security \
          --report security_requirements.md

# Export requirements tagged with either security or compliance
reqstream --requirements "docs/**/*.yaml" \
          --filter security,compliance \
          --report compliance_report.md
```

**Filtering behavior:**

- Requirements match if they have **any** of the filter tags (OR logic)
- Requirements without tags are excluded when filtering is active
- When no filter is specified, all requirements are exported (default behavior)
- Filtering applies to all exports: requirements reports, justifications, and trace matrices
- Summary counts (satisfied/total requirements) reflect only the filtered requirements

**Example use cases:**

1. **Security audits** - Export only security-related requirements for security reviews
2. **Compliance documentation** - Generate compliance-focused documentation for auditors
3. **Critical requirements** - Filter and enforce test coverage for critical requirements only

```bash
# Generate trace matrix for critical requirements only
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --filter critical \
          --matrix critical_trace_matrix.md

# Enforce test coverage for security requirements
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --filter security \
          --enforce
```

## File Includes

Large projects can be split across multiple YAML files using the `includes` section:

```yaml
---
sections:
  - title: Core Requirements
    requirements:
      - id: Core-Functional
        title: Core requirement

includes:
  - security_requirements.yaml
  - performance_requirements.yaml
  - test_mappings.yaml
```

Included files can contain:

- Additional sections and requirements
- Test mappings
- Additional includes (includes can be nested)

File paths are relative to the including file.

## Section Merging

When multiple files define sections with the same full hierarchy path, ReqStream automatically merges them. This
allows included files to add requirements to existing sections.

**main_requirements.yaml:**

```yaml
---
sections:
  - title: System Requirements
    sections:
      - title: Security
        requirements:
          - id: Security-AuthRequired
            title: Authentication required
```

**additional_requirements.yaml:**

```yaml
---
sections:
  - title: System Requirements
    sections:
      - title: Security
        requirements:
          - id: Security-AuthorizationRequired
            title: Authorization required
```

When both files are loaded, the "Security" section will contain both Security-AuthRequired and
Security-AuthorizationRequired.

## Complete Example

Here's a comprehensive example showing all features:

```yaml
---
# Main requirements file

sections:
  - title: System Security
    requirements:
      - id: Security-CredentialAuthentication
        title: The system shall support credentials authentication.
        children:
          - "Auth-CredentialValidation"
          - "Auth-FailedAttemptLogging"

  - title: Data Management
    sections:
      - title: User Authentication
        requirements:
          - id: Auth-CredentialValidation
            title: All requests shall have their credentials authenticated before being processed.
            tests:
              - Credentials_Valid_Allowed
              - Credentials_Invalid_Refused
              - Credentials_Missing_Refused
          
          - id: Auth-FailedAttemptLogging
            title: Failed authentication attempts shall be logged.
            tests:
              - Authentication_Failed_Logged

      - title: Logging
        requirements:
          - id: Logging-RequestLogging
            title: All requests shall be logged with timestamp and user information.
          
          - id: Logging-RetentionPolicy
            title: Logs shall be retained for at least 90 days.

# Include additional requirements from other files
includes:
  - performance_requirements.yaml
  - ui_requirements.yaml

# Test mappings separate from requirements
mappings:
  - id: Logging-RequestLogging
    tests:
      - Logging_ValidRequest_Logged
      - Logging_InvalidRequest_Logged
      - Logging_ContainsTimestamp
      - Logging_ContainsUserInfo
  
  - id: Logging-RetentionPolicy
    tests:
      - LogRetention_OldLogs_Retained
      - LogRetention_VeryOldLogs_Deleted
```

# Command-Line Interface

## Basic Usage

Display help information:

```bash
reqstream --help
```

Display version:

```bash
reqstream --version
```

Process requirements files:

```bash
reqstream --requirements "**/*.requirements.yaml"
```

## Command-Line Options

ReqStream supports the following command-line options:

| Option | Description |
| ------ | ----------- |
| `-v`, `--version` | Display version information |
| `-?`, `-h`, `--help` | Display help message |
| `--silent` | Suppress console output (useful in CI/CD) |
| `--validate` | Run self-validation and display test results |
| `--results <file>` | Write validation test results to a file (TRX or JUnit format, use .trx or .xml extension) |
| `--lint` | Lint requirements files for structural issues |
| `--log <file>` | Write output to specified log file |
| `--depth <depth>` | Default starting header depth for all reports (default: 1) |
| `--requirements <pattern>` | Glob pattern for requirements YAML files |
| `--report <file>` | Export requirements to markdown file |
| `--report-depth <depth>` | Starting header depth for requirements report (overrides `--depth`) |
| `--filter <tags>` | Comma-separated list of tags to filter requirements by |
| `--tests <pattern>` | Glob pattern for test result files (TRX or JUnit format) |
| `--matrix <file>` | Export trace matrix to markdown file |
| `--matrix-depth <depth>` | Starting header depth for trace matrix (overrides `--depth`) |
| `--justifications <file>` | Export justifications to markdown file |
| `--justifications-depth <depth>` | Starting header depth for justifications (overrides `--depth`) |
| `--enforce` | Fail if requirements are not fully tested |

## Examples

### Running Validation

Run self-validation to verify core functionality:

```bash
reqstream --validate
```

Run self-validation and save results to a TRX file:

```bash
reqstream --validate --results validation-results.trx
```

Run self-validation and save results to a JUnit XML file:

```bash
reqstream --validate --results validation-results.xml
```

### Validation Report

The validation report contains the tool version, machine name, operating system version, .NET runtime version,
timestamp, and test results.

Example validation report:

```text
# DEMA Consulting ReqStream

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| ReqStream Version   | 1.0.0                                              |
| Machine Name        | BUILD-SERVER                                       |
| OS Version          | Ubuntu 22.04.3 LTS                                 |
| DotNet Runtime      | .NET 10.0.0                                        |
| Time Stamp          | 2024-01-15 10:30:00 UTC                            |

✓ ReqStream_RequirementsProcessing - Passed
✓ ReqStream_TraceMatrix - Passed
✓ ReqStream_ReportExport - Passed
✓ ReqStream_TagsFiltering - Passed
✓ ReqStream_EnforcementMode - Passed
✓ ReqStream_Lint - Passed

Total Tests: 6
Passed: 6
Failed: 0
```

### Validation Tests

Each test proves specific functionality works correctly:

- **`ReqStream_RequirementsProcessing`** - requirements YAML files are correctly loaded and processed.
- **`ReqStream_TraceMatrix`** - trace matrix is correctly generated from requirements and test results.
- **`ReqStream_ReportExport`** - requirements report is correctly exported to a markdown file.
- **`ReqStream_TagsFiltering`** - requirements are correctly filtered by tags.
- **`ReqStream_EnforcementMode`** - enforcement mode correctly validates requirement test coverage.
- **`ReqStream_Lint`** - lint mode correctly identifies and reports issues in requirements files.

### Linting Requirements Files

Use the `--lint` flag to inspect requirements files for structural problems before processing them.
Unlike normal processing, linting reports **all** issues found rather than stopping at the first error.

**Lint a single requirements file (including all its includes):**

```bash
reqstream --requirements requirements.yaml --lint
```

**Lint multiple requirements files:**

```bash
reqstream --requirements "docs/**/*.yaml" --lint
```

**Example output when issues are found:**

```text
docs/requirements/unit.yaml(42,5): error: Unknown field 'tittle' in requirement
docs/requirements/unit.yaml(57,13): error: Duplicate requirement ID 'Core-BasicRequirement' (first seen in docs/requirements/base.yaml)
docs/requirements/other.yaml(10,1): error: Section missing required field 'title'
```

**Example output when no issues are found:**

When no issues are found, `--lint` produces **no output** and exits with code `0`. This makes it easy
to integrate into lint scripts where silence means success.

The exit code is `0` when no issues are found, and `1` when any issues are reported — making `--lint`
suitable for use in CI/CD quality gates. The application banner is also suppressed during lint so that
only actionable issue lines appear in the output.

**Issues detected by the linter:**

| Issue | Description |
| ----- | ----------- |
| Malformed YAML | File cannot be parsed as valid YAML |
| Unknown document field | Top-level key other than `sections`, `mappings`, or `includes` |
| Unknown section field | Section key other than `title`, `requirements`, or `sections` |
| Unknown requirement field | Requirement key other than `id`, `title`, `justification`, `tests`, `children`, `tags` |
| Unknown mapping field | Mapping key other than `id` or `tests` |
| Missing section title | Section does not have a `title` field |
| Blank section title | Section `title` is empty or whitespace |
| Missing requirement id | Requirement does not have an `id` field |
| Blank requirement id | Requirement `id` is empty or whitespace |
| Missing requirement title | Requirement does not have a `title` field |
| Blank requirement title | Requirement `title` is empty or whitespace |
| Missing mapping id | Mapping does not have an `id` field |
| Blank mapping id | Mapping `id` is empty or whitespace |
| Blank test name | A test name in a `tests` list is empty or whitespace |
| Blank tag name | A tag name in a `tags` list is empty or whitespace |
| Duplicate requirement ID | Two requirements share the same `id` (within or across files) |

### Requirements Processing

**Process requirements and create a report:**

```bash
reqstream --requirements "docs/**/*.yaml" --report requirements_report.md
```

**Create a trace matrix with test results:**

```bash
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --matrix trace_matrix.md
```

**Generate both reports with custom header depths:**

```bash
reqstream --requirements "docs/**/*.yaml" \
          --report requirements.md \
          --tests "test-results/**/*.trx" \
          --matrix matrix.md \
          --depth 2
```

**Silent mode for CI/CD:**

```bash
reqstream --silent \
          --requirements "docs/**/*.yaml" \
          --report requirements.md \
          --tests "test-results/**/*.trx" \
          --matrix matrix.md \
          --log reqstream.log
```

**Using glob patterns:**

```bash
# Process all YAML files in any subdirectory
reqstream --requirements "**/*.yaml"

# Process only requirements files in specific directory
reqstream --requirements "requirements/*.requirements.yaml"

# Process multiple test result formats
reqstream --requirements "**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --tests "test-results/**/*.xml"
```

**Requirements enforcement in CI/CD:**

```bash
# Enforce that all requirements have passing tests
reqstream --requirements "**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --enforce

# Generate reports and enforce coverage
reqstream --requirements "**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --report requirements.md \
          --matrix trace-matrix.md \
          --enforce
```

**Tag filtering for focused reports:**

```bash
# Export only security-tagged requirements
reqstream --requirements "docs/**/*.yaml" \
          --filter security \
          --report security_requirements.md

# Generate trace matrix for critical requirements only
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --filter critical \
          --matrix critical_trace_matrix.md

# Enforce test coverage for security requirements
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --filter security \
          --enforce

# Export multiple tags (OR logic - matches any tag)
reqstream --requirements "docs/**/*.yaml" \
          --filter security,compliance \
          --report security_and_compliance.md
```

# Exporting

ReqStream can export requirements and test trace matrices to markdown format for documentation and review.

## Requirements Reports

A requirements report exports all requirements in a structured markdown format that follows your section hierarchy.

**Generate a requirements report:**

```bash
reqstream --requirements "docs/**/*.yaml" --report requirements_report.md
```

**Example output structure:**

```markdown
# System Security

## Security-CredentialAuthentication

The system shall support credentials authentication.

Children:
- Auth-CredentialValidation
- Auth-FailedAttemptLogging

# Data Management

## User Authentication

### Auth-CredentialValidation

All requests shall have their credentials authenticated before being processed.

Tests:
- Credentials_Valid_Allowed
- Credentials_Invalid_Refused
- Credentials_Missing_Refused
```

## Trace Matrix

A trace matrix shows the mapping between requirements and test cases, helping verify that all requirements have
adequate test coverage.

**Generate a trace matrix:**

```bash
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --matrix trace_matrix.md
```

The trace matrix includes:

- Each requirement with its ID and title
- List of test cases mapped to that requirement
- Test status (Passed, Failed, Skipped) from test results
- Coverage analysis

**Example trace matrix output:**

```markdown
# Trace Matrix

## Security-CredentialAuthentication: The system shall support credentials authentication.

No direct tests (parent requirement)

Child requirements: Auth-CredentialValidation, Auth-FailedAttemptLogging

## Auth-CredentialValidation: All requests shall have their credentials authenticated before being processed.

Tests:
- ✓ Credentials_Valid_Allowed (Passed)
- ✗ Credentials_Invalid_Refused (Failed)
- ✓ Credentials_Missing_Refused (Passed)

Coverage: 3 tests mapped
```

## Justifications Export

A justifications report documents the rationale behind each requirement, helping developers and stakeholders
understand why requirements exist. This is especially valuable for onboarding new team members and providing
context for decision-making.

**Generate a justifications report:**

```bash
reqstream --requirements "docs/**/*.yaml" \
          --justifications justifications.md
```

The justifications report includes:

- Each requirement's ID and title as a header
- The justification text explaining why the requirement exists
- Preserves the hierarchical section structure

**Example justifications output:**

```markdown
# System Security

## Security-CredentialAuthentication

**The system shall support credentials authentication.**

Authentication is critical to ensure only authorized users can access the system.
This requirement establishes the foundation for our security posture.

## Auth-CredentialValidation

**All requests shall have their credentials authenticated before being processed.**

Prevents unauthorized access to system resources and ensures compliance with
security standards. Each request must be verified to maintain system integrity.
```

**Control justifications header depth:**

Use the `--justifications-depth` option to control the starting markdown header level:

```bash
# Start justifications sections with ## (level 2)
reqstream --requirements "docs/**/*.yaml" \
          --justifications justifications.md \
          --justifications-depth 2
```

## Export Options

**Control header depth:**

Use `--depth` to set a single default header depth for all reports, or use `--report-depth`,
`--matrix-depth`, and `--justifications-depth` to override the depth for individual reports:

```bash
# Set all reports to start at ## (level 2)
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --report requirements.md \
          --matrix matrix.md \
          --justifications justifications.md \
          --depth 2

# Set default depth to 2 but override trace matrix to level 3
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --report requirements.md \
          --matrix matrix.md \
          --depth 2 \
          --matrix-depth 3
```

This is useful when embedding generated reports into larger documents.

**Logging output:**

Use the `--log` option to capture console output to a file:

```bash
reqstream --requirements "docs/**/*.yaml" \
          --report requirements.md \
          --log process.log
```

**Silent mode:**

Use `--silent` to suppress console output (useful in automated builds):

```bash
reqstream --silent \
          --requirements "docs/**/*.yaml" \
          --report requirements.md
```

# Requirements Enforcement

ReqStream can enforce that all requirements have adequate test coverage, making it ideal for use in CI/CD pipelines
as a quality gate to ensure requirements are properly verified.

## Overview

Requirements enforcement validates that:

- Every requirement has at least one test mapped (either directly or through child requirements)
- All mapped tests are present in the test result files
- All mapped tests pass

If any requirement doesn't meet these criteria, the tool reports an error and exits with a non-zero status code,
causing CI/CD builds to fail.

## Usage

Enable enforcement with the `--enforce` flag:

```bash
reqstream --requirements "**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --enforce
```

## How It Works

Requirements can be satisfied in two ways:

1. **Direct tests**: Tests mapped directly to the requirement via the `tests` field
2. **Transitive tests**: Tests mapped to child requirements

For a requirement to be satisfied:

- It must have at least one test (direct or via children)
- All tests must have been executed in the test results
- All tests must have passed

**Example:**

```yaml
sections:
  - title: System Security
    requirements:
      - id: Security-CredentialAuthentication
        title: The system shall support authentication.
        children:
          - "Auth-CredentialValidation"
          - "Auth-FailedAttemptLogging"
      
      - id: Auth-CredentialValidation
        title: Users shall authenticate with username and password.
        tests:
          - Test_UsernamePassword_Valid
          - Test_UsernamePassword_Invalid
      
      - id: Auth-FailedAttemptLogging
        title: Failed authentication attempts shall be logged.
        tests:
          - Test_FailedAuth_Logged
```

In this example:

- `Auth-CredentialValidation` is satisfied if both its tests pass
- `Auth-FailedAttemptLogging` is satisfied if its test passes
- `Security-CredentialAuthentication` is satisfied transitively through its children (if both
  `Auth-CredentialValidation` and `Auth-FailedAttemptLogging` are satisfied)

## Enforcement Output

When enforcement mode is enabled, ReqStream processes normally and generates any requested reports. After all
processing is complete, it checks requirement satisfaction.

**If all requirements are satisfied:**

```text
...
Trace matrix report generated successfully.
```

Exit code: **0** (success)

**If requirements are not satisfied:**

```text
...
Trace matrix report generated successfully.
Error: Only 15 of 20 requirements are satisfied with tests.
```

Exit code: **1** (failure)

The error message clearly indicates how many requirements are satisfied, making it easy to track progress toward
full coverage.

## CI/CD Integration

Requirements enforcement is designed for CI/CD pipelines. Here are examples for common platforms:

**GitHub Actions:**

```yaml
name: Validate Requirements

on: [push, pull_request]

jobs:
  requirements:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      
      - name: Install ReqStream
        run: dotnet tool install -g DemaConsulting.ReqStream
      
      - name: Run Tests
        run: dotnet test --logger trx
      
      - name: Validate Requirements Coverage
        run: |
          reqstream \
            --requirements "docs/**/*.yaml" \
            --tests "**/*.trx" \
            --matrix trace-matrix.md \
            --enforce
      
      - name: Upload Trace Matrix
        if: always()
        uses: actions/upload-artifact@v4
        with:
          name: trace-matrix
          path: trace-matrix.md
```

**Azure Pipelines:**

```yaml
steps:
  - task: DotNetCoreCLI@2
    displayName: 'Install ReqStream'
    inputs:
      command: 'custom'
      custom: 'tool'
      arguments: 'install -g DemaConsulting.ReqStream'
  
  - task: DotNetCoreCLI@2
    displayName: 'Run Tests'
    inputs:
      command: 'test'
      arguments: '--logger trx'
  
  - script: |
      reqstream \
        --requirements "docs/**/*.yaml" \
        --tests "**/*.trx" \
        --matrix trace-matrix.md \
        --enforce
    displayName: 'Validate Requirements Coverage'
```

**GitLab CI:**

```yaml
validate-requirements:
  stage: test
  script:
    - dotnet tool install -g DemaConsulting.ReqStream
    - export PATH="$PATH:$HOME/.dotnet/tools"
    - dotnet test --logger trx
    - reqstream --requirements "docs/**/*.yaml" --tests "**/*.trx" --matrix trace-matrix.md --enforce
  artifacts:
    when: always
    paths:
      - trace-matrix.md
```

## Best Practices

**Start without enforcement:**

When first adopting ReqStream, start by generating trace matrices without enforcement to understand your current
coverage:

```bash
reqstream --requirements "**/*.yaml" \
          --tests "**/*.trx" \
          --matrix trace-matrix.md
```

Review the trace matrix to identify gaps, then work toward full coverage before enabling enforcement.

**Enable enforcement incrementally:**

If you have a large requirements set with incomplete coverage, consider:

1. Start with enforcement on critical requirements only
2. Gradually expand coverage
3. Enable enforcement for all requirements once baseline is achieved

**Use in pull requests:**

Enable enforcement in PR validation to prevent coverage from decreasing:

```yaml
# GitHub Actions PR validation
on: [pull_request]

jobs:
  requirements-coverage:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: dotnet test --logger trx
      - run: dotnet reqstream --requirements "**/*.yaml" --tests "**/*.trx" --enforce
```

**Generate reports for failure analysis:**

Always generate the trace matrix when using enforcement so you can review which requirements are not satisfied:

```bash
reqstream --requirements "**/*.yaml" \
          --tests "**/*.trx" \
          --matrix trace-matrix.md \
          --enforce
```

The trace matrix will show which requirements lack tests or have failing tests.

**Leverage transitive coverage:**

Use parent-child relationships to organize requirements hierarchically. High-level requirements don't need direct
tests if they're satisfied through child requirements:

```yaml
requirements:
  - id: Security-Overall
    title: System shall be secure
    children:
      - "Security-AuthRequired"
      - "Security-AuthorizationRequired"
      - "Security-DataProtection"
  
  # Children have direct tests
  - id: Security-AuthRequired
    title: Authentication required
    tests:
      - Test_Auth_Required
```

## Troubleshooting Enforcement

### Error: Cannot enforce requirements without test results

This error occurs when `--enforce` is used without the `--tests` option. You must provide test result files to
validate coverage:

```bash
# Wrong - no test results
reqstream --requirements "**/*.yaml" --enforce

# Correct - with test results
reqstream --requirements "**/*.yaml" --tests "**/*.trx" --enforce
```

### All requirements show as unsatisfied

If all or most requirements are showing as unsatisfied, check:

1. Test names in requirements YAML match test names in test result files exactly (case-sensitive)
2. Test result files are in TRX or JUnit format
3. Tests are actually being executed (check test result file contents)
4. Tests are passing (failing tests count as unsatisfied)

### Some tests don't match

If specific tests aren't being recognized:

1. Verify exact test name match (including namespaces if present)
2. Check for typos in requirements YAML
3. If using source-specific tests (`filepart@testname`), verify the file part matches the test result filename
4. Run without `--enforce` first and review the trace matrix to see which tests are found

### Requirements with no direct tests show as unsatisfied

Ensure parent requirements reference their children via the `children` field:

```yaml
requirements:
  - id: System-ParentRequirement
    title: Parent requirement
    children:
      - "System-ChildRequirement"  # Add child references
  
  - id: System-ChildRequirement
    title: Child requirement
    tests:
      - Test_Child
```

# FAQ

## General Questions

**Q: What file extensions should I use for requirements files?**

A: You can use `.yaml` or `.yml`. ReqStream doesn't require a specific extension, but `.requirements.yaml` is a good
convention to distinguish requirements files from other YAML files in your project.

**Q: Can I use ReqStream with non-.NET projects?**

A: Yes! ReqStream is a command-line tool that works with YAML files and test results. While it's written in .NET, it
can be used with projects in any language. The test results need to be in TRX (Visual Studio Test Results) or JUnit
format.

**Q: How do I integrate ReqStream into my CI/CD pipeline?**

A: Install ReqStream as a local tool and add the commands to your build script. Example for GitHub Actions:

```yaml
- name: Install ReqStream
  run: dotnet tool restore

- name: Generate Requirements Report
  run: dotnet reqstream --requirements "docs/**/*.yaml" --report requirements.md
```

**Q: Can I have requirements without IDs?**

A: No, every requirement must have a unique `id` field. The ID is essential for referencing, traceability, and
generating reports.

## YAML Format Questions

**Q: What format should requirement IDs follow?**

A: ReqStream doesn't enforce a specific ID format. You can use any format that makes sense for your project:

- `Core-BasicRequirement`, `Core-AnotherRequirement`, ...
- `System-TopLevel`, `Auth-CredentialValidation`, ...
- `FR-1.1`, `FR-1.2`, ...

The only requirement is that IDs must be unique across all requirements files.

**Q: Can I have multiple requirements with the same title?**

A: Yes, titles don't need to be unique. The `id` field is what must be unique.

**Q: How deeply can I nest sections?**

A: There's no hard limit on section nesting depth. Use as many levels as needed for logical organization.

**Q: Can included files include other files?**

A: Yes, includes can be nested. ReqStream will resolve all includes recursively.

**Q: How do I handle circular includes?**

A: Avoid circular includes (A includes B, B includes A). ReqStream will detect this and report an error.

**Q: What happens if I have duplicate requirement IDs?**

A: ReqStream will detect duplicate IDs and report an error during validation.

## Test Mapping Questions

**Q: What test result formats are supported?**

A: ReqStream supports TRX (Visual Studio Test Results) and JUnit XML formats.

**Q: Do test names need to match exactly?**

A: Yes, test names in requirements YAML must match the test names in your test result files exactly (case-sensitive).

**Q: Can a test verify multiple requirements?**

A: Yes, you can map the same test name to multiple requirements. This is useful for tests that verify multiple
related requirements.

**Q: What if a requirement has no tests mapped?**

A: ReqStream will still include the requirement in reports, but it will be noted as having no test coverage in the
trace matrix.

**Q: Can I use wildcards in test names?**

A: No, test names must be specified exactly. However, you can use patterns when specifying test result files
(`--tests "**/*.trx"`).

**Q: How can I link tests to specific test result files?**

A: Use the test source linking feature with the `[filepart@]testname` pattern. The `filepart` is a substring that
matches the base filename (without extension) of the test result file. For example:

```yaml
requirements:
  - id: Platform-Windows
    title: Shall support Windows
    tests:
      - "windows-latest@Test_PlatformFeature"  # Matches only from files containing "windows-latest"
  
  - id: Platform-Linux
    title: Shall support Linux
    tests:
      - "ubuntu-latest@Test_PlatformFeature"   # Matches only from files containing "ubuntu-latest"
```

File part matching is case-insensitive and supports partial matches. Tests without the `filepart@` prefix aggregate
results from all test result files.

**Q: Can I mix plain and source-specific test names?**

A: Yes, you can mix both styles in the same requirement. Plain test names will aggregate results from all test result
files, while source-specific test names will only match their specified sources.

## Enforcement Questions

**Q: What does the --enforce flag do?**

A: The `--enforce` flag validates that all requirements have adequate test coverage. If any requirement lacks tests or
has failing tests, the tool will exit with a non-zero status code, failing the build. This is useful for CI/CD
pipelines to ensure requirements are properly verified.

**Q: When should I use --enforce?**

A: Use `--enforce` in CI/CD pipelines to prevent merging code that reduces requirements coverage. Start by reviewing
your coverage with trace matrices first, then enable enforcement once you have acceptable baseline coverage.

**Q: How does transitive coverage work with --enforce?**

A: A parent requirement can be satisfied through its child requirements. If a requirement references children via the
`children` field, it's considered satisfied if all its children are satisfied with tests. This allows high-level
requirements to be validated through their detailed child requirements.

**Q: What happens if tests fail when --enforce is enabled?**

A: Requirements with failing tests are considered not satisfied. The tool will report an error and exit with code 1.
Review the trace matrix (use `--matrix`) to see which tests failed.

**Q: Can I use --enforce without generating reports?**

A: Yes, but it's recommended to generate the trace matrix (`--matrix`) alongside enforcement. The matrix provides
detailed information about which requirements are not satisfied, making it easier to identify and fix coverage gaps.

**Q: Why does --enforce require test results?**

A: Enforcement validates that requirements have passing tests, which requires test result files. If you use `--enforce`
without `--tests`, the tool will report an error asking you to specify test result files.

## Export Questions

**Q: Can I customize the markdown format of reports?**

A: Currently, ReqStream uses a fixed markdown format. You can control the header depth with `--depth`
(applies to all reports) or `--report-depth`, `--matrix-depth`, and `--justifications-depth` to override
individual reports, but other formatting is not customizable.

**Q: Can I export to formats other than markdown?**

A: Currently, only markdown export is supported. Markdown is widely supported and can be converted to other formats
using tools like Pandoc.

**Q: How are sections displayed in the requirements report?**

A: Sections are displayed as markdown headers, with nesting reflected in the header levels. Requirements within
sections appear under the appropriate section headers.

**Q: What information is included in the trace matrix?**

A: The trace matrix includes requirement IDs, titles, mapped test names, test status (from test results), and
coverage information.

## Troubleshooting

**Q: I get an error "No requirements files specified". What's wrong?**

A: This means the glob pattern in `--requirements` didn't match any files. Check that:

- The path is correct relative to your current directory
- The file extension matches (`.yaml` or `.yml`)
- The files exist

**Q: My glob pattern isn't matching files. What should I check?**

A: Ensure you're using quotes around the pattern to prevent shell expansion:

```bash
reqstream --requirements "**/*.yaml"  # Correct
reqstream --requirements **/*.yaml     # May not work as expected
```

**Q: I get validation errors about missing requirements. What does this mean?**

A: This typically means a requirement references a child requirement ID that doesn't exist, or a test mapping
references a requirement ID that doesn't exist. Check your `children` arrays and `mappings` section for typos.

**Q: Can I see what ReqStream is doing without generating reports?**

A: Yes, run ReqStream with just the `--requirements` option (no export options). It will load and validate your
requirements and report any issues:

```bash
reqstream --requirements "docs/**/*.yaml"
```

**Q: The tool fails with "could not be found". What's wrong?**

A: If you get `reqstream: command not found`, the tool isn't installed or not in your PATH. For global installation,
ensure `~/.dotnet/tools` is in your PATH. For local installation, use `dotnet reqstream` instead of `reqstream`.

---

For more information, visit the [ReqStream GitHub repository][repo].

For support, please [open an issue][issues] or [start a discussion][discussions].

<!-- Link References -->
[dotnet-sdk]: https://dotnet.microsoft.com/download
[repo]: https://github.com/demaconsulting/ReqStream
[issues]: https://github.com/demaconsulting/ReqStream/issues
[discussions]: https://github.com/demaconsulting/ReqStream/discussions
[continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
