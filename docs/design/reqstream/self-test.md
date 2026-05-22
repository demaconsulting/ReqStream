## SelfTest

### Overview

The `SelfTest` subsystem provides the self-validation framework for ReqStream. It is invoked
when the user passes `--validate` on the command line and exercises the tool's own capabilities
end-to-end, reporting a pass/fail summary. It can also write test results to a file in TRX or
JUnit XML format for integration with CI/CD pipelines, enabling the tool to produce compliance
evidence about its own correctness.

The `SelfTest` subsystem contains the following software unit:

- **Validation** (`SelfTest/Validation.cs`) — Orchestrating and executing self-validation tests.

### Interfaces

**Validation.Run**: Runs all self-validation tests, prints a summary, and writes results.

- *Type*: In-process .NET internal API (static method).
- *Role*: Provider (called by `Program.Run` when `--validate` is present).
- *Contract*: Accepts a `Context`; executes six validation tests sequentially; prints a summary.
  If `context.ResultsFile` is set, writes results to that file in TRX or JUnit format.
- *Constraints*: Must not be called concurrently (mutates the process working directory).

### Design

The `SelfTest` subsystem contains a single unit, `Validation`. Its internal design follows a
test-runner pattern:

1. `Validation.Run` prints a header block and then executes each of the six test methods
   sequentially. Each test method creates a `TemporaryDirectory` and a `DirectorySwitch`,
   writes fixture files, invokes tool methods, and returns a `TestResult` with outcome
   `Passed` or `Failed`.
2. After all tests complete, `Run` prints a summary and optionally writes results to a file
   via `WriteResultsFile`.

The six tests exercise: requirements processing, trace matrix construction, report export,
tag filtering, enforcement mode, and lint detection. Each runs in a dedicated temporary
directory for isolation.

The two nested helper classes, `TemporaryDirectory` and `DirectorySwitch`, are used exclusively
within the test methods and have no visibility outside `Validation`. Each test uses both classes
together to guarantee clean file system state and ensure no test artifacts persist after
completion.
