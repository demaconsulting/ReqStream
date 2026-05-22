### Context Unit Verification

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
requirement in the Coverage Summary is mapped to at least one passing test method.

#### Test Scenarios

##### CLI Parsing Scenario

Tests verify that Context correctly parses all supported command-line arguments.

Test methods:

- `Context_Create_NoArguments_ReturnsDefaultContext` — no args → default context
- `Context_Create_VersionFlag_SetsVersionProperty` — `--version` sets Version
- `Context_Create_HelpFlags_SetsHelpProperty` — `--help`/`-h`/`-?` sets Help
- `Context_Create_SilentFlag_SetsSilentProperty` — `--silent` sets Silent
- `Context_Create_ValidateFlag_SetsValidateProperty` — `--validate` sets Validate
- `Context_Create_EnforceFlag_SetsEnforceProperty` — `--enforce` sets Enforce
- `Context_Create_LintFlag_SetsLintProperty` — `--lint` sets Lint
- `Context_Create_UnsupportedArgument_ThrowsException` — unknown arg throws
- `Context_Create_MultipleArguments_ParsesAllCorrectly` — multiple flags parsed
- `Context_Create_MissingLogFilename_ThrowsException` — `--log` without value throws
- `Context_Create_MissingResultsFilename_ThrowsException` — `--results` without value throws
- `Context_Create_FilterArgumentMissingValue_ThrowsException` — `--filter` without value throws

##### Requirements and Tests Pattern Scenario

Test methods:

- `Context_Create_WithRequirementsPattern_ExpandsGlobPattern` — glob patterns for requirements
- `Context_Create_WithTestsPattern_ExpandsGlobPattern` — glob patterns for test results

##### Results and Report Flags Scenario

Test methods:

- `Context_Create_ResultsFlag_SetsResultsFileProperty` — `--results` sets path
- `Context_Create_ResultFlag_SetsResultsFileProperty` — `--result` alias sets path
- `Context_Create_ReportFile_SetsReportProperty` — `--report` sets path
- `Context_Create_MissingReportFilename_ThrowsException` — `--report` without value throws
- `Context_Create_MatrixFile_SetsMatrixProperty` — `--matrix` sets path
- `Context_Create_MissingMatrixFilename_ThrowsException` — `--matrix` without value throws
- `Context_Create_JustificationsFile_SetsJustificationsFileProperty` — `--justifications` sets path
- `Context_Create_MissingJustificationsFilename_ThrowsException` — `--justifications` without value throws

##### Depth Flags Scenario

Test methods:

- `Context_Create_ReportDepth_SetsReportDepthProperty` — `--report-depth` sets report depth
- `Context_Create_MatrixDepth_SetsMatrixDepthProperty` — `--matrix-depth` sets matrix depth
- `Context_Create_JustificationsDepth_SetsJustificationsDepthProperty` — `--justifications-depth`
- `Context_Create_Depth_SetsAllDepths` — `--depth` sets all depth properties
- `Context_Create_SpecificDepthOverridesDefaultDepth` — per-report overrides `--depth`
- `Context_Create_MissingDepth_ThrowsException` — `--depth` without value throws
- `Context_Create_InvalidDepth_ThrowsException` — non-integer depth throws
- `Context_Create_MissingJustificationsDepth_ThrowsException` — missing justifications depth throws
- `Context_Create_InvalidJustificationsDepth_ThrowsException` — invalid justifications depth throws

##### Tag Filter Scenario

Test methods:

- `Context_Create_FilterArgument_ParsesTagsCorrectly` — `--filter` parses comma-separated tags
- `Context_Create_FilterArgumentWithSpaces_TrimsAndParsesTagsCorrectly` — spaces are trimmed
- `Context_Create_FilterSingleTag_ParsesCorrectly` — single tag parsed
- `Context_Create_MultipleFilterArguments_MergesIntoSingleSet` — multiple `--filter` merged

##### Output Channel Scenario

Test methods:

