## SelfTest Subsystem Verification

### Verification Strategy

The SelfTest subsystem is verified using xUnit integration tests in `SelfTestTests.cs`. Tests
invoke `Validation.Run` through a silent `Context` and assert on the exit code, results file
content, and error reporting behavior.

### Test Scenarios

#### Qualification Scenario

Tests verify that the self-validation suite completes successfully.

Test methods:

- `SelfTest_Qualification_Run_PassesAllTests` — validation passes with exit code 0

#### Results Output Scenario

Tests verify that TRX and JUnit XML result files are written.

Test methods:

- `SelfTest_ResultsOutput_TrxResultsPath_WritesTrxFile` — TRX file written
- `SelfTest_ResultsOutput_XmlResultsPath_WritesJUnitFile` — JUnit XML file written

#### Failure Reporting Scenario

Tests verify that errors are reported and exit code is 1 on failures.

Test methods:

- `SelfTest_FailureReporting_WithErrors_SetsExitCode1` — errors → exit code 1

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-SelfTest-Qualification` | `SelfTest_Qualification_Run_PassesAllTests` |
| `ReqStream-SelfTest-ResultsOutput` | `SelfTest_ResultsOutput_TrxResultsPath_WritesTrxFile`, `SelfTest_ResultsOutput_XmlResultsPath_WritesJUnitFile` |
| `ReqStream-SelfTest-FailureReporting` | `SelfTest_FailureReporting_WithErrors_SetsExitCode1` |
