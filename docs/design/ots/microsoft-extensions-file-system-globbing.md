## Microsoft.Extensions.FileSystemGlobbing Integration Design

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

The following `Microsoft.Extensions.FileSystemGlobbing` features are used by ReqStream:

- **`Microsoft.Extensions.FileSystemGlobbing` namespace** — the primary namespace for the
  `Matcher` class.
- **`Matcher`** — the core class; one instance is created per glob expansion call. Accepts one
  or more include patterns via `AddInclude` and executes them against a directory abstraction.
- **`Matcher.AddInclude(string pattern)`** — registers a relative glob pattern for matching.
- **`Matcher.Execute(DirectoryInfoBase directoryInfo)`** — runs all registered patterns against
  the supplied directory and returns a `PatternMatchingResult`.
- **`PatternMatchingResult.Files`** — the collection of `FilePatternMatch` objects describing
  each matched file; iterated to build the results list.
- **`FilePatternMatch.Path`** — the relative path of each matched file; combined with the root
  directory to produce an absolute path.
- **`Microsoft.Extensions.FileSystemGlobbing.Abstractions` namespace** — the file-system
  abstraction layer.
- **`DirectoryInfoWrapper`** — wraps a `System.IO.DirectoryInfo` to implement the
  `DirectoryInfoBase` abstraction required by `Matcher.Execute`.

### Integration

`GlobMatcher` uses `Microsoft.Extensions.FileSystemGlobbing.Matcher` to perform pattern
matching. The `Matcher` class accepts include patterns and executes them against a
`DirectoryInfoWrapper` to return matching file paths.

### Usage

- `new Matcher()` is instantiated per glob expansion call.
- `matcher.AddInclude(pattern)` adds relative include patterns.
- `matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(rootDir)))` runs the match.
- Results from `matchResult.Files` are converted to absolute paths and added to the
  deduplication set.

### Error Handling

`GlobMatcher` is designed to be non-throwing. Patterns that reference non-existent directories
are skipped; the `Matcher` returns empty results rather than throwing. The caller (`Context.Create`)
is responsible for deciding whether zero matching files is an error condition.

### Dependencies

Only `GlobMatcher` uses this package. No other ReqStream unit depends on it directly.