- `Context_WriteLine_SilentMode_WritesToLogFile` — silent mode still writes to log file
- `Context_WriteError_SilentMode_WritesToLogFile` — silent mode still writes error to log file
- `Context_WriteError_NormalMode_WritesToLogFile` — normal mode writes error to log file
- `Context_ExitCode_AfterWriteError_ReturnsOne` — exit code is 1 after error

##### Log File Scenario

Test methods:

- `Context_Create_WithLogFile_WritesToLogFile` — log file receives output
- `Context_Create_WithLogFileAndSilent_WritesToLogOnly` — silent + log writes only to log
- `Context_Dispose_WithLogFile_ClosesLogFile` — dispose closes log file
- `Context_Create_InvalidLogPath_ThrowsException` — invalid log path throws

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Command-Cli` | CLI Parsing Scenario | `Context_Create_NoArguments_ReturnsDefaultContext`, `Context_Create_MultipleArguments_ParsesAllCorrectly` |
| `ReqStream-Command-Version` | CLI Parsing Scenario | `Context_Create_VersionFlag_SetsVersionProperty` |
| `ReqStream-Command-Help` | CLI Parsing Scenario | `Context_Create_HelpFlags_SetsHelpProperty` |
| `ReqStream-Command-Silent` | CLI Parsing Scenario, Output Channel Scenario | `Context_Create_SilentFlag_SetsSilentProperty`, `Context_WriteLine_SilentMode_WritesToLogFile`, `Context_WriteError_SilentMode_WritesToLogFile` |
| `ReqStream-Command-ErrorOutput` | Output Channel Scenario | `Context_WriteError_NormalMode_WritesToLogFile` |
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
| `ReqStream-Command-Depth` | Depth Flags Scenario | `Context_Create_Depth_SetsAllDepths`, `Context_Create_SpecificDepthOverridesDefaultDepth`, `Context_Create_MissingDepth_ThrowsException`, `Context_Create_InvalidDepth_ThrowsException` |
| `ReqStream-Command-TagFilter` | Tag Filter Scenario | `Context_Create_FilterArgument_ParsesTagsCorrectly`, `Context_Create_FilterArgumentWithSpaces_TrimsAndParsesTagsCorrectly`, `Context_Create_FilterSingleTag_ParsesCorrectly`, `Context_Create_MultipleFilterArguments_MergesIntoSingleSet` |
| `ReqStream-Command-Lint` | CLI Parsing Scenario | `Context_Create_LintFlag_SetsLintProperty` |
| `ReqStream-Command-Results` | Results and Report Flags Scenario | `Context_Create_ResultsFlag_SetsResultsFileProperty`, `Context_Create_ResultFlag_SetsResultsFileProperty`, `Context_Create_MissingResultsFilename_ThrowsException` |
| `ReqStream-Command-Report` | Results and Report Flags Scenario | `Context_Create_ReportFile_SetsReportProperty`, `Context_Create_MissingReportFilename_ThrowsException` |
| `ReqStream-Command-Matrix` | Results and Report Flags Scenario | `Context_Create_MatrixFile_SetsMatrixProperty`, `Context_Create_MissingMatrixFilename_ThrowsException` |
| `ReqStream-Command-Justifications` | Results and Report Flags Scenario | `Context_Create_JustificationsFile_SetsJustificationsFileProperty`, `Context_Create_MissingJustificationsFilename_ThrowsException` |
| `ReqStream-Command-JustificationsDepth` | Depth Flags Scenario | `Context_Create_JustificationsDepth_SetsJustificationsDepthProperty`, `Context_Create_MissingJustificationsDepth_ThrowsException`, `Context_Create_InvalidJustificationsDepth_ThrowsException` |
| `ReqStream-Command-LogFileOutput` | Log File Scenario | `Context_Create_WithLogFile_WritesToLogFile`, `Context_Create_WithLogFileAndSilent_WritesToLogOnly`, `Context_Dispose_WithLogFile_ClosesLogFile`, `Context_Create_InvalidLogPath_ThrowsException` |
