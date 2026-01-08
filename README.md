# ReqStream

Requirements Management Tool

## Overview

ReqStream is a .NET command-line tool for managing requirements written in YAML files. It provides functionality to
create, validate, and manage requirement documents in a structured and maintainable way.

## Features

- Manage requirements in YAML format
- Command-line interface for automation
- Multi-platform support (.NET 8, 9, and 10)

## Installation

Install ReqStream as a global .NET tool:

```bash
dotnet tool install -g DemaConsulting.ReqStream
```

Or as a local tool in your project:

```bash
dotnet tool install DemaConsulting.ReqStream
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

## License

This project is licensed under the MIT License - see the [LICENSE][license] file for details.

## Contributing

Contributions are welcome! Please see our [Contributing Guidelines][contributing] for details on how to get started.

Please note that this project is released with a [Contributor Code of Conduct][code-of-conduct]. By participating
in this project you agree to abide by its terms.

## Security

For information about reporting security vulnerabilities, please see our [Security Policy][security].

[license]: https://github.com/demaconsulting/ReqStream/blob/main/LICENSE
[contributing]: https://github.com/demaconsulting/ReqStream/blob/main/CONTRIBUTING.md
[code-of-conduct]: https://github.com/demaconsulting/ReqStream/blob/main/CODE_OF_CONDUCT.md
[security]: https://github.com/demaconsulting/ReqStream/blob/main/SECURITY.md
