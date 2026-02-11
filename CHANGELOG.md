# Changelog

All notable changes to the ReqStream project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

- Build scripts for convenient development (`build.sh`, `build.bat`)
- Lint scripts for code quality checks (`lint.sh`, `lint.bat`)
- VS Code tasks for improved developer experience (`.vscode/tasks.json`)
- Markdownlint configuration with ignore patterns (`.markdownlint-cli2.jsonc`)
- Enhanced spell-check configuration with binary file exclusions (`.cspell.json`)
- CHANGELOG.md following Keep a Changelog format

### Changed

- Converted solution file from `.sln` to `.slnx` format (XML-based modern format)
- Updated `.gitignore` to exclude agent-generated reports while tracking VS Code tasks
- Enhanced build workflow to include integration test results in documentation build
- Updated all tooling references to use `.slnx` format
- Removed version and date metadata from ARCHITECTURE.md (should reflect current repo state)

### Changed

- Updated `.gitignore` to exclude agent-generated reports while tracking VS Code tasks
- Fixed ARCHITECTURE.md date (changed from future date to correct date)
- Enhanced build workflow to include integration test results in documentation build

### Fixed

- Code formatting issues (24 whitespace and charset violations)
- Integration test results now properly included in documentation generation

## [1.0.0] - Initial Release

### Added

- Requirements management tool for YAML files
- Command-line interface with support for:
  - `--version`: Display version information
  - `--help`: Display usage help
  - `--validate`: Run self-validation tests
  - `--requirements`: Specify requirements YAML file
  - `--tests`: Specify test results (TRX format)
  - `--report`: Generate requirements report
  - `--matrix`: Generate trace matrix
  - `--justifications`: Export justifications
  - `--enforce`: Enforce all requirements are satisfied
  - `--silent`: Suppress normal output
  - `--log`: Write output to log file
- Support for multiple .NET target frameworks (net8.0, net9.0, net10.0)
- Comprehensive test suite with 117 unit tests
- Self-validation tests demonstrating real-world usage
- Integration tests across multiple platforms and .NET versions
- GitHub Actions CI/CD pipeline
- CodeQL security analysis
- SonarCloud quality analysis
- Documentation generation with Pandoc and Weasyprint
- Requirements traceability with test source linking
- NuGet package distribution

[Unreleased]: https://github.com/demaconsulting/ReqStream/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/demaconsulting/ReqStream/releases/tag/v1.0.0
