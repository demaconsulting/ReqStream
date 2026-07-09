## Program

![ReqStream System Structure](ReqStreamView.svg)

### Purpose

`Program` is the entry point of the ReqStream executable. It owns the top-level execution flow,
dispatches to the appropriate subsystem based on the parsed command-line options, and establishes
the error-handling boundary for the entire process. All meaningful work is delegated to `Context`,
`Validation`, `Requirements`, and `TraceMatrix`; `Program` itself contains no domain logic.

There is no subsystem containing `Program`; it sits directly under the ReqStream system as a
top-level unit.

### Data Model

N/A — `Program` is a static entry-point class with no instance fields. The private static field
`_version` is an implementation detail that caches the assembly version string; it is documented
in Key Methods.

### Key Methods

**Program.Main**: Process entry point.

- *Type*: CLI entry point.
- *Role*: Provider (host environment calls this).
- *Contract*: Accepts `string[] args`; returns process exit code (`0` or `1`).
- *Constraints*: Must never block waiting for interactive input.

**Program.Run**: Internal dispatch method.

- *Type*: In-process .NET internal method.
- *Role*: Provider (called by `Main` and `Validation`).
- *Contract*: Accepts a `Context`; dispatches to the appropriate workflow based on flags using a
  priority-ordered sequence:
  1. `--version` — print version string and return.
  2. If not lint mode — print banner (falls through to next step).
  3. `--help` — print usage and return.
  4. `--validate` — call `Validation.Run(context)` and return.
  5. `--lint` with no files — print "No requirements files specified" and return.
  6. `--lint` — load requirements, report lint issues, and return.
  7. Default — call `ProcessRequirements(context)`.
- *Constraints*: None.

**Program.Version**: Static read-only property.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Returns the assembly informational version string. Backed by the private `_version`
  field, which is initialized once at class load by reading
  `AssemblyInformationalVersionAttribute`, falling back to `AssemblyName.Version`, then
  `"Unknown"`. This avoids repeated reflection on every access.
- *Constraints*: Never throws; never returns `null`.

**ProcessRequirements** (private): Orchestrates the normal (non-version, non-help, non-validate,
non-lint) run.

- If `context.RequirementsFiles.Count == 0`, writes an informational "No requirements files
  specified." message via `context.WriteLine` and returns early without performing any further
  processing.
- Loads requirements via `Requirements.Load`.
- Reports any lint issues found; aborts if loading failed.
- Exports the requirements report if `context.RequirementsReport` is set.
- If `context.JustificationsFile` is set, `requirements.ExportJustifications` is called to
  produce the justifications report before the trace matrix is constructed.
- Constructs a `TraceMatrix` if `context.TestFiles` is non-empty; exports the matrix if
  `context.Matrix` is set.
- If `context.Matrix` is set and `context.TestFiles` is empty, writes an error via
  `context.WriteError` and returns without constructing a `TraceMatrix`.
- If `--enforce` is set but `context.TestFiles` is empty (so no `TraceMatrix` was constructed),
  an error is reported via `context.WriteError` and the method returns without enforcing coverage.
- Enforces coverage if `--enforce` is set, via `EnforceRequirementsCoverage`.

**EnforceRequirementsCoverage** (private): Evaluates whether all requirements are covered by
passing tests. Never throws; all failure signalling goes through `context.WriteError`.

### Error Handling

`Main` is the sole error boundary for the process:

- **`ArgumentException`**: Caught; the exception message is written to `Console.Error` prefixed
  with `"Error: "` and exit code `1` is returned. This covers invalid or missing argument values
  detected during `Context.Create`.
- **`InvalidOperationException`**: Caught; handled identically to `ArgumentException`. This
  covers operational failures raised by domain logic (e.g., malformed input files).
- **Unexpected exceptions**: Not caught; the exception message is first written to `Console.Error`
  (prefixed with `"Unexpected error: "`) so the error is visible even when the runtime's
  unhandled-exception handler suppresses the stack trace, then the exception is re-thrown to
  preserve the full stack trace and generate event logs.

Errors that occur within `Run` and its callees (e.g., I/O failures, parse errors) that are not
`ArgumentException` or `InvalidOperationException` propagate unhandled through `Run` and are
caught by the `Main` unexpected-exception handler.

### Interactions

**Dependencies** (units and subsystems this unit calls):

- `Cli.Context` — created by `Main` via `Context.Create(args)`; all output and program state flow
  through it.
- `Modeling.Requirements` — loaded via `Requirements.Load` in both the lint and default dispatch
  paths.
- `SelfTest.Validation` — invoked via `Validation.Run(context)` in the `--validate` dispatch
  path.
- `Tracing.TraceMatrix` — constructed directly in `ProcessRequirements` when test files are
  present.

**Callers** (who calls this unit):

- The host operating system calls `Main` as the process entry point.
- `SelfTest.Validation` calls `Program.Run` during self-validation to exercise the full dispatch
  path without spawning a child process.
- Test code calls `Program.Run` directly, supplying a pre-constructed `Context`, to exercise all
  dispatch branches without spawning a child process.
