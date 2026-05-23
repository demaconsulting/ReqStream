### Context

#### Verification Approach

The Context unit is verified using xUnit unit tests in `ContextTests.cs`. Tests create `Context`
instances with specific command-line argument arrays and assert the resulting property values,
file system effects, and exception behavior. Temporary directories are created for tests
requiring file system access.

#### Test Environment

The Context unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary directories are created by tests that exercise log file or output routing behavior and
are deleted on test completion.

#### Acceptance Criteria

The Context unit verification is complete when all xUnit tests in `ContextTests.cs` pass without
uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**CLI Parsing**: Tests verify that Context correctly parses all supported command-line arguments.
This scenario is tested by `Context_Create_NoArguments_ReturnsDefaultContext`,
`Context_Create_VersionFlag_SetsVersionProperty`, `Context_Create_HelpFlags_SetsHelpProperty`,
`Context_Create_SilentFlag_SetsSilentProperty`, `Context_Create_ValidateFlag_SetsValidateProperty`,
`Context_Create_EnforceFlag_SetsEnforceProperty`, `Context_Create_LintFlag_SetsLintProperty`,
`Context_Create_UnsupportedArgument_ThrowsException`, `Context_Create_MultipleArguments_ParsesAllCorrectly`,
`Context_Create_MissingLogFilename_ThrowsException`, `Context_Create_MissingResultsFilename_ThrowsException`,
and `Context_Create_FilterArgumentMissingValue_ThrowsException`.

**Requirements and Tests Pattern**: Tests verify that glob patterns for requirements and test
files are correctly expanded. This scenario is tested by
`Context_Create_WithRequirementsPattern_ExpandsGlobPattern` and
`Context_Create_WithTestsPattern_ExpandsGlobPattern`.

**Results and Report Flags**: Tests verify that results file, report, matrix, and justifications
flags set the corresponding properties. This scenario is tested by
`Context_Create_ResultsFlag_SetsResultsFileProperty`, `Context_Create_ResultFlag_SetsResultsFileProperty`,
`Context_Create_ReportFile_SetsReportProperty`, `Context_Create_MissingReportFilename_ThrowsException`,
`Context_Create_MatrixFile_SetsMatrixProperty`, `Context_Create_MissingMatrixFilename_ThrowsException`,
`Context_Create_JustificationsFile_SetsJustificationsFileProperty`, and
`Context_Create_MissingJustificationsFilename_ThrowsException`.

**Depth Flags**: Tests verify that depth flags set the corresponding depth properties and that
per-report depth values override the global `--depth` setting. This scenario is tested by
`Context_Create_ReportDepth_SetsReportDepthProperty`, `Context_Create_MatrixDepth_SetsMatrixDepthProperty`,
`Context_Create_JustificationsDepth_SetsJustificationsDepthProperty`,
`Context_Create_Depth_SetsAllDepths`, `Context_Create_SpecificDepthOverridesDefaultDepth`,
`Context_Create_MissingDepth_ThrowsException`, `Context_Create_InvalidDepth_ThrowsException`,
`Context_Create_MissingJustificationsDepth_ThrowsException`, and
`Context_Create_InvalidJustificationsDepth_ThrowsException`.

**Tag Filter**: Tests verify that `--filter` parses comma-separated tags, trims whitespace, and
merges multiple filter arguments. This scenario is tested by
`Context_Create_FilterArgument_ParsesTagsCorrectly`,
`Context_Create_FilterArgumentWithSpaces_TrimsAndParsesTagsCorrectly`,
`Context_Create_FilterSingleTag_ParsesCorrectly`, and
`Context_Create_MultipleFilterArguments_MergesIntoSingleSet`.

**Output Channel**: Tests verify that output and error output are correctly routed in silent and
normal modes, and that WriteError sets the exit code to 1. This scenario is tested by
`Context_WriteLine_SilentMode_WritesToLogFile`, `Context_WriteError_SilentMode_WritesToLogFile`,
`Context_WriteError_NormalMode_WritesToLogFile`, `Context_WriteError_NormalMode_WritesToStderr`,
and `Context_ExitCode_AfterWriteError_ReturnsOne`.

