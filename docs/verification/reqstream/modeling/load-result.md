## LoadResult Unit Verification

### Verification Strategy

The LoadResult unit is verified using xUnit integration tests in `LoadResultTests.cs`. Tests
create YAML requirements files and `LoadResult` instances with specific issue lists, then invoke
`ReportIssues` through a `Context` instance and assert on the exit code, log file content,
`HasErrors` property, and `Requirements` reference.

### Test Scenarios

#### Issue Routing Scenario

Tests verify that error-level issues route to the error channel and warning-level issues
do not set the exit code.

Test methods:

- `LoadResult_ReportIssues_ErrorIssue_SetsContextError` — error issue → exit code 1
- `LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError` — warning issue → exit code 0
- `LoadResult_ReportIssues_NoIssues_ProducesNoOutput` — no issues → no output

#### HasErrors Scenario

Tests verify the `HasErrors` property behavior.

Test methods:

- `LoadResult_HasErrors_WithOnlyWarnings_ReturnsFalse` — warnings only → HasErrors false
- `LoadResult_HasErrors_WithErrorIssue_ReturnsTrue` — error issue → HasErrors true

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Requirements-UnifiedLoad` | `LoadResult_ReportIssues_ErrorIssue_SetsContextError`, `LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError`, `LoadResult_ReportIssues_NoIssues_ProducesNoOutput` |
| `ReqStream-LoadResult-ReportIssues` | `LoadResult_ReportIssues_ErrorIssue_SetsContextError`, `LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError` |
| `ReqStream-LoadResult-HasErrors` | `LoadResult_HasErrors_WithErrorIssue_ReturnsTrue`, `LoadResult_HasErrors_WithOnlyWarnings_ReturnsFalse` |
