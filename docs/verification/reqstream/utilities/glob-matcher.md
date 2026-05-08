### GlobMatcher Unit Verification

#### Verification Strategy

The GlobMatcher unit is verified using xUnit unit tests in `GlobMatcherTests.cs`. Tests create
temporary directories with known file contents and assert that `FindMatchingFiles` and
`SplitAbsolutePattern` return the correct results for both relative and absolute patterns.

#### Test Scenarios

##### Relative Pattern Scenario

Tests verify that relative glob patterns are resolved against the current working directory.

Test methods:

- `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles` — relative wildcard matches files in cwd

##### Absolute Pattern Scenario

Tests verify that absolute glob patterns are resolved from their rooted prefix.

Test methods:

- `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles` — absolute `*.ext` pattern
- `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories` — absolute `**/*.ext` pattern
- `GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile` — absolute literal path with no wildcard
- `GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty` — non-existent root returns empty

##### Result Format Scenario

Tests verify the format of returned paths.

Test methods:

- `GlobMatcher_FindMatchingFiles_ReturnsAbsolutePaths` — all returned paths are rooted

##### SplitAbsolutePattern Scenario

Tests verify that `SplitAbsolutePattern` correctly decomposes absolute patterns.

Test methods:

- `GlobMatcher_SplitAbsolutePattern_WildcardAtTopLevel_SplitsAtRoot` — wildcard immediately after root splits at root
- `GlobMatcher_SplitAbsolutePattern_LiteralPath_SplitsAtLastSeparator` — literal path splits at last separator
- `GlobMatcher_SplitAbsolutePattern_DoubleStarWildcard_SplitsBeforeWildcard` — `**` pattern splits before wildcard segment

#### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Command-RequirementsGlobPatterns` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories`, `GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile`, `GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty` |
| `ReqStream-Command-TestGlobPatterns` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories`, `GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile`, `GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty` |
