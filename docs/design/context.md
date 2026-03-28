# Context Unit Design

## Overview

`Context` is the command-line argument parser and I/O owner for ReqStream. It is the single
authoritative source for all runtime options and is the only unit permitted to write to the console
or the log file. `Context` never touches YAML content, test result data, or domain objects; its
sole concerns are parsing arguments and surfacing results to the caller.

`Context` implements `IDisposable` so that the log-file `StreamWriter` is closed deterministically
when the enclosing `using` block in `Program.Main` exits.

## Private State

| Field | Type | Purpose |
| ----- | ---- | ------- |
| `_logWriter` | `StreamWriter?` | Open writer for the optional log file; `null` when no log file was requested |
| `_hasErrors` | `bool` | Accumulates error state; initially `false`; set to `true` by `WriteError` |

## Properties

| Property | Type | CLI flag | Notes |
| -------- | ---- | -------- | ----- |
| `Version` | `bool` | `--version` | Print version and exit |
| `Help` | `bool` | `--help` | Print usage and exit |
| `Silent` | `bool` | `--silent` | Suppress console output |
| `Validate` | `bool` | `--validate` | Run self-validation tests |
| `Lint` | `bool` | `--lint` | Lint requirements files |
| `ResultsFile` | `string?` | `--results` | Path for validation test-results output file |
| `Enforce` | `bool` | `--enforce` | Fail if requirements are not fully covered |
| `FilterTags` | `HashSet<string>?` | `--filter` | Comma-separated tag filter; `null` when not specified |
| `RequirementsFiles` | `List<string>` | `--requirements` | Expanded list of requirement file paths |
| `TestFiles` | `List<string>` | `--tests` | Expanded list of test-result file paths |
| `RequirementsReport` | `string?` | `--report` | Destination path for requirements report |
| `ReportDepth` | `int` | `--report-depth` | Heading depth for requirements report |
| `Matrix` | `string?` | `--matrix` | Destination path for trace matrix report |
| `MatrixDepth` | `int` | `--matrix-depth` | Heading depth for trace matrix report |
| `JustificationsFile` | `string?` | `--justifications` | Destination path for justifications report |
| `JustificationsDepth` | `int` | `--justifications-depth` | Heading depth for justifications report |
| `ExitCode` | `int` | — | Computed: `_hasErrors ? 1 : 0` |

## Methods

### `Create(args)`

`Create` is the static factory method that constructs and returns a fully initialized `Context`. It
implements a sequential switch-based parser over the `args` array. Each recognized flag sets the
corresponding property; flags that consume the next element (e.g., `--requirements`) advance the
index by one additional step. An unrecognized argument or a missing value for a flag that requires
one causes an `ArgumentException`, which surfaces to the caller as a user-actionable error message
rather than an unhandled exception.

`--filter` values are split on `','` and accumulated into `FilterTags`; multiple `--filter`
arguments merge into the same set. `--requirements` and `--tests` values are passed to
`ExpandGlobPattern` and appended to the respective file lists. If `--log` is specified, the named
file is opened for writing and assigned to `_logWriter` before the method returns.

### `ExpandGlobPattern(pattern)`

`ExpandGlobPattern` resolves a single pattern (which may contain `*` or `**` wildcards) to a list
of absolute file paths using `Microsoft.Extensions.FileSystemGlobbing.Matcher` against the current
working directory.

**Known limitation**: the `Matcher` library silently ignores patterns that are themselves absolute
paths. Callers that pass absolute paths directly will receive an empty result set. This is an
accepted limitation of the underlying library; users should use relative paths or glob wildcards.

### `WriteLine(message)`

`WriteLine` writes a message to the console (unless `Silent` is `true`) and to `_logWriter` if a
log file is open.

### `WriteError(message)`

`WriteError` sets `_hasErrors = true`, writes the message to `Console.Error` in red (unless
`Silent` is `true`), and also writes it to `_logWriter` if a log file is open. Setting
`_hasErrors` ensures that `ExitCode` returns `1` after any error is reported.

### `Dispose()`

`Dispose` flushes and closes `_logWriter` if it is not `null`, then sets it to `null`. This
ensures the log file is not truncated and file handles are not leaked even when the process exits
via an early return path.

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Program` | Creates `Context` via `Create`; calls `WriteLine` and `WriteError`; reads `ExitCode` |
| `Validation` | Calls `context.WriteLine`, `context.WriteError`, reads `ResultsFile`, `Silent` |
| `Linter` | Calls `context.WriteError` to report linting issues |
| `Requirements` | Receives `RequirementsFiles`; does not hold a reference to `Context` |
| `TraceMatrix` | Receives `TestFiles`; does not hold a reference to `Context` |

## References

- [ReqStream Architecture][arch]
- [ReqStream Repository][repo]

[arch]: ../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
