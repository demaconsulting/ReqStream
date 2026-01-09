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

| Feature | Description |
|---------|-------------|
| 📝 **YAML Format** | Manage requirements in human-readable YAML format |
| 🔧 **Command-Line Interface** | Automate requirement management with CLI tools |
| 🌐 **Multi-Platform** | Support for .NET 8, 9, and 10 across Windows, Linux, and macOS |
| 🔗 **Hierarchical Structure** | Organize requirements with sections and subsections |
| 🧪 **Test Mapping** | Link requirements to test cases for traceability |
| 📦 **File Includes** | Modularize requirements across multiple YAML files |
| ✅ **Validation** | Built-in validation for requirement structure and references |

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

```bash
reqstream help
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
- **File Includes**: Use the `includes` section to reference other YAML files containing additional requirements
  or test mappings

## Development

### Prerequisites

- .NET SDK 8.0, 9.0, or 10.0
- C# 12

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

### 🐛 Report a Bug

Found a bug? Please [open an issue][bug-report] with:

- A clear description of the problem
- Steps to reproduce
- Expected vs. actual behavior
- Version information

### 💡 Request a Feature

Have an idea? Please [open an issue][feature-request] with:

- Description of the problem you're solving
- Your proposed solution
- Any alternatives you've considered

### 💬 Ask Questions

Have questions? Feel free to:

- [Open a discussion][discussions]
- [Open an issue][issues]
- Check existing documentation

## Acknowledgements

ReqStream is built with the help of these amazing open-source projects:

- [.NET][dotnet] - Cross-platform framework by Microsoft
- [YamlDotNet][yamldotnet] - YAML parser for .NET
- [MSTest][mstest] - Testing framework
- [GitHub Actions][github-actions] - CI/CD automation
- [SonarCloud][sonarcloud] - Code quality and security analysis

## Security

For information about reporting security vulnerabilities, please see our [Security Policy][security].

[forks-shield]: https://img.shields.io/github/forks/demaconsulting/ReqStream?style=flat-square
[forks-url]: https://github.com/demaconsulting/ReqStream/network/members
[stars-shield]: https://img.shields.io/github/stars/demaconsulting/ReqStream?style=flat-square
[stars-url]: https://github.com/demaconsulting/ReqStream/stargazers
[contributors-shield]: https://img.shields.io/github/contributors/demaconsulting/ReqStream?style=flat-square
[contributors-url]: https://github.com/demaconsulting/ReqStream/graphs/contributors
[license-shield]: https://img.shields.io/github/license/demaconsulting/ReqStream?style=flat-square
[license-url]: https://github.com/demaconsulting/ReqStream/blob/main/LICENSE
[build-shield]: https://img.shields.io/github/actions/workflow/status/demaconsulting/ReqStream/build_on_push.yaml?style=flat-square
[build-url]: https://github.com/demaconsulting/ReqStream/actions/workflows/build_on_push.yaml
[quality-gate-shield]: https://img.shields.io/sonar/quality_gate/demaconsulting_ReqStream?server=https%3A%2F%2Fsonarcloud.io&style=flat-square
[quality-gate-url]: https://sonarcloud.io/dashboard?id=demaconsulting_ReqStream
[security-shield]: https://img.shields.io/sonar/security_rating/demaconsulting_ReqStream?server=https%3A%2F%2Fsonarcloud.io&style=flat-square
[security-url]: https://sonarcloud.io/dashboard?id=demaconsulting_ReqStream
[nuget-shield]: https://img.shields.io/nuget/v/DemaConsulting.ReqStream?style=flat-square
[nuget-url]: https://www.nuget.org/packages/DemaConsulting.ReqStream
[license]: https://github.com/demaconsulting/ReqStream/blob/main/LICENSE
[contributing]: https://github.com/demaconsulting/ReqStream/blob/main/CONTRIBUTING.md
[code-of-conduct]: https://github.com/demaconsulting/ReqStream/blob/main/CODE_OF_CONDUCT.md
[security]: https://github.com/demaconsulting/ReqStream/blob/main/SECURITY.md
[dotnet-sdk]: https://dotnet.microsoft.com/download
[bug-report]: https://github.com/demaconsulting/ReqStream/issues/new?template=bug_report.md
[feature-request]: https://github.com/demaconsulting/ReqStream/issues/new?template=feature_request.md
[issues]: https://github.com/demaconsulting/ReqStream/issues
[discussions]: https://github.com/demaconsulting/ReqStream/discussions
[dotnet]: https://dotnet.microsoft.com/
[yamldotnet]: https://github.com/aaubry/YamlDotNet
[mstest]: https://github.com/microsoft/testfx
[github-actions]: https://github.com/features/actions
[sonarcloud]: https://sonarcloud.io
