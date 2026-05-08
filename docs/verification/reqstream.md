# ReqStream System Verification

## System Verification Strategy

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

## System Test Scenarios

### Version Display Scenario

Verifies that the tool prints version information and exits with code 0 when `--version` is
passed. The test captures stdout and asserts it contains a non-empty version string.

Test method: `ReqStream_System_CliInterface_VersionFlag_PrintsVersion`

### Help Display Scenario

Verifies that the tool prints usage information and exits with code 0 when `--help` is passed.
The test captures stdout and asserts it contains expected option descriptions.

Test method: `ReqStream_System_CliInterface_HelpFlag_PrintsHelp`

### Full Pipeline Scenario

Verifies that the tool executes the full requirements-processing pipeline in a single invocation,
including loading YAML, tracing test results, and generating all reports.

Test method: `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces`

### Source Filter Scenario

Verifies that source-specific test matching restricts coverage evidence to tests from named
result files.

Test method: `ReqStream_SourceFilter_NamedSourceInRequirement_MatchesTestsBySourceFile`

### Enforcement Mode Scenario

Verifies that the tool exits with a non-zero code when enforcement is active and a requirement
lacks passing test evidence.

Test method: `ReqStream_EnforcementMode_RequirementLacksTestEvidence_FailsWithNonZeroExitCode`

### Lint Scenario

Verifies that the tool identifies and reports all structural issues in a single linting invocation
and exits silently when no issues are found.

Test methods:

- `ReqStream_System_Lint_Flag_ReportsLintIssues`
- `ReqStream_System_Lint_ValidRequirementsFile_ExitsSilentlyWithZero`

### Validate Scenario

Verifies that the tool runs a built-in self-test suite when `--validate` is passed.

Test method: `ReqStream_System_Validate_Flag_RunsSelfValidation`

### Validate Results Output Scenario

Verifies that the tool writes self-validation test results to a file when `--results` is passed.

Test method: `ReqStream_System_ValidateResultsOutput_ResultsFlag_WritesResultsFile`

### Requirements Report Scenario

Verifies that the tool exports a requirements Markdown report when the `--report` flag is
provided.

Test method: `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces`

### Trace Matrix Scenario

Verifies that the tool exports a trace matrix Markdown report when the `--matrix` flag is
provided.

Test method: `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces`

### Justifications Scenario

Verifies that the tool exports requirement justifications when the `--justifications` flag is
provided.

Test method: `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces`

### Tag Filter Scenario

Verifies that the tool filters requirements output by tags when the `--filter` flag is provided.

Test method: `ReqStream_System_TagFilter_Flag_FiltersRequirements`

### Output Routing Scenario

Verifies that the tool supports log file output and console output suppression.

Test methods:

- `ReqStream_System_OutputControl_LogFlag_WritesOutputToFile`
- `ReqStream_System_OutputControl_SilentFlag_SuppressesConsoleOutput`

### Report Depth Scenario

Verifies that the tool supports configurable report heading depth.

Test method: `ReqStream_System_ReportDepth_DepthFlag_GeneratesReportWithCorrectHeadingLevel`

### File Includes Scenario

Verifies that the tool loads requirements from multiple YAML files via file includes.

Test method: `ReqStream_System_FileIncludes_RequirementsWithIncludes_LoadsAllRequirements`

## Platform Test Scenarios

Platform requirements are verified by running the self-validation tests on each platform and
runtime. The CI pipeline runs the tool on Windows, Linux (Ubuntu), and macOS, and under
.NET 8, .NET 9, and .NET 10.

### Windows Platform Scenario

Test methods: `windows@ReqStream_VersionDisplay`, `windows@ReqStream_HelpDisplay`

### Linux Platform Scenario

Test methods: `ubuntu@ReqStream_VersionDisplay`, `ubuntu@ReqStream_HelpDisplay`

### macOS Platform Scenario

Test methods: `macos@ReqStream_VersionDisplay`, `macos@ReqStream_HelpDisplay`

### .NET 8 Runtime Scenario

Test methods: `dotnet8.x@ReqStream_VersionDisplay`, `dotnet8.x@ReqStream_HelpDisplay`

### .NET 9 Runtime Scenario

Test methods: `dotnet9.x@ReqStream_VersionDisplay`, `dotnet9.x@ReqStream_HelpDisplay`

### .NET 10 Runtime Scenario

Test methods: `dotnet10.x@ReqStream_VersionDisplay`, `dotnet10.x@ReqStream_HelpDisplay`

## Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-System-VersionDisplay` | `ReqStream_System_CliInterface_VersionFlag_PrintsVersion` |
| `ReqStream-System-HelpDisplay` | `ReqStream_System_CliInterface_HelpFlag_PrintsHelp` |
| `ReqStream-System-FullPipeline` | `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces` |
| `ReqStream-System-SourceFilter` | `ReqStream_SourceFilter_NamedSourceInRequirement_MatchesTestsBySourceFile` |
| `ReqStream-System-EnforceMode` | `ReqStream_EnforcementMode_RequirementLacksTestEvidence_FailsWithNonZeroExitCode` |
| `ReqStream-System-Lint` | `ReqStream_System_Lint_Flag_ReportsLintIssues`, `ReqStream_System_Lint_ValidRequirementsFile_ExitsSilentlyWithZero` |
| `ReqStream-System-Validate` | `ReqStream_System_Validate_Flag_RunsSelfValidation` |
| `ReqStream-System-ValidateResultsOutput` | `ReqStream_System_ValidateResultsOutput_ResultsFlag_WritesResultsFile` |
| `ReqStream-System-RequirementsReport` | `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces` |
| `ReqStream-System-TraceMatrix` | `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces` |
| `ReqStream-System-Justifications` | `ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces` |
| `ReqStream-System-TagFilter` | `ReqStream_System_TagFilter_Flag_FiltersRequirements` |
| `ReqStream-System-OutputRouting` | `ReqStream_System_OutputControl_LogFlag_WritesOutputToFile`, `ReqStream_System_OutputControl_SilentFlag_SuppressesConsoleOutput` |
| `ReqStream-System-ReportDepth` | `ReqStream_System_ReportDepth_DepthFlag_GeneratesReportWithCorrectHeadingLevel` |
| `ReqStream-System-CrossPlatform` | `windows@ReqStream_VersionDisplay`, `ubuntu@ReqStream_VersionDisplay`, `macos@ReqStream_VersionDisplay` |
| `ReqStream-System-FileIncludes` | `ReqStream_System_FileIncludes_RequirementsWithIncludes_LoadsAllRequirements` |
| `ReqStream-Platform-Windows` | `windows@ReqStream_VersionDisplay`, `windows@ReqStream_HelpDisplay` |
| `ReqStream-Platform-Linux` | `ubuntu@ReqStream_VersionDisplay`, `ubuntu@ReqStream_HelpDisplay` |
| `ReqStream-Platform-MacOS` | `macos@ReqStream_VersionDisplay`, `macos@ReqStream_HelpDisplay` |
| `ReqStream-Platform-Net8` | `dotnet8.x@ReqStream_VersionDisplay`, `dotnet8.x@ReqStream_HelpDisplay` |
| `ReqStream-Platform-Net9` | `dotnet9.x@ReqStream_VersionDisplay`, `dotnet9.x@ReqStream_HelpDisplay` |
| `ReqStream-Platform-Net10` | `dotnet10.x@ReqStream_VersionDisplay`, `dotnet10.x@ReqStream_HelpDisplay` |
