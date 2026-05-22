### LoadResult

#### Verification Approach

The LoadResult unit is verified using xUnit integration tests in `LoadResultTests.cs`. Tests
create YAML requirements files and `LoadResult` instances with specific issue lists, then invoke
`ReportIssues` through a `Context` instance and assert on the exit code, log file content,
`HasErrors` property, and `Requirements` reference.

#### Test Environment

The LoadResult unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary YAML requirements files are created by tests that need a loaded `Requirements` instance,
and are deleted on test completion.

#### Acceptance Criteria

The LoadResult unit verification is complete when all xUnit tests in `LoadResultTests.cs` pass
without uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**Unified Load**: Tests verify the full `Requirements.Load` pipeline, covering successful loading,
error propagation, missing files, malformed YAML, multiple simultaneous errors, include-file
linting, and location reporting in issues. This scenario is tested by
`Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues`,
`Requirements_Load_WithLintError_ReturnsNullAndIssues`,
`Requirements_Load_MissingFile_ReturnsNullAndIssues`,
`Requirements_Load_MalformedYaml_ReturnsNullAndIssues`,
`Requirements_Load_WithMultipleLintErrors_ReportsAllIssues`,
`Requirements_Load_WithIncludes_LintsIncludedFiles`, and
`Requirements_Load_WithLintError_IssueIncludesLocation`.

**Issue Routing**: Tests verify that error-level issues route to the error channel and set the
exit code to 1, while warning-level issues do not. This scenario is tested by
`LoadResult_ReportIssues_ErrorIssue_SetsContextError`,
`LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError`, and
`LoadResult_ReportIssues_NoIssues_ProducesNoOutput`.

**HasErrors**: Tests verify the `HasErrors` property behavior for issues with errors, warnings
only, and no issues. This scenario is tested by
`LoadResult_HasErrors_WithOnlyWarnings_ReturnsFalse`,
`LoadResult_HasErrors_WithErrorIssue_ReturnsTrue`, and
`LoadResult_HasErrors_NoIssues_ReturnsFalse`.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-LoadResult-UnifiedLoad` | Unified Load Scenario | `Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues` |
| `ReqStream-LoadResult-UnifiedLoad` | Unified Load Scenario | `Requirements_Load_WithLintError_ReturnsNullAndIssues` |
| `ReqStream-LoadResult-UnifiedLoad` | Unified Load Scenario | `Requirements_Load_MissingFile_ReturnsNullAndIssues` |
| `ReqStream-LoadResult-UnifiedLoad` | Unified Load Scenario | `Requirements_Load_MalformedYaml_ReturnsNullAndIssues` |
| `ReqStream-LoadResult-UnifiedLoad` | Unified Load Scenario | `Requirements_Load_WithMultipleLintErrors_ReportsAllIssues` |
| `ReqStream-LoadResult-UnifiedLoad` | Unified Load Scenario | `Requirements_Load_WithIncludes_LintsIncludedFiles` |
| `ReqStream-LoadResult-UnifiedLoad` | Unified Load Scenario | `Requirements_Load_WithLintError_IssueIncludesLocation` |
| `ReqStream-LoadResult-ReportIssues` | Issue Routing Scenario | `LoadResult_ReportIssues_ErrorIssue_SetsContextError` |
| `ReqStream-LoadResult-ReportIssues` | Issue Routing Scenario | `LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError` |
| `ReqStream-LoadResult-ReportIssues` | Issue Routing Scenario | `LoadResult_ReportIssues_NoIssues_ProducesNoOutput` |
| `ReqStream-LoadResult-HasErrors` | HasErrors Scenario | `LoadResult_HasErrors_WithErrorIssue_ReturnsTrue` |
| `ReqStream-LoadResult-HasErrors` | HasErrors Scenario | `LoadResult_HasErrors_WithOnlyWarnings_ReturnsFalse` |
| `ReqStream-LoadResult-HasErrors` | HasErrors Scenario | `LoadResult_HasErrors_NoIssues_ReturnsFalse` |
