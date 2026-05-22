# System Integration Design

## Overview

This chapter describes how the ReqStream software units work together as an integrated system.
Where the unit and subsystem chapters (Program, Cli, Context, Modeling, LintIssue, LoadResult,
Requirement, Requirements, RequirementsLoader, Section, Tracing, TraceMatrix, SelfTest, Validation)
each describe one component in isolation, this chapter focuses on the end-to-end data flow, the
coordination points between units, and the integrated scenarios that the units collectively
enable.

## Architecture

The ReqStream system is a single executable (.NET tool package) organized into the following
software items:

| Item | Type | Responsibility |
| ---- | ---- | -------------- |
| `Program` | Unit | Entry point and top-level dispatch orchestrator |
| `Cli` | Subsystem | Command-line argument parsing and I/O ownership |
| `Context` | Unit | Argument parser, output channels, and exit code |
| `Utilities` | Subsystem | Shared low-level file-system helpers |
| `GlobMatcher` | Unit | Glob pattern expansion to file paths |
| `PathHelpers` | Unit | Path combination with traversal protection |
| `Modeling` | Subsystem | YAML requirements data model and parsing |
| `LintIssue` | Unit | Lint issue severity and data model |
| `LoadResult` | Unit | Combined load outcome (tree and issues) |
| `Requirement` | Unit | Single requirement data-transfer object |
| `Requirements` | Unit | Requirements tree root and export entry point |
| `RequirementsLoader` | Unit | YAML deserializer and lint validator |
| `Section` | Unit | Named requirements group and tree node |
| `Tracing` | Subsystem | Test result loading and requirement traceability |
| `TraceMatrix` | Unit | Test result loader, mapper, and coverage enforcer |
| `SelfTest` | Subsystem | Built-in self-validation framework |
| `Validation` | Unit | Self-validation test runner |

The collaboration model is strictly hierarchical: `Program` is the only unit that calls into
multiple subsystems; subsystems do not call other subsystems directly. The `Context` object is
the single shared state carrier passed from `Program` to the units that need to produce output.
See the subsystem and unit design chapters for individual collaboration details.

## External Interfaces

| Interface | Direction | Format | Constraints |
| --------- | --------- | ------ | ----------- |
| Command-line arguments | Input | Space-separated flag/value pairs | Defined flag set only; unknown flags cause `ArgumentException` |
| Standard output (stdout) | Output | Plain text lines via `Context.WriteLine` | Suppressed when `--silent` is active |
| Standard error (stderr) | Output | Plain text error lines via `Context.WriteError` | Suppressed when `--silent` is active; triggers exit code `1` |
| YAML requirements files | Input | YAML mapping nodes per the ReqStream requirements schema | Validated by `RequirementsLoader`; parse errors returned as `LintIssue` objects |
| Test result files | Input | TRX (MSTest) or JUnit XML | Auto-detected by `DemaConsulting.TestResults.IO.Serializer`; fatal error reported if a file is missing or cannot be parsed |
| Requirements report file | Output | Markdown | Path from `--report`; written by `Requirements.Export` |
| Justifications report file | Output | Markdown | Path from `--justifications`; written by `Requirements.ExportJustifications` |
| Trace matrix report file | Output | Markdown | Path from `--matrix`; written by `TraceMatrix.Export` |
| Log file | Output | Plain text (same as stdout) | Path from `--log`; written by Context log writer; optional |
| Validation results file | Output | TRX or JUnit XML | Path from `--results`; written by `Validation.WriteResultsFile` |
| Process exit code | Output | Integer (0 or 1) | `0` = success; `1` = any error reported via `Context.WriteError` |

## Dependencies

The ReqStream system depends on the following external software items. Detailed integration and
usage design for each is in the OTS and Shared Package design chapters.

| Item | Type | Purpose |
| ---- | ---- | ------- |
| `YamlDotNet` | OTS | YAML parsing via the RepresentationModel DOM API in `RequirementsLoader` |
| `Microsoft.Extensions.FileSystemGlobbing` | OTS | Glob pattern matching used by `GlobMatcher.FindMatchingFiles` |
| `DemaConsulting.TestResults` | Shared Package | Test result deserialization (TRX, JUnit) in `TraceMatrix` and `Validation` |

