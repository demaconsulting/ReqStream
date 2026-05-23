### TraceMatrix

#### Verification Approach

The TraceMatrix unit is verified using xUnit unit tests across `TraceMatrixTests.cs`,
`TraceMatrixReadTests.cs`, and `TraceMatrixExportTests.cs`. Tests create temporary TRX and
JUnit XML test result files, construct `TraceMatrix` instances, and assert on test result
retrieval, coverage queries, and Markdown export output.

#### Test Environment

The TraceMatrix unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary TRX and JUnit XML test result files and YAML requirements files are created in per-test
temporary directories and deleted on test completion.

#### Acceptance Criteria

The TraceMatrix unit verification is complete when all xUnit tests across `TraceMatrixTests.cs`,
`TraceMatrixReadTests.cs`, and `TraceMatrixExportTests.cs` pass without uncaught exceptions and all
assertions succeed. The unit is considered verified when every requirement in the Requirements
Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**Constructor**: Tests verify that TraceMatrix handles various file conditions correctly,
including no files, missing files, multiple files, TRX parsing, failed test tracking, JUnit
parsing, JUnit failure tracking, and mixed format processing. This scenario is tested by
`TraceMatrix_Constructor_WithNoFiles_CreatesEmptyMatrix`,
`TraceMatrix_Constructor_MissingFile_ThrowsFileNotFoundException`,
`TraceMatrix_Constructor_WithMultipleFiles_AggregatesResults`,
`TraceMatrix_Constructor_WithTrxFile_ParsesCorrectly`,
`TraceMatrix_Constructor_WithFailedTests_TracksFailures`,
`TraceMatrix_Constructor_WithJUnitFile_ParsesCorrectly`,
`TraceMatrix_Constructor_WithJUnitFailedTests_TracksFailures`, and
`TraceMatrix_Constructor_WithMixedFormats_ProcessesBoth`.

**Test Result Retrieval**: Tests verify the `GetTestResult` method with various test name formats,
including source-specific matching, cross-source isolation, multiple source specifiers,
case-insensitive matching, partial filename matching, plain test names, mixed test names, and
mixed filter and plain references. This scenario is tested by
`TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesCorrectly`,
`TraceMatrix_GetTestResult_WithSourceSpecificTests_DoesNotMatchOtherSources`,
`TraceMatrix_GetTestResult_WithMultipleSourceSpecifiers_MatchesAllRequirements`,
`TraceMatrix_GetTestResult_WithSourceSpecificTests_IsCaseInsensitive`,
`TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesPartialFilename`,
`TraceMatrix_GetTestResult_WithPlainTestNames_MatchesAllSources`,
`TraceMatrix_GetTestResult_WithMixedTestNames_MatchesAppropriately`, and
`TraceMatrix_GetTestResult_WithMixedFilterAndPlainReferences_MatchesBoth`.

