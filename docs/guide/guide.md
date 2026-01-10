# ReqStream User Guide

## Table of Contents

- [Introduction](#introduction)
  - [What is ReqStream](#what-is-reqstream)
  - [Key Features](#key-features)
  - [Use Cases](#use-cases)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
  - [Global Installation](#global-installation)
  - [Local Installation](#local-installation)
  - [Updating](#updating)
- [Requirements File Format](#requirements-file-format)
  - [Basic Structure](#basic-structure)
  - [Sections and Subsections](#sections-and-subsections)
  - [Requirements](#requirements)
  - [Test Mappings](#test-mappings)
  - [File Includes](#file-includes)
  - [Section Merging](#section-merging)
  - [Complete Example](#complete-example)
- [Command-Line Interface](#command-line-interface)
  - [Basic Usage](#basic-usage)
  - [Command-Line Options](#command-line-options)
  - [Examples](#examples)
- [Exporting](#exporting)
  - [Requirements Reports](#requirements-reports)
  - [Trace Matrix](#trace-matrix)
  - [Export Options](#export-options)
- [FAQ](#faq)

## Introduction

### What is ReqStream

ReqStream is a .NET command-line tool designed to help teams manage software requirements in a structured,
version-controllable, and maintainable way. By using YAML files to define requirements, ReqStream enables requirements
to be treated as code, stored in source control, and integrated into CI/CD pipelines.

### Key Features

- **YAML Format** - Manage requirements in human-readable YAML format that can be easily edited and reviewed
- **Command-Line Interface** - Automate requirement management with CLI tools that integrate with build systems
- **Multi-Platform** - Works on Windows, Linux, and macOS with .NET 8, 9, and 10
- **Hierarchical Structure** - Organize requirements with sections and subsections for better organization
- **Test Mapping** - Link requirements to test cases for traceability and verification
- **File Includes** - Modularize requirements across multiple YAML files for better maintainability
- **Validation** - Built-in validation ensures requirement structure and references are correct
- **Export Capabilities** - Generate markdown reports for requirements and test trace matrices

### Use Cases

ReqStream is ideal for:

- Software development projects requiring formal requirements documentation
- Teams practicing DevOps and want requirements versioned alongside code
- Projects needing traceability between requirements and test cases
- Organizations requiring compliance documentation
- Agile teams wanting lightweight, maintainable requirements management

## Prerequisites

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

## Installation

ReqStream is distributed as a .NET tool and can be installed globally for system-wide use or locally for specific
projects.

### Global Installation

For individual use or when you want ReqStream available system-wide:

```bash
dotnet tool install -g DemaConsulting.ReqStream
```

Verify the installation:

```bash
reqstream --version
```

### Local Installation

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

### Updating

To update to the latest version:

```bash
# For global tools
dotnet tool update -g DemaConsulting.ReqStream

# For local tools
dotnet tool update DemaConsulting.ReqStream
```

## Requirements File Format

ReqStream uses YAML files to define requirements. The format is designed to be human-readable while providing
structure for tooling.

### Basic Structure

A requirements YAML file has a top-level `sections` array:

```yaml
---
sections:
  - title: "My Section"
    requirements:
      - id: "REQ-001"
        title: "My first requirement"
```

### Sections and Subsections

Sections provide hierarchical organization. Sections can contain requirements and/or nested subsections:

```yaml
---
sections:
  - title: "System Requirements"
    requirements:
      - id: "SYS-001"
        title: "Top-level system requirement"
    
    sections:
      - title: "Security"
        requirements:
          - id: "SEC-001"
            title: "Security requirement"
      
      - title: "Performance"
        requirements:
          - id: "PERF-001"
            title: "Performance requirement"
```

You can nest sections as deeply as needed to organize your requirements logically.

### Requirements

Each requirement must have:

- **id** - A unique identifier (can be any format, but must be unique across all files)
- **title** - A clear description of the requirement

Requirements can optionally include:

- **tests** - Array of test names that verify this requirement
- **children** - Array of requirement IDs that are children of this requirement

Example:

```yaml
requirements:
  - id: "SYS-SEC-001"
    title: "The system shall support credentials authentication."
    children:
      - "AUTH-001"
      - "AUTH-002"
```

### Test Mappings

Tests can be mapped to requirements in two ways:

**Inline with requirements:**

```yaml
requirements:
  - id: "AUTH-001"
    title: "All requests shall have their credentials authenticated before being processed."
    tests:
      - "Credentials_Valid_Allowed"
      - "Credentials_Invalid_Refused"
      - "Credentials_Missing_Refused"
```

**Separate mappings section:**

```yaml
sections:
  - title: "Logging"
    requirements:
      - id: "DATA-001"
        title: "All requests shall be logged."

mappings:
  - id: "DATA-001"
    tests:
      - "Logging_ValidRequest_Logged"
      - "Logging_InvalidRequest_Logged"
```

The separate `mappings` section is useful when test mappings are maintained by a different team or in a different
file from the requirements.

### Test Source Linking

When testing requirements across multiple platforms or configurations, use test source linking to distinguish tests
from different sources. This is particularly useful for matrix testing scenarios.

**Pattern**: `[filepart@]testname`

- `filepart` (optional): A substring matching the base filename (without extension) of the test result file
- `testname`: The exact test name from the test result file

**Example - Platform-specific testing:**

```yaml
requirements:
  - id: "PLAT-001"
    title: "Shall support Windows operating systems"
    tests:
      - "windows-latest@Test_PlatformBasic"
      - "windows-latest@Test_FileSystem"
  
  - id: "PLAT-002"
    title: "Shall support Linux operating systems"
    tests:
      - "ubuntu-latest@Test_PlatformBasic"
      - "ubuntu-latest@Test_FileSystem"
  
  - id: "PLAT-003"
    title: "Shall support cross-platform APIs"
    tests:
      - "Test_CrossPlatformAPI"  # Aggregates from all platforms
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

### File Includes

Large projects can be split across multiple YAML files using the `includes` section:

```yaml
---
sections:
  - title: "Core Requirements"
    requirements:
      - id: "CORE-001"
        title: "Core requirement"

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

### Section Merging

When multiple files define sections with the same full hierarchy path, ReqStream automatically merges them. This
allows included files to add requirements to existing sections.

**main_requirements.yaml:**

```yaml
---
sections:
  - title: "System Requirements"
    sections:
      - title: "Security"
        requirements:
          - id: "SEC-001"
            title: "Authentication required"
```

**additional_requirements.yaml:**

```yaml
---
sections:
  - title: "System Requirements"
    sections:
      - title: "Security"
        requirements:
          - id: "SEC-002"
            title: "Authorization required"
```

When both files are loaded, the "Security" section will contain both SEC-001 and SEC-002.

### Complete Example

Here's a comprehensive example showing all features:

```yaml
---
# Main requirements file

sections:
  - title: "System Security"
    requirements:
      - id: "SYS-SEC-001"
        title: "The system shall support credentials authentication."
        children:
          - "AUTH-001"
          - "AUTH-002"

  - title: "Data Management"
    sections:
      - title: "User Authentication"
        requirements:
          - id: "AUTH-001"
            title: "All requests shall have their credentials authenticated before being processed."
            tests:
              - "Credentials_Valid_Allowed"
              - "Credentials_Invalid_Refused"
              - "Credentials_Missing_Refused"
          
          - id: "AUTH-002"
            title: "Failed authentication attempts shall be logged."
            tests:
              - "Authentication_Failed_Logged"

      - title: "Logging"
        requirements:
          - id: "DATA-001"
            title: "All requests shall be logged with timestamp and user information."
          
          - id: "DATA-002"
            title: "Logs shall be retained for at least 90 days."

# Include additional requirements from other files
includes:
  - performance_requirements.yaml
  - ui_requirements.yaml

# Test mappings separate from requirements
mappings:
  - id: "DATA-001"
    tests:
      - "Logging_ValidRequest_Logged"
      - "Logging_InvalidRequest_Logged"
      - "Logging_ContainsTimestamp"
      - "Logging_ContainsUserInfo"
  
  - id: "DATA-002"
    tests:
      - "LogRetention_OldLogs_Retained"
      - "LogRetention_VeryOldLogs_Deleted"
```

## Command-Line Interface

### Basic Usage

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

### Command-Line Options

ReqStream supports the following command-line options:

| Option | Description |
| ------ | ----------- |
| `-v`, `--version` | Display version information |
| `-?`, `-h`, `--help` | Display help message |
| `--silent` | Suppress console output (useful in CI/CD) |
| `--validate` | Run self-validation and display test results |
| `--results <file>` | Write validation test results to a file (TRX or JUnit format, use .trx or .xml extension) |
| `--log <file>` | Write output to specified log file |
| `--requirements <pattern>` | Glob pattern for requirements YAML files |
| `--report <file>` | Export requirements to markdown file |
| `--report-depth <depth>` | Starting header depth for requirements report (default: 1) |
| `--tests <pattern>` | Glob pattern for test result files (TRX or JUnit format) |
| `--matrix <file>` | Export trace matrix to markdown file |
| `--matrix-depth <depth>` | Starting header depth for trace matrix (default: 1) |
| `--enforce` | Fail if requirements are not fully tested |

### Examples

#### Running Self-Validation

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

#### Requirements Processing

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
          --report-depth 2 \
          --tests "test-results/**/*.trx" \
          --matrix matrix.md \
          --matrix-depth 1
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

## Exporting

ReqStream can export requirements and test trace matrices to markdown format for documentation and review.

### Requirements Reports

A requirements report exports all requirements in a structured markdown format that follows your section hierarchy.

**Generate a requirements report:**

```bash
reqstream --requirements "docs/**/*.yaml" --report requirements_report.md
```

**Example output structure:**

```markdown
# System Security

## SYS-SEC-001

The system shall support credentials authentication.

Children:
- AUTH-001
- AUTH-002

# Data Management

## User Authentication

### AUTH-001

All requests shall have their credentials authenticated before being processed.

Tests:
- Credentials_Valid_Allowed
- Credentials_Invalid_Refused
- Credentials_Missing_Refused
```

### Trace Matrix

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

## SYS-SEC-001: The system shall support credentials authentication.

No direct tests (parent requirement)

Child requirements: AUTH-001, AUTH-002

## AUTH-001: All requests shall have their credentials authenticated before being processed.

Tests:
- ✓ Credentials_Valid_Allowed (Passed)
- ✗ Credentials_Invalid_Refused (Failed)
- ✓ Credentials_Missing_Refused (Passed)

Coverage: 3 tests mapped
```

### Export Options

**Control header depth:**

The `--report-depth` and `--matrix-depth` options control the starting markdown header level:

```bash
# Start requirements with ## (level 2) instead of # (level 1)
reqstream --requirements "docs/**/*.yaml" \
          --report requirements.md \
          --report-depth 2

# Start trace matrix sections with ### (level 3)
reqstream --requirements "docs/**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --matrix matrix.md \
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

## Requirements Enforcement

ReqStream can enforce that all requirements have adequate test coverage, making it ideal for use in CI/CD pipelines
as a quality gate to ensure requirements are properly verified.

### Overview

Requirements enforcement validates that:

- Every requirement has at least one test mapped (either directly or through child requirements)
- All mapped tests are present in the test result files
- All mapped tests pass

If any requirement doesn't meet these criteria, the tool reports an error and exits with a non-zero status code,
causing CI/CD builds to fail.

### Usage

Enable enforcement with the `--enforce` flag:

```bash
reqstream --requirements "**/*.yaml" \
          --tests "test-results/**/*.trx" \
          --enforce
```

### How It Works

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
  - title: "System Security"
    requirements:
      - id: "SYS-SEC-001"
        title: "The system shall support authentication."
        children:
          - "AUTH-001"
          - "AUTH-002"
      
      - id: "AUTH-001"
        title: "Users shall authenticate with username and password."
        tests:
          - "Test_UsernamePassword_Valid"
          - "Test_UsernamePassword_Invalid"
      
      - id: "AUTH-002"
        title: "Failed authentication attempts shall be logged."
        tests:
          - "Test_FailedAuth_Logged"
```

In this example:

- `AUTH-001` is satisfied if both its tests pass
- `AUTH-002` is satisfied if its test passes
- `SYS-SEC-001` is satisfied transitively through its children (if both `AUTH-001` and `AUTH-002` are satisfied)

### Enforcement Output

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

### CI/CD Integration

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

### Best Practices

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
  - id: "HIGH-LEVEL-001"
    title: "System shall be secure"
    children:
      - "SEC-001"
      - "SEC-002"
      - "SEC-003"
  
  # Children have direct tests
  - id: "SEC-001"
    title: "Authentication required"
    tests:
      - "Test_Auth_Required"
```

### Troubleshooting Enforcement

#### Error: Cannot enforce requirements without test results

This error occurs when `--enforce` is used without the `--tests` option. You must provide test result files to
validate coverage:

```bash
# Wrong - no test results
reqstream --requirements "**/*.yaml" --enforce

# Correct - with test results
reqstream --requirements "**/*.yaml" --tests "**/*.trx" --enforce
```

#### All requirements show as unsatisfied

If all or most requirements are showing as unsatisfied, check:

1. Test names in requirements YAML match test names in test result files exactly (case-sensitive)
2. Test result files are in TRX or JUnit format
3. Tests are actually being executed (check test result file contents)
4. Tests are passing (failing tests count as unsatisfied)

#### Some tests don't match

If specific tests aren't being recognized:

1. Verify exact test name match (including namespaces if present)
2. Check for typos in requirements YAML
3. If using source-specific tests (`filepart@testname`), verify the file part matches the test result filename
4. Run without `--enforce` first and review the trace matrix to see which tests are found

#### Requirements with no direct tests show as unsatisfied

Ensure parent requirements reference their children via the `children` field:

```yaml
requirements:
  - id: "PARENT-001"
    title: "Parent requirement"
    children:
      - "CHILD-001"  # Add child references
  
  - id: "CHILD-001"
    title: "Child requirement"
    tests:
      - "Test_Child"
```

## FAQ

### General Questions

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

### YAML Format Questions

**Q: What format should requirement IDs follow?**

A: ReqStream doesn't enforce a specific ID format. You can use any format that makes sense for your project:

- `REQ-001`, `REQ-002`, ...
- `SYS-001`, `AUTH-001`, ...
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

### Test Mapping Questions

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
  - id: "WIN-001"
    title: "Shall support Windows"
    tests:
      - "windows-latest@Test_PlatformFeature"  # Matches only from files containing "windows-latest"
  
  - id: "LIN-001"
    title: "Shall support Linux"
    tests:
      - "ubuntu-latest@Test_PlatformFeature"   # Matches only from files containing "ubuntu-latest"
```

File part matching is case-insensitive and supports partial matches. Tests without the `filepart@` prefix aggregate
results from all test result files.

**Q: Can I mix plain and source-specific test names?**

A: Yes, you can mix both styles in the same requirement. Plain test names will aggregate results from all test result
files, while source-specific test names will only match their specified sources.

### Enforcement Questions

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

### Export Questions

**Q: Can I customize the markdown format of reports?**

A: Currently, ReqStream uses a fixed markdown format. You can control the header depth with `--report-depth` and
`--matrix-depth`, but other formatting is not customizable.

**Q: Can I export to formats other than markdown?**

A: Currently, only markdown export is supported. Markdown is widely supported and can be converted to other formats
using tools like Pandoc.

**Q: How are sections displayed in the requirements report?**

A: Sections are displayed as markdown headers, with nesting reflected in the header levels. Requirements within
sections appear under the appropriate section headers.

**Q: What information is included in the trace matrix?**

A: The trace matrix includes requirement IDs, titles, mapped test names, test status (from test results), and
coverage information.

### Troubleshooting

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

[dotnet-sdk]: https://dotnet.microsoft.com/download
[repo]: https://github.com/demaconsulting/ReqStream
[issues]: https://github.com/demaconsulting/ReqStream/issues
[discussions]: https://github.com/demaconsulting/ReqStream/discussions
