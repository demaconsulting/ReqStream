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
| `--validate` | Run self-validation (placeholder for future use) |
| `--log <file>` | Write output to specified log file |
| `--requirements <pattern>` | Glob pattern for requirements YAML files |
| `--report <file>` | Export requirements to markdown file |
| `--report-depth <depth>` | Starting header depth for requirements report (default: 1) |
| `--tests <pattern>` | Glob pattern for test result files (TRX or JUnit format) |
| `--matrix <file>` | Export trace matrix to markdown file |
| `--matrix-depth <depth>` | Starting header depth for trace matrix (default: 1) |

### Examples

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
