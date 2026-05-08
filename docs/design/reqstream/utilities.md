## Utilities Subsystem Design

The `Utilities` subsystem provides shared, general-purpose helper utilities for ReqStream.
It contains units that perform low-level operations reused by multiple subsystems, keeping
domain-specific subsystems focused on their own responsibilities.

### Overview

The `Utilities` subsystem acts as an internal library of reusable components. It has no
dependency on any other ReqStream subsystem; all other subsystems may depend on it.

### Units

The `Utilities` subsystem contains the following software unit:

| Unit           | File                     | Responsibility                                            |
|----------------|--------------------------|-----------------------------------------------------------|
| `GlobMatcher`  | `Utilities/GlobMatcher.cs` | Glob-pattern file matching supporting absolute and relative paths. |

### Interfaces

The `Utilities` subsystem exposes the following interface to the rest of the tool:

- **`GlobMatcher.FindMatchingFiles`** — Returns a list of absolute file paths that match a
  given glob pattern. Supports both relative patterns (resolved against the current working
  directory) and absolute patterns (resolved from the rooted prefix of the pattern).

### Interactions

The `Utilities` subsystem has no dependencies on other tool subsystems. It uses only .NET
base class library types and `Microsoft.Extensions.FileSystemGlobbing`. The `Cli` subsystem
(`Context`) is the primary consumer.

### Error Handling

`GlobMatcher.FindMatchingFiles` does not throw for non-matching patterns or non-existent
directories; it returns an empty list in those cases, leaving the caller to decide whether
zero matches is an error condition.
