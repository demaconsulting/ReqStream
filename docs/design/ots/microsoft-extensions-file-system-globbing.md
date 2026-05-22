## Microsoft.Extensions.FileSystemGlobbing

`Microsoft.Extensions.FileSystemGlobbing` provides glob pattern matching used by the `GlobMatcher`
unit in the Utilities subsystem to expand `--requirements` and `--tests` command-line patterns
into resolved file path lists.

### Purpose

`Microsoft.Extensions.FileSystemGlobbing` was chosen because it is a first-party Microsoft
package that ships as part of the .NET extension ecosystem, carries an MIT license compatible
with ReqStream's own license, and provides well-tested glob semantics (`*`, `**`, `?`, `[abc]`
ranges) with built-in file-system abstraction support. Using a first-party package reduces
dependency risk and aligns with the .NET platform where ReqStream runs.

### Features Used

- **`Matcher`** — the core class; one instance is created per glob expansion call. Accepts one
  or more include patterns via `AddInclude` and executes them against a directory abstraction.
- **`Matcher.AddInclude(string pattern)`** — registers a relative glob pattern for matching.
- **`Matcher.Execute(DirectoryInfoBase directoryInfo)`** — runs all registered patterns against
  the supplied directory and returns a `PatternMatchingResult`.
- **`PatternMatchingResult.Files`** — the collection of `FilePatternMatch` objects describing
  each matched file; iterated to build the results list.
- **`FilePatternMatch.Path`** — the relative path of each matched file; combined with the root
  directory to produce an absolute path.
- **`DirectoryInfoWrapper`** — wraps a `System.IO.DirectoryInfo` to implement the
  `DirectoryInfoBase` abstraction required by `Matcher.Execute`.

### Integration Pattern

`GlobMatcher` uses `Microsoft.Extensions.FileSystemGlobbing.Matcher` to perform pattern
matching. The integration follows these steps:

1. `new Matcher()` is instantiated per glob expansion call.
2. `matcher.AddInclude(pattern)` adds relative include patterns.
3. `matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(rootDir)))` runs the match.
4. Results from `matchResult.Files` are converted to absolute paths and added to the
   deduplication set.

`GlobMatcher` is designed to be non-throwing. Patterns that reference non-existent directories
are skipped; the `Matcher` returns empty results rather than throwing. Only `GlobMatcher` uses
this package; no other ReqStream unit depends on it directly.
