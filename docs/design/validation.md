# Validation Unit Design

## Overview

`Validation` is the self-validation test runner for ReqStream. Its purpose is to execute a suite
of end-to-end tests that verify the tool's own behavior and to produce structured test-result
evidence in TRX or JUnit format. This evidence can then be fed back into ReqStream to validate the
tool's own requirements — enabling a self-hosting compliance workflow.

All tests run in temporary directories to avoid side effects and are isolated from one another.

## Methods

### `Run(context)`

`Run` is the single public entry point. Its sequence is:

1. Print a header block to `context` containing the tool version, machine name, operating system,
   .NET runtime version, and current UTC timestamp.
2. Execute the six validation tests in order, collecting a `TestResult` for each.
3. Print a summary line showing the number of passed and failed tests.
4. If `context.ResultsFile` is set, call `WriteResultsFile(context, testResults)`.

The six validation tests are listed in the order they are executed:

| # | Method | What it verifies |
| - | ------ | ---------------- |
| 1 | `RunRequirementsProcessingTest` | Requirements YAML files are read, merged, and exported |
| 2 | `RunTraceMatrixTest` | Test results are loaded and mapped to requirements |
| 3 | `RunReportExportTest` | Requirements and justifications reports are written correctly |
| 4 | `RunTagsFilteringTest` | Tag-based filtering restricts output and coverage calculation |
| 5 | `RunEnforcementModeTest` | `--enforce` produces a non-zero exit code when coverage fails |
| 6 | `RunLintTest` | The linter detects and reports structural issues in YAML files |

Each test method:

1. Creates a `DirectorySwitch` (see below) to operate in a fresh temporary directory.
2. Writes one or more YAML or test-result fixture files to the temporary directory.
3. Invokes a `Program` method or builds a `Context` and executes the relevant workflow.
4. Asserts the expected outcomes (file content, exit code, error messages).
5. Returns a `TestResult` with outcome `Passed` or `Failed`.

### `WriteResultsFile(context, testResults)`

`WriteResultsFile` serializes the collected `TestResult` list to a structured file.

**Format dispatch**:

| File extension | Serializer |
| -------------- | ---------- |
| `.trx` | TRX serializer (`DemaConsulting.TestResults.IO`) |
| `.xml` | JUnit serializer (`DemaConsulting.TestResults.IO`) |
| Any other | Throws `ArgumentException` |

The serializer is invoked with the assembled `TestResults` object and the resolved output path.

## Supporting Types

### `DirectorySwitch` (nested helper class)

`DirectorySwitch` is an `IDisposable` helper that manages temporary working-directory lifetime for
test isolation.

**Construction**:

1. Capture `Directory.GetCurrentDirectory()` as the original directory.
2. Create a new temporary directory (e.g., via `Path.GetTempPath()` + a unique name).
3. Call `Directory.SetCurrentDirectory` to make the temporary directory the working directory.

**Disposal**:

1. Call `Directory.SetCurrentDirectory` to restore the original directory.
2. Delete the temporary directory and all its contents recursively.

This pattern guarantees that each test starts with a clean file system state and that no test
artifacts persist after the test completes, regardless of whether the test passes or fails.

## Dependencies

| Library / Type | Role |
| -------------- | ---- |
| `DemaConsulting.TestResults` | `TestResults`, `TestResult`, `TestOutcome` model types |
| `DemaConsulting.TestResults.IO.Serializer` | TRX and JUnit file serialization |

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Context` | Reads `ResultsFile`, `Version`, `Silent`; calls `WriteLine` for headers and summary |
| `Program` | `Run` internally exercises `Program.Run` or individual workflow methods |
| `Requirements` | Tests exercise `Requirements.Read` with fixture YAML files |
| `TraceMatrix` | Tests exercise `TraceMatrix` construction with fixture test-result files |
| `Linter` | `RunLintTest` exercises `Linter.Lint` with fixture YAML files |

## References

- [ReqStream Architecture][arch]
- [ReqStream Repository][repo]

[arch]: ../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
