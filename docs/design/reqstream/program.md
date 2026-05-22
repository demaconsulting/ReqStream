## Program

### Overview

`Program` is the entry point of the ReqStream executable. It owns the top-level execution flow,
dispatches to the appropriate subsystem based on the parsed command-line options, and establishes
the error-handling boundary for the entire process. All meaningful work is delegated to `Context`,
`Validation`, `Requirements`, and `TraceMatrix`; `Program` itself contains no domain logic.

There is no subsystem containing `Program`; it sits directly under the ReqStream system as a
top-level unit.

### Interfaces

**Program.Main**: Process entry point.

- *Type*: CLI entry point.
- *Role*: Provider (host environment calls this).
- *Contract*: Accepts `string[] args`; returns process exit code (`0` or `1`).
- *Constraints*: Must never block waiting for interactive input.

**Program.Run**: Internal dispatch method.

- *Type*: In-process .NET internal method.
- *Role*: Provider (called by `Main` and `Validation`).
- *Contract*: Accepts a `Context`; dispatches to the appropriate workflow based on flags.
- *Constraints*: None.

**Program.Version**: Static read-only property.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Returns the assembly informational version string, falling back to
  `AssemblyName.Version`, then `"Unknown"`.
- *Constraints*: Never throws; never returns `null`.

### Design

`Program` implements a priority-ordered dispatch in `Run`:

1. `--version` — print version string and return.
2. If not lint mode — print banner (falls through to next step).
3. `--help` — print usage and return.
4. `--validate` — call `Validation.Run(context)` and return.
5. `--lint` with no files — print "No requirements files specified" and return.
6. `--lint` — load requirements, report lint issues, and return.
7. Default — call `ProcessRequirements(context)`.

`ProcessRequirements` orchestrates the normal run: loads requirements, generates reports,
constructs `TraceMatrix` if test files are provided, exports the trace matrix, and enforces
coverage if `--enforce` is set. All export methods respect `context.FilterTags` for tag-filtered
output.

`EnforceRequirementsCoverage` evaluates whether all requirements are covered by passing tests.
It never throws; all failure signalling goes through `context.WriteError`.

`Main` establishes the error boundary: `ArgumentException` and `InvalidOperationException` are
caught and their messages written to `Console.Error` with exit code `1`. All other exceptions are
re-thrown to preserve the full stack trace.
