# Program Unit Design

## Overview

`Program` is the entry point of the ReqStream executable. It owns the top-level execution flow,
dispatches to the appropriate subsystem based on the parsed command-line options, and establishes the
error-handling boundary for the entire process. All meaningful work is delegated to `Context`,
`Validation`, `Requirements`, and `TraceMatrix`; `Program` itself contains no domain logic.

## Properties

### `Version`

`Version` is a static read-only string property that resolves the tool's version at runtime.

Resolution order:

| Priority | Source | API |
| -------- | ------ | --- |
| 1 | `AssemblyInformationalVersionAttribute` | `Assembly.GetExecutingAssembly()` |
| 2 | `AssemblyName.Version` | `Assembly.GetExecutingAssembly().GetName().Version` |
| 3 | Fallback literal | `"Unknown"` |

The informational version (set by the build system) is preferred because it carries pre-release
labels and build metadata. If the attribute is absent or empty the numeric `AssemblyName.Version`
string is used. If neither is available the string `"Unknown"` is returned so that the property
never throws and never returns `null`.

## Methods

### `Main(args)`

`Main` is the process entry point. Its responsibilities are:

1. Create a `Context` instance via `Context.Create(args)`.
2. Invoke `Run(context)` inside a `using` block so that `Context.Dispose()` is called on exit.
3. Return `context.ExitCode` as the process exit code.

**Error-handling contract**:

| Exception type | Handling |
| -------------- | -------- |
| `ArgumentException` | Message written to `Console.Error`; returns exit code `1` |
| `InvalidOperationException` | Message written to `Console.Error`; returns exit code `1` |
| Any other exception | Message written to `Console.Error`; exception re-thrown |

`ArgumentException` is thrown by `Context.Create` for invalid arguments and is user-actionable.
`InvalidOperationException` signals a domain error (YAML validation, test-result parse failure);
its message is sufficient for diagnosis. All other exceptions are re-thrown so the operating
system or process supervisor captures the full stack trace for unexpected failures.

### `Run(context)`

`Run` implements the priority-ordered dispatch shown in the table below. Return steps exit
immediately; the banner step (row 2) prints the banner and then falls through to the next step.

| Priority | Condition | Action |
| -------- | --------- | ------ |
| 1 | `context.Version` is `true` | Print version string only; return |
| 2 | `context.Lint` is `false` | Call `PrintBanner` (no return; falls through to next step) |
| 3 | `context.Help` is `true` | Call `PrintHelp`; return |
| 4 | `context.Validate` is `true` | Call `Validation.Run(context)`; return |
| 5 | `context.Lint` is `true` and `context.RequirementsFiles` is empty | Print informational message ("No requirements files specified"); return (exit code 0) |
| 5 | `context.Lint` is `true` | Call `Requirements.Load(context.RequirementsFiles)`; report lint issues; return |
| 6 | (default) | Call `ProcessRequirements(context)` |

### `PrintBanner`

`PrintBanner` writes three lines to `context`: the tool name with version string, the copyright
notice, and a blank line. It is called at priority step 2 for all invocations except version
queries and lint runs, so that every non-trivial invocation identifies the running version.
The banner is suppressed during lint to keep output clean for lint script integration — only
actionable issue lines are emitted.

### `PrintHelp`

`PrintHelp` writes the full option listing to `context`. It documents every supported flag and
argument, grouped logically. It is only called when `--help` is present.

### `ProcessRequirements`

`ProcessRequirements` orchestrates the normal (non-version, non-help, non-validate, non-lint) run.
It begins by calling `Requirements.Load(context.RequirementsFiles)` to build the parsed requirement
tree. It then conditionally generates the requirements report (if `--report` is set) and the
justifications report (if `--justifications` is set). If `--tests` files are provided, a
`TraceMatrix` is constructed from the requirement tree and the test result files to enable coverage
analysis. If `--matrix` is set and a `TraceMatrix` was built, the trace matrix report is exported.
If `--enforce` is active, `EnforceRequirementsCoverage` is called last so that all reports are
generated even when coverage fails. All export methods respect `context.FilterTags` for tag-filtered
output.

### `EnforceRequirementsCoverage`

`EnforceRequirementsCoverage` evaluates whether all requirements are covered by passing tests. If
no `TraceMatrix` was built (i.e., no `--tests` argument was provided), it reports an error
indicating that enforcement requires test results. Otherwise, it calls
`traceMatrix.CalculateSatisfiedRequirements(context.FilterTags)` to obtain satisfied and total
counts. If any requirements are unsatisfied, it calls
`traceMatrix.GetUnsatisfiedRequirements(context.FilterTags)` to retrieve the list of unsatisfied
requirement IDs and reports each one via `context.WriteError`.

This method never throws; all failure signalling goes through `context.WriteError`, which sets the
internal error flag and eventually produces a non-zero exit code.

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Context` | Created in `Main`; passed to all subsystems; owns output and exit code |
| `Validation` | Called by `Run` when `--validate` is present |
| `Requirements` | Constructed in `ProcessRequirements`; provides the requirement tree; also used for linting |
| `TraceMatrix` | Constructed in `ProcessRequirements` when test files are present |

## References

- [ReqStream System Design][arch]
- [ReqStream Repository][repo]

[arch]: reqstream.md
[repo]: https://github.com/demaconsulting/ReqStream
