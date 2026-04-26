# SelfTest Subsystem Design

The `SelfTest` subsystem provides the self-validation framework for ReqStream.
It runs a built-in suite of tests to demonstrate the tool is functioning correctly in the
deployment environment.

## Overview

The `SelfTest` subsystem is invoked when the user passes `--validate` on the command line.
It exercises the tool's own capabilities and reports a pass/fail summary. It can also write
test results to a file in TRX or JUnit XML format for integration with CI/CD pipelines.

## Units

The `SelfTest` subsystem contains the following software unit:

| Unit         | File                     | Responsibility                                     |
|--------------|--------------------------|----------------------------------------------------|
| `Validation` | `SelfTest/Validation.cs` | Orchestrating and executing self-validation tests. |

## Interfaces

The `SelfTest` subsystem exposes the following interface to the rest of the tool:

| Interface        | Description                                                           |
|------------------|-----------------------------------------------------------------------|
| `Validation.Run` | Runs all self-validation tests, prints a summary, and writes results. |

## Interactions

| Dependency | Direction | Purpose                                                      |
|------------|-----------|--------------------------------------------------------------|
| `Context`  | Uses      | Output channel for header lines, test summaries, and errors. |
| `Program`  | Uses      | `Program.Run` is called internally to exercise the tool.     |

## Error Handling

The `SelfTest` subsystem handles the following error conditions:

- **One or more self-validation tests fail** — `context.WriteError` is called for each failing test;
  the method returns without setting a success state, so `context.ExitCode` is `1`.
- **Results file has an unsupported extension** — `context.WriteError` is called with a descriptive
  message; no results file is written.
- **Results file cannot be written** (e.g., permission denied, path invalid) — `context.WriteError`
  is called with the exception message; the file is not written and execution continues normally.

> **Thread-safety constraint**: `Validation.Run` must not be called concurrently. Each test
> method uses `DirectorySwitch` to mutate the process working directory, which is a process-wide
> resource. See the [Validation unit design][validation] for details.

## References

- [ReqStream System Design][arch]
- [Validation Unit Design][validation]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[validation]: validation.md
[repo]: https://github.com/demaconsulting/ReqStream
