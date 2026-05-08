## Utilities Subsystem Verification

### Verification Strategy

The Utilities subsystem is verified using xUnit unit tests. Each unit within the subsystem
has its own test class that exercises the public methods with various input combinations,
including edge cases and boundary conditions.

### Test Scenarios

#### GlobMatcher Scenario

Tests verify that `GlobMatcher` correctly resolves both relative and absolute glob patterns
to absolute file paths.

Test methods:

- `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles` — relative pattern matches files
- `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles` — absolute wildcard matches
- `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories` — `**` matches subdirectories
- `GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile` — literal absolute path
- `GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty` — missing root returns empty
- `GlobMatcher_FindMatchingFiles_ReturnsAbsolutePaths` — returned paths are rooted
- `GlobMatcher_SplitAbsolutePattern_WildcardAtTopLevel_SplitsAtRoot` — wildcard at top splits at root
- `GlobMatcher_SplitAbsolutePattern_LiteralPath_SplitsAtLastSeparator` — literal splits at separator
- `GlobMatcher_SplitAbsolutePattern_DoubleStarWildcard_SplitsBeforeWildcard` — `**` splits before wildcard

#### PathHelpers Scenario

Tests verify that `PathHelpers.SafePathCombine` correctly combines paths and rejects
path-traversal attempts.

Test methods:

- `PathHelpers_SafePathCombine_ValidRelativePath_ReturnsCombinedPath` — normal relative path is combined
- `PathHelpers_SafePathCombine_ValidSubdirectory_ReturnsCombinedPath` — subdirectory component is combined
- `PathHelpers_SafePathCombine_DotDotPath_ThrowsArgumentException` — `..` single traversal is rejected
- `PathHelpers_SafePathCombine_DeepDotDotPath_ThrowsArgumentException` — nested `../..` traversal is rejected
- `PathHelpers_SafePathCombine_AbsoluteOverridePath_ThrowsArgumentException` — absolute path override is rejected
- `PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException` — null base path is rejected
- `PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException` — null relative path is rejected

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Command-RequirementsGlobPatterns` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories` |
| `ReqStream-Command-TestGlobPatterns` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`, `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories` |
