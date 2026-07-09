### Validation

![SelfTest Structure](SelfTestView.svg)

#### Purpose

`Validation` is the self-validation test runner for ReqStream. It executes a suite of end-to-end
tests that verify the tool's own behavior and produces structured test-result evidence in TRX or
JUnit format. This evidence can then be fed back into ReqStream to validate the tool's own
requirements — enabling a self-hosting compliance workflow. All tests run in temporary directories
to avoid side effects and are isolated from one another.

#### Data Model

N/A — `Validation` is a static class with no instance state. All shared state within a
validation run is allocated locally within `Run` and the individual test methods. The nested
helper class `DirectorySwitch` carries local instance state but is scoped to individual test
method calls.

**DirectorySwitch** (nested helper class): `IDisposable` that changes the process working
directory on construction and restores the original on disposal.

#### Key Methods

**Run(context)**: Single public entry point that executes all validation tests.

- *Parameters*: `Context context` — provides output channels and `ResultsFile` path.
- *Returns*: `void`.
- *Preconditions*: Must not be called concurrently (mutates process working directory).
- *Postconditions*: Summary printed; results file written if `context.ResultsFile` is set.

Prints a header block (version, machine name, OS, .NET runtime, UTC timestamp), executes six
tests sequentially, and prints a multi-line summary. The six tests:

1. `RunRequirementsProcessingTest` — verifies YAML files are read, merged, and exported.
2. `RunTraceMatrixTest` — verifies test results are loaded and mapped to requirements.
3. `RunReportExportTest` — verifies requirements and justifications reports are written.
4. `RunTagsFilteringTest` — verifies tag-based filtering restricts output and coverage.
5. `RunEnforcementModeTest` — verifies `--enforce` produces non-zero exit code on failure.
6. `RunLintTest` — verifies the linter detects and reports structural issues.

**WriteResultsFile(context, testResults)**: Serializes test results to a structured file.

- *Parameters*: `Context context`; `List<TestResult> testResults`.
- *Returns*: `void`.
- *Preconditions*: None — if `context.ResultsFile` is null the method returns immediately without
  error.
- *Postconditions*: File written in TRX (`.trx`) or JUnit (`.xml`) format when `context.ResultsFile`
  is non-null.

Dispatches based on file extension: `.trx` uses TRX serializer, `.xml` uses JUnit serializer,
any other extension reports an error via `context.WriteError`.

#### Error Handling

`Validation.Run` does not throw for test failures; each failing test is recorded as a
`TestResult` with outcome `Failed` and reported via `context.WriteError`. The method runs all
tests regardless of individual failures.

The following conditions are handled without throwing:

- **Test failure** — `context.WriteError` is called for each failing test.
- **Unsupported results file extension** — `context.WriteError` with descriptive message.
- **Results file write failure** — `context.WriteError` with exception message; execution
  continues.
- `ArgumentNullException` — thrown by `Run` when `context` is `null`.

The only exception that can escape `Run` is an unexpected runtime error, which propagates to
`Program.Run`.

#### Interactions

##### Dependencies

- **DemaConsulting.TestResults** — provides `TestResults`, `TestResult`, `TestOutcome` model
  types.
- **DemaConsulting.TestResults.IO.Serializer** — provides TRX and JUnit file serialization.
- **Context** — output channel for header lines, test summaries, and errors; provides
  `ResultsFile` and `Silent`.
- **Program** — `Program.Version` is read for the header block; `Program.Run` is exercised by
  individual test methods.
- **Requirements** — `Requirements.Load` is called with fixture YAML files.
- **TraceMatrix** — constructed with fixture test-result files.
- **TemporaryDirectory** — used by each test method to create an isolated scratch directory;
  replaces the former private nested `TemporaryDirectory` class that used `Path.GetTempPath()`.

##### Callers

- **Program** — calls `Validation.Run(context)` when `--validate` is present on the command
  line.
