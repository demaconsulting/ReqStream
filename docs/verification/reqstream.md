# ReqStream

## Verification Strategy

The ReqStream system is verified at the system level using integration tests that invoke the
published `dotnet` tool end-to-end. Tests are written using xUnit in `IntegrationTests.cs` and
exercise the complete tool — from command-line argument parsing through report generation and
enforcement — in a temporary directory. No mocking or stubbing is used at the system level;
tests exercise the actual binary on the actual file system.

## Test Environment

System integration tests run in the CI/CD pipeline on all three supported platforms (Windows,
Linux, and macOS) under all three supported .NET runtimes (.NET 8, .NET 9, .NET 10). Each test
creates a temporary working directory, writes fixture YAML requirements files and TRX/JUnit test
result files, invokes the tool, and asserts on the exit code, console output, and generated
report files as appropriate.

## Acceptance Criteria

A system-level test scenario passes when the xUnit test method completes without an uncaught
exception and all assertions succeed. For scenarios that exercise success paths, the tool must
exit with code 0; for scenarios that exercise failure paths, the tool must exit with a non-zero
code. Console output and generated file content must match expected patterns where asserted.

The system verification as a whole is complete when every scenario in this chapter passes on all
three supported platforms (Windows, Linux, macOS) and all three supported .NET runtimes (.NET 8,
.NET 9, .NET 10), and the Requirements Coverage table shows every system requirement mapped to at
least one passing test method.

## Test Scenarios

**Version Display**: Verifies that the tool prints version information and exits with code 0 when
`--version` is passed. The test captures stdout and asserts it contains a non-empty version
string. This scenario is tested by
`ReqStream_System_CliInterface_VersionFlag_PrintsVersion`.

**Help Display**: Verifies that the tool prints usage information and exits with code 0 when
`--help` is passed. The test captures stdout and asserts it contains expected option
descriptions. This scenario is tested by `ReqStream_System_CliInterface_HelpFlag_PrintsHelp`.

**Full Pipeline**: Verifies that the tool executes the full requirements-processing pipeline in a
single invocation, including loading YAML, tracing test results, and generating all reports. This
scenario is tested by
`ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces`.

**Source Filter**: Verifies that source-specific test matching restricts coverage evidence to
tests from named result files. This scenario is tested by
`ReqStream_SourceFilter_NamedSourceInRequirement_MatchesTestsBySourceFile`.

**Enforcement Mode**: Verifies that the tool exits with a non-zero code when enforcement is
active and a requirement lacks passing test evidence. This scenario is tested by
`ReqStream_EnforcementMode_RequirementLacksTestEvidence_FailsWithNonZeroExitCode`.

**Orphan Checking**: Verifies that the tool identifies requirements not reachable from any
root-tagged requirement, warning about them by default and reporting them as a build-breaking
error when `--enforce` is active, independent of whether test result files are provided. This
scenario is tested by `ReqStream_System_OrphanChecking_RootTagsWithOrphan_EnforceReportsError`.

**Lint**: Verifies that the tool identifies and reports all structural issues in a single linting
invocation and exits silently when no issues are found. This scenario is tested by
`ReqStream_System_Lint_Flag_ReportsLintIssues` and
`ReqStream_System_Lint_ValidRequirementsFile_ExitsSilentlyWithZero`.

**Validate**: Verifies that the tool runs a built-in self-test suite when `--validate` is passed.
This scenario is tested by `ReqStream_System_Validate_Flag_RunsSelfValidation`.

**Validate Results Output**: Verifies that the tool writes self-validation test results to a file
when `--results` is passed. This scenario is tested by
`ReqStream_System_ValidateResultsOutput_ResultsFlag_WritesResultsFile`.

**Requirements Report**: Verifies that the tool exports a requirements Markdown report when the
`--report` flag is provided. This scenario is tested by
`ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces`.

**Trace Matrix**: Verifies that the tool exports a trace matrix Markdown report when the
`--matrix` flag is provided. This scenario is tested by
`ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces`.

**Justifications**: Verifies that the tool exports requirement justifications when the
`--justifications` flag is provided. This scenario is tested by
`ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces`.

**Tag Filter**: Verifies that the tool filters requirements output by tags when the `--filter`
flag is provided. This scenario is tested by
`ReqStream_System_TagFilter_Flag_FiltersRequirements`.

