# System Integration Design

## Overview

This chapter describes how the ReqStream software units work together as an integrated system.
Where the unit chapters (Program, Context, Validation, Requirements, TraceMatrix, Linter) each
describe one component in isolation, this chapter focuses on the end-to-end data flow, the
coordination points between units, and the integrated scenarios that the units collectively
enable.

## System Data Flow

The following table shows the direction of data between units during a standard requirements
processing invocation:

| Source | Data | Destination |
| ------ | ---- | ----------- |
| CLI arguments | Parsed options | `Context` |
| `Context.RequirementsFiles` | Glob-expanded file paths | `Requirements.Read` |
| `Requirements.Read` | Requirement tree | `Program.ProcessRequirements` |
| `Context.TestFiles` | Glob-expanded file paths | `TraceMatrix` constructor |
| Requirement tree | Requirements | `TraceMatrix` constructor |
| `TraceMatrix` | Coverage data | `Program.EnforceRequirementsCoverage` |
| `TraceMatrix` / requirement tree | Export input | Report files |

## Integrated Processing Pipeline

The following sequence describes the full pipeline executed during a normal (non-version, non-help,
non-validate, non-lint) invocation:

1. **Argument parsing** — `Context.Create(args)` parses all CLI flags and expands any glob
   patterns in `--requirements` and `--tests` arguments.
2. **Requirements loading** — `Requirements.Read(context.RequirementsFiles)` reads and merges all
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

## Self-Validation Flow

When `--validate` is specified, `Program.Run` delegates entirely to `Validation.Run(context)`.
`Validation` is self-contained: it creates temporary directories, writes fixture files, and invokes
the same `Program` methods used in normal processing. The self-validation path exercises the
integrated pipeline internally and produces structured test-result output in TRX or JUnit format
so that the evidence can be fed back into ReqStream's own requirements enforcement.

## Interactions Between Units

| Calling unit | Called unit | Call site | Purpose |
| ------------ | ----------- | --------- | ------- |
| `Program` | `Context` | `Main` | Parses CLI arguments; owns output and exit code |
| `Program` | `Validation` | `Run` | Runs self-validation suite when `--validate` is set |
| `Program` | `Linter` | `Run` | Lints requirements files when `--lint` is set |
| `Program` | `Requirements` | `ProcessRequirements` | Reads and merges YAML requirement files |
| `Program` | `TraceMatrix` | `ProcessRequirements` | Loads test results and maps them to requirements |
| `Validation` | `Program` | test methods | Invokes `Program.Run` to exercise the full pipeline |

## References

- [ReqStream Architecture][arch]
- [ReqStream Repository][repo]

[arch]: ../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
