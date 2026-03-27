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

`Create` is the static factory method that constructs and returns a fully initialized `Context`.
It implements a sequential switch-based parser over the `args` array.

**Parse loop**:

1. Iterate `args` with an index variable `i`.
2. Match `args[i]` against known flags using a `switch` statement.
3. For flags that consume the next element (e.g., `--requirements`), check `i + 1 >= args.Length`
   before advancing; if the check fails an `ArgumentException` is thrown.
4. An unrecognized argument causes an `ArgumentException` listing the unknown argument.

**`--filter` handling**:

The value following `--filter` is split on `','`. Each non-empty token is added to `FilterTags`.
If `FilterTags` is `null` at the point the first `--filter` is encountered, the `HashSet` is
created before adding tokens. Multiple `--filter` arguments are accumulated into the same set.

**`--requirements` and `--tests` handling**:

Each value is passed to `ExpandGlobPattern`; the resulting paths are appended to
`RequirementsFiles` or `TestFiles` respectively.

**Log file**:

If `--log` was specified, `Create` opens the named file for writing and assigns the resulting
`StreamWriter` to `_logWriter` before returning.

### `ExpandGlobPattern(pattern)`

`ExpandGlobPattern` resolves a single pattern (which may contain `*` or `**` wildcards) to a list
of absolute file paths.

**Implementation**:

1. Construct a `Microsoft.Extensions.FileSystemGlobbing.Matcher`.
2. Add `pattern` as an include pattern.
3. Execute the matcher against `Directory.GetCurrentDirectory()`.
4. Return the matched absolute paths.

**Known limitation**: the `Matcher` library silently ignores patterns that are themselves absolute
paths. Callers that pass absolute paths directly will receive an empty result set. This is an
accepted limitation of the underlying library; users should use relative paths or glob wildcards.

### `WriteLine(message)`

`WriteLine` writes a message to the output channel.

1. If `Silent` is `false`, write to `Console.WriteLine`.
2. If `_logWriter` is not `null`, write to `_logWriter`.

### `WriteError(message)`

`WriteError` records an error and writes it to the error channel.

1. Set `_hasErrors = true`.
2. If `Silent` is `false`, set `Console.ForegroundColor` to red, write to `Console.Error`, then
   restore the original foreground color.
3. If `_logWriter` is not `null`, write to `_logWriter`.

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
