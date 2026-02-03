# ReqStream

[![GitHub forks][forks-shield]][forks-url]
[![GitHub stars][stars-shield]][stars-url]
[![GitHub contributors][contributors-shield]][contributors-url]
[![License][license-shield]][license-url]
[![Build][build-shield]][build-url]
[![Quality Gate][quality-gate-shield]][quality-gate-url]
[![Security][security-shield]][security-url]
[![NuGet][nuget-shield]][nuget-url]

Requirements Management Tool

## Overview

ReqStream is a .NET command-line tool for managing requirements written in YAML files. It provides functionality to
create, validate, and manage requirement documents in a structured and maintainable way.

## Features

- 📝 **YAML Format** - Manage requirements in human-readable YAML format
- 🔧 **Command-Line Interface** - Automate requirement management with CLI tools
- 🌐 **Multi-Platform** - Support for .NET 8, 9, and 10 across Windows, Linux, and macOS
- 🔗 **Hierarchical Structure** - Organize requirements with sections and subsections
- 🧪 **Test Mapping** - Link requirements to test cases for traceability
- 📦 **File Includes** - Modularize requirements across multiple YAML files
- ✅ **Validation** - Built-in validation for requirement structure and references

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
  -v, --version              Display version information
  -?, -h, --help             Display this help message
  --silent                   Suppress console output
  --validate                 Run self-validation
  --results <file>           Write validation results to file (TRX or JUnit format)
  --log <file>               Write output to log file
  --requirements <pattern>   Requirements files glob pattern
  --report <file>            Export requirements to markdown file
  --report-depth <depth>     Markdown header depth for requirements report (default: 1)
  --tests <pattern>          Test result files glob pattern (TRX or JUnit)
  --matrix <file>            Export trace matrix to markdown file
  --matrix-depth <depth>     Markdown header depth for trace matrix (default: 1)
  --enforce                  Fail if requirements are not fully tested
```

## YAML Format

ReqStream uses YAML files to define and manage requirements. The YAML format supports a hierarchical structure
with sections, requirements, test mappings, and file includes.

### Basic Structure

```yaml
---
# Requirements YAML file

# Requirement sections 
sections:
  - title: "System Security"
    requirements:
      - id: "SYS-SEC-001"
        title: "The system shall support credentials authentication."
        children: # Support linking to child requirements
          - "AUTH-001"

  - title: "Data Management"
    sections:
      - title: "User Authentication"
        requirements:
          - id: "AUTH-001"
            title: "All requests shall have their credentials authenticated before being processed."
            tests: # Support test-mapping inline with requirements
              - "Credentials_Valid_Allowed"
              - "Credentials_Invalid_Refused"
              - "Credentials_Missing_Refused"

      - title: "Logging"
        requirements:
          - id: "DATA-001"
            title: "All requests shall be logged."

# Include other requirement files - may contain requirements and/or test mappings
includes:
  - more_requirements.yaml
  - test_mappings.yaml

# Test mappings support defining tests separate from requirements
mappings:
  - id: "DATA-001"
    tests:
      - "Logging_ValidRequest_Logged"
      - "Logging_InvalidRequest_Logged"
```

### Key Features

- **Requirement IDs**: Can be of any format, but must be unique across all requirement files
- **Section Merging**: Identical sections (where the full hierarchy is identical) are automatically merged,
  allowing included files to add more sections or requirements to existing sections
- **Child Requirements**: Requirements can reference other requirements as children using the `children` field
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
  - id: "PLAT-001"
    title: "Shall support Windows"
    tests:
      - "windows-latest@Test_PlatformFeature"  # Matches only from files containing "windows-latest"
  
  - id: "PLAT-002"
    title: "Shall support Linux"
    tests:
      - "ubuntu-latest@Test_PlatformFeature"   # Matches only from files containing "ubuntu-latest"
  
  - id: "PLAT-003"
    title: "Shall support cross-platform features"
    tests:
      - "Test_CrossPlatformFeature"             # Aggregates from all test result files
```

**Key behaviors**:

- Tests with source specifiers only match results from files containing the specified `filepart`
- Tests without source specifiers aggregate results from all test result files
- File part matching is case-insensitive and supports partial filename matching
- Both plain and source-specific test names can be mixed in the same requirement

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

## Development

### Requirements

- .NET SDK 8.0, 9.0, or 10.0
- C# 12

### Architecture

For a comprehensive guide to the architecture and internal workings of ReqStream, see the
[Architecture Documentation][architecture].

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Packaging

```bash
dotnet pack
```

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

This project is licensed under the MIT License - see the [LICENSE][license] file for details.

## Support

- 🐛 **[Report a Bug][bug-report]** - Found an issue? Let us know
- 💡 **[Request a Feature][feature-request]** - Have an idea? Share it with us
- 💬 **[Ask Questions][discussions]** - Need help? Start a discussion

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
[architecture]: https://github.com/demaconsulting/ReqStream/blob/main/ARCHITECTURE.md
[contributing]: https://github.com/demaconsulting/ReqStream/blob/main/CONTRIBUTING.md
[code-of-conduct]: https://github.com/demaconsulting/ReqStream/blob/main/CODE_OF_CONDUCT.md
[security]: https://github.com/demaconsulting/ReqStream/blob/main/SECURITY.md
[dotnet-sdk]: https://dotnet.microsoft.com/download
[bug-report]: https://github.com/demaconsulting/ReqStream/issues/new?template=bug_report.md
[feature-request]: https://github.com/demaconsulting/ReqStream/issues/new?template=feature_request.md
[discussions]: https://github.com/demaconsulting/ReqStream/discussions
[dotnet]: https://dotnet.microsoft.com/
[yamldotnet]: https://github.com/aaubry/YamlDotNet
[mstest]: https://github.com/microsoft/testfx
[github-actions]: https://github.com/features/actions
[sonarcloud]: https://sonarcloud.io