**Log File Output**: Verifies that the tool routes all output to the specified log file when
`--log` is provided. This scenario is tested by
`ReqStream_System_OutputControl_LogFlag_WritesOutputToFile` and
`ReqStream_System_OutputControl_LogFlag_WithoutSilent_WritesOutputToFileAndConsole`.

**Silent Mode**: Verifies that the tool suppresses console output when `--silent` is provided.
This scenario is tested by
`ReqStream_System_OutputControl_SilentFlag_SuppressesConsoleOutput`.

**Report Depth**: Verifies that the tool supports configurable report heading depth. This
scenario is tested by
`ReqStream_System_ReportDepth_DepthFlag_GeneratesReportWithCorrectHeadingLevel`.

**File Includes**: Verifies that the tool loads requirements from multiple YAML files via file
includes. This scenario is tested by
`ReqStream_System_FileIncludes_RequirementsWithIncludes_LoadsAllRequirements`.

**Section Merging**: Verifies that sections with the same title in different included files are
automatically merged into a single section in the output. This scenario is tested by
`ReqStream_System_SectionMerging_TwoFilesWithSameSection_ProducesSingleMergedSection`.

**Circular Include Detection**: Verifies that the tool detects circular include references (where
file A includes file B and file B includes file A) and reports an error, exiting with a non-zero
code. This scenario is tested by
`ReqStream_System_CircularIncludeDetection_CircularInclude_ReportsError`.

**Test File Error Handling**: Verifies that the tool reports a fatal error and exits with a
non-zero code when a test result file path is specified but the file is missing, or when the file
cannot be parsed. This scenario is tested by
`ReqStream_System_TestFileErrorHandling_MissingTestFile_ReportsFatalError` and
`ReqStream_System_TestFileErrorHandling_MalformedTestFile_ReportsFatalError`.

**Matrix Error Handling**: Verifies that the tool reports an error and exits with a non-zero code
when `--matrix` is requested but no `--tests` files are provided. This scenario is tested by
`ReqStream_System_MatrixErrorHandling_MatrixWithoutTests_ReportsError`.

**Enforce No Tests**: Verifies that the tool reports an error and exits with a non-zero code when
`--enforce` is requested but no `--tests` files are provided, preventing silent no-op enforcement
runs. This scenario is tested by
`ReqStream_System_EnforceNoTests_EnforceWithoutTests_ReportsError`.

**Cyclic Child Detection**: Verifies that the tool detects cyclic references in the
child-requirement graph (where requirement A lists B as a child and B lists A as a child) and
reports an error. This is distinct from circular include detection (which detects cycles in
`includes` file references). This scenario is tested by
`ReqStream_System_CyclicChildDetection_CyclicChildRequirements_ReportsError`.

**Windows Platform**: Platform requirements are verified by running the self-validation tests on
Windows in the CI pipeline. This scenario is tested by `windows@ReqStream_VersionDisplay` and
`windows@ReqStream_HelpDisplay`.

**Linux Platform**: Platform requirements are verified by running the self-validation tests on
Linux (Ubuntu) in the CI pipeline. This scenario is tested by `ubuntu@ReqStream_VersionDisplay`
and `ubuntu@ReqStream_HelpDisplay`.

**macOS Platform**: Platform requirements are verified by running the self-validation tests on
macOS in the CI pipeline. This scenario is tested by `macos@ReqStream_VersionDisplay` and
`macos@ReqStream_HelpDisplay`.

**.NET 8 Runtime**: Runtime requirements are verified by running the self-validation tests under
.NET 8 in the CI pipeline. This scenario is tested by `dotnet8.x@ReqStream_VersionDisplay` and
`dotnet8.x@ReqStream_HelpDisplay`.

**.NET 9 Runtime**: Runtime requirements are verified by running the self-validation tests under
.NET 9 in the CI pipeline. This scenario is tested by `dotnet9.x@ReqStream_VersionDisplay` and
`dotnet9.x@ReqStream_HelpDisplay`.

**.NET 10 Runtime**: Runtime requirements are verified by running the self-validation tests under
.NET 10 in the CI pipeline. This scenario is tested by `dotnet10.x@ReqStream_VersionDisplay` and
`dotnet10.x@ReqStream_HelpDisplay`.

