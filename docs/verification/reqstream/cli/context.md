## Context Unit Verification

### Verification Strategy

The Context unit is verified using xUnit unit tests in `ContextTests.cs`. Tests create `Context`
instances with specific command-line argument arrays and assert the resulting property values,
file system effects, and exception behavior. Temporary directories are created for tests
requiring file system access.

### Test Scenarios

#### CLI Parsing Scenario

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

#### Requirements and Tests Pattern Scenario

Test methods:

- `Context_Create_WithRequirementsPattern_ExpandsGlobPattern` — glob patterns for requirements
- `Context_Create_WithTestsPattern_ExpandsGlobPattern` — glob patterns for test results

#### Results and Report Flags Scenario

Test methods:

- `Context_Create_ResultsFlag_SetsResultsFileProperty` — `--results` sets path
- `Context_Create_ResultFlag_SetsResultsFileProperty` — `--result` alias sets path
- `Context_Create_ReportFile_SetsReportProperty` — `--report` sets path
- `Context_Create_MissingReportFilename_ThrowsException` — `--report` without value throws
- `Context_Create_MatrixFile_SetsMatrixProperty` — `--matrix` sets path
- `Context_Create_MissingMatrixFilename_ThrowsException` — `--matrix` without value throws
- `Context_Create_JustificationsFile_SetsJustificationsFileProperty` — `--justifications` sets path
- `Context_Create_MissingJustificationsFilename_ThrowsException` — `--justifications` without value throws

#### Depth Flags Scenario

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

#### Tag Filter Scenario

Test methods:

- `Context_Create_FilterArgument_ParsesTagsCorrectly` — `--filter` parses comma-separated tags
- `Context_Create_FilterArgumentWithSpaces_TrimsAndParsesTagsCorrectly` — spaces are trimmed
- `Context_Create_FilterSingleTag_ParsesCorrectly` — single tag parsed
- `Context_Create_MultipleFilterArguments_MergesIntoSingleSet` — multiple `--filter` merged

#### Output Channel Scenario

Test methods:

- `Context_WriteLine_SilentMode_DoesNotWriteToConsole` — silent suppresses stdout
- `Context_WriteError_SilentMode_DoesNotWriteToConsole` — silent suppresses stderr
- `Context_WriteError_NormalMode_WritesToConsole` — normal mode writes to stderr
- `Context_ExitCode_AfterWriteError_ReturnsOne` — exit code is 1 after error

#### Log File Scenario

Test methods:

- `Context_Create_WithLogFile_WritesToLogFile` — log file receives output
- `Context_Create_WithLogFileAndSilent_WritesToLogOnly` — silent + log writes only to log
- `Context_Dispose_WithLogFile_ClosesLogFile` — dispose closes log file
- `Context_Create_InvalidLogPath_ThrowsException` — invalid log path throws

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Command-Cli` | `Context_Create_NoArguments_ReturnsDefaultContext`, `Context_Create_MultipleArguments_ParsesAllCorrectly` |
| `ReqStream-Command-Version` | `Context_Create_VersionFlag_SetsVersionProperty` |
| `ReqStream-Command-Help` | `Context_Create_HelpFlags_SetsHelpProperty` |
| `ReqStream-Command-Silent` | `Context_Create_SilentFlag_SetsSilentProperty`, `Context_WriteLine_SilentMode_DoesNotWriteToConsole`, `Context_WriteError_SilentMode_DoesNotWriteToConsole` |
| `ReqStream-Command-ErrorOutput` | `Context_WriteError_NormalMode_WritesToConsole` |
| `ReqStream-Command-UnknownArgs` | `Context_Create_UnsupportedArgument_ThrowsException` |
| `ReqStream-Command-MalformedArgs` | `Context_Create_MissingLogFilename_ThrowsException`, `Context_Create_MissingResultsFilename_ThrowsException`, `Context_Create_FilterArgumentMissingValue_ThrowsException` |
| `ReqStream-Command-RequirementsGlobPatterns` | `Context_Create_WithRequirementsPattern_ExpandsGlobPattern` |
| `ReqStream-Command-TestGlobPatterns` | `Context_Create_WithTestsPattern_ExpandsGlobPattern` |
| `ReqStream-Command-Validate` | `Context_Create_ValidateFlag_SetsValidateProperty` |
| `ReqStream-Command-Enforce` | `Context_Create_EnforceFlag_SetsEnforceProperty` |
| `ReqStream-Command-ExitCode` | `Context_ExitCode_AfterWriteError_ReturnsOne` |
| `ReqStream-Command-ReportDepth` | `Context_Create_ReportDepth_SetsReportDepthProperty` |
| `ReqStream-Command-MatrixDepth` | `Context_Create_MatrixDepth_SetsMatrixDepthProperty` |
| `ReqStream-Command-Depth` | `Context_Create_Depth_SetsAllDepths`, `Context_Create_SpecificDepthOverridesDefaultDepth`, `Context_Create_MissingDepth_ThrowsException`, `Context_Create_InvalidDepth_ThrowsException` |
| `ReqStream-Command-TagFilter` | `Context_Create_FilterArgument_ParsesTagsCorrectly`, `Context_Create_FilterArgumentWithSpaces_TrimsAndParsesTagsCorrectly`, `Context_Create_FilterSingleTag_ParsesCorrectly`, `Context_Create_MultipleFilterArguments_MergesIntoSingleSet` |
| `ReqStream-Command-Lint` | `Context_Create_LintFlag_SetsLintProperty` |
| `ReqStream-Command-Results` | `Context_Create_ResultsFlag_SetsResultsFileProperty`, `Context_Create_ResultFlag_SetsResultsFileProperty`, `Context_Create_MissingResultsFilename_ThrowsException` |
| `ReqStream-Command-Report` | `Context_Create_ReportFile_SetsReportProperty`, `Context_Create_MissingReportFilename_ThrowsException` |
| `ReqStream-Command-Matrix` | `Context_Create_MatrixFile_SetsMatrixProperty`, `Context_Create_MissingMatrixFilename_ThrowsException` |
| `ReqStream-Command-Justifications` | `Context_Create_JustificationsFile_SetsJustificationsFileProperty`, `Context_Create_MissingJustificationsFilename_ThrowsException` |
| `ReqStream-Command-JustificationsDepth` | `Context_Create_JustificationsDepth_SetsJustificationsDepthProperty`, `Context_Create_MissingJustificationsDepth_ThrowsException`, `Context_Create_InvalidJustificationsDepth_ThrowsException` |
| `ReqStream-Command-LogFileOutput` | `Context_Create_WithLogFile_WritesToLogFile`, `Context_Create_WithLogFileAndSilent_WritesToLogOnly`, `Context_Dispose_WithLogFile_ClosesLogFile`, `Context_Create_InvalidLogPath_ThrowsException` |
