# Cli Subsystem Design

The `Cli` subsystem provides the command-line interface for ReqStream.
It is responsible for accepting user input from the command line and routing output to
the console and an optional log file.

## Overview

The `Cli` subsystem acts as the primary boundary between the user's shell invocation and
the tool's internal logic. It owns argument parsing, output formatting, and error tracking.
All other subsystems receive a `Context` object from the `Cli` subsystem to read parsed
flags and write output.

## Units

The `Cli` subsystem contains the following software unit:

| Unit                          | File             | Responsibility                                                |
|-------------------------------|------------------|---------------------------------------------------------------|
| `Context`                     | `Cli/Context.cs` | Argument parsing, output channels, and exit code.             |

## Interfaces

The `Cli` subsystem exposes the following interface to the rest of the tool:

| Interface                     | Direction | Description                                                          |
|-------------------------------|-----------|----------------------------------------------------------------------|
| `Context.Create`              | Outbound  | Factory method constructing a `Context` from `string[] args`.        |
| `Context.Version`             | Outbound  | `true` when `--version` was specified.                               |
| `Context.Help`                | Outbound  | `true` when `--help` was specified.                                  |
| `Context.Silent`              | Outbound  | `true` when `--silent` was specified.                                |
| `Context.Validate`            | Outbound  | `true` when `--validate` was specified.                              |
| `Context.Lint`                | Outbound  | `true` when `--lint` was specified.                                  |
| `Context.Enforce`             | Outbound  | `true` when `--enforce` was specified.                               |
| `Context.ResultsFile`         | Outbound  | Path for validation results output file (`--results`), or `null`.    |
| `Context.FilterTags`          | Outbound  | Set of filter tags from `--filter`, or `null` when not specified.    |
| `Context.RequirementsFiles`   | Outbound  | Glob-expanded list of requirements file paths from `--requirements`. |
| `Context.TestFiles`           | Outbound  | Glob-expanded list of test result file paths from `--tests`.         |
| `Context.RequirementsReport`  | Outbound  | Path for requirements report output file (`--report`), or `null`.    |
| `Context.Depth`               | Outbound  | Default markdown header depth for all reports (`--depth`; default 1).|
| `Context.ReportDepth`         | Outbound  | Markdown header depth for the requirements report.                   |
| `Context.Matrix`              | Outbound  | Path for trace matrix output file (`--matrix`), or `null`.           |
| `Context.MatrixDepth`         | Outbound  | Markdown header depth for the trace matrix.                          |
| `Context.JustificationsFile`  | Outbound  | Path for justifications output file (`--justifications`), or `null`. |
| `Context.JustificationsDepth` | Outbound  | Markdown header depth for the justifications report.                 |
| `Context.WriteLine`           | Outbound  | Writes a message to console and optional log file.                   |
| `Context.WriteError`          | Outbound  | Writes an error to stderr and sets the error exit code.              |
| `Context.ExitCode`            | Outbound  | Returns 0 for success or 1 when errors have been reported.           |

## Interactions

The `Cli` subsystem has no dependencies on other tool subsystems. It uses only .NET base
class library types. The `Program` unit at system level creates the `Context` and passes it
to all subsystems that need to produce output.

## References

- [ReqStream System Design][arch]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[repo]: https://github.com/demaconsulting/ReqStream
