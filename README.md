# ReqStream

[![GitHub forks][forks-shield]][forks-url]
[![GitHub stars][stars-shield]][stars-url]
[![GitHub contributors][contributors-shield]][contributors-url]
[![License][license-shield]][license-url]
[![Build][build-shield]][build-url]
[![Quality Gate][quality-gate-shield]][quality-gate-url]
[![Security][security-shield]][security-url]
[![NuGet][nuget-shield]][nuget-url]

## Overview

ReqStream is a .NET command-line tool for managing software requirements in YAML format,
providing validation, linting, traceability, and test-mapping capabilities.

## Features

- 📝 **YAML Format** - Manage requirements in human-readable YAML format
- 🔗 **Hierarchical Structure** - Organize requirements with sections, subsections, and file includes
- 🧪 **Test Mapping** - Link requirements to test cases for traceability, including source-specific matching
- 🔍 **Linting** - Validate requirements YAML structure and references, reporting all issues in one pass
- 🏷️ **Tag Filtering** - Categorize and filter requirements using tags
- 📤 **Export Capabilities** - Generate markdown reports for requirements, trace matrices, and justifications
- 🔒 **Continuous Compliance** - Compliance evidence generated automatically on every CI run, following
  the [Continuous Compliance][link-continuous-compliance] methodology

## Installation

### Prerequisites

- [.NET SDK][dotnet-sdk] 8.0, 9.0, or 10.0

### Global Installation

Install ReqStream as a global .NET tool for system-wide use:

```bash
dotnet tool install -g DemaConsulting.ReqStream
```

Verify the installation:

```bash
reqstream --version
```

### Local Installation

Install ReqStream as a local tool in your project (recommended for team projects):

```bash
dotnet new tool-manifest  # if you don't have a tool manifest already
dotnet tool install DemaConsulting.ReqStream
```

Run the tool:

```bash
dotnet reqstream --version
```

### Update

To update to the latest version:

```bash
# For global tools
dotnet tool update -g DemaConsulting.ReqStream

# For local tools
dotnet tool update DemaConsulting.ReqStream
```

## Usage

Run the tool with the `--help` option to see available commands and options:

```bash
reqstream --help
```

This will display:

```text
Usage: reqstream [options]

Options:
  -v, --version                    Display version information
  -?, -h, --help                   Display this help message
  --silent                         Suppress console output
  --validate                       Run self-validation
  --results <file>                 Write validation results to file (TRX or JUnit format; use .trx or .xml extension)
  --log <file>                     Write output to log file
  --lint                           Lint requirements files for structural issues
  --depth <depth>                  Default markdown header depth for all reports (default: 1)
  --requirements <pattern>         Requirements files glob pattern
  --report <file>                  Export requirements to markdown file
  --report-depth <depth>           Markdown header depth for requirements report (overrides --depth)
  --tests <pattern>                Test result files glob pattern (TRX or JUnit)
  --matrix <file>                  Export trace matrix to markdown file
  --matrix-depth <depth>           Markdown header depth for trace matrix (overrides --depth)
  --justifications <file>          Export requirement justifications to markdown file
  --justifications-depth <depth>   Markdown header depth for justifications (overrides --depth)
  --filter <tags>                  Comma-separated list of tags to filter requirements
  --enforce                        Fail if requirements are not fully tested
```

## Self Validation

Running self-validation produces a report containing the following information:

```text
# DEMA Consulting ReqStream

| Information         | Value                                              |
| :------------------ | :------------------------------------------------- |
| ReqStream Version   | <version>                                          |
| Machine Name        | <machine-name>                                     |
| OS Version          | <os-version>                                       |
| DotNet Runtime      | <dotnet-runtime-version>                           |
| Time Stamp          | <timestamp> UTC                                    |

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

Each test in the report proves:

- **`ReqStream_RequirementsProcessing`** - requirements YAML files are correctly loaded and processed.
- **`ReqStream_TraceMatrix`** - trace matrix is correctly generated from requirements and test results.
- **`ReqStream_ReportExport`** - requirements report is correctly exported to a markdown file.
- **`ReqStream_TagsFiltering`** - requirements are correctly filtered by tags.
- **`ReqStream_EnforcementMode`** - enforcement mode correctly validates requirement test coverage.
- **`ReqStream_Lint`** - lint mode correctly identifies and reports issues in requirements files.

See the [User Guide][link-guide] for more details on the self-validation tests.

On validation failure the tool will exit with a non-zero exit code.

## YAML Format

ReqStream uses YAML files to define and manage requirements. The YAML format supports a hierarchical structure
with sections, requirements, test mappings, and file includes.

### Basic Structure

```yaml
---
# Requirements YAML file

