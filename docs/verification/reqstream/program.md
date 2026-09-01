## Program

### Verification Approach

The Program unit is verified using xUnit unit tests in `ProgramTests.cs`. Tests call the
`Program.Run` static method directly; console output is captured by redirecting `Console.Out`
to a `StringWriter` before creating the `Context`. Temporary directories and fixture YAML
requirements files are created on disk where processing requires real files.

### Test Environment

The Program unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary directories are created on disk by tests that invoke `Program.Run` with requirements
files or test result files, and are deleted on test completion.

### Acceptance Criteria

The Program unit verification is complete when all xUnit tests in `ProgramTests.cs` pass without
uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage is mapped to at least one passing test method.

### Test Scenarios

**Version Display**: Tests verify that `--version` causes `Run` to print the version string and
return exit code 0. This scenario is tested by `Program_Run_WithVersionFlag_PrintsVersion`,
which asserts `--version` prints the version and returns 0.

**Help Display**: Tests verify that `--help` causes `Run` to print usage information and return
exit code 0. This scenario is tested by `Program_Run_WithHelpFlag_PrintsHelp`, which asserts
`--help` prints usage text.

**Validate**: Tests verify that the `--validate` flag causes `Run` to invoke the self-validation
framework and return exit code 0. This scenario is tested by
`Program_Run_WithValidateFlag_RunsValidation`, which asserts validation runs and exits cleanly,
and `Program_Run_WithValidateAndResults_WritesResultsFile`, which asserts the results file is
written.

**Requirements Processing**: Tests verify that the default execution path loads and processes
requirements files. This scenario is tested by `Program_Run_WithNoRequirementsFiles_ShowsMessage`,
which asserts an informational message when no files are provided;
`Program_Run_WithRequirementsFiles_ProcessesSuccessfully`, which asserts processing succeeds;
`Program_Run_WithRequirementsExport_GeneratesReport`, which asserts a requirements report is
generated; `Program_Run_WithTraceMatrixExport_GeneratesMatrix`, which asserts a trace matrix is
generated; and `Program_Run_WithJustificationsExport_GeneratesJustificationsReport`, which
asserts a justifications report.

**Matrix Without Tests**: Tests verify that requesting `--matrix` without providing test files
produces an error. This scenario is tested by
`Program_Run_WithMatrixButNoTestFiles_ReportsError`, which asserts an error message and non-zero
exit code when `--tests` is omitted, and
`Program_Run_WithMatrixAndUnmatchedTestsPattern_ReportsError`, which asserts an error message and
non-zero exit code when the `--tests` pattern matches no files.

**Enforcement**: Tests verify that enforcement mode exits with a non-zero code when requirements
are unsatisfied. This scenario is tested by
`Program_Run_WithEnforcementAndFullySatisfiedRequirements_Succeeds`, which asserts all satisfied
requirements produce exit code 0; `Program_Run_WithEnforcementAndUnsatisfiedRequirements_Fails`,
which asserts unsatisfied requirements produce a non-zero exit code;
`Program_Run_WithEnforcementAndNoTests_Fails`, which asserts no tests produce a non-zero exit
code; and `Program_Run_WithEnforcementAndFailedTests_Fails`, which asserts failed tests produce a
non-zero exit code.

**Lint**: Tests verify that `--lint` reports issues and exits appropriately. This scenario is
tested by `Program_Run_WithLintFlag_RunsLinter`, which asserts lint runs;
`Program_Run_WithLintFlag_SuppressesBanner`, which asserts the banner is suppressed;
`Program_Run_WithLintFlag_OnlyOutputsIssues`, which asserts only issues are output; and
`Program_Run_WithLintAndNoRequirements_PrintsInformationalMessage`, which asserts an
informational message.

**Orphan Warning**: Tests verify that orphan checking runs whenever the merged root-tag set
(YAML and/or CLI) is non-empty, prints a non-fatal warning listing orphaned requirements when
`--enforce` is not set, produces no warning text when there are no orphans, and is skipped
entirely (no orphan-related text at all) when no root tags are configured anywhere — preserving
full backward compatibility. This scenario is tested by
`Program_Run_WithRootTagsAndOrphans_PrintsWarningWithoutFailing`,
`Program_Run_WithRootTagsNoOrphans_NoWarningPrinted`,
`Program_Run_WithRootTagsDeclaredOnlyInYaml_NoCliFlag_StillChecksOrphans`,
`Program_Run_WithCliRootTagsFlagOnly_NoYamlDeclaration_StillChecksOrphans`, and
`Program_Run_WithNoRootTagsAnywhere_SkipsOrphanCheckEntirely`.

