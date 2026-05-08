## Tracing Subsystem Verification

### Verification Strategy

The Tracing subsystem is verified using xUnit integration tests in `TracingTests.cs`. Tests
create temporary YAML requirements files and TRX/JUnit test result files, construct a
`TraceMatrix`, and assert on test result retrieval, coverage determination, error handling,
and Markdown report generation. No dependencies are mocked; isolation is achieved by creating
all required YAML requirement files and test result files in a per-test temporary directory
that is deleted on disposal.

### Test Scenarios

#### Test Results Loading Scenario

Tests verify that TRX and JUnit result files are loaded correctly.

Test methods:

- `Tracing_TestResults_TrxFile_LoadsTestResults` — TRX file loaded and results accessible
- `Tracing_TestResults_JUnitFile_LoadsTestResults` — JUnit file loaded and results accessible

#### Coverage Scenario

Tests verify that requirements are correctly classified as satisfied or unsatisfied.

Test methods:

- `Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied` — passing tests → no unsatisfied
- `Tracing_Coverage_WithMissingTests_RequirementIsUnsatisfied` — missing tests → unsatisfied

#### Error Handling Scenario

Tests verify that missing and malformed files produce appropriate exceptions.

Test methods:

- `Tracing_FileLoading_NonExistentFile_ThrowsFileNotFoundException` — missing file → FileNotFoundException
- `Tracing_FileLoading_MalformedFile_ThrowsInvalidOperationException` — malformed → InvalidOperationException

#### Reporting Scenario

Tests verify that a Markdown trace matrix report is generated.

Test methods:

- `Tracing_Reporting_SimpleMatrix_CreatesMarkdownFile` — Markdown report generated

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Tracing-TestResults` | `Tracing_TestResults_TrxFile_LoadsTestResults`, `Tracing_TestResults_JUnitFile_LoadsTestResults` |
| `ReqStream-Tracing-Mapping` | `Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied` |
| `ReqStream-Tracing-Coverage` | `Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied`, `Tracing_Coverage_WithMissingTests_RequirementIsUnsatisfied` |
| `ReqStream-Tracing-MissingFile` | `Tracing_FileLoading_NonExistentFile_ThrowsFileNotFoundException` |
| `ReqStream-Tracing-MalformedFile` | `Tracing_FileLoading_MalformedFile_ThrowsInvalidOperationException` |
| `ReqStream-Tracing-Reporting` | `Tracing_Reporting_SimpleMatrix_CreatesMarkdownFile` |