# Requirement sections 
sections:
  - title: System Security
    requirements:
      - id: Security-CredentialAuthentication
        title: The system shall support credentials authentication.
        justification: |
          Authentication is critical to ensure only authorized users can access the system.
          This requirement establishes the foundation for our security posture.
        children: # Support linking to child requirements
          - Auth-CredentialValidation

  - title: Data Management
    sections:
      - title: User Authentication
        requirements:
          - id: Auth-CredentialValidation
            title: All requests shall have their credentials authenticated before being processed.
            justification: |
              Prevents unauthorized access to system resources and ensures compliance with
              security standards. Each request must be verified to maintain system integrity.
            tests: # Support test-mapping inline with requirements
              - Credentials_Valid_Allowed
              - Credentials_Invalid_Refused
              - Credentials_Missing_Refused

      - title: Logging
        requirements:
          - id: Logging-RequestLogging
            title: All requests shall be logged.

# Include other requirement files - may contain requirements and/or test mappings
includes:
  - more_requirements.yaml
  - test_mappings.yaml

# Test mappings support defining tests separate from requirements
mappings:
  - id: Logging-RequestLogging
    tests:
      - Logging_ValidRequest_Logged
      - Logging_InvalidRequest_Logged
```

### Key Features

- **Requirement IDs**: Can be of any format, but must be unique across all requirement files
- **Section Merging**: Identical sections (where the full hierarchy is identical) are automatically merged,
  allowing included files to add more sections or requirements to existing sections
- **Child Requirements**: Requirements can reference other requirements as children using the `children` field
- **Justifications**: Requirements can include an optional `justification` field to document the rationale behind
  the requirement, explaining why it exists and its purpose
- **Tags**: Requirements can include an optional `tags` field with a list of tag identifiers for categorization
  and filtering
- **Test Mappings**: Tests can be mapped to requirements either inline (within the requirement definition) or
  separately (using the `mappings` section)
- **Test Source Linking**: Support for source-specific test matching using the `[filepart@]testname` pattern,
  allowing requirements to specify tests from specific result files (e.g., `windows-latest@MyTest`)
- **File Includes**: Use the `includes` section to reference other YAML files containing additional requirements
  or test mappings

### Test Source Linking

When testing requirements across multiple platforms or configurations, test result files often include platform
identifiers in their names (e.g., `test-results-windows-latest.trx`, `test-results-ubuntu-latest.junit.xml`).
Test source linking allows requirements to specify which test results should come from which source files.

**Pattern**: `[filepart@]testname`

- `filepart` (optional): A substring that matches the base filename (without extension) of the test result file.
  Matching is case-insensitive and supports partial matches.
- `testname`: The exact name of the test as it appears in the test result file.

**Examples**:

```yaml
requirements:
  - id: Platform-Windows
    title: Shall support Windows
    tests:
      - windows@Test_PlatformFeature  # Matches only from files containing "windows"
  
  - id: Platform-Linux
    title: Shall support Linux
    tests:
      - ubuntu@Test_PlatformFeature   # Matches only from files containing "ubuntu"
  
  - id: Platform-CrossPlatform
    title: Shall support cross-platform features
    tests:
      - Test_CrossPlatformFeature     # Aggregates from all test result files
```

**Key behaviors**:

- Tests with source specifiers only match results from files containing the specified `filepart`
- Tests without source specifiers aggregate results from all test result files
- File part matching is case-insensitive and supports partial filename matching
- Both plain and source-specific test names can be mixed in the same requirement

## Tag Filtering

ReqStream supports tagging requirements for categorization and filtering. Tags enable you to organize requirements by
themes (e.g., security, performance, compliance) and selectively export subsets of requirements based on your needs.

### Adding Tags to Requirements

Tags are defined as an optional list in the requirement definition:

```yaml
sections:
  - title: System Security
    requirements:
      - id: Security-CredentialAuthentication
        title: The system shall support credentials authentication.
        tags:
          - security
          - critical
      
      - id: Security-AuditLogging
        title: The system shall log all authentication attempts.
        tags:
          - security
          - audit
  
  - title: Performance
    requirements:
      - id: Performance-ResponseTime
        title: The system shall respond within 100ms.
        tags:
          - performance
          - critical
