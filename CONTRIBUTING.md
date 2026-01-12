# Contributing to ReqStream

Thank you for your interest in contributing to ReqStream! This document provides guidelines and instructions for
contributing to the project.

## Code of Conduct

This project adheres to the Contributor Covenant [Code of Conduct][code-of-conduct]. By participating, you are
expected to uphold this code. Please report unacceptable behavior through [GitHub Issues][issues] or by contacting
the project maintainers directly.

## How to Contribute

### Reporting Bugs

If you find a bug, please create an issue using the bug report template. Include:

- A clear description of the problem
- Steps to reproduce the issue
- Expected vs. actual behavior
- Version information (ReqStream version, .NET version, OS)
- Any relevant error messages or logs

### Suggesting Features

We welcome feature suggestions! Please create an issue using the feature request template. Include:

- A clear description of the problem you're trying to solve
- Your proposed solution
- Any alternative solutions you've considered
- Examples or mockups if applicable

### Contributing Code

1. **Fork the repository** and create a branch for your changes
2. **Make your changes** following the coding standards below
3. **Test your changes** thoroughly
4. **Submit a pull request** with a clear description of your changes

## Development Setup

### Prerequisites

- .NET SDK 8.0, 9.0, or 10.0
- Git
- A code editor (Visual Studio, VS Code, JetBrains Rider, etc.)

### Getting Started

1. Clone the repository:

   ```bash
   git clone https://github.com/demaconsulting/ReqStream.git
   cd ReqStream
   ```

2. Restore dependencies:

   ```bash
   dotnet restore
   ```

3. Build the solution:

   ```bash
   dotnet build
   ```

4. Run tests:

   ```bash
   dotnet test
   ```

## Coding Standards

### Code Style

This project follows the coding standards defined in `.editorconfig`. Key conventions:

- **Indentation**: 4 spaces for C#, 2 spaces for YAML/JSON/XML
- **Line endings**: LF (Unix-style)
- **Encoding**: UTF-8 with BOM
- **Braces**: Required for all control statements
- **Naming**:
  - Interfaces: `IRequirementParser`
  - Classes/Structs/Enums: `PascalCase`
  - Methods/Properties: `PascalCase`
  - Parameters/Local variables: `camelCase`

### Documentation

- Use XML documentation comments (`///`) for all public APIs
- Include meaningful comments for complex logic
- Keep README.md and other documentation up to date

### Testing

- All new features must include tests
- Tests should follow the AAA (Arrange, Act, Assert) pattern
- Test method naming: `TestMethod_Scenario_ExpectedBehavior`
- All tests must pass before submitting a PR
- Aim for high code coverage (>80%)

### Commit Messages

Write clear, descriptive commit messages:

- Use the imperative mood ("Add feature" not "Added feature")
- Keep the first line under 72 characters
- Add details in the body if needed

Example:

```text
Add validation for requirement IDs

- Ensures requirement IDs are unique
- Adds tests for duplicate ID detection
- Updates error messages for clarity
```

## Quality Checks

Before submitting a pull request, ensure your code passes all quality checks:

### Build and Test

```bash
dotnet restore
dotnet build --configuration Release
dotnet test --configuration Release
```

### Linting

The CI pipeline runs the following checks:

- **Markdown linting**: Checks markdown file formatting
- **Spell checking**: Validates spelling in markdown and C# files
- **YAML linting**: Validates YAML file structure

You can run these locally if you have the tools installed:

```bash
# Markdown linting
markdownlint-cli2 "**/*.md"

# Spell checking
cspell "**/*.md" "**/*.cs"

# YAML linting
yamllint .
```

## Pull Request Process

1. **Update documentation** if your changes affect usage or behavior
2. **Add tests** for new functionality or bug fixes
3. **Ensure all tests pass** and the code builds without warnings
4. **Update the README** if necessary
5. **Submit the PR** with a clear description of your changes
6. **Address review feedback** promptly

### PR Guidelines

- Keep PRs focused on a single feature or fix
- Write a clear PR description explaining what and why
- Reference any related issues
- Be responsive to review feedback
- Ensure CI checks pass

## Project Structure

```text
ReqStream/
├── .github/              # GitHub configuration (workflows, issue templates)
├── src/                  # Source code
│   └── DemaConsulting.ReqStream/
├── test/                 # Test projects
│   └── DemaConsulting.ReqStream.Tests/
├── .editorconfig         # Code style configuration
├── AGENTS.md             # AI agent guidelines
├── CODE_OF_CONDUCT.md    # Code of conduct
├── CONTRIBUTING.md       # This file
├── LICENSE               # MIT License
└── README.md             # Project documentation
```

## Questions?

If you have questions about contributing, feel free to:

- Open an issue for discussion
- Reach out to the maintainers
- Check the [AGENTS.md][agents] file for detailed technical guidelines

## License

By contributing to ReqStream, you agree that your contributions will be licensed under the MIT License.

Thank you for contributing to ReqStream!

[code-of-conduct]: CODE_OF_CONDUCT.md
[agents]: AGENTS.md
[issues]: https://github.com/demaconsulting/ReqStream/issues
