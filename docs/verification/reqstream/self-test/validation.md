### Validation

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
requirement in the Requirements Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**Self-Validation**: Tests verify that `Validation.Run` completes successfully and produces
expected output, including TRX and JUnit XML results file writing. This scenario is tested by
`Validation_Run_WithSilentContext_CompletesSuccessfully`,
`Validation_Run_WithTrxResultsFile_WritesTrxFile`, and
`Validation_Run_WithXmlResultsFile_WritesXmlFile`.

**Error and Continuation**: Tests verify error handling, including guard conditions that throw for
invalid inputs, error handling when result files cannot be written, and when an unsupported results
file extension is provided, and confirm that processing continues even after a write failure. This
scenario is tested by `Validation_Run_WithNullContext_ThrowsArgumentNullException`,
`Validation_Run_WithUnwritableResultsFile_ReportsError`,
`Validation_Run_WithUnwritableResultsFile_Continues`, and
`Validation_Run_WithInvalidResultsExtension_ReportsError`.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Validation-SelfValidation` | Self-Validation Scenario | `Validation_Run_WithSilentContext_CompletesSuccessfully` |
| `ReqStream-Validation-SelfValidation` | Self-Validation Scenario | `Validation_Run_WithTrxResultsFile_WritesTrxFile` |
| `ReqStream-Validation-SelfValidation` | Self-Validation Scenario | `Validation_Run_WithXmlResultsFile_WritesXmlFile` |
| `ReqStream-Validation-NullContext` | Error and Continuation Scenario | `Validation_Run_WithNullContext_ThrowsArgumentNullException` |
| `ReqStream-Validation-UnsupportedResultsFormat` | Error and Continuation Scenario | `Validation_Run_WithInvalidResultsExtension_ReportsError` |
| `ReqStream-Validation-WriteFailure-ReportsError` | Error and Continuation Scenario | `Validation_Run_WithUnwritableResultsFile_ReportsError` |
| `ReqStream-Validation-WriteFailure-Continues` | Error and Continuation Scenario | `Validation_Run_WithUnwritableResultsFile_Continues` |
