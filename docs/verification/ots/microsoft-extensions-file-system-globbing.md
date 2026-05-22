## Microsoft.Extensions.FileSystemGlobbing

### Required Functionality

Microsoft.Extensions.FileSystemGlobbing (`ReqStream-OTS-FileSystemGlobbing`) shall match file
paths against glob patterns, returning all matching absolute file paths. It is used by the
`GlobMatcher` unit to expand `--requirements` and `--tests` command-line patterns.

### Verification Approach

Microsoft.Extensions.FileSystemGlobbing is verified by integration test evidence. The
`GlobMatcher` unit tests exercise the library on absolute patterns, relative patterns, and
non-existent directories. Passing tests confirm that the library correctly matches patterns
and returns the expected results.

### Test Scenarios

**Absolute Pattern Matching**: Verifies that the library correctly matches absolute glob patterns
with wildcard characters to the expected files. This scenario is tested by
`GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`.

**Relative Pattern Matching**: Verifies that relative glob patterns are resolved against the
current working directory and return matching files. This scenario is tested by
`GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`.

**Non-Existent Directory Handling**: Verifies that a pattern with a non-existent root directory
returns an empty result rather than throwing an exception. This scenario is tested by
`GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty`.
