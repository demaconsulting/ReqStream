## LintIssue Unit Verification

### Verification Strategy

The LintIssue unit is verified using xUnit unit tests in `LintIssueTests.cs`. Tests create
`LintIssue` instances with specific severity and content values and assert on the formatted
`ToString()` output.

### Test Scenarios

#### Issue Formatting Scenario

Tests verify that `LintIssue.ToString()` formats the issue correctly for both error and
warning severities.

Test methods:

- `LintIssue_ToString_ErrorSeverity_FormatsAsError` — error severity formats as "error"
- `LintIssue_ToString_WarningSeverity_FormatsAsWarning` — warning severity formats as "warning"
- `LintIssue_ToString_EmptyLocation_FormatsCorrectly` — empty location still formats correctly
- `LintIssue_ToString_EmptyDescription_FormatsCorrectly` — empty description still formats correctly

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Lint-IssueType` | `LintIssue_ToString_ErrorSeverity_FormatsAsError`, `LintIssue_ToString_WarningSeverity_FormatsAsWarning` |
| `ReqStream-Lint-SeverityString` | `LintIssue_ToString_ErrorSeverity_FormatsAsError`, `LintIssue_ToString_WarningSeverity_FormatsAsWarning` |
