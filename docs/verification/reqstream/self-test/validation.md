### Validation Unit Verification

#### Verification Approach

The Validation unit is verified using xUnit unit tests in `ValidationTests.cs`. Tests invoke
`Validation.Run` with `Context` instances configured for specific scenarios and assert on exit
codes, log file content, and result file content.

#### Test Environment

The Validation unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Tests that verify results file output create temporary files on disk, which are deleted on test
completion.

#### Acceptance Criteria

The Validation unit verification is complete when all xUnit tests in `ValidationTests.cs` pass
without uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage is mapped to at least one passing test method.

#### Test Scenarios

##### Self-Validation Scenario

Tests verify that `Validation.Run` completes successfully and produces expected output.

Test methods:

- `Validation_Run_WithNullContext_ThrowsArgumentNullException` — null → ArgumentNullException
- `Validation_Run_WithSilentContext_CompletesSuccessfully` — validation runs and produces summary
- `Validation_Run_WithTrxResultsFile_WritesTrxFile` — TRX file written and contains TestRun
- `Validation_Run_WithXmlResultsFile_WritesXmlFile` — JUnit XML file written and contains testsuite

##### Error and Continuation Scenario

Tests verify error handling when result files cannot be written.

Test methods:

- `Validation_Run_WithUnwritableResultsFile_ReportsError` — write failure → exit code 1
- `Validation_Run_WithUnwritableResultsFile_Continues` — write failure → summary still produced
- `Validation_Run_WithInvalidResultsExtension_ReportsError` — unsupported extension → exit code 1

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Validation-SelfValidation` | Self-Validation Scenario | `Validation_Run_WithSilentContext_CompletesSuccessfully`, `Validation_Run_WithTrxResultsFile_WritesTrxFile`, `Validation_Run_WithXmlResultsFile_WritesXmlFile` |
| `ReqStream-Validation-NullContext` | Self-Validation Scenario | `Validation_Run_WithNullContext_ThrowsArgumentNullException` |
| `ReqStream-Validation-UnsupportedResultsFormat` | Error and Continuation Scenario | `Validation_Run_WithInvalidResultsExtension_ReportsError` |
| `ReqStream-Validation-WriteFailure-ReportsError` | Error and Continuation Scenario | `Validation_Run_WithUnwritableResultsFile_ReportsError` |
| `ReqStream-Validation-WriteFailure-Continues` | Error and Continuation Scenario | `Validation_Run_WithUnwritableResultsFile_Continues` |
