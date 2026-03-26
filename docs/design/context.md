# Context Design

## Overview

The `Context` class is responsible for parsing command-line arguments and providing a unified interface for
program output. It is the sole source of truth for all runtime configuration options and controls whether
output is written to the console, a log file, or suppressed entirely.

## Structure

`Context` is a `sealed` class implementing `IDisposable`. It is instantiated exclusively through the static
factory method `Context.Create(string[] args)`, which keeps the constructor private.

### Properties

All properties are immutable after construction (`private init`):

| Property | Type | Description |
| :--- | :--- | :--- |
| `Version` | `bool` | `--version` flag was specified |
| `Help` | `bool` | `--help` flag was specified |
| `Silent` | `bool` | `--silent` flag suppresses console output |
| `Validate` | `bool` | `--validate` flag triggers self-validation |
| `Enforce` | `bool` | `--enforce` flag enables coverage enforcement |
| `FilterTags` | `HashSet<string>?` | Tag filter set; `null` if not specified |
| `RequirementsFiles` | `List<string>` | Expanded paths from `--requirements` glob pattern |
| `TestFiles` | `List<string>` | Expanded paths from `--tests` glob pattern |
| `RequirementsReport` | `string?` | Output path for `--report` |
| `ReportDepth` | `int` | Markdown heading depth for the report (default: 1) |
| `Matrix` | `string?` | Output path for `--matrix` |
| `MatrixDepth` | `int` | Markdown heading depth for the matrix (default: 1) |
| `JustificationsFile` | `string?` | Output path for `--justifications` |
| `JustificationsDepth` | `int` | Markdown heading depth for justifications (default: 1) |
| `ResultsFile` | `string?` | Output path for `--results` (validation test results) |
| `ExitCode` | `int` | Returns 0 (success) or 1 (errors reported) |

### Output Methods

| Method | Behavior |
| :--- | :--- |
| `WriteLine(string)` | Writes to console (unless `Silent`) and optional log file |
| `WriteError(string)` | Writes to stderr in red (unless `Silent`), writes to log file, sets `_hasErrors = true` |

### Argument Parsing

`Create` uses a `while` loop with a `switch` statement over each argument token. Arguments requiring a
value consume the next token from the array. An `ArgumentException` is thrown for unknown arguments or
missing values.

Glob patterns (from `--requirements` and `--tests`) are expanded using
`Microsoft.Extensions.FileSystemGlobbing.Matcher` against the current working directory.

### Log File

If `--log <file>` is specified, a `StreamWriter` is opened with `AutoFlush = true`. The writer is stored
in the private `_logWriter` field and is closed by `Dispose()`.

## Key Design Decisions

- **Private constructor with factory method**: Enforces that instances are only created through `Create`,
  keeping validation logic centralized.
- **Immutable properties**: All configuration properties use `private init` to prevent accidental mutation
  after parsing.
- **Glob expansion at parse time**: File lists are fully resolved during argument parsing so the rest of the
  program works with concrete file paths.
- **Error tracking via `WriteError`**: Any code path that calls `WriteError` automatically marks the session
  as having errors, ensuring a non-zero exit code without requiring explicit error propagation.

## Relationships

- **Created by**: `Program.Main` passes `string[] args` to `Context.Create`
- **Consumed by**: `Program.Run`, `Program.ProcessRequirements`, `Validation.Run`, and all export methods
  use `Context` for output and configuration
- **Owns**: Optional `StreamWriter` for log file output (released via `IDisposable`)
