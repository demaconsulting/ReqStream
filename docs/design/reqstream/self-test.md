## SelfTest Subsystem Design

### Overview

The `SelfTest` subsystem provides the self-validation framework for ReqStream. It is invoked
when the user passes `--validate` on the command line and exercises the tool's own capabilities
end-to-end, reporting a pass/fail summary. It can also write test results to a file in TRX or
JUnit XML format for integration with CI/CD pipelines, enabling the tool to produce compliance
evidence about its own correctness.

The `SelfTest` subsystem contains the following software unit:

| Unit | File | Responsibility |
| ---- | ---- | -------------- |
| `Validation` | `SelfTest/Validation.cs` | Orchestrating and executing self-validation tests. |

### Interfaces

The `SelfTest` subsystem exposes the following interface to the rest of the tool:

| Interface        | Description                                                           |
|------------------|-----------------------------------------------------------------------|
| `Validation.Run` | Runs all self-validation tests, prints a summary, and writes results. |

### Design

The `SelfTest` subsystem contains a single unit, `Validation`. Its internal design follows a
test-runner pattern:

1. `Validation.Run` prints a header block and then executes each of the six test methods
   sequentially. Each test method creates a `TemporaryDirectory` and a `DirectorySwitch`,
   writes fixture files, invokes tool methods, and returns a `TestResult` with outcome
   `Passed` or `Failed`.
2. After all tests complete, `Run` prints a summary and optionally writes results to a file
   via `WriteResultsFile`.

The two nested helper classes, `TemporaryDirectory` and `DirectorySwitch`, are used exclusively
within the test methods and have no visibility outside `Validation`.

> **Thread-safety constraint**: each test method uses `DirectorySwitch`, which mutates the
> process-wide current working directory. `Validation.Run` must not be called concurrently;
> see the Validation unit design documentation for details.

### Interactions

| Unit | Relationship | Description |
| ---- | ------------ | ----------- |
| `Context` | Uses | Output channel for header lines, test summaries, and errors. |
| `Program` | Uses | `Program.Run` is called internally to exercise the tool. |

### Error Handling

The `SelfTest` subsystem handles the following error conditions:

- **One or more self-validation tests fail** — `context.WriteError` is called for each failing test;
  the method returns without setting a success state, so `context.ExitCode` is `1`.
- **Results file has an unsupported extension** — `context.WriteError` is called with a descriptive
  message; no results file is written.
- **Results file cannot be written** (e.g., permission denied, path invalid) — `context.WriteError`
  is called with the exception message; the file is not written and execution continues normally.

> **Thread-safety constraint**: `Validation.Run` must not be called concurrently. Each test
> method uses `DirectorySwitch` to mutate the process working directory, which is a process-wide
> resource. See the Validation unit design documentation for details.
