## Utilities

### Verification Approach

The Utilities subsystem is verified using xUnit unit tests. Each unit within the subsystem
has its own test class that exercises the public methods with various input combinations,
including edge cases and boundary conditions.

### Test Environment

The Utilities subsystem tests require no setup beyond the standard xUnit test runner and .NET
runtime. Tests that exercise file matching create temporary directories with known file contents,
which are deleted on test completion.

### Acceptance Criteria

The Utilities subsystem verification is complete when all xUnit unit tests for `GlobMatcher` and
`PathHelpers` pass without uncaught exceptions and all assertions succeed. The subsystem is
considered verified when every requirement in the Requirements Coverage is mapped to at least one
passing test method.

### Test Scenarios

**GlobMatcher**: Tests verify that `GlobMatcher` correctly resolves both relative and absolute
glob patterns to absolute file paths. This scenario is tested by
`GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`, which verifies a relative pattern
matches files; `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`, which
verifies an absolute wildcard pattern matches files;
`GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories`,
which verifies `**` matches files in subdirectories;
`GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile`, which verifies a literal
absolute path matches a single file;
`GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty`, which verifies a
missing root returns an empty result; `GlobMatcher_FindMatchingFiles_ReturnsAbsolutePaths`, which
verifies returned paths are rooted;
`GlobMatcher_SplitAbsolutePattern_WildcardAtTopLevel_SplitsAtRoot`, which verifies a wildcard at
the top level splits at the root;
`GlobMatcher_SplitAbsolutePattern_LiteralPath_SplitsAtLastSeparator`, which verifies a literal
path splits at the last separator; and
`GlobMatcher_SplitAbsolutePattern_DoubleStarWildcard_SplitsBeforeWildcard`, which verifies `**`
splits before the wildcard.

**PathHelpers**: Tests verify that `PathHelpers.SafePathCombine` correctly combines paths and
rejects path-traversal attempts. This scenario is tested by
`PathHelpers_SafePathCombine_ValidRelativePath_ReturnsCombinedPath`, which verifies a normal
relative path is combined;
`PathHelpers_SafePathCombine_ValidSubdirectory_ReturnsCombinedPath`, which verifies a subdirectory
component is combined;
`PathHelpers_SafePathCombine_DotDotPath_ThrowsArgumentException`, which verifies `..` single
traversal is rejected;
`PathHelpers_SafePathCombine_DeepDotDotPath_ThrowsArgumentException`, which verifies nested
`../..` traversal is rejected;
`PathHelpers_SafePathCombine_AbsoluteOverridePath_ThrowsArgumentException`, which verifies an
absolute path override is rejected;
`PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException`, which verifies a null base
path is rejected; and `PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException`,
which verifies a null relative path is rejected.

### Requirements Coverage

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Command-RequirementsGlobPatterns` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories` |
| `ReqStream-Command-TestGlobPatterns` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories` |
