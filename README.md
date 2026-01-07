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

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
