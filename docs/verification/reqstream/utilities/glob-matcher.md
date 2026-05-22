### GlobMatcher Unit Verification

#### Verification Strategy

The GlobMatcher unit is verified using xUnit unit tests in `GlobMatcherTests.cs`. Tests create
temporary directories with known file contents and assert that `FindMatchingFiles` and
`SplitAbsolutePattern` return the correct results for both relative and absolute patterns.

#### Test Environment

The GlobMatcher unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Tests create temporary directories with known file structures on disk and delete them on test
completion.

#### Acceptance Criteria

The GlobMatcher unit verification is complete when all xUnit tests in `GlobMatcherTests.cs` pass
without uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Coverage Summary is mapped to at least one passing test method.

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

##### Multi-Pattern Scenario

Tests verify that multiple patterns are combined and deduplicated correctly.

Test methods:

- `GlobMatcher_FindMatchingFiles_MultiplePatterns_DeduplicatesResults` — overlapping patterns do not produce duplicate paths
- `GlobMatcher_FindMatchingFiles_MultiplePatterns_CombinesFromDifferentSources` — patterns from separate
  directories are merged

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
