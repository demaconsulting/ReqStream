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

| Unit      | File             | Responsibility                                    |
|-----------|------------------|---------------------------------------------------|
| `Context` | `Cli/Context.cs` | Argument parsing, output channels, and exit code. |

## Interfaces

The `Cli` subsystem exposes the following interface to the rest of the tool:

| Interface            | Direction | Description                                                   |
|----------------------|-----------|---------------------------------------------------------------|
| `Context.Create`     | Outbound  | Factory method constructing a `Context` from `string[] args`. |
| `Context.WriteLine`  | Outbound  | Writes a message to console and optional log file.            |
| `Context.WriteError` | Outbound  | Writes an error to stderr and sets the error exit code.       |
| `Context.ExitCode`   | Outbound  | Returns 0 for success or 1 when errors have been reported.    |

## Interactions

The `Cli` subsystem has no dependencies on other tool subsystems. It uses only .NET base
class library types. The `Program` unit at system level creates the `Context` and passes it
to all subsystems that need to produce output.

## References

- [ReqStream Architecture][arch]
- [ReqStream Repository][repo]

[arch]: ../../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
