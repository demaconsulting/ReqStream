## Tracing Subsystem Verification

### Verification Strategy

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

### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Tracing-TestResults` | Test Results Loading Scenario | `Tracing_TestResults_TrxFile_LoadsTestResults`, `Tracing_TestResults_JUnitFile_LoadsTestResults` |
| `ReqStream-Tracing-Mapping` | Coverage Scenario | `Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied` |
| `ReqStream-Tracing-Coverage` | Coverage Scenario | `Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied`, `Tracing_Coverage_WithMissingTests_RequirementIsUnsatisfied` |
| `ReqStream-Tracing-MissingFile` | Error Handling Scenario | `Tracing_FileLoading_NonExistentFile_ThrowsFileNotFoundException` |
| `ReqStream-Tracing-MalformedFile` | Error Handling Scenario | `Tracing_FileLoading_MalformedFile_ThrowsInvalidOperationException` |
| `ReqStream-Tracing-Reporting` | Reporting Scenario | `Tracing_Reporting_SimpleMatrix_CreatesMarkdownFile` |
