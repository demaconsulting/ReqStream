### LintIssue

#### Verification Approach

The LintIssue unit is verified using xUnit unit tests in `LintIssueTests.cs`. Tests create
`LintIssue` instances with specific severity and content values and assert on the formatted
`ToString()` output.

#### Test Environment

The LintIssue unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
No file system access or external dependencies are used.

#### Acceptance Criteria

The LintIssue unit verification is complete when all xUnit tests in `LintIssueTests.cs` pass
without uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**Issue Formatting**: Tests verify that `LintIssue.ToString()` formats the issue correctly for
both error and warning severities, and handles empty location and description fields correctly.
This scenario is tested by `LintIssue_ToString_ErrorSeverity_FormatsAsError`,
`LintIssue_ToString_WarningSeverity_FormatsAsWarning`,
`LintIssue_ToString_EmptyLocation_FormatsCorrectly`, and
`LintIssue_ToString_EmptyDescription_FormatsCorrectly`.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Lint-IssueType` | Issue Formatting Scenario | `LintIssue_ToString_ErrorSeverity_FormatsAsError`, `LintIssue_ToString_WarningSeverity_FormatsAsWarning` |
| `ReqStream-Lint-SeverityString` | Issue Formatting Scenario | `LintIssue_ToString_ErrorSeverity_FormatsAsError`, `LintIssue_ToString_WarningSeverity_FormatsAsWarning` |
