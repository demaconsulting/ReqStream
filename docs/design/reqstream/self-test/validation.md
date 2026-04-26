# Validation Unit Design

## Overview

`Validation` is the self-validation test runner for ReqStream. Its purpose is to execute a suite
of end-to-end tests that verify the tool's own behavior and to produce structured test-result
evidence in TRX or JUnit format. This evidence can then be fed back into ReqStream to validate the
tool's own requirements — enabling a self-hosting compliance workflow.

All tests run in temporary directories to avoid side effects and are isolated from one another.

## Methods

### `Run(context)`

`Run` is the single public entry point. It prints a header block to `context` containing the tool
version, machine name, operating system, .NET runtime version, and current UTC timestamp. It then
executes the six validation tests in order and prints a multi-line summary block showing the total
number of tests, how many passed, and how many failed (using `WriteError` for the failed count when
any tests have failed). If `context.ResultsFile` is set, it calls `WriteResultsFile(context, testResults)`
to persist the results.

> **Thread-safety constraint**: `Run` must not be called concurrently. Each validation test uses
> `DirectorySwitch`, which mutates the process-wide current working directory
> (`Directory.SetCurrentDirectory`). Concurrent calls would race on this shared state, causing
> tests to resolve relative paths against the wrong directory. The validation subsystem therefore
> accesses global process state and is not thread-safe.

The six validation tests exist to provide structured, machine-readable evidence that ReqStream
correctly processes its own input formats. This evidence can be fed back into ReqStream to verify
the tool's own requirements coverage, enabling a self-hosting compliance workflow.

The six tests are listed in the order they are executed:

| # | Method | What it verifies |
| - | ------ | ---------------- |
| 1 | `RunRequirementsProcessingTest` | Requirements YAML files are read, merged, and exported |
| 2 | `RunTraceMatrixTest` | Test results are loaded and mapped to requirements |
| 3 | `RunReportExportTest` | Requirements and justifications reports are written correctly |
| 4 | `RunTagsFilteringTest` | Tag-based filtering restricts output and coverage calculation |
| 5 | `RunEnforcementModeTest` | `--enforce` produces a non-zero exit code when coverage fails |
| 6 | `RunLintTest` | The linter detects and reports structural issues in YAML files |

Each test runs in a dedicated `TemporaryDirectory` with `DirectorySwitch` active, writes fixture
files, invokes the relevant workflow, asserts expected outcomes, and returns a `TestResult` with
outcome `Passed` or `Failed`.

### `WriteResultsFile(context, testResults)`

`WriteResultsFile` serializes the collected `TestResult` list to a structured file.

**Format dispatch**:

| File extension | Serializer |
| -------------- | ---------- |
| `.trx` | TRX serializer (`DemaConsulting.TestResults.IO`) |
| `.xml` | JUnit serializer (`DemaConsulting.TestResults.IO`) |
| Any other | Reports error via `context.WriteError` and returns |

The serializer is called with the assembled `TestResults` object, returning a serialized string.
The string is then written to the resolved output path via `File.WriteAllText`.

## Supporting Types

### `TemporaryDirectory` (nested helper class)

`TemporaryDirectory` is an `IDisposable` helper that creates a uniquely named directory under
`Path.GetTempPath()` on construction and deletes it recursively on disposal. It exists to give
each validation test a clean, isolated file-system workspace that is guaranteed to be removed after
the test completes, regardless of whether the test passes or fails.

### `DirectorySwitch` (nested helper class)

`DirectorySwitch` is an `IDisposable` helper that changes the process working directory to a
supplied path on construction and restores the original directory on disposal. It exists because
ReqStream resolves relative paths against the working directory; tests must operate within their
temporary directory for file references to resolve correctly.

Each test uses both classes together: `TemporaryDirectory` owns the directory lifetime and
`DirectorySwitch` makes it the working directory for the duration of the test. This pattern
guarantees that each test starts with a clean file system state and that no test artifacts persist
after the test completes, regardless of whether the test passes or fails.

## Dependencies

| Library / Type | Role |
| -------------- | ---- |
| `DemaConsulting.TestResults` | `TestResults`, `TestResult`, `TestOutcome` model types |
| `DemaConsulting.TestResults.IO.Serializer` | TRX and JUnit file serialization |

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Context` | Reads `ResultsFile`, `Silent`; calls `WriteLine` for headers and summary |
| `Program` | Reads `Program.Version`; `Run` exercises `Program.Run` or individual workflow methods |
| `Requirements` | Tests exercise `Requirements.Load` with fixture YAML files |
| `TraceMatrix` | Tests exercise `TraceMatrix` construction with fixture test-result files |
