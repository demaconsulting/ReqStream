### TraceMatrix Unit Verification

#### Verification Strategy

The TraceMatrix unit is verified using xUnit unit tests across `TraceMatrixTests.cs`,
`TraceMatrixReadTests.cs`, and `TraceMatrixExportTests.cs`. Tests create temporary TRX and
JUnit XML test result files, construct `TraceMatrix` instances, and assert on test result
retrieval, coverage queries, and Markdown export output.

#### Test Scenarios

##### Constructor Scenario

Tests verify that TraceMatrix handles various file conditions correctly.

Test methods:

- `TraceMatrix_Constructor_WithNoFiles_CreatesEmptyMatrix` — no files → empty matrix
- `TraceMatrix_Constructor_MissingFile_ThrowsFileNotFoundException` — missing file → exception
- `TraceMatrix_Constructor_WithMultipleFiles_AggregatesResults` — multiple files aggregated
- `TraceMatrix_Constructor_WithTrxFile_ParsesCorrectly` — TRX parsed correctly
- `TraceMatrix_Constructor_WithFailedTests_TracksFailures` — failed tests tracked
- `TraceMatrix_Constructor_WithJUnitFile_ParsesCorrectly` — JUnit parsed correctly
- `TraceMatrix_Constructor_WithJUnitFailedTests_TracksFailures` — JUnit failures tracked
- `TraceMatrix_Constructor_WithMixedFormats_ProcessesBoth` — mixed formats processed

##### Test Result Retrieval Scenario

Tests verify the `GetTestResult` method with various test name formats.

Test methods:

- `TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesCorrectly` — source@test matches
- `TraceMatrix_GetTestResult_WithSourceSpecificTests_DoesNotMatchOtherSources` — no cross-match
- `TraceMatrix_GetTestResult_WithMultipleSourceSpecifiers_MatchesAllRequirements` — multiple specifiers
- `TraceMatrix_GetTestResult_WithSourceSpecificTests_IsCaseInsensitive` — case-insensitive matching
- `TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesPartialFilename` — partial filenames
- `TraceMatrix_GetTestResult_WithPlainTestNames_MatchesAllSources` — plain names match all
- `TraceMatrix_GetTestResult_WithMixedTestNames_MatchesAppropriately` — mixed types match
- `TraceMatrix_GetTestResult_WithMixedFilterAndPlainReferences_MatchesBoth` — mixed refs

##### Export Scenario

Tests verify Markdown trace matrix export.

Test methods:

- `TraceMatrix_Export_SimpleTraceMatrix_CreatesMarkdownFile` — Markdown export created
- `TraceMatrix_Export_WithFailedTests_ShowsFailures` — failures shown
- `TraceMatrix_Export_WithNoTests_ShowsNotSatisfied` — not satisfied shown
- `TraceMatrix_Export_WithNotExecutedTests_ShowsNotExecuted` — not executed shown
- `TraceMatrix_Export_WithCustomDepth_UsesCorrectHeaderLevel` — custom depth applied
- `TraceMatrix_Export_WithFilterTags_ExportsOnlyMatchingRequirements` — tag filter applied
- `TraceMatrix_Export_WithChildRequirements_ConsidersChildTests` — child requirements considered
- `TraceMatrix_CalculateSatisfiedRequirements_WithFilterTags_CountsOnlyMatchingRequirements` — tag filter count
- `TraceMatrix_GetUnsatisfiedRequirements_WithFilterTags_ReturnsOnlyMatchingRequirements` — tag filter unsatisfied

#### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Test-ResultFiles` | `TraceMatrix_Constructor_WithNoFiles_CreatesEmptyMatrix`, `TraceMatrix_Constructor_MissingFile_ThrowsFileNotFoundException`, `TraceMatrix_Constructor_WithMultipleFiles_AggregatesResults` |
| `ReqStream-Test-ChildRequirements` | `TraceMatrix_Export_WithChildRequirements_ConsidersChildTests` |
| `ReqStream-Test-TrxFormat` | `TraceMatrix_Constructor_WithTrxFile_ParsesCorrectly`, `TraceMatrix_Constructor_WithFailedTests_TracksFailures` |
| `ReqStream-Test-JUnitFormat` | `TraceMatrix_Constructor_WithJUnitFile_ParsesCorrectly`, `TraceMatrix_Constructor_WithJUnitFailedTests_TracksFailures` |
| `ReqStream-Test-MixedFormats` | `TraceMatrix_Constructor_WithMixedFormats_ProcessesBoth` |
| `ReqStream-Test-SourceFiltering` | `TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesCorrectly`, `TraceMatrix_GetTestResult_WithSourceSpecificTests_DoesNotMatchOtherSources`, `TraceMatrix_GetTestResult_WithMultipleSourceSpecifiers_MatchesAllRequirements` |
| `ReqStream-Test-CaseInsensitive` | `TraceMatrix_GetTestResult_WithSourceSpecificTests_IsCaseInsensitive` |
| `ReqStream-Test-PartialFilenames` | `TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesPartialFilename` |
| `ReqStream-Test-PlainTestNames` | `TraceMatrix_GetTestResult_WithPlainTestNames_MatchesAllSources` |
| `ReqStream-Test-MixedTestNames` | `TraceMatrix_GetTestResult_WithMixedTestNames_MatchesAppropriately` |
| `ReqStream-Test-MultipleRequirements` | `TraceMatrix_GetTestResult_WithMixedFilterAndPlainReferences_MatchesBoth` |
| `ReqStream-Report-TraceMatrix` | `TraceMatrix_Export_SimpleTraceMatrix_CreatesMarkdownFile`, `TraceMatrix_Export_WithFailedTests_ShowsFailures`, `TraceMatrix_Export_WithNoTests_ShowsNotSatisfied`, `TraceMatrix_Export_WithNotExecutedTests_ShowsNotExecuted` |
| `ReqStream-Report-TraceMatrixDepth` | `TraceMatrix_Export_WithCustomDepth_UsesCorrectHeaderLevel` |
| `ReqStream-Report-TagFiltering` | `TraceMatrix_Export_WithFilterTags_ExportsOnlyMatchingRequirements`, `TraceMatrix_CalculateSatisfiedRequirements_WithFilterTags_CountsOnlyMatchingRequirements`, `TraceMatrix_GetUnsatisfiedRequirements_WithFilterTags_ReturnsOnlyMatchingRequirements` |
