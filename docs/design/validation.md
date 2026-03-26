# Validation Design

## Overview

The `Validation` class provides ReqStream's self-validation capability. When invoked with the `--validate`
flag, it runs a suite of functional tests against the `Program` class itself, verifying that the core
features of ReqStream work correctly on the current platform and runtime.

## Structure

`Validation` is a `public static` class containing one public method and several private helpers.

### Run Method

```csharp
public static void Run(Context context)
```

Orchestrates the self-validation process:

1. Prints a validation header with system information (tool version, machine name, OS, .NET runtime,
   timestamp)
2. Creates a `DemaConsulting.TestResults.TestResults` collection named `"ReqStream Self-Validation"`
3. Runs five functional tests (see below)
4. Prints a summary of total, passed, and failed tests
5. If `context.ResultsFile` is set, writes results using `DemaConsulting.TestResults.IO.Serializer`

### Test Suite

Each test method creates temporary files in a temporary directory, invokes `Program.Run()` with a
constructed `Context`, and inspects the output files or `Context.ExitCode` to verify behavior.

| Test Name | What It Verifies |
| :--- | :--- |
| `ReqStream_RequirementsProcessing` | Reading and processing requirements YAML files |
| `ReqStream_TraceMatrix` | Trace matrix construction from test result files |
| `ReqStream_ReportExport` | Exporting requirements to a markdown report |
| `ReqStream_TagsFiltering` | Tag-based filtering of requirements during export |
| `ReqStream_EnforcementMode` | Enforcement of requirements coverage |

### Test Result Naming

Test results are named using the base name of the results file as a source prefix:

```text
windows@ReqStream_RequirementsProcessing
```

This allows CI pipelines running on multiple platforms to aggregate results while keeping per-platform
attribution. The prefix is matched as a substring of the result file's base name, so a prefix of `windows`
matches a file such as `windows-latest-results.trx`.

### Results File Output

If `context.ResultsFile` is specified, the collected `TestResults` object is serialized using
`DemaConsulting.TestResults.IO.Serializer.Serialize`. The format (TRX or JUnit) is determined
automatically from the file extension.

## Key Design Decisions

- **In-process testing**: Tests run `Program.Run()` directly in the same process, using temporary
  directories and files. This avoids the complexity of spawning child processes while still exercising
  the full code path.
- **Public class for testability**: `Validation` is `public` so that unit tests in the test project can
  also call it directly.
- **Source filter prefix in test names**: Using `basename@testName` allows the same logical test to be
  tracked independently per platform when results from multiple CI agents are merged.
- **Error propagation via `Context`**: Test failures are reported through `context.WriteError`, which
  automatically sets the exit code to 1 without requiring explicit error handling in callers.

## Relationships

- **Invoked by**: `Program.Run` when `context.Validate` is set
- **Uses**: `Program.Run` (to execute the program under test), `Context` (for output and configuration),
  `DemaConsulting.TestResults` (for collecting and serializing test results)
