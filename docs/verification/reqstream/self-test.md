## SelfTest

### Verification Approach

The SelfTest subsystem is verified using xUnit integration tests in `SelfTestTests.cs`. Tests
invoke `Validation.Run` through a silent `Context` and assert on the exit code, results file
content, and error reporting behavior.

### Test Environment

The SelfTest subsystem tests require no setup beyond the standard xUnit test runner and .NET
runtime. Tests that verify results file output create temporary files on disk, which are deleted
on test completion.

### Acceptance Criteria

The SelfTest subsystem verification is complete when all xUnit tests in `SelfTestTests.cs` pass
without uncaught exceptions and all assertions succeed. The subsystem is considered verified when
every requirement in the Requirements Coverage table is mapped to at least one passing test method.

### Test Scenarios

**Qualification**: Tests verify that the self-validation suite completes successfully. This
scenario is tested by `SelfTest_Qualification_Run_PassesAllTests`, which verifies validation
passes with exit code 0.

**Results Output**: Tests verify that TRX and JUnit XML result files are written. This scenario
is tested by `SelfTest_ResultsOutput_TrxResultsPath_WritesTrxFile`, which verifies a TRX file is
written, and `SelfTest_ResultsOutput_XmlResultsPath_WritesJUnitFile`, which verifies a JUnit XML
file is written.

**Failure Reporting**: Tests verify that errors are reported and exit code is 1 on failures. This
scenario is tested by `SelfTest_FailureReporting_WithErrors_SetsExitCode1`, which verifies errors
produce exit code 1.

### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-SelfTest-Qualification` | Qualification Scenario | `SelfTest_Qualification_Run_PassesAllTests` |
| `ReqStream-SelfTest-ResultsOutput` | Results Output Scenario | `SelfTest_ResultsOutput_TrxResultsPath_WritesTrxFile`, `SelfTest_ResultsOutput_XmlResultsPath_WritesJUnitFile` |
| `ReqStream-SelfTest-FailureReporting` | Failure Reporting Scenario | `SelfTest_FailureReporting_WithErrors_SetsExitCode1` |
