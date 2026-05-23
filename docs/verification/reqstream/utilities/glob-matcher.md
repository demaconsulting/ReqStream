### GlobMatcher

#### Verification Approach

The GlobMatcher unit is verified using xUnit unit tests in `GlobMatcherTests.cs`. Tests create
temporary directories with known file contents and assert that `FindMatchingFiles` and
`SplitAbsolutePattern` return the correct results for both relative and absolute patterns.

#### Test Environment

The GlobMatcher unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Tests create temporary directories with known file structures on disk and delete them on test
completion.

Two tests — `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles` and
`GlobMatcher_FindMatchingFiles_ReturnsAbsolutePaths` — call `Directory.SetCurrentDirectory`, which
is a process-wide side effect. Both tests protect against interference using a `try/finally` block
that restores the original working directory, but reviewers should be aware that these two tests
must not run concurrently with any other test that depends on or modifies the current working
directory. xUnit's default sequential-within-class execution provides the required protection for
tests in the same class.

#### Acceptance Criteria

The GlobMatcher unit verification is complete when all xUnit tests in `GlobMatcherTests.cs` pass
without uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**Relative Pattern**: Tests verify that relative glob patterns are resolved against the current
working directory. This scenario is tested by
`GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`.

**Absolute Pattern**: Tests verify that absolute glob patterns are resolved from their rooted
prefix, including wildcard, double-wildcard, literal, and non-existent directory cases. This
scenario is tested by `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`,
`GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories`,
`GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile`, and
`GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty`.

**Result Format**: Tests verify that all returned paths are rooted absolute paths. This scenario
is tested by `GlobMatcher_FindMatchingFiles_ReturnsAbsolutePaths`.

**Multi-Pattern**: Tests verify that multiple patterns are combined and deduplicated correctly.
This scenario is tested by
`GlobMatcher_FindMatchingFiles_MultiplePatterns_DeduplicatesResults` and
`GlobMatcher_FindMatchingFiles_MultiplePatterns_CombinesFromDifferentSources`.

**SplitAbsolutePattern**: Tests verify that `SplitAbsolutePattern` correctly decomposes absolute
patterns at the boundary before the first wildcard segment. This scenario is tested by
`GlobMatcher_SplitAbsolutePattern_WildcardAtTopLevel_SplitsAtRoot`,
`GlobMatcher_SplitAbsolutePattern_LiteralPath_SplitsAtLastSeparator`, and
`GlobMatcher_SplitAbsolutePattern_DoubleStarWildcard_SplitsBeforeWildcard`.

**Sort Order**: Tests verify that results are returned in lexicographic ascending order regardless
of file-system enumeration order. This scenario is tested by
`GlobMatcher_FindMatchingFiles_MultipleMatches_ReturnsSortedResults`.

**Null Patterns**: Tests verify that passing null as the patterns argument throws
`ArgumentNullException`. This scenario is tested by
`GlobMatcher_FindMatchingFiles_NullPatterns_ThrowsArgumentNullException`.

**Null Element in Patterns**: Tests verify that a null element within the patterns collection is
silently skipped and non-null patterns are still processed. This scenario is tested by
`GlobMatcher_FindMatchingFiles_NullElementInPatterns_SkipsElement`.

#### Requirements Coverage

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-GlobMatcher-RelativePatterns` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles` |
| `ReqStream-GlobMatcher-AbsolutePatterns` | `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles` |
| `ReqStream-GlobMatcher-AbsolutePatterns` | `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories` |
| `ReqStream-GlobMatcher-AbsolutePatterns` | `GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile` |
| `ReqStream-GlobMatcher-AbsolutePatterns` | `GlobMatcher_SplitAbsolutePattern_WildcardAtTopLevel_SplitsAtRoot` |
| `ReqStream-GlobMatcher-AbsolutePatterns` | `GlobMatcher_SplitAbsolutePattern_LiteralPath_SplitsAtLastSeparator` |
| `ReqStream-GlobMatcher-AbsolutePatterns` | `GlobMatcher_SplitAbsolutePattern_DoubleStarWildcard_SplitsBeforeWildcard` |
| `ReqStream-GlobMatcher-Deduplication` | `GlobMatcher_FindMatchingFiles_MultiplePatterns_DeduplicatesResults` |
| `ReqStream-GlobMatcher-Deduplication` | `GlobMatcher_FindMatchingFiles_MultiplePatterns_CombinesFromDifferentSources` |
| `ReqStream-GlobMatcher-AbsolutePaths` | `GlobMatcher_FindMatchingFiles_ReturnsAbsolutePaths` |
| `ReqStream-GlobMatcher-EmptyResult` | `GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty` |
| `ReqStream-GlobMatcher-SortOrder` | `GlobMatcher_FindMatchingFiles_MultipleMatches_ReturnsSortedResults` |
| `ReqStream-GlobMatcher-NullPatterns` | `GlobMatcher_FindMatchingFiles_NullPatterns_ThrowsArgumentNullException` |
| `ReqStream-GlobMatcher-NullElement` | `GlobMatcher_FindMatchingFiles_NullElementInPatterns_SkipsElement` |
