## Tracing

### Verification Approach

The Tracing subsystem is verified using xUnit integration tests in `TracingTests.cs`. Tests
create temporary YAML requirements files and TRX/JUnit test result files, construct a
`TraceMatrix`, and assert on test result retrieval, coverage determination, error handling,
and Markdown report generation. No dependencies are mocked; isolation is achieved by creating
all required YAML requirement files and test result files in a per-test temporary directory
that is deleted on disposal.

### Test Environment

The Tracing subsystem tests require no setup beyond the standard xUnit test runner and .NET
runtime. Each test creates temporary YAML requirements files and TRX or JUnit XML test result
files in a per-test temporary directory that is deleted on disposal.

### Acceptance Criteria

The Tracing subsystem verification is complete when all xUnit tests in `TracingTests.cs` pass
without uncaught exceptions and all assertions succeed. The subsystem is considered verified when
every requirement in the Requirements Coverage table is mapped to at least one passing test method.

### Test Scenarios

**Test Results Loading**: Tests verify that TRX and JUnit result files are loaded correctly. This
scenario is tested by `Tracing_TestResults_TrxFile_LoadsTestResults`, which verifies a TRX file
is loaded and results are accessible, and `Tracing_TestResults_JUnitFile_LoadsTestResults`,
which verifies a JUnit file is loaded and results are accessible.

**Coverage**: Tests verify that requirements are correctly classified as satisfied or unsatisfied.
This scenario is tested by `Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied`, which
verifies passing tests produce no unsatisfied requirements, and
`Tracing_Coverage_WithMissingTests_RequirementIsUnsatisfied`, which verifies missing tests produce
an unsatisfied requirement.

**Error Handling**: Tests verify that missing and malformed files produce appropriate exceptions.
This scenario is tested by `Tracing_FileLoading_NonExistentFile_ThrowsFileNotFoundException`,
which verifies a missing file produces `FileNotFoundException`, and
`Tracing_FileLoading_MalformedFile_ThrowsInvalidOperationException`, which verifies a malformed
file produces `InvalidOperationException`.

**Reporting**: Tests verify that a Markdown trace matrix report is generated. This scenario is
tested by `Tracing_Reporting_SimpleMatrix_CreatesMarkdownFile`, which verifies a Markdown report
is generated.

### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Tracing-TestResults` | Test Results Loading Scenario | `Tracing_TestResults_TrxFile_LoadsTestResults`, `Tracing_TestResults_JUnitFile_LoadsTestResults` |
| `ReqStream-Tracing-Mapping` | Coverage Scenario | `Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied` |
| `ReqStream-Tracing-Coverage` | Coverage Scenario | `Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied`, `Tracing_Coverage_WithMissingTests_RequirementIsUnsatisfied` |
| `ReqStream-Tracing-MissingFile` | Error Handling Scenario | `Tracing_FileLoading_NonExistentFile_ThrowsFileNotFoundException` |
| `ReqStream-Tracing-MalformedFile` | Error Handling Scenario | `Tracing_FileLoading_MalformedFile_ThrowsInvalidOperationException` |
| `ReqStream-Tracing-Reporting` | Reporting Scenario | `Tracing_Reporting_SimpleMatrix_CreatesMarkdownFile` |