## Requirements Coverage

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-System-VersionDisplay` | `ReqStream_System_CliInterface_VersionFlag_PrintsVersion` |
| `ReqStream-System-HelpDisplay` | `ReqStream_System_CliInterface_HelpFlag_PrintsHelp` |
| `ReqStream-System-FullPipeline` | `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces` |
| `ReqStream-System-SourceFilter` | `ReqStream_SourceFilter_NamedSourceInRequirement_MatchesTestsBySourceFile` |
| `ReqStream-System-EnforceMode` | `ReqStream_EnforcementMode_RequirementLacksTestEvidence_FailsWithNonZeroExitCode` |
| `ReqStream-System-OrphanChecking` | `ReqStream_System_OrphanChecking_RootTagsWithOrphan_EnforceReportsError` |
| `ReqStream-System-Lint` | `ReqStream_System_Lint_Flag_ReportsLintIssues`, `ReqStream_System_Lint_ValidRequirementsFile_ExitsSilentlyWithZero` |
| `ReqStream-System-Validate` | `ReqStream_System_Validate_Flag_RunsSelfValidation` |
| `ReqStream-System-ValidateResultsOutput` | `ReqStream_System_ValidateResultsOutput_ResultsFlag_WritesResultsFile` |
| `ReqStream-System-RequirementsReport` | `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces` |
| `ReqStream-System-TraceMatrix` | `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces` |
| `ReqStream-System-Justifications` | `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces` |
| `ReqStream-System-TagFilter` | `ReqStream_System_TagFilter_Flag_FiltersRequirements` |
| `ReqStream-System-LogFileOutput` | `ReqStream_System_OutputControl_LogFlag_WritesOutputToFile`, `ReqStream_System_OutputControl_LogFlag_WithoutSilent_WritesOutputToFileAndConsole` |
| `ReqStream-System-SilentMode` | `ReqStream_System_OutputControl_SilentFlag_SuppressesConsoleOutput` |
| `ReqStream-System-ReportDepth` | `ReqStream_System_ReportDepth_DepthFlag_GeneratesReportWithCorrectHeadingLevel` |
| `ReqStream-System-CrossPlatform` | Satisfied by children: `ReqStream-Platform-Windows`, `ReqStream-Platform-Linux`, `ReqStream-Platform-MacOS`, `ReqStream-Platform-Net8`, `ReqStream-Platform-Net9`, `ReqStream-Platform-Net10` |
| `ReqStream-System-FileIncludes` | `ReqStream_System_FileIncludes_RequirementsWithIncludes_LoadsAllRequirements` |
| `ReqStream-System-SectionMerging` | `ReqStream_System_SectionMerging_TwoFilesWithSameSection_ProducesSingleMergedSection` |
| `ReqStream-System-CircularIncludeDetection` | `ReqStream_System_CircularIncludeDetection_CircularInclude_ReportsError` |
| `ReqStream-System-TestFileErrorHandling` | `ReqStream_System_TestFileErrorHandling_MissingTestFile_ReportsFatalError`, `ReqStream_System_TestFileErrorHandling_MalformedTestFile_ReportsFatalError` |
| `ReqStream-System-MatrixErrorHandling` | `ReqStream_System_MatrixErrorHandling_MatrixWithoutTests_ReportsError` |
| `ReqStream-System-EnforceNoTests` | `ReqStream_System_EnforceNoTests_EnforceWithoutTests_ReportsError` |
| `ReqStream-System-CyclicChildDetection` | `ReqStream_System_CyclicChildDetection_CyclicChildRequirements_ReportsError` |
| `ReqStream-Platform-Windows` | `windows@ReqStream_VersionDisplay`, `windows@ReqStream_HelpDisplay` |
| `ReqStream-Platform-Linux` | `ubuntu@ReqStream_VersionDisplay`, `ubuntu@ReqStream_HelpDisplay` |
| `ReqStream-Platform-MacOS` | `macos@ReqStream_VersionDisplay`, `macos@ReqStream_HelpDisplay` |
| `ReqStream-Platform-Net8` | `dotnet8.x@ReqStream_VersionDisplay`, `dotnet8.x@ReqStream_HelpDisplay` |
| `ReqStream-Platform-Net9` | `dotnet9.x@ReqStream_VersionDisplay`, `dotnet9.x@ReqStream_HelpDisplay` |
| `ReqStream-Platform-Net10` | `dotnet10.x@ReqStream_VersionDisplay`, `dotnet10.x@ReqStream_HelpDisplay` |
