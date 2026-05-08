### PathHelpers Unit Design

#### Overview

`PathHelpers` is a static utility class that provides safe path combination guarded against
path-traversal attacks. It wraps `Path.Combine` with a validation step that ensures the
resolved combined path remains within the intended base directory. This prevents situations
where user-supplied path components (for example, `include` entries in requirements YAML files
or glob results) could escape to unintended locations.

`PathHelpers` has no mutable state; all methods are `internal static`.

#### Methods

##### `SafePathCombine(basePath, relativePath)`

`SafePathCombine` combines two path strings and validates the result.

The algorithm is:

1. Combine the paths using `Path.Combine(basePath, relativePath)` to produce `combinedPath`.
2. Resolve both `basePath` and `combinedPath` to absolute form using `Path.GetFullPath`.
3. Compute `checkRelative = Path.GetRelativePath(absoluteBase, absoluteCombined)`.
4. If `checkRelative` starts with `..` (with either separator) or is itself rooted, throw
   `ArgumentException` — the combined path has escaped the base directory.
5. Otherwise return `combinedPath` (the non-resolved form, preserving the caller's style).

The method never permits traversal outside `basePath`. It throws `ArgumentNullException` for
null inputs and `ArgumentException` for path-traversal attempts or invalid path formats.

#### Security Rationale

`Path.Combine` on its own accepts relative components such as `../../etc/passwd` and absolute
components that completely override the base. By normalising both sides with `Path.GetFullPath`
and calling `Path.GetRelativePath`, `SafePathCombine` detects any escape attempt independent of
platform separator style or case-sensitivity. The CodeQL `cs/path-combine` rule is suppressed
specifically for `PathHelpers.cs` because this is the one location where the raw `Path.Combine`
call is validated and therefore safe to use.