## Risk Control Measures

The following segregation measures are implemented to satisfy IEC 62304 §5.3.3 software system
design requirements:

- **Error isolation at process boundary** — `Program.Main` catches `ArgumentException` and
  `InvalidOperationException`, preventing unhandled exceptions from reaching the operating system
  in normal error conditions. Unexpected exceptions are re-thrown to preserve the full stack trace
  for diagnosis.
- **No shared mutable state across subsystems** — each subsystem receives only the data it needs.
  `Context` is the sole shared state carrier; subsystem units do not hold direct references to
  each other.
- **Output channel separation** — normal output uses `Context.WriteLine` (stdout); errors use
  `Context.WriteError` (stderr). The two channels are never mixed, enabling downstream tooling
  to detect failures by reading stderr or the exit code without parsing stdout.
- **Path traversal prevention** — `PathHelpers.SafePathCombine` validates all relative path
  combinations against the base directory before use, preventing `includes` directives in YAML
  files from escaping the file-system boundary.
- **Include loop guard** — `RequirementsLoader` tracks visited file paths in a `HashSet`,
  preventing infinite recursion on circular `includes` graphs.
- **Acyclic child-requirement graph** — `RequirementsLoader.ValidateCycles` confirms the
  child-requirement graph is acyclic before downstream analysis; `TraceMatrix.CollectAllTests`
  can therefore recurse without a cycle guard. This satisfies `ReqStream-System-CyclicChildDetection`.

## System Data Flow

The following table shows the direction of data between units during a standard requirements
processing invocation:

| Source | Data | Destination |
| ------ | ---- | ----------- |
| CLI arguments | Parsed options | `Context` |
| `Context.RequirementsFiles` | Glob-expanded file paths | `Requirements.Load` |
| `Requirements.Load` | Requirement tree | `Program.ProcessRequirements` |
| `Context.TestFiles` | Glob-expanded file paths | `TraceMatrix` constructor |
| Requirement tree | Requirements | `TraceMatrix` constructor |
| `TraceMatrix` | Coverage data | `Program.EnforceRequirementsCoverage` |
| Requirement tree | Export input | Requirements and justifications report files (`--report`, `--justifications`) |
| `TraceMatrix` + requirement tree | Export input | Trace matrix report file (`--matrix`) |
| `Context.ResultsFile` | Output file path | `Validation.Run` |

## Integrated Processing Pipeline

The following sequence describes the full pipeline executed during a normal (non-version, non-help,
non-validate, non-lint) invocation:

> **Note**: `--version` prints the version string and exits immediately, before any pipeline steps.
> `--help` prints usage information and exits immediately. Both flags take precedence over all other
> flags and no file system access occurs.

1. **Argument parsing** — `Context.Create(args)` parses all CLI flags and expands any glob
   patterns in `--requirements` and `--tests` arguments.
2. **Requirements loading** — `Requirements.Load(context.RequirementsFiles)` reads and merges all
   YAML requirements files into a single requirement tree. Files listed via `includes` are resolved
   recursively.
3. **Report generation** — if `--report` is set, the requirements report is exported. If
   `--justifications` is set, the justifications report is exported.
4. **Test result loading** — if `--tests` is set, a `TraceMatrix` is constructed. It reads each
   test result file (TRX or JUnit), applies source-specific matching rules, and maps each test
   result to the requirements that reference it.
5. **Trace matrix export** — if `--matrix` is set and a `TraceMatrix` was constructed, the trace
   matrix report is exported. If `--matrix` is set but no `TraceMatrix` was constructed (i.e., no
   `--tests` files were provided), an error is reported: "No test files provided. Cannot generate
   trace matrix."
6. **Enforcement** — if `--enforce` is set and a `TraceMatrix` was constructed,
   `EnforceRequirementsCoverage` compares the satisfied-requirement count against the total count.
   Any unsatisfied requirement causes an error to be written to `context`, which results in a
   non-zero exit code.
