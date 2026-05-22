### PathHelpers

#### Purpose

`PathHelpers` is a static utility class that provides safe path combination guarded against
path-traversal attacks. It wraps `Path.Combine` with a validation step that ensures the
resolved combined path remains within the intended base directory. This prevents situations
where user-supplied path components (for example, `include` entries in requirements YAML files)
could escape to unintended locations. `PathHelpers` has no mutable state; all methods are
`internal static`.

#### Data Model

N/A — `PathHelpers` is a static class with no mutable instance or class-level state. All state
is local to individual `SafePathCombine` calls.

#### Key Methods

**SafePathCombine(basePath, relativePath)**: Combines two path strings and validates the result.

- *Parameters*: `string basePath` — the base directory; `string relativePath` — the relative
  path to combine.
- *Returns*: `string` — the combined path (non-resolved form, preserving the caller's style).
- *Preconditions*: Neither argument is `null`.
- *Postconditions*: The resolved combined path is within `basePath`.

The algorithm:

1. Combine the paths using `Path.Combine(basePath, relativePath)`.
2. Resolve both `basePath` and the combined path to absolute form using `Path.GetFullPath`.
3. Compute relative path from the absolute base to the absolute combined path.
4. If the relative path starts with `..` or is itself rooted, throw `ArgumentException`.
5. Otherwise return the combined path.

The method never permits traversal outside `basePath`. The CodeQL `cs/path-combine` rule is
suppressed via the repository-level CodeQL configuration in `.github/codeql-config.yml` because
this is the one location where the raw `Path.Combine` call is validated and therefore safe to use.

#### Error Handling

- **`ArgumentNullException`** — thrown when `basePath` or `relativePath` is `null`.
- **`ArgumentException`** — thrown when the resolved combined path escapes `basePath` (path
  traversal attempt detected), or when either argument contains invalid path characters.

- **`NotSupportedException`** — propagated from `Path.Combine` or `Path.GetFullPath` when a
  supplied path contains an unsupported format (e.g. a path with an unrecognised prefix on
  Windows). This exception originates in the .NET runtime; `SafePathCombine` does not throw it
  explicitly.
- **`PathTooLongException`** — propagated from `Path.Combine` or `Path.GetFullPath` when the
  combined or resolved path exceeds the system-defined maximum path length. This exception
  originates in the .NET runtime; `SafePathCombine` does not throw it explicitly.

#### Interactions

##### Dependencies

N/A — `PathHelpers` depends only on .NET base class library path APIs (`System.IO.Path`). It
has no dependencies on other ReqStream units, OTS packages, or shared packages.

##### Callers

- **RequirementsLoader** — calls `PathHelpers.SafePathCombine` to combine include-file directory
  paths with relative `includes` entries before recursing.
