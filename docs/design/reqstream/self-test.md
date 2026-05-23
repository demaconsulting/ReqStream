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

**Provided Interface**:

**Validation.Run**: Runs all self-validation tests, prints a summary, and writes results.

- *Type*: In-process .NET internal API (static method).
- *Role*: Provider (called by `Program.Run` when `--validate` is present).
- *Contract*: Accepts a `Context`; executes six validation tests sequentially; prints a summary.
  If `context.ResultsFile` is set, writes results to that file in TRX or JUnit format.
  Throws `ArgumentNullException` when `context` is null (backed by `ReqStream-Validation-NullContext`).
  If the results file extension is unsupported, the error is reported via `context.WriteError`
  and execution continues to the summary rather than aborting. If the results file write fails,
  the write error is similarly reported via `context.WriteError` and execution continues to
  the summary.
- *Constraints*: Must not be called concurrently (mutates the process working directory).

**Consumed Interfaces**:

**Program.Run**: Invoked by all six internal validation test methods to exercise the full tool
dispatch pipeline end-to-end.

- *Type*: In-process .NET internal API (static method).
- *Role*: Consumer (each of the six test methods creates a `Context` and passes it to
  `Program.Run`).
- *Contract*: Accepts a `Context`; dispatches to the appropriate processing path; reports
  results and errors via the context. Exit code 0 on success, non-zero on failure.

**Context.Create**: Invoked within each test method to construct a silent `Context` for that
test's `Program.Run` invocation.

- *Type*: In-process .NET internal API (static factory method), `Cli` subsystem.
- *Role*: Consumer.
- *Contract*: Accepts a string array of command-line arguments; returns a configured
  `Context` scoped to that invocation.

**PathHelpers.SafePathCombine**: Invoked during test fixture setup to construct absolute file
paths safely, and by `WriteResultsFile` when building the output file path.

- *Type*: In-process .NET internal API (static method), `Utilities` subsystem.
- *Role*: Consumer.
- *Contract*: Accepts a base path and a relative segment; returns the combined path; throws
  `ArgumentException` if the combined path would escape the base directory.

**TrxSerializer.Serialize / JUnitSerializer.Serialize**: Invoked by `WriteResultsFile` to
serialize the `TestResults` collection into TRX or JUnit XML format.

- *Type*: OTS API, `DemaConsulting.TestResults.IO` package.
- *Role*: Consumer.
- *Contract*: Accepts a `TestResults` collection; returns a serialized string in the
  respective format.

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

Each test method invokes `Program.Run` via a locally created `Context` to drive the tool's
full dispatch pipeline. `Context.Create` (from the `Cli` subsystem) constructs the context for
each `Program.Run` call. `PathHelpers.SafePathCombine` (from the `Utilities` subsystem) builds
all fixture file paths within the test methods and within `WriteResultsFile`. The
`WriteResultsFile` helper calls `TrxSerializer.Serialize` or `JUnitSerializer.Serialize`
(from the OTS `DemaConsulting.TestResults.IO` package) to produce the results output.

The nested helper class `DirectorySwitch` is used exclusively within the test methods and has
no visibility outside `Validation`. Each test uses `TemporaryDirectory` (from the `Utilities`
subsystem) together with `DirectorySwitch` to guarantee clean file-system state and ensure no
test artifacts persist after completion.
