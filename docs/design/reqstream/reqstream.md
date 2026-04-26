# System Integration Design

## Overview

This chapter describes how the ReqStream software units work together as an integrated system.
Where the unit and subsystem chapters (Program, Cli, Context, Modeling, LintIssue, LoadResult,
Requirement, Requirements, RequirementsLoader, Section, Tracing, TraceMatrix, SelfTest, Validation)
each describe one component in isolation, this chapter focuses on the end-to-end data flow, the
coordination points between units, and the integrated scenarios that the units collectively
enable.

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
   matrix report is exported.
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
| CLI arguments | Parsed flags and file paths | `Context` |
| YAML files | Requirements content | `Requirements.Load` |
| Test result files | Test execution records | `TraceMatrix` |
| `Context` | Expanded file lists and flags | `Requirements.Load` |
| `Requirements.Load` | Parsed requirement tree | `TraceMatrix` |
| `Requirements.Load` + `result.ReportIssues` | Lint warnings/errors | `context.WriteError` → `Context.ExitCode` |
| `TraceMatrix` | Coverage analysis | Markdown reports |
| `Program.EnforceRequirementsCoverage` | Unsatisfied requirements | `context.WriteError` → `Context.ExitCode` |

## Platform Support

ReqStream targets the `net8.0`, `net9.0`, and `net10.0` target framework monikers using .NET's
multi-targeting build. Because all runtime dependencies (YamlDotNet, DemaConsulting.TestResults)
ship as portable NuGet packages and no platform-specific APIs are used anywhere in the codebase,
the resulting binaries run without modification on Windows, Linux, and macOS. The GitHub Actions
CI matrix executes the full test suite on all three operating systems and all three .NET versions
on every build, providing continuous evidence that the platform requirements are satisfied.

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

## References

- [ReqStream Repository][repo]

[repo]: https://github.com/demaconsulting/ReqStream
