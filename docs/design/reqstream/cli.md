## Cli

![Cli Structure](CliView.svg)

### Overview

The `Cli` subsystem provides the command-line interface for ReqStream. It acts as the primary
boundary between the user's shell invocation and the tool's internal logic, owning argument
parsing, output formatting, and error tracking. All other subsystems receive a `Context` object
from the `Cli` subsystem to read parsed flags and write output.

The `Cli` subsystem contains the following software unit:

- **Context** (`Cli/Context.cs`) — Argument parsing, output channels, and exit code.

### Interfaces

**Context.Create**: Factory method constructing a `Context` from `string[] args`.

- *Type*: In-process .NET public API (static factory).
- *Role*: Provider (other units consume the returned `Context`).
- *Contract*: Parses CLI flags; expands glob patterns via `GlobMatcher`; returns a fully
  initialized `Context`. Throws `ArgumentException` for invalid arguments.
- *Constraints*: Must not write to console output channels (`Console.Out` / `Console.Error`); read-only filesystem access for glob pattern expansion is permitted.

**Context output channels**: `WriteLine`, `WriteError`, and `WriteWarning` methods.

- *Type*: In-process .NET public API.
- *Role*: Provider (all subsystems write output through these methods).
- *Contract*: `WriteLine` writes to stdout and optional log; `WriteError` writes to stderr,
  sets the error flag, and optionally logs; `WriteWarning` writes a warning-level message to
  stdout and optional log without setting the error flag or affecting the exit code.
- *Constraints*: Suppressed when `--silent` is active.

**Context properties**: Parsed flags and file lists (`Version`, `Help`, `Silent`, `Validate`,
`Lint`, `Enforce`, `RequirementsFiles`, `TestFiles`, `RequirementsReport`, `Matrix`,
`JustificationsFile`, `ResultsFile`, `FilterTags`, `RootTags`, `Depth`, `ReportDepth`,
`MatrixDepth`, `JustificationsDepth`, `ExitCode`).

- *Type*: In-process .NET public API (read-only properties).
- *Role*: Provider.
- *Contract*: Each property reflects the parsed command-line state. `RootTags` is `null` when
  `--root-tags` was not supplied, and a merged `HashSet<string>` (comma-split, trimmed, combined
  across repeated occurrences of the flag — matching the `--filter`/`FilterTags` parsing
  convention exactly) otherwise.
- *Constraints*: Immutable after construction (except `_hasErrors` which is set by `WriteError`).

### Design

The `Cli` subsystem contains a single unit, `Context`, which is constructed by `Program.Main`
via the static factory method `Context.Create(args)`. After construction, `Program` holds the
`Context` and passes it to all subsystems that need to produce output. The subsystem has no
internal unit-to-unit collaboration; its design is defined entirely by the `Context` unit.

`Context` implements `IDisposable` so that the log-file `StreamWriter` is closed
deterministically when `Program.Main`'s `using` block exits.

**Depth inheritance rule**: When `--depth N` is specified on the command line, it sets the
global default heading depth (`Depth = N`). Each per-report depth property (`ReportDepth`,
`MatrixDepth`, `JustificationsDepth`) falls back to this default if its corresponding specific
flag (`--report-depth`, `--matrix-depth`, `--justifications-depth`) is not supplied. In other
words, omitting a specific depth flag causes that report to use the same depth as `--depth`. If
neither `--depth` nor a specific depth flag is present, the value defaults to `1`.

The `Cli` subsystem has no dependencies on other tool subsystems. It uses only .NET base
class library types and `GlobMatcher` from the Utilities subsystem for pattern expansion.
