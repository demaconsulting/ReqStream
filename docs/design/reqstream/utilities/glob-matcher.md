### GlobMatcher

#### Purpose

`GlobMatcher` is a static utility class that resolves glob patterns to lists of absolute
file paths. It wraps `Microsoft.Extensions.FileSystemGlobbing.Matcher` and adds support for
absolute patterns in addition to the relative patterns natively supported by the underlying
library. This allows callers such as `Context` to accept `--requirements` and `--tests`
patterns in either form without special-casing. `GlobMatcher` has no mutable state; all methods
are `internal static`.

#### Data Model

N/A — `GlobMatcher` is a static class with no mutable instance or class-level state. All state
is allocated locally within `FindMatchingFiles` calls and is not shared between invocations.

#### Key Methods

**FindMatchingFiles(patterns)**: Accepts a collection of glob pattern strings and returns a
`List<string>` of absolute file paths that match any of the supplied patterns.

- *Parameters*: `IEnumerable<string> patterns` — glob patterns to resolve.
- *Returns*: `List<string>` — sorted, deduplicated absolute paths.
- *Preconditions*: `patterns` must not be null; `ArgumentNullException` is thrown if null.
  Individual null elements within the collection are skipped silently. An empty collection
  produces an empty result.
- *Postconditions*: Duplicates are removed using a `HashSet<string>` with the appropriate
  file-system comparer: on Windows, `StringComparer.OrdinalIgnoreCase` is used (case-insensitive,
  matching NTFS semantics); on non-Windows systems, `StringComparer.Ordinal` is used
  (case-sensitive, matching ext4/APFS default semantics). Platform detection uses
  `OperatingSystem.IsWindows()`. Results are sorted using the same comparer so that sort order
  is also consistent with the file system's case rules.

For each pattern, the method checks `Path.IsPathRooted(pattern)`:

- **Absolute pattern** — calls `SplitAbsolutePattern` to decompose into a root directory and a
  relative sub-pattern. If the root directory does not exist the pattern is skipped. Otherwise a
  `Matcher` is created with the relative sub-pattern as an include rule and executed against the
  root directory.
- **Relative pattern** — collected into a list and processed together after all absolute patterns.
  A single `Matcher` with all relative include rules is executed against the current working
  directory.

**SplitAbsolutePattern(absolutePattern)**: Decomposes an absolute glob pattern into a
`(rootDir, relativePattern)` tuple.

- *Parameters*: `string absolutePattern` — an absolute glob pattern.
- *Returns*: `(string rootDir, string relativePattern)` tuple.
- *Preconditions*: Pattern must be rooted.
- *Postconditions*: `rootDir` is a valid directory path; `relativePattern` is relative.

The algorithm finds the first wildcard character (`*`, `?`, or `[`) and splits the pattern at
the last directory separator preceding that wildcard. Edge cases (no wildcard, no separator
before wildcard, empty root) are handled with fallback logic.

#### Error Handling

`GlobMatcher` is designed to be non-throwing for all normal use cases:

- Non-matching patterns return an empty result; no exception is raised.
- Patterns that reference non-existent directories are skipped silently.
- `SplitAbsolutePattern` does not throw; it handles edge cases using fallback logic.
- `FindMatchingFiles` throws `ArgumentNullException` if `patterns` itself is null.

The caller (`Context.Create`) is responsible for deciding whether zero matching files is an
error condition.

#### Interactions

##### Dependencies

- **Microsoft.Extensions.FileSystemGlobbing** — provides the `Matcher` class used to evaluate
  glob patterns against the file system.

##### Callers

- **Context** — `Context.Create` calls `GlobMatcher.FindMatchingFiles` to resolve
  `--requirements` and `--tests` patterns.