```

**Key points**:

- Tags are optional - requirements without tags are always included when no filter is specified
- Tag identifiers can be any string value
- A requirement can have multiple tags
- Tag matching is case-sensitive

### Filtering by Tags

Use the `--filter` option to export only requirements with specific tags:

```bash
# Export only security-related requirements
reqstream --requirements "**/*.yaml" --filter security --report security_report.md

# Export requirements with multiple tags (OR logic - matches any tag)
reqstream --requirements "**/*.yaml" --filter security,critical --report critical_report.md
```

**Filtering applies to**:

- **Requirements Reports** (`--report`): Only requirements with matching tags are included
- **Trace Matrix** (`--matrix`): Only requirements with matching tags appear in the matrix. Summary counts
  (satisfied/total) reflect only the filtered requirements
- **Justifications** (`--justifications`): Only justifications for requirements with matching tags are exported
- **Enforcement** (`--enforce`): Only requirements with matching tags are checked for test coverage

**Filter behavior**:

- Multiple tags are comma-separated and use OR logic (requirement matches if it has ANY of the specified tags)
- If no `--filter` is specified, all requirements are included (default behavior)
- Requirements without any tags are excluded when a filter is active
- Empty tag filters (`--filter ""`) are treated as no filter (all requirements included)

### Use Cases

**Security Audit**:

```bash
# Export only security requirements and their trace matrix
reqstream \
  --requirements "docs/**/*.yaml" \
  --tests "test-results/**/*.trx" \
  --filter security \
  --report security_requirements.md \
  --matrix security_trace_matrix.md
```

**Critical Requirements Enforcement**:

```bash
# Enforce test coverage only for critical requirements
reqstream \
  --requirements "docs/**/*.yaml" \
  --tests "test-results/**/*.trx" \
  --filter critical \
  --enforce
```

**Compliance Documentation**:

```bash
# Export justifications for compliance-related requirements
reqstream \
  --requirements "docs/**/*.yaml" \
  --filter compliance,regulatory \
  --justifications compliance_justifications.md
```

## Requirements Enforcement

ReqStream can enforce that all requirements have adequate test coverage, making it ideal for use in CI/CD pipelines
to ensure quality gates are met.

### Enforcement Mode

Use the `--enforce` flag to fail the build if any requirements are not fully satisfied with tests:

```bash
reqstream --requirements "**/*.yaml" --tests "**/*.trx" --enforce
```

When enforcement mode is enabled:

- All requirements must have at least one test mapped (either directly or through child requirements)
- All mapped tests must be present in the test results
- All mapped tests must pass
- If any requirement is not satisfied, an error is reported and the exit code is non-zero

### CI/CD Integration

Enforcement mode is designed for CI/CD pipelines. The error message is printed after all reports are generated,
allowing you to review the reports for failure analysis:

```bash
# GitHub Actions example
- name: Validate Requirements Coverage
  run: |
    dotnet reqstream \
      --requirements "docs/**/*.yaml" \
      --tests "test-results/**/*.trx" \
      --matrix trace-matrix.md \
      --enforce
```

If requirements are not fully satisfied, the tool will print:

```text
Error: Only X of Y requirements are satisfied with tests.
```

And exit with code 1, failing the build.

### Best Practices

- Use `--enforce` in CI/CD to prevent merging code that reduces requirements coverage
- Generate the trace matrix (`--matrix`) alongside enforcement to review coverage details
- Start without enforcement initially, then enable it once baseline coverage is established
- Use transitive coverage through child requirements for high-level requirements that don't have direct tests

## Justifications Export

ReqStream can export a markdown document that shows each requirement's ID, title, and justification. This is useful
for documentation, compliance audits, and communicating the rationale behind requirements to stakeholders.

### Exporting Justifications

Use the `--justifications` flag to export a justifications document:

```bash
reqstream --requirements "**/*.yaml" --justifications justifications.md
```

This generates a markdown file organized by sections, with each requirement showing:

- Requirement ID and title as a header
- The justification text (if provided)

### Example Output

Given requirements with justifications:

```yaml
sections:
  - title: Security
    requirements:
      - id: Security-DataEncryption
        title: The system shall encrypt all data at rest.
        justification: |
          Data encryption at rest protects sensitive information from unauthorized access
          in case of physical storage theft or unauthorized access to storage media.
```

The exported justifications document would look like:

```markdown
# Security

## Security-DataEncryption

**The system shall encrypt all data at rest.**

