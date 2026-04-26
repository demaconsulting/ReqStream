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

- **`Context.Create`** (Outbound) — Factory method constructing a `Context` from `string[] args`.
- **`Context.Version`** (Outbound) — `true` when `--version` was specified.
- **`Context.Help`** (Outbound) — `true` when `--help` was specified.
- **`Context.Silent`** (Outbound) — `true` when `--silent` was specified.
- **`Context.Validate`** (Outbound) — `true` when `--validate` was specified.
- **`Context.Lint`** (Outbound) — `true` when `--lint` was specified.
- **`Context.Enforce`** (Outbound) — `true` when `--enforce` was specified.
- **`Context.ResultsFile`** (Outbound) — Path for validation results output file (`--results`), or `null`.
- **`Context.FilterTags`** (Outbound) — Set of filter tags from `--filter`, or `null` when not specified.
- **`Context.RequirementsFiles`** (Outbound) — Glob-expanded list of requirements file paths from `--requirements`.
- **`Context.TestFiles`** (Outbound) — Glob-expanded list of test result file paths from `--tests`.
- **`Context.RequirementsReport`** (Outbound) — Path for requirements report output file (`--report`), or `null`.
- **`Context.Depth`** (Outbound) — Default markdown header depth for all reports (`--depth`; default 1).
- **`Context.ReportDepth`** (Outbound) — Markdown header depth for the requirements report.
- **`Context.Matrix`** (Outbound) — Path for trace matrix output file (`--matrix`), or `null`.
- **`Context.MatrixDepth`** (Outbound) — Markdown header depth for the trace matrix.
- **`Context.JustificationsFile`** (Outbound) — Path for justifications output file (`--justifications`), or `null`.
- **`Context.JustificationsDepth`** (Outbound) — Markdown header depth for the justifications report.
- **`Context.WriteLine`** (Outbound) — Writes a message to console and optional log file.
- **`Context.WriteError`** (Outbound) — Writes an error to stderr (suppressed when `--silent` is active),
  appends it to the log file if logging is enabled, and sets the error exit code.
- **`Context.ExitCode`** (Outbound) — Returns 0 for success or 1 when errors have been reported.
- **`Context.Dispose`** (Outbound) — Closes the log file writer and releases resources.

## Interactions

The `Cli` subsystem has no dependencies on other tool subsystems. It uses only .NET base
class library types. The `Program` unit at system level creates the `Context` and passes it
to all subsystems that need to produce output.

## Error Handling

`Context.Create` throws `ArgumentException` under the following conditions:

- **Unknown argument** — An unrecognized flag is present in `args`.
- **Missing argument value** — A flag that requires a value is the last argument (no value follows).
- **Invalid depth value** — A `--depth`, `--report-depth`, `--matrix-depth`,
  or `--justifications-depth` value is not a positive integer.
- **Log file open failure** — The file path provided to `--log` cannot be opened for writing.

## Depth Inheritance

`Context.ReportDepth`, `Context.MatrixDepth`, and `Context.JustificationsDepth` all default
to `Context.Depth` when not individually overridden by `--report-depth`, `--matrix-depth`, or
`--justifications-depth` respectively. This means that `--depth 2` applies to all three reports
unless a report-specific depth flag is also present.

## References

- [ReqStream System Design][arch]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[repo]: https://github.com/demaconsulting/ReqStream
