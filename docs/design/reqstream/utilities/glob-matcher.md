### GlobMatcher Unit Design

#### Overview

`GlobMatcher` is a static utility class that resolves glob patterns to lists of absolute
file paths. It wraps `Microsoft.Extensions.FileSystemGlobbing.Matcher` and adds support for
*absolute* patterns in addition to the relative patterns natively supported by the underlying
library. This allows callers such as `Context` to accept `--requirements` and `--tests`
patterns in either form without special-casing.

`GlobMatcher` has no mutable state; all methods are `internal static`.

#### Methods

##### `FindMatchingFiles(patterns)`

`FindMatchingFiles` accepts a collection of glob pattern strings and returns a `List<string>` of
absolute file paths that match any of the supplied patterns. Duplicates are removed using a
`HashSet<string>` with the appropriate file-system comparer (ordinal ignore-case on Windows,
ordinal on case-sensitive systems). Results are sorted using the same comparer.

For each pattern in `patterns`, the method checks `Path.IsPathRooted(pattern)`:

- **Absolute pattern** — calls `SplitAbsolutePattern` to decompose the pattern into a root
  directory and a relative sub-pattern. If the root directory does not exist the pattern is
  skipped. Otherwise a `Matcher` is created with the relative sub-pattern as an include rule,
  executed against the root directory, and the results are added to the deduplication set.
- **Relative pattern** — collected into a list and processed together after all absolute
  patterns. A single `Matcher` with all relative include rules is executed against
  `Directory.GetCurrentDirectory()`, and results are added to the deduplication set.

The method never throws for non-matching patterns or non-existent directories; it returns an
empty list in those cases.

##### `SplitAbsolutePattern(absolutePattern)`

`SplitAbsolutePattern` decomposes an absolute glob pattern into a `(rootDir, relativePattern)`
tuple. The algorithm is:

1. Determine `pathRoot` via `Path.GetPathRoot`.
2. Find the index of the first wildcard character (`*`, `?`, or `[`) in `absolutePattern`.
3. If **no wildcard** is present, treat the pattern as a literal file path:
   - `rootDir` = `Path.GetDirectoryName(absolutePattern)` (or `pathRoot` if null)
   - `relativePattern` = `Path.GetFileName(absolutePattern)`
4. If a **wildcard is present**, find the last directory separator (`/` or `\`) that precedes
   the wildcard:
   - If no separator precedes the wildcard, `rootDir` = `pathRoot` and `relativePattern` =
     the pattern after stripping the path root prefix.
   - Otherwise split at that separator: `rootDir` = the left portion, `relativePattern` = the
     right portion.
   - If `rootDir` is empty (e.g. Unix pattern `/file.yaml` where the separator is at index 0),
     `rootDir` is set to `pathRoot`.
   - If `rootDir` equals the path root without its trailing separator (e.g. Windows `C:` from
     `C:\*.yaml`), the path root with its trailing separator is used instead, so that
     `DirectoryInfo` receives a valid drive-root path.

#### Interactions with Other Units

| Unit        | Nature of interaction                                                                    |
| ----------- | ---------------------------------------------------------------------------------------- |
| `Context`   | Calls `GlobMatcher.FindMatchingFiles` to expand `--requirements` and `--tests` patterns  |
