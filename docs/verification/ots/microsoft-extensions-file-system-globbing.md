## Microsoft.Extensions.FileSystemGlobbing Verification

### Required Functionality

Microsoft.Extensions.FileSystemGlobbing (`ReqStream-OTS-FileSystemGlobbing`) shall match file
paths against glob patterns, returning all matching absolute file paths. It is used by the
`GlobMatcher` unit to expand `--requirements` and `--tests` command-line patterns.

### Verification Approach

Microsoft.Extensions.FileSystemGlobbing is verified by integration test evidence. The
`GlobMatcher` unit tests exercise the library on absolute patterns, relative patterns, and
non-existent directories. Passing tests confirm that the library correctly matches patterns
and returns the expected results. The following representative test methods are linked as
evidence:

- `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles`
- `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles`
- `GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty`

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-FileSystemGlobbing` | `GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles` |
| `ReqStream-OTS-FileSystemGlobbing` | `GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles` |
| `ReqStream-OTS-FileSystemGlobbing` | `GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty` |
