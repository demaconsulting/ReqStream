## Utilities

### Overview

The `Utilities` subsystem provides shared, general-purpose helper utilities for ReqStream. It
acts as an internal library of reusable low-level components that are consumed by multiple other
subsystems. It has no dependency on any other ReqStream subsystem, meaning all other subsystems
may depend on it without creating circular references.

The `Utilities` subsystem contains the following software units:

- **GlobMatcher** (`Utilities/GlobMatcher.cs`) — Glob-pattern file matching supporting absolute
  and relative paths.
- **PathHelpers** (`Utilities/PathHelpers.cs`) — Safe path combination that guards against
  path-traversal attacks.

### Interfaces

**GlobMatcher.FindMatchingFiles**: Returns a sorted, deduplicated list of absolute file paths
matching any of the supplied glob patterns.

- *Type*: In-process .NET internal API (static method).
- *Role*: Provider (Cli subsystem consumes this).
- *Contract*: Accepts a collection of glob pattern strings; returns `List<string>` of absolute
  paths. Supports both relative patterns (resolved against the current working directory) and
  absolute patterns (resolved from the rooted prefix of the pattern).
- *Constraints*: Never throws for non-matching patterns or non-existent directories.

**PathHelpers.SafePathCombine**: Combines two paths and validates the result stays within the
base directory.

- *Type*: In-process .NET internal API (static method).
- *Role*: Provider (Modeling subsystem consumes this).
- *Contract*: Accepts `basePath` and `relativePath`; returns the combined path. Throws
  `ArgumentException` if the combined path escapes the base (path traversal attempt). Throws
  `ArgumentNullException` if `basePath` or `relativePath` is null.
- *Constraints*: Never permits traversal outside `basePath`.

### Design

The `Utilities` subsystem contains two units, `GlobMatcher` and `PathHelpers`, with a
one-directional dependency: `GlobMatcher` depends on `PathHelpers`, but `PathHelpers` has no
dependency on `GlobMatcher`. Both are declared `internal static`; they expose no instances
and are accessible only within the assembly.

`GlobMatcher` is used by `Context.Create` to expand `--requirements` and `--tests` glob patterns
into resolved file path lists. Within `FindMatchingFiles`, `GlobMatcher` calls
`PathHelpers.SafePathCombine` to construct safe absolute file paths for each matched result.
`PathHelpers.SafePathCombine` is also used by `RequirementsLoader` to combine `includes`
directory paths with relative include paths before recursing into included files.
`PathHelpers` itself does not call `GlobMatcher`.

The subsystem uses only .NET base class library types and
`Microsoft.Extensions.FileSystemGlobbing`. The `Cli`, `Modeling`, and `SelfTest` subsystems are
consumers.
