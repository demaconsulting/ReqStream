### LoadResult Unit Verification

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
requirement in the Coverage Summary is mapped to at least one passing test method.

#### Test Scenarios

##### Unified Load Scenario

Tests verify the full `Requirements.Load` pipeline, covering successful loading, error
propagation, missing files, malformed YAML, multiple simultaneous errors, include-file
linting, and location reporting in issues. These tests exercise the `ReqStream-LoadResult-UnifiedLoad`
requirement by driving `Requirements.Load` through `LoadResult` and asserting on the returned
instance.

Test methods:

- `Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues` — valid file returns non-null requirements and empty issues
- `Requirements_Load_WithLintError_ReturnsNullAndIssues` — lint error makes requirements null and issues non-empty
- `Requirements_Load_MissingFile_ReturnsNullAndIssues` — missing file returns null requirements with an error issue
- `Requirements_Load_MalformedYaml_ReturnsNullAndIssues` — malformed YAML returns null requirements with an error issue
- `Requirements_Load_WithMultipleLintErrors_ReportsAllIssues` — multiple lint errors are all collected
- `Requirements_Load_WithIncludes_LintsIncludedFiles` — lint validation covers included files
- `Requirements_Load_WithLintError_IssueIncludesLocation` — issue location string identifies the source file

##### Issue Routing Scenario

Tests verify that error-level issues route to the error channel and warning-level issues
do not set the exit code.

Test methods:

- `LoadResult_ReportIssues_ErrorIssue_SetsContextError` — error issue → exit code 1
- `LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError` — warning issue → exit code 0
- `LoadResult_ReportIssues_NoIssues_ProducesNoOutput` — no issues → no output

##### HasErrors Scenario

Tests verify the `HasErrors` property behavior.

Test methods:

- `LoadResult_HasErrors_WithOnlyWarnings_ReturnsFalse` — warnings only → HasErrors false
- `LoadResult_HasErrors_WithErrorIssue_ReturnsTrue` — error issue → HasErrors true

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-LoadResult-UnifiedLoad` | Unified Load Scenario | `Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues`, `Requirements_Load_WithLintError_ReturnsNullAndIssues`, `Requirements_Load_MissingFile_ReturnsNullAndIssues`, `Requirements_Load_MalformedYaml_ReturnsNullAndIssues`, `Requirements_Load_WithMultipleLintErrors_ReportsAllIssues`, `Requirements_Load_WithIncludes_LintsIncludedFiles`, `Requirements_Load_WithLintError_IssueIncludesLocation` |
| `ReqStream-LoadResult-ReportIssues` | Issue Routing Scenario | `LoadResult_ReportIssues_ErrorIssue_SetsContextError`, `LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError`, `LoadResult_ReportIssues_NoIssues_ProducesNoOutput` |
| `ReqStream-LoadResult-HasErrors` | HasErrors Scenario | `LoadResult_HasErrors_WithErrorIssue_ReturnsTrue`, `LoadResult_HasErrors_WithOnlyWarnings_ReturnsFalse`, `LoadResult_HasErrors_NoIssues_ReturnsFalse` |
