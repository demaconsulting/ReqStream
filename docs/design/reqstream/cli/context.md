### Context Unit Design

#### Purpose

`Context` is the command-line argument parser and I/O owner for ReqStream. It is the single
authoritative source for all runtime options and is the only unit permitted to write to the console
or the log file. `Context` never touches YAML content, test result data, or domain objects; its
sole concerns are parsing arguments and surfacing results to the caller.

`Context` implements `IDisposable` so that the log-file `StreamWriter` is closed deterministically
when the enclosing `using` block in `Program.Main` exits.

#### Data Model

##### Private State

- **`_logWriter`** (`StreamWriter?`, `--log`) — Open writer for the optional log file; `null` when no log file was requested.
- **`_hasErrors`** (`bool`) — Accumulates error state; initially `false`; set to `true` by `WriteError`.

##### Properties

| Property | Type | CLI flag | Notes |
| -------- | ---- | -------- | ----- |
| `Version` | `bool` | `--version` / `-v` | Print version and exit |
| `Help` | `bool` | `--help` / `-?` / `-h` | Print usage and exit |
| `Silent` | `bool` | `--silent` | Suppress console output |
| `Validate` | `bool` | `--validate` | Run self-validation tests |
| `Lint` | `bool` | `--lint` | Lint requirements files |
| `ResultsFile` | `string?` | `--results` / `--result` | Path for validation test-results output file |
| `Enforce` | `bool` | `--enforce` | Fail if requirements are not fully covered |
| `FilterTags` | `HashSet<string>?` | `--filter` | Comma-separated tag filter; `null` when not specified |
| `RequirementsFiles` | `List<string>` | `--requirements` | Expanded list of requirement file paths |
| `TestFiles` | `List<string>` | `--tests` | Expanded list of test-result file paths |
| `RequirementsReport` | `string?` | `--report` | Destination path for requirements report |
| `Depth` | `int` | `--depth` | Default heading depth for all reports (default: 1) |
| `ReportDepth` | `int` | `--report-depth` | Heading depth for requirements report; defaults to `Depth` |
| `Matrix` | `string?` | `--matrix` | Destination path for trace matrix report |
| `MatrixDepth` | `int` | `--matrix-depth` | Heading depth for trace matrix report; defaults to `Depth` |
| `JustificationsFile` | `string?` | `--justifications` | Destination path for justifications report |
| `JustificationsDepth` | `int` | `--justifications-depth` | Justifications report heading depth; defaults to `Depth` |
| `ExitCode` | `int` | — | Computed: `_hasErrors ? 1 : 0` |

#### Key Methods

##### `Create(args)`

`Create` is the static factory method that constructs and returns a fully initialized `Context`. It
implements a sequential switch-based parser over the `args` array. Each recognized flag sets the
corresponding property; flags that consume the next element (e.g., `--requirements`) advance the
index by one additional step. An unrecognized argument or a missing value for a flag that requires
one causes an `ArgumentException`, which surfaces to the caller as a user-actionable error message
rather than an unhandled exception.

`--filter` values are split on `','` and accumulated into `FilterTags`; multiple `--filter`
arguments merge into the same set. `--requirements` and `--tests` values are passed to
`GlobMatcher.FindMatchingFiles` and the resulting absolute paths are appended to the respective
file lists. If `--log` is specified, the named
file is opened for writing with `AutoFlush = true` and assigned to `_logWriter` before the method returns. The `AutoFlush = true` setting ensures that all output is flushed to disk immediately on each write, preventing log truncation if the process exits unexpectedly before `Dispose` is called. If the log file
cannot be opened (for example, due to an invalid path or insufficient permissions), `Create` catches
the underlying I/O exception, wraps it in an `ArgumentException`, and rethrows it so the caller
receives a user-actionable error message rather than an unhandled exception.

`--depth` sets the default heading depth (`Depth`). The per-report depth arguments
(`--report-depth`, `--matrix-depth`, `--justifications-depth`) override this default if
specified; otherwise each report inherits the value of `Depth`.

##### Glob Resolution

`Create` accumulates each `--requirements` and `--tests` pattern value into a separate list
during argument parsing. After all arguments are parsed, `GlobMatcher.FindMatchingFiles` is
called once per list, receiving all patterns together. `GlobMatcher` supports both relative
patterns (resolved against the current working directory) and absolute patterns (resolved from
the rooted prefix of the pattern), and deduplicates results across all supplied patterns. The
resolved absolute file paths are stored in `RequirementsFiles` and `TestFiles` respectively.

##### `WriteLine(message)`

`WriteLine` writes a message to the console (unless `Silent` is `true`) and to `_logWriter` if a
log file is open.

##### `WriteError(message)`

`WriteError` sets `_hasErrors = true`, writes the message to `Console.Error` in red (unless
`Silent` is `true`), and also writes it to `_logWriter` if a log file is open. Setting
`_hasErrors` ensures that `ExitCode` returns `1` after any error is reported.

##### `Dispose()`

`Dispose` flushes and closes `_logWriter` if it is not `null`, then sets it to `null`. This
ensures the log file is not truncated and file handles are not leaked even when the process exits
via an early return path.

#### Error Handling

`Context.Create` throws `ArgumentNullException` when `args` is `null`.

`Context.Create` throws `ArgumentException` under the following conditions:

- **Unknown argument** — An unrecognized flag is present in `args`.
- **Missing argument value** — A flag that requires a value is the last argument (no value follows).
- **Invalid depth value** — A `--depth`, `--report-depth`, `--matrix-depth`,
  or `--justifications-depth` value is not a positive integer.
- **Log file open failure** — The file path provided to `--log` cannot be opened for writing.

All other `Context` methods (`WriteLine`, `WriteError`, `Dispose`) do not throw; they handle
internal failure cases silently (for example, `Dispose` sets `_logWriter` to `null` after
closing it, preventing double-disposal errors).

#### Interactions

| Unit | Nature of interaction |
| ---- | --------------------- |
| `GlobMatcher` | Called by `Create` to expand `--requirements` and `--tests` glob patterns into absolute file path lists |
| `Program` | Creates `Context` via `Create`; calls `WriteLine` and `WriteError`; reads `ExitCode` |
| `Validation` | Calls `context.WriteLine`, `context.WriteError`; reads `ResultsFile` and `Silent` |
| `LoadResult` | Calls `context.WriteError` via `ReportIssues` to route lint issues |
| `Requirements` | Receives `RequirementsFiles` from context; does not hold a reference to `Context` |
| `TraceMatrix` | Receives `TestFiles` from context; does not hold a reference to `Context` |
