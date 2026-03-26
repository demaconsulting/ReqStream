# Program Design

## Overview

The `Program` class is the application entry point and top-level orchestrator. It coordinates the processing
pipeline based on the flags parsed by `Context`, delegating to the appropriate units in priority order.

## Structure

`Program` is an `internal static` class with the following members:

### Version Property

```csharp
public static string Version { get; }
```

Reads the informational version string from `AssemblyInformationalVersionAttribute` on the executing
assembly. Falls back to the assembly version, then `"Unknown"` if neither is available.

### Main Entry Point

```csharp
private static int Main(string[] args)
```

The `Main` method:

1. Creates a `Context` via `Context.Create(args)`
2. Calls `Run(context)` to execute the program logic
3. Returns `context.ExitCode`

Caught exceptions and their handling:

| Exception | Handling |
| :--- | :--- |
| `ArgumentException` | Prints message to stderr, returns exit code 1 |
| `InvalidOperationException` | Prints message to stderr, returns exit code 1 |
| All other exceptions | Prints message to stderr, re-throws to generate event log entries |

### Run Method

```csharp
public static void Run(Context context)
```

Executes the program logic in strict priority order:

1. **Version** — if `context.Version` is set, prints the version string and returns
2. **Banner** — prints the application banner via `PrintBanner`
3. **Help** — if `context.Help` is set, prints usage via `PrintHelp` and returns
4. **Self-Validation** — if `context.Validate` is set, delegates to `Validation.Run` and returns
5. **Requirements Processing** — calls `ProcessRequirements`

`Run` is `public` to enable direct invocation from self-validation tests without going through `Main`.

### ProcessRequirements Method

```csharp
private static void ProcessRequirements(Context context)
```

Loads and processes requirements files in the following order:

1. If no requirements files are specified, prints a message and returns
2. Calls `Requirements.Read` to load and merge all requirements files
3. If `--report` was specified, calls `requirements.Export`
4. If `--justifications` was specified, calls `requirements.ExportJustifications`
5. If test files are specified, constructs a `TraceMatrix`
6. If `--matrix` was specified, calls `traceMatrix.Export`
7. If `--enforce` was specified, calls `EnforceRequirementsCoverage`

### EnforceRequirementsCoverage Method

```csharp
private static void EnforceRequirementsCoverage(Context context, TraceMatrix? traceMatrix)
```

Validates that all requirements (within any tag filter) are satisfied by passing tests:

- If no trace matrix is available, reports an error via `context.WriteError`
- Calls `traceMatrix.CalculateSatisfiedRequirements` to get totals
- If unsatisfied requirements exist, lists each ID via `context.WriteError`

## Key Design Decisions

- **Priority-based dispatch**: The `Run` method uses an explicit priority order rather than flags being
  independent, ensuring deterministic behavior when multiple flags are combined.
- **Public `Run` for testability**: Making `Run` public allows `Validation` tests to call it directly with
  a controlled `Context`, avoiding the need to spawn a separate process.
- **Exception handling at the boundary**: `Main` catches expected exception types to produce clean error
  messages; unexpected exceptions are re-thrown to allow operating system event logging.

## Relationships

- **Uses**: `Context` (created in `Main`), `Requirements` (via `ProcessRequirements`), `TraceMatrix`
  (via `ProcessRequirements`), `Validation` (via `Run`)
- **Called by**: Operating system (via `Main`), `Validation` test methods (via `Run`)
