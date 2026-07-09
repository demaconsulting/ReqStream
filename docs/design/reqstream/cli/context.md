### Context

![Cli Structure](CliView.svg)

#### Purpose

`Context` is the command-line argument parser and I/O owner for ReqStream. It is the single
authoritative source for all runtime options and is the only unit permitted to write to the console
or the log file. `Context` never touches YAML content, test result data, or domain objects; its
sole concerns are parsing arguments and surfacing results to the caller. It implements
`IDisposable` so that the log-file `StreamWriter` is closed deterministically when the enclosing
`using` block in `Program.Main` exits.

#### Data Model

**`_logWriter`**: `StreamWriter?` — Open writer for the optional log file; `null` when no log file
was requested.

**`_hasErrors`**: `bool` — Accumulates error state; initially `false`; set to `true` by
`WriteError`.

**`Version`**: `bool` — `true` when `--version` or `-v` was specified.

**`Help`**: `bool` — `true` when `--help`, `-?`, or `-h` was specified.

**`Silent`**: `bool` — `true` when `--silent` was specified.

**`Validate`**: `bool` — `true` when `--validate` was specified.

**`Lint`**: `bool` — `true` when `--lint` was specified.

**`ResultsFile`**: `string?` — Path for validation test-results output file (`--results` /
`--result`); `null` when not specified.

**`Enforce`**: `bool` — `true` when `--enforce` was specified.

**`FilterTags`**: `HashSet<string>?` — Comma-separated tag filter from `--filter`; `null` when
not specified.

**`RequirementsFiles`**: `List<string>` — Expanded list of requirement file paths from
`--requirements`.

**`TestFiles`**: `List<string>` — Expanded list of test-result file paths from `--tests`.

**`RequirementsReport`**: `string?` — Destination path for requirements report (`--report`).

**`Depth`**: `int` — Default heading depth for all reports (`--depth`; default: 1).

**`ReportDepth`**: `int` — Heading depth for requirements report; defaults to `Depth`.

**`Matrix`**: `string?` — Destination path for trace matrix report (`--matrix`).

**`MatrixDepth`**: `int` — Heading depth for trace matrix report; defaults to `Depth`.

**`JustificationsFile`**: `string?` — Destination path for justifications report
(`--justifications`).

**`JustificationsDepth`**: `int` — Justifications report heading depth; defaults to `Depth`.

**`ExitCode`**: `int` — Computed: `_hasErrors ? 1 : 0`.

#### Key Methods

**Create(args)**: Static factory method that constructs and returns a fully initialized `Context`.

- *Parameters*: `string[] args` — command-line arguments.
- *Returns*: `Context` — fully initialized instance.
- *Preconditions*: `args` must not be `null`.
- *Postconditions*: All properties reflect the parsed state; glob patterns are expanded.

Implements a sequential switch-based parser over the `args` array. Each recognized flag sets the
corresponding property; flags that consume the next element advance the index by one additional
step. `--filter` values are split on `','` and accumulated into `FilterTags`. `--requirements` and
`--tests` values are passed to `GlobMatcher.FindMatchingFiles`. If `--log` is specified, the file
is opened with `AutoFlush = true`. `--depth` sets the default; per-report depth arguments override
it.

**WriteLine(message)**: Writes a message to the console (unless `Silent` is `true`) and to
`_logWriter` if a log file is open.

- *Parameters*: `string message` — text to write.
- *Returns*: `void`.
- *Preconditions*: None.
- *Postconditions*: Message is written to applicable outputs.

**WriteError(message)**: Sets `_hasErrors = true`, writes the message to `Console.Error` in red
(unless `Silent` is `true`), and writes to `_logWriter` if a log file is open.

- *Parameters*: `string message` — error text.
- *Returns*: `void`.
- *Preconditions*: None.
- *Postconditions*: `_hasErrors` is `true`; message is written.

**Dispose()**: Flushes and closes `_logWriter` if it is not `null`, then sets it to `null`.

- *Parameters*: None.
- *Returns*: `void`.
- *Preconditions*: None.
- *Postconditions*: Log file handle is released; `_logWriter` is `null`.

#### Error Handling

`Context.Create` throws `ArgumentNullException` when `args` is `null`.

`Context.Create` throws `ArgumentException` under the following conditions:

- **Unknown argument** — an unrecognized flag is present in `args`.
- **Missing argument value** — a flag that requires a value is the last argument.
- **Invalid depth value** — a `--depth`, `--report-depth`, `--matrix-depth`, or
  `--justifications-depth` value is not a positive integer.
- **Log file open failure** — the file path provided to `--log` cannot be opened for writing.

All other `Context` methods (`WriteLine`, `WriteError`, `Dispose`) do not throw.

#### Interactions

##### Dependencies

- **GlobMatcher** — called by `Create` to expand `--requirements` and `--tests` glob patterns
  into absolute file path lists.

##### Callers

- **Program** — creates `Context` via `Create`; calls `WriteLine` and `WriteError`; reads
  `ExitCode`.
- **Validation** — calls `context.WriteLine`, `context.WriteError`; reads `ResultsFile` and
  `Silent`.
- **LoadResult** — calls `context.WriteError` via `ReportIssues` to route lint issues.