**Export**: Tests verify Markdown trace matrix export for simple matrices, matrices with failed
tests, matrices with no tests, matrices with not-executed tests, custom heading depth, tag
filtering, child requirements, and tag-filtered counts and unsatisfied requirement retrieval.
This scenario is tested by `TraceMatrix_Export_SimpleTraceMatrix_CreatesMarkdownFile`,
`TraceMatrix_Export_WithFailedTests_ShowsFailures`,
`TraceMatrix_Export_WithNoTests_ShowsNotSatisfied`,
`TraceMatrix_Export_WithNotExecutedTests_ShowsNotExecuted`,
`TraceMatrix_Export_WithCustomDepth_UsesCorrectHeaderLevel`,
`TraceMatrix_Export_WithFilterTags_ExportsOnlyMatchingRequirements`,
`TraceMatrix_Export_WithChildRequirements_ConsidersChildTests`,
`TraceMatrix_CalculateSatisfiedRequirements_WithFilterTags_CountsOnlyMatchingRequirements`, and
`TraceMatrix_GetUnsatisfiedRequirements_WithFilterTags_ReturnsOnlyMatchingRequirements`.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Test-ResultFiles` | Constructor Scenario | `TraceMatrix_Constructor_WithNoFiles_CreatesEmptyMatrix` |
| `ReqStream-Test-ResultFiles` | Constructor Scenario | `TraceMatrix_Constructor_MissingFile_ThrowsFileNotFoundException` |
| `ReqStream-Test-ResultFiles` | Constructor Scenario | `TraceMatrix_Constructor_WithMultipleFiles_AggregatesResults` |
| `ReqStream-Test-ChildRequirements` | Export Scenario | `TraceMatrix_Export_WithChildRequirements_ConsidersChildTests` |
| `ReqStream-Test-TrxFormat` | Constructor Scenario | `TraceMatrix_Constructor_WithTrxFile_ParsesCorrectly` |
| `ReqStream-Test-TrxFormat` | Constructor Scenario | `TraceMatrix_Constructor_WithFailedTests_TracksFailures` |
| `ReqStream-Test-JUnitFormat` | Constructor Scenario | `TraceMatrix_Constructor_WithJUnitFile_ParsesCorrectly` |
| `ReqStream-Test-JUnitFormat` | Constructor Scenario | `TraceMatrix_Constructor_WithJUnitFailedTests_TracksFailures` |
| `ReqStream-Test-MixedFormats` | Constructor Scenario | `TraceMatrix_Constructor_WithMixedFormats_ProcessesBoth` |
| `ReqStream-Test-SourceFiltering` | Test Result Retrieval Scenario | `TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesCorrectly` |
| `ReqStream-Test-SourceFiltering` | Test Result Retrieval Scenario | `TraceMatrix_GetTestResult_WithSourceSpecificTests_DoesNotMatchOtherSources` |
| `ReqStream-Test-SourceFiltering` | Test Result Retrieval Scenario | `TraceMatrix_GetTestResult_WithMultipleSourceSpecifiers_MatchesAllRequirements` |
| `ReqStream-Test-CaseInsensitive` | Test Result Retrieval Scenario | `TraceMatrix_GetTestResult_WithSourceSpecificTests_IsCaseInsensitive` |
| `ReqStream-Test-PartialFilenames` | Test Result Retrieval Scenario | `TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesPartialFilename` |
| `ReqStream-Test-PlainTestNames` | Test Result Retrieval Scenario | `TraceMatrix_GetTestResult_WithPlainTestNames_MatchesAllSources` |
| `ReqStream-Test-MixedTestNames` | Test Result Retrieval Scenario | `TraceMatrix_GetTestResult_WithMixedTestNames_MatchesAppropriately` |
| `ReqStream-Test-MultipleRequirements` | Test Result Retrieval Scenario | `TraceMatrix_GetTestResult_WithMixedFilterAndPlainReferences_MatchesBoth` |
| `ReqStream-Report-TraceMatrix` | Export Scenario | `TraceMatrix_Export_SimpleTraceMatrix_CreatesMarkdownFile` |
| `ReqStream-Report-TraceMatrix` | Export Scenario | `TraceMatrix_Export_WithFailedTests_ShowsFailures` |
| `ReqStream-Report-TraceMatrix` | Export Scenario | `TraceMatrix_Export_WithNoTests_ShowsNotSatisfied` |
| `ReqStream-Report-TraceMatrix` | Export Scenario | `TraceMatrix_Export_WithNotExecutedTests_ShowsNotExecuted` |
| `ReqStream-Report-TraceMatrixDepth` | Export Scenario | `TraceMatrix_Export_WithCustomDepth_UsesCorrectHeaderLevel` |
| `ReqStream-Report-TagFiltering` | Export Scenario | `TraceMatrix_Export_WithFilterTags_ExportsOnlyMatchingRequirements` |
| `ReqStream-Report-TagFiltering` | Export Scenario | `TraceMatrix_CalculateSatisfiedRequirements_WithFilterTags_CountsOnlyMatchingRequirements` |
| `ReqStream-Report-TagFiltering` | Export Scenario | `TraceMatrix_GetUnsatisfiedRequirements_WithFilterTags_ReturnsOnlyMatchingRequirements` |