**Orphan Enforcement**: Tests verify that `--enforce` reports orphans as a build-breaking error
independently of test-coverage enforcement — even when no `--tests` were supplied — reports both
orphan and missing-coverage failures together when both apply, still reports the pre-existing
"nothing to enforce" error only when neither check applies, that orphan checking runs against
the full tree regardless of `--filter`, and that orphan-freedom enforcement still runs when
`--matrix` is requested with a `--tests` pattern that matches no files (a combined-guard
regression). This scenario is tested by
`Program_Run_WithEnforcementRootTagsAndOrphansNoTests_FailsEvenWithoutTests`,
`Program_Run_WithEnforcementOrphansAndMissingCoverage_ReportsBothErrorBlocks`,
`Program_Run_WithEnforcementNoTestsNoRootTags_ReportsNothingToEnforceError`,
`Program_Run_WithFilterAndRootTagsOrphans_OrphanCheckIgnoresFilter`, and
`Program_Run_WithMatrixNoMatchAndEnforceRootTagsOrphan_ReportsBothMatrixAndOrphanErrors`.

### Requirements Coverage

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Program-Version` | `Program_Run_WithVersionFlag_PrintsVersion` |
| `ReqStream-Program-Help` | `Program_Run_WithHelpFlag_PrintsHelp` |
| `ReqStream-Program-Validate` | `Program_Run_WithValidateFlag_RunsValidation`, `Program_Run_WithValidateAndResults_WritesResultsFile` |
| `ReqStream-Program-Requirements` | `Program_Run_WithNoRequirementsFiles_ShowsMessage`, `Program_Run_WithRequirementsFiles_ProcessesSuccessfully`, `Program_Run_WithRequirementsExport_GeneratesReport`, `Program_Run_WithTraceMatrixExport_GeneratesMatrix`, `Program_Run_WithJustificationsExport_GeneratesJustificationsReport` |
| `ReqStream-Program-MatrixNoTests` | `Program_Run_WithMatrixButNoTestFiles_ReportsError`, `Program_Run_WithMatrixAndUnmatchedTestsPattern_ReportsError` |
| `ReqStream-Program-Enforce` | `Program_Run_WithEnforcementAndFullySatisfiedRequirements_Succeeds`, `Program_Run_WithEnforcementAndUnsatisfiedRequirements_Fails`, `Program_Run_WithEnforcementAndNoTests_Fails`, `Program_Run_WithEnforcementAndFailedTests_Fails` |
| `ReqStream-Program-Lint` | `Program_Run_WithLintFlag_RunsLinter` |
| `ReqStream-Program-LintNoBanner` | `Program_Run_WithLintFlag_SuppressesBanner` |
| `ReqStream-Program-LintVerbosity` | `Program_Run_WithLintFlag_RunsLinter`, `Program_Run_WithLintFlag_SuppressesBanner` |
| `ReqStream-Program-LintFailure` | `Program_Run_WithLintFlag_OnlyOutputsIssues` |
| `ReqStream-Program-LintNoFiles` | `Program_Run_WithLintAndNoRequirements_PrintsInformationalMessage` |
| `ReqStream-Program-OrphanWarning` | `Program_Run_WithRootTagsAndOrphans_PrintsWarningWithoutFailing`, `Program_Run_WithRootTagsNoOrphans_NoWarningPrinted`, `Program_Run_WithRootTagsDeclaredOnlyInYaml_NoCliFlag_StillChecksOrphans`, `Program_Run_WithCliRootTagsFlagOnly_NoYamlDeclaration_StillChecksOrphans`, `Program_Run_WithNoRootTagsAnywhere_SkipsOrphanCheckEntirely` |
| `ReqStream-Program-OrphanEnforcement` | `Program_Run_WithEnforcementRootTagsAndOrphansNoTests_FailsEvenWithoutTests`, `Program_Run_WithEnforcementOrphansAndMissingCoverage_ReportsBothErrorBlocks`, `Program_Run_WithEnforcementNoTestsNoRootTags_ReportsNothingToEnforceError`, `Program_Run_WithFilterAndRootTagsOrphans_OrphanCheckIgnoresFilter`, `Program_Run_WithMatrixNoMatchAndEnforceRootTagsOrphan_ReportsBothMatrixAndOrphanErrors` |