**Log File**: Tests verify that output is written to the log file when `--log` is provided,
that silent mode writes only to the log file, that the log file is closed on disposal, and that
an invalid log path throws an exception. This scenario is tested by
`Context_Create_WithLogFile_WritesToLogFile`, `Context_Create_WithLogFileAndSilent_WritesToLogOnly`,
`Context_Dispose_WithLogFile_ClosesLogFile`, and `Context_Create_InvalidLogPath_ThrowsException`.

**Additional Defensive Tests**: The following eight tests provide supplementary error-path and
robustness coverage beyond the stated requirements. They guard against misconfiguration edge
cases and verify that malformed or incomplete argument lists are rejected with clear exceptions.
These tests are not directly mapped to a single requirement but strengthen overall defensive
coverage:
`Context_Create_MissingRequirementsPattern_ThrowsException`,
`Context_Create_MissingTestsPattern_ThrowsException`,
`Context_Create_MissingReportDepth_ThrowsException`,
`Context_Create_MissingMatrixDepth_ThrowsException`,
`Context_Create_InvalidReportDepth_ThrowsException`,
`Context_Create_InvalidMatrixDepth_ThrowsException`,
`Context_Create_MissingResultFilename_ThrowsException`, and
`Context_WriteLine_NormalMode_WritesToLogFile`.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Command-Cli` | CLI Parsing Scenario | `Context_Create_NoArguments_ReturnsDefaultContext` |
| `ReqStream-Command-Cli` | CLI Parsing Scenario | `Context_Create_MultipleArguments_ParsesAllCorrectly` |
| `ReqStream-Command-Version` | CLI Parsing Scenario | `Context_Create_VersionFlag_SetsVersionProperty` |
| `ReqStream-Command-Help` | CLI Parsing Scenario | `Context_Create_HelpFlags_SetsHelpProperty` |
| `ReqStream-Command-Silent` | CLI Parsing Scenario, Output Channel Scenario | `Context_Create_SilentFlag_SetsSilentProperty` |
| `ReqStream-Command-Silent` | CLI Parsing Scenario, Output Channel Scenario | `Context_WriteLine_SilentMode_WritesToLogFile` |
| `ReqStream-Command-Silent` | CLI Parsing Scenario, Output Channel Scenario | `Context_WriteError_SilentMode_WritesToLogFile` |
| `ReqStream-Command-ErrorOutput` | Output Channel Scenario | `Context_WriteError_NormalMode_WritesToLogFile` |
| `ReqStream-Command-ErrorOutput` | Output Channel Scenario | `Context_WriteError_NormalMode_WritesToStderr` |
| `ReqStream-Command-UnknownArgs` | CLI Parsing Scenario | `Context_Create_UnsupportedArgument_ThrowsException` |
| `ReqStream-Command-MissingLogValue` | CLI Parsing Scenario | `Context_Create_MissingLogFilename_ThrowsException` |
| `ReqStream-Command-MissingResultsValue` | CLI Parsing Scenario, Results and Report Flags Scenario | `Context_Create_MissingResultsFilename_ThrowsException` |
| `ReqStream-Command-MissingFilterValue` | CLI Parsing Scenario, Tag Filter Scenario | `Context_Create_FilterArgumentMissingValue_ThrowsException` |
| `ReqStream-Command-RequirementsGlobPatterns` | Requirements and Tests Pattern Scenario | `Context_Create_WithRequirementsPattern_ExpandsGlobPattern` |
| `ReqStream-Command-TestGlobPatterns` | Requirements and Tests Pattern Scenario | `Context_Create_WithTestsPattern_ExpandsGlobPattern` |
| `ReqStream-Command-Validate` | CLI Parsing Scenario | `Context_Create_ValidateFlag_SetsValidateProperty` |
| `ReqStream-Command-Enforce` | CLI Parsing Scenario | `Context_Create_EnforceFlag_SetsEnforceProperty` |
| `ReqStream-Command-ExitCode` | Output Channel Scenario | `Context_ExitCode_AfterWriteError_ReturnsOne` |
| `ReqStream-Command-ReportDepth` | Depth Flags Scenario | `Context_Create_ReportDepth_SetsReportDepthProperty` |
| `ReqStream-Command-MatrixDepth` | Depth Flags Scenario | `Context_Create_MatrixDepth_SetsMatrixDepthProperty` |
| `ReqStream-Command-Depth` | Depth Flags Scenario | `Context_Create_Depth_SetsAllDepths` |
| `ReqStream-Command-Depth` | Depth Flags Scenario | `Context_Create_SpecificDepthOverridesDefaultDepth` |
| `ReqStream-Command-Depth` | Depth Flags Scenario | `Context_Create_MissingDepth_ThrowsException` |
| `ReqStream-Command-Depth` | Depth Flags Scenario | `Context_Create_InvalidDepth_ThrowsException` |
| `ReqStream-Command-TagFilter` | Tag Filter Scenario | `Context_Create_FilterArgument_ParsesTagsCorrectly` |
| `ReqStream-Command-TagFilter` | Tag Filter Scenario | `Context_Create_FilterArgumentWithSpaces_TrimsAndParsesTagsCorrectly` |
| `ReqStream-Command-TagFilter` | Tag Filter Scenario | `Context_Create_FilterSingleTag_ParsesCorrectly` |
| `ReqStream-Command-TagFilter` | Tag Filter Scenario | `Context_Create_MultipleFilterArguments_MergesIntoSingleSet` |
| `ReqStream-Command-Lint` | CLI Parsing Scenario | `Context_Create_LintFlag_SetsLintProperty` |
| `ReqStream-Command-Results` | Results and Report Flags Scenario | `Context_Create_ResultsFlag_SetsResultsFileProperty` |
| `ReqStream-Command-Results` | Results and Report Flags Scenario | `Context_Create_ResultFlag_SetsResultsFileProperty` |
| `ReqStream-Command-Results` | Results and Report Flags Scenario | `Context_Create_MissingResultsFilename_ThrowsException` |
| `ReqStream-Command-Report` | Results and Report Flags Scenario | `Context_Create_ReportFile_SetsReportProperty` |
| `ReqStream-Command-Report` | Results and Report Flags Scenario | `Context_Create_MissingReportFilename_ThrowsException` |
| `ReqStream-Command-Matrix` | Results and Report Flags Scenario | `Context_Create_MatrixFile_SetsMatrixProperty` |
| `ReqStream-Command-Matrix` | Results and Report Flags Scenario | `Context_Create_MissingMatrixFilename_ThrowsException` |
| `ReqStream-Command-Justifications` | Results and Report Flags Scenario | `Context_Create_JustificationsFile_SetsJustificationsFileProperty` |
| `ReqStream-Command-Justifications` | Results and Report Flags Scenario | `Context_Create_MissingJustificationsFilename_ThrowsException` |
| `ReqStream-Command-JustificationsDepth` | Depth Flags Scenario | `Context_Create_JustificationsDepth_SetsJustificationsDepthProperty` |
| `ReqStream-Command-JustificationsDepth` | Depth Flags Scenario | `Context_Create_MissingJustificationsDepth_ThrowsException` |
| `ReqStream-Command-JustificationsDepth` | Depth Flags Scenario | `Context_Create_InvalidJustificationsDepth_ThrowsException` |
| `ReqStream-Command-LogFileOutput` | Log File Scenario | `Context_Create_WithLogFile_WritesToLogFile` |
| `ReqStream-Command-LogFileOutput` | Log File Scenario | `Context_Create_WithLogFileAndSilent_WritesToLogOnly` |
| `ReqStream-Command-LogFileOutput` | Log File Scenario | `Context_Dispose_WithLogFile_ClosesLogFile` |
| `ReqStream-Command-LogFileOutput` | Log File Scenario | `Context_Create_InvalidLogPath_ThrowsException` |
