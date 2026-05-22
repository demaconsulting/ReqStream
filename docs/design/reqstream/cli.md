## Cli

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
- *Constraints*: Must not perform I/O beyond opening the log file.

**Context output channels**: `WriteLine` and `WriteError` methods.

- *Type*: In-process .NET public API.
- *Role*: Provider (all subsystems write output through these methods).
- *Contract*: `WriteLine` writes to stdout and optional log; `WriteError` writes to stderr,
  sets the error flag, and optionally logs.
- *Constraints*: Suppressed when `--silent` is active.

**Context properties**: Parsed flags and file lists (`Version`, `Help`, `Silent`, `Validate`,
`Lint`, `Enforce`, `RequirementsFiles`, `TestFiles`, `RequirementsReport`, `Matrix`,
`JustificationsFile`, `ResultsFile`, `FilterTags`, `Depth`, `ReportDepth`, `MatrixDepth`,
`JustificationsDepth`, `ExitCode`).

- *Type*: In-process .NET public API (read-only properties).
- *Role*: Provider.
- *Contract*: Each property reflects the parsed command-line state.
- *Constraints*: Immutable after construction (except `_hasErrors` which is set by `WriteError`).

### Design

The `Cli` subsystem contains a single unit, `Context`, which is constructed by `Program.Main`
via the static factory method `Context.Create(args)`. After construction, `Program` holds the
`Context` and passes it to all subsystems that need to produce output. The subsystem has no
internal unit-to-unit collaboration; its design is defined entirely by the `Context` unit.

`Context` implements `IDisposable` so that the log-file `StreamWriter` is closed
deterministically when `Program.Main`'s `using` block exits.

The `Cli` subsystem has no dependencies on other tool subsystems. It uses only .NET base
class library types and `GlobMatcher` from the Utilities subsystem for pattern expansion.
