## Utilities

### Verification Strategy

The Utilities subsystem is verified using xUnit unit tests. Each unit within the subsystem
has its own test class that exercises the public methods with various input combinations,
including edge cases and boundary conditions.

Unit tests serve as the sole compliance evidence for the subsystem-level requirements because
no separate subsystem integration tests exist. The subsystem contains three units —
`GlobMatcher`, `PathHelpers`, and `TemporaryDirectory` — which have no dependency on each
other (other than `GlobMatcher` and `TemporaryDirectory` both depending on `PathHelpers`) and
are individually fully verified by their respective unit test classes. Together these unit tests
fully satisfy all subsystem requirements.

### Test Environment

The Utilities subsystem tests require no setup beyond the standard xUnit test runner and .NET
runtime. Tests that exercise file matching create temporary directories with known file contents,
which are deleted on test completion.

### Acceptance Criteria

The Utilities subsystem verification is complete when all xUnit unit tests for `GlobMatcher`,
`PathHelpers`, and `TemporaryDirectory` pass without uncaught exceptions and all assertions
succeed. The subsystem is considered verified when every requirement in the Requirements
Coverage is mapped to at least one passing test method.

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
path splits at the last separator;
`GlobMatcher_SplitAbsolutePattern_DoubleStarWildcard_SplitsBeforeWildcard`, which verifies `**`
splits before the wildcard;
`GlobMatcher_FindMatchingFiles_MultiplePatterns_DeduplicatesResults`, which verifies that a file
matched by more than one pattern appears only once in the result;
`GlobMatcher_FindMatchingFiles_MultipleMatches_ReturnsSortedResults`, which verifies that results
are returned in lexicographic ascending order regardless of file-system enumeration order; and
`GlobMatcher_FindMatchingFiles_MultiplePatterns_CombinesFromDifferentSources`, which verifies that
patterns rooted in different directories are all combined into a single result list.

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

**TemporaryDirectory**: Tests verify that a `TemporaryDirectory` instance creates a directory on
construction, that two instances receive distinct paths, that `GetFilePath` returns a path rooted
inside the temporary directory and creates intermediate directories, that path-traversal attempts
are rejected with `ArgumentException`, and that `Dispose` deletes the directory including any
files written inside it and that disposing an already-deleted directory does not throw. This
scenario is tested by `TemporaryDirectory_Constructor_Default_CreatesDirectory`,
`TemporaryDirectory_Constructor_TwoInstances_CreateUniqueDirectories`,
`TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory`,
`TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories`,
`TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException`,
`TemporaryDirectory_Dispose_PopulatedDirectory_DeletesDirectory`, and
`TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow`.

### Requirements Coverage

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_SplitAbsolutePattern_WildcardAtTopLevel_SplitsAtRoot` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_SplitAbsolutePattern_LiteralPath_SplitsAtLastSeparator` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_SplitAbsolutePattern_DoubleStarWildcard_SplitsBeforeWildcard` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_MultiplePatterns_DeduplicatesResults` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_MultiplePatterns_CombinesFromDifferentSources` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_ReturnsAbsolutePaths` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty` |
| `ReqStream-Utilities-GlobMatching` | `GlobMatcher_FindMatchingFiles_MultipleMatches_ReturnsSortedResults` |
| `ReqStream-GlobMatcher-NullPatterns` | `GlobMatcher_FindMatchingFiles_NullPatterns_ThrowsArgumentNullException` |
| `ReqStream-GlobMatcher-NullElement` | `GlobMatcher_FindMatchingFiles_NullElementInPatterns_SkipsElement` |
| `ReqStream-Utilities-SafePath` | `PathHelpers_SafePathCombine_ValidRelativePath_ReturnsCombinedPath` |
| `ReqStream-Utilities-SafePath` | `PathHelpers_SafePathCombine_ValidSubdirectory_ReturnsCombinedPath` |
| `ReqStream-Utilities-SafePath` | `PathHelpers_SafePathCombine_DotDotPath_ThrowsArgumentException` |
| `ReqStream-Utilities-SafePath` | `PathHelpers_SafePathCombine_DeepDotDotPath_ThrowsArgumentException` |
| `ReqStream-Utilities-SafePath` | `PathHelpers_SafePathCombine_AbsoluteOverridePath_ThrowsArgumentException` |
| `ReqStream-Utilities-SafePath` | `PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException` |
| `ReqStream-Utilities-SafePath` | `PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException` |
| `ReqStream-Utilities-TemporaryDirectory` | `TemporaryDirectory_Constructor_Default_CreatesDirectory` |
| `ReqStream-Utilities-TemporaryDirectory` | `TemporaryDirectory_Constructor_TwoInstances_CreateUniqueDirectories` |
| `ReqStream-Utilities-TemporaryDirectory` | `TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory` |
| `ReqStream-Utilities-TemporaryDirectory` | `TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories` |
| `ReqStream-Utilities-TemporaryDirectory` | `TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException` |
| `ReqStream-Utilities-TemporaryDirectory` | `TemporaryDirectory_Dispose_PopulatedDirectory_DeletesDirectory` |
| `ReqStream-Utilities-TemporaryDirectory` | `TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow` |
