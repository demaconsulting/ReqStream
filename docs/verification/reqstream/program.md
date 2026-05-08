## Program Unit Verification

### Verification Strategy

The Program unit is verified using xUnit unit tests in `ProgramTests.cs`. Tests call the
`Program.Run` static method directly; console output is captured by redirecting `Console.Out`
to a `StringWriter` before creating the `Context`. Temporary directories and fixture YAML
requirements files are created on disk where processing requires real files.

### Test Scenarios

#### Version Display Scenario

Tests verify that `--version` causes `Run` to print the version string and return exit code 0.

Test methods:

- `Program_Run_WithVersionFlag_PrintsVersion` — asserts `--version` prints the version and returns 0

#### Help Display Scenario

Tests verify that `--help` causes `Run` to print usage information and return exit code 0.

Test methods:

- `Program_Run_WithHelpFlag_PrintsHelp` — asserts `--help` prints usage text

#### Validate Scenario

Tests verify that the `--validate` flag causes `Run` to invoke the self-validation framework
and return exit code 0.

Test methods:

- `Program_Run_WithValidateFlag_RunsValidation` — asserts validation runs and exits cleanly
- `Program_Run_WithValidateAndResults_WritesResultsFile` — asserts results file is written

#### Requirements Processing Scenario

Tests verify that the default execution path loads and processes requirements files.

Test methods:

- `Program_Run_WithNoRequirementsFiles_ShowsMessage` — asserts informational message when no files
- `Program_Run_WithRequirementsFiles_ProcessesSuccessfully` — asserts processing succeeds
- `Program_Run_WithRequirementsExport_GeneratesReport` — asserts requirements report is generated
- `Program_Run_WithTraceMatrixExport_GeneratesMatrix` — asserts trace matrix is generated
- `Program_Run_WithJustificationsExport_GeneratesJustificationsReport` — asserts justifications report

#### Enforcement Scenario

Tests verify that enforcement mode exits with a non-zero code when requirements are unsatisfied.

Test methods:

- `Program_Run_WithEnforcementAndFullySatisfiedRequirements_Succeeds` — all satisfied → exit code 0
- `Program_Run_WithEnforcementAndUnsatisfiedRequirements_Fails` — unsatisfied → non-zero exit code
- `Program_Run_WithEnforcementAndNoTests_Fails` — no tests → non-zero exit code
- `Program_Run_WithEnforcementAndFailedTests_Fails` — failed tests → non-zero exit code

#### Lint Scenario

Tests verify that `--lint` reports issues and exits appropriately.

Test methods:

- `Program_Run_WithLintFlag_RunsLinter` — asserts lint runs
- `Program_Run_WithLintFlag_SuppressesBanner` — asserts banner is suppressed
- `Program_Run_WithLintFlag_OnlyOutputsIssues` — asserts only issues are output
- `Program_Run_WithLintAndNoRequirements_PrintsInformationalMessage` — informational message

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Program-Version` | `Program_Run_WithVersionFlag_PrintsVersion` |
| `ReqStream-Program-Help` | `Program_Run_WithHelpFlag_PrintsHelp` |
| `ReqStream-Program-Validate` | `Program_Run_WithValidateFlag_RunsValidation`, `Program_Run_WithValidateAndResults_WritesResultsFile` |
| `ReqStream-Program-Requirements` | `Program_Run_WithNoRequirementsFiles_ShowsMessage`, `Program_Run_WithRequirementsFiles_ProcessesSuccessfully`, `Program_Run_WithRequirementsExport_GeneratesReport`, `Program_Run_WithTraceMatrixExport_GeneratesMatrix`, `Program_Run_WithJustificationsExport_GeneratesJustificationsReport` |
| `ReqStream-Program-Enforce` | `Program_Run_WithEnforcementAndFullySatisfiedRequirements_Succeeds`, `Program_Run_WithEnforcementAndUnsatisfiedRequirements_Fails`, `Program_Run_WithEnforcementAndNoTests_Fails`, `Program_Run_WithEnforcementAndFailedTests_Fails` |
| `ReqStream-Program-Lint` | `Program_Run_WithLintFlag_RunsLinter` |
| `ReqStream-Program-LintVerbosity` | `Program_Run_WithLintFlag_SuppressesBanner`, `Program_Run_WithLintFlag_OnlyOutputsIssues` |
| `ReqStream-Program-LintFailure` | `Program_Run_WithLintFlag_OnlyOutputsIssues` |
| `ReqStream-Program-LintNoFiles` | `Program_Run_WithLintAndNoRequirements_PrintsInformationalMessage` |