7. **Tag filtering** — if `--filter` is set, `Context.FilterTags` holds the list of tag strings.
   This list is passed as the `filterTags` parameter to `Requirements.Export`,
   `TraceMatrix.Export`, `TraceMatrix.CalculateSatisfiedRequirements`, and
   `TraceMatrix.GetUnsatisfiedRequirements`. Tag filtering is therefore applied transparently
   at each operation in steps 3, 5, and 6 rather than as a separate pipeline stage; only
   requirements carrying at least one matching tag are included in reports and enforcement.
8. **Report depth** — the `--depth` flag sets a default heading level for all reports.
   Per-report overrides (`--report-depth`, `--matrix-depth`, `--justifications-depth`) take
   precedence over the default. When generating reports in steps 3 and 5,
   `Context.ReportDepth`, `Context.MatrixDepth`, and `Context.JustificationsDepth` are passed
   as the `depth` parameter to `Requirements.Export`, `Requirements.ExportJustifications`, and
   `TraceMatrix.Export`. This satisfies `ReqStream-System-ReportDepth`.

## Source-Specific Test Matching

When test results are collected from multiple platforms or configurations, each result file
typically carries a platform identifier in its file name (for example `windows-latest.trx` or
`ubuntu-latest.trx`). The `TraceMatrix` unit supports source-specific matching through the
`filepart@testname` syntax in requirement test lists:

```text
tests:
  - windows-latest@Test_WindowsOnlyFeature
  - ubuntu@Test_LinuxFeature
  - Test_CrossPlatformFeature
```

A `filepart@testname` entry matches only test result files whose names contain `filepart`. A plain
`testname` entry aggregates results from all files. This mechanism is used in ReqStream's own
requirements to enforce that platform-specific requirements are satisfied by evidence from the
correct platform.

## Lint Flow

When `--lint` is specified, `Program.Run` loads the requirements files via
`Requirements.Load(context.RequirementsFiles)`, calls `result.ReportIssues(context)` to route each
lint issue through the `Context` output channel, and then exits. The lint flow differs from normal
processing in two important ways:

1. **No banner** — `PrintBanner` is suppressed so that only actionable issue lines appear in the
   output, making it straightforward to integrate `--lint` into editor tooling or CI scripts that
   treat non-empty output as failure.
2. **No summary** — when no issues are found the tool exits silently with code `0`; when issues are
   found each issue line is written to the output and the tool exits with code `1`.

## Self-Validation Flow

When `--validate` is specified, `Program.Run` delegates entirely to `Validation.Run(context)`.
`Validation` is self-contained: it creates temporary directories, writes fixture files, and invokes
the same `Program` methods used in normal processing. The self-validation path exercises the
integrated pipeline internally and produces structured test-result output in TRX or JUnit format
so that the evidence can be fed back into ReqStream's own requirements enforcement. When
`Context.ResultsFile` is non-`null` (set by the `--results` flag), `Validation.Run` writes the
test-result output to that file path; otherwise the results are written only to the console
output channel.

## Interactions Between Units

| Calling unit | Called unit | Call site | Purpose |
| ------------ | ----------- | --------- | ------- |
| `Program` | `Context` | `Main` | Parses CLI arguments; owns output and exit code |
| `Program` | `Validation` | `Run` | Runs self-validation suite when `--validate` is set |
| `Program` | `Requirements` | `Run` | Loads and lints requirements files when `--lint` is set |
| `Program` | `Requirements` | `ProcessRequirements` | Reads and merges YAML requirement files |
| `Program` | `TraceMatrix` | `ProcessRequirements` | Loads test results and maps them to requirements |
| `Validation` | `Program` | test methods | Invokes `Program.Run` to exercise the full pipeline |

## Error and Output Data Flow

The following table shows how errors and results flow between units during a standard requirements
processing invocation, tracing them from origin to the process exit code:

