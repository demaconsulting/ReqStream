## Utilities Subsystem Design

### Overview

The `Utilities` subsystem provides shared, general-purpose helper utilities for ReqStream. It
acts as an internal library of reusable low-level components that are consumed by multiple other
subsystems. It has no dependency on any other ReqStream subsystem, meaning all other subsystems
may depend on it without creating circular references.

The `Utilities` subsystem contains the following software units:

| Unit | File | Responsibility |
| ---- | ---- | -------------- |
| `GlobMatcher` | `Utilities/GlobMatcher.cs` | Glob-pattern file matching supporting absolute and relative paths. |
| `PathHelpers` | `Utilities/PathHelpers.cs` | Safe path combination that guards against path-traversal attacks. |

### Interfaces

The `Utilities` subsystem exposes the following interfaces to the rest of the tool:

- **`GlobMatcher.FindMatchingFiles`** — Returns a sorted, deduplicated list of absolute file
  paths that match any of the supplied glob patterns. Supports both relative patterns (resolved
  against the current working directory) and absolute patterns (resolved from the rooted prefix
  of the pattern).
- **`PathHelpers.SafePathCombine`** — Combines two paths and validates the result stays within
  the base directory. Throws `ArgumentException` if the combined path escapes the base (path
  traversal attempt).

### Design

The `Utilities` subsystem contains two independent units, `GlobMatcher` and `PathHelpers`, which
have no dependency on each other. Both are declared `internal static`; they expose no instances
and are accessible only within the assembly.

`GlobMatcher` is used by `Context.Create` to expand `--requirements` and `--tests` glob patterns
into resolved file path lists. `PathHelpers.SafePathCombine` is used by `RequirementsLoader` to
combine `includes` directory paths with relative include paths before recursing into included
files. Neither unit calls the other.

### Interactions

It uses only .NET base class library types and `Microsoft.Extensions.FileSystemGlobbing`. The
`Cli`, `Modeling`, and `SelfTest` subsystems are consumers.

### Error Handling

`GlobMatcher.FindMatchingFiles` does not throw for non-matching patterns or non-existent
directories; it returns an empty list in those cases, leaving the caller to decide whether
zero matches is an error condition.

`PathHelpers.SafePathCombine` throws `ArgumentException` if the resolved combined path escapes
the base directory. Callers are responsible for handling this exception if graceful recovery is
required.