Data encryption at rest protects sensitive information from unauthorized access
in case of physical storage theft or unauthorized access to storage media.
```

### Configuring Header Depth

Use the `--depth` option to set the default markdown header depth for all reports (default: 1). Individual
report depths can be overridden with `--report-depth`, `--matrix-depth`, or `--justifications-depth`:

```bash
reqstream --requirements "**/*.yaml" --justifications justifications.md --depth 2
```

```bash
reqstream --requirements "**/*.yaml" --justifications justifications.md --depth 2 --justifications-depth 3
```

This adjusts the header levels in the output, useful when embedding the reports in larger documentation
structures.

## Building

```pwsh
pwsh ./build.ps1
```

## User Guide

The ReqStream User Guide is available on the
[ReqStream releases page](https://github.com/demaconsulting/ReqStream/releases).

## Contributing

Contributions are welcome! We appreciate your interest in improving ReqStream.

Please see our [Contributing Guidelines][contributing] for details on:

- Reporting bugs
- Suggesting features
- Submitting pull requests
- Development setup
- Coding standards

Please note that this project is released with a [Contributor Code of Conduct][code-of-conduct]. By participating
in this project you agree to abide by its terms.

## License

This project is licensed under the MIT License — see the [LICENSE][license] file for details.

By contributing to this project, you agree that your contributions will be licensed under the MIT License.

## Support

- [Report a bug or request a feature][issues]
- [Ask a question or start a discussion][discussions]

## Acknowledgements

ReqStream is built with the help of these amazing open-source projects:

- [.NET][dotnet] - Cross-platform framework by Microsoft
- [YamlDotNet][yamldotnet] - YAML parser for .NET
- [MSTest][mstest] - Testing framework
- [GitHub Actions][github-actions] - CI/CD automation
- [SonarCloud][sonarcloud] - Code quality and security analysis

## Security

For information about reporting security vulnerabilities, please see our [Security Policy][security].

[forks-shield]: https://img.shields.io/github/forks/demaconsulting/ReqStream?style=plastic
[forks-url]: https://github.com/demaconsulting/ReqStream/network/members
[stars-shield]: https://img.shields.io/github/stars/demaconsulting/ReqStream?style=plastic
[stars-url]: https://github.com/demaconsulting/ReqStream/stargazers
[contributors-shield]: https://img.shields.io/github/contributors/demaconsulting/ReqStream
[contributors-url]: https://github.com/demaconsulting/ReqStream/graphs/contributors
[license-shield]: https://img.shields.io/github/license/demaconsulting/ReqStream
[license-url]: https://github.com/demaconsulting/ReqStream/blob/main/LICENSE
[build-shield]: https://img.shields.io/github/actions/workflow/status/demaconsulting/ReqStream/build_on_push.yaml
[build-url]: https://github.com/demaconsulting/ReqStream/actions/workflows/build_on_push.yaml
[quality-gate-shield]: https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_ReqStream&metric=alert_status
[quality-gate-url]: https://sonarcloud.io/dashboard?id=demaconsulting_ReqStream
[security-shield]: https://sonarcloud.io/api/project_badges/measure?project=demaconsulting_ReqStream&metric=security_rating
[security-url]: https://sonarcloud.io/dashboard?id=demaconsulting_ReqStream
[nuget-shield]: https://img.shields.io/nuget/v/DemaConsulting.ReqStream
[nuget-url]: https://www.nuget.org/packages/DemaConsulting.ReqStream
[license]: https://github.com/demaconsulting/ReqStream/blob/main/LICENSE
[contributing]: https://github.com/demaconsulting/ReqStream/blob/main/CONTRIBUTING.md
[code-of-conduct]: https://github.com/demaconsulting/ReqStream/blob/main/CODE_OF_CONDUCT.md
[security]: https://github.com/demaconsulting/ReqStream/blob/main/SECURITY.md
[dotnet-sdk]: https://dotnet.microsoft.com/download
[issues]: https://github.com/demaconsulting/ReqStream/issues
[discussions]: https://github.com/demaconsulting/ReqStream/discussions
[dotnet]: https://dotnet.microsoft.com/
[yamldotnet]: https://github.com/aaubry/YamlDotNet
[mstest]: https://github.com/microsoft/testfx
[github-actions]: https://github.com/features/actions
[sonarcloud]: https://sonarcloud.io
[link-guide]: https://github.com/demaconsulting/ReqStream/releases
[link-continuous-compliance]: https://github.com/demaconsulting/ContinuousCompliance