| Source | Data | Destination |
| ------ | ---- | ----------- |
| YAML files | Requirements content | `Requirements.Load` |
| Test result files | Test execution records | `TraceMatrix` |
| `Context` | Expanded file lists and flags | `Requirements.Load` |
| `Requirements.Load` | Parsed requirement tree | `Program` |
| `Program` | Parsed requirement tree | `TraceMatrix` |
| `Requirements.Load` + `result.ReportIssues` | Lint warnings/errors | `context.WriteError` → `Context.ExitCode` |
| `TraceMatrix` | Coverage analysis | Markdown reports |
| `Program.EnforceRequirementsCoverage` | Unsatisfied requirements | `context.WriteError` → `Context.ExitCode` |
| `Program.ProcessRequirements` | Error when `--matrix` requested without `--tests` | `context.WriteError` → `Context.ExitCode` |
| `TraceMatrix` constructor | Fatal error when a test result file is missing or cannot be parsed | `context.WriteError` → `Context.ExitCode` |

## Platform Support

ReqStream targets the `net8.0`, `net9.0`, and `net10.0` target framework monikers using .NET's
multi-targeting build. Because all runtime dependencies (YamlDotNet, DemaConsulting.TestResults)
ship as portable NuGet packages and no platform-specific APIs are used anywhere in the codebase,
the resulting binaries run without modification on Windows, Linux, and macOS. The GitHub Actions
CI matrix executes the full test suite on all three operating systems and all three .NET versions
on every build, providing continuous evidence that the platform requirements are satisfied.

## Design Constraints

The following constraints govern the design of the ReqStream system:

- **Platform portability** — no platform-specific APIs are used. All runtime dependencies ship as
  portable NuGet packages. The tool must function identically on Windows, Linux, and macOS.
- **Multi-framework targeting** — the project targets `net8.0`, `net9.0`, and `net10.0`.
  All language and library features used must be available on all three target frameworks.
- **Nullable reference types enabled** — the compiler is configured with `<Nullable>enable</Nullable>`.
  All APIs must handle null inputs explicitly; no null-reference exceptions are permitted at runtime.
- **Zero compiler warnings** — the project uses `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`.
  All code must compile without warnings on all three target frameworks.
- **No interactive prompts** — ReqStream is designed for CI/CD use. The tool must never block
  waiting for user input; all required information must be supplied on the command line.
- **Stateless between invocations** — the tool holds no persistent state between runs. Each
  invocation is independent and idempotent given the same inputs and file system state.

## Design Decisions

### Separation of Concerns

Each unit owns a clearly bounded responsibility and never reaches across boundaries:

- `Context` owns CLI argument handling and output; it never touches YAML or test results
- `Requirements` owns YAML parsing and merging; it never touches test results or reports
- `TraceMatrix` owns test result analysis; it receives an already-validated requirements tree
- `Program` owns execution flow; it delegates all work to the other units

### Why Sections Are Merged by Title

Title-based merging enables modular requirements management without any explicit namespace or import
declaration. Multiple files can contribute to the same logical section; requirements, mappings, and
justifications can live in separate files owned by separate teams. A repository can organize files
by feature, component, or responsibility and still produce one coherent requirement tree.

### Immutable Data Structures

Properties prevent modification after construction; collections allow population during construction
but not replacement. Internal value types within `TraceMatrix` are immutable records. This removes an entire
class of concurrency and aliasing bugs and makes the data model easy to reason about.

### Error Context and Testability

Validation and parsing errors always include the source file path for actionable debugging. Static
factory methods (`Context.Create`, `Requirements.Load`) decouple construction from consumers. Public
satisfaction-calculation methods and clear parsing/analysis separation enable direct unit testing
without mocking or fixtures.

### Key Architectural Patterns

| Pattern | Usage |
| ------- | ----- |
| Factory | `Context.Create(args)` and `Requirements.Load(paths)` encapsulate object construction |
| Composite | `Section` trees enable recursive traversal for export and satisfaction calculation |
| Strategy | Test result parsing tries TRX first, then JUnit; name matching tries source-specific first, then plain |
| Disposable | `Context` implements `IDisposable` for deterministic log file cleanup |
