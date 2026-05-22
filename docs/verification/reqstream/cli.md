## Cli Subsystem Verification

### Verification Strategy

The Cli subsystem is verified using xUnit integration tests in `CliTests.cs`. Each test
constructs a `Context` from a specific set of command-line arguments and then asserts on the
combined observable behavior: the relevant flag property, exit code, or (where applicable)
file system state. The tests operate at the subsystem boundary — validating the interaction
between argument parsing (`Context`) and the I/O routing it provides — without mocking any
internal components.

### Test Environment

The Cli subsystem tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary directories are created by tests that require file system access (e.g. log file tests)
and are deleted on test completion.

### Acceptance Criteria

The Cli subsystem verification is complete when all xUnit tests in `CliTests.cs` pass without
uncaught exceptions and all assertions succeed. The subsystem is considered verified when every
requirement in the Coverage Summary is mapped to at least one passing test method.

### Test Scenarios

#### Interface Scenario

Tests verify that the Cli subsystem correctly parses flags and rejects unknown arguments.

Test methods:

- `Cli_Interface_VersionFlag_SetsVersionProperty` — `--version` sets Version property
- `Cli_Interface_HelpFlag_SetsHelpProperty` — `--help` sets Help property
- `Cli_Interface_UnknownArgument_ThrowsArgumentException` — unknown arg throws ArgumentException
- `Cli_Interface_MissingArgumentValue_ThrowsArgumentException` — missing value throws ArgumentException
- `Cli_Interface_InvalidDepthValue_ThrowsArgumentException` — non-integer depth throws ArgumentException
- `Cli_Interface_LogFileOpenFailure_ThrowsArgumentException` — inaccessible log path throws ArgumentException
- `Cli_Interface_DepthFlag_SetsDefaultForAllReportDepths` — `--depth` sets all per-report depths

#### Output Scenario

Tests verify that output is correctly routed to the console and/or log file.

Test methods:

- `Cli_Output_SilentFlag_SetsSilentProperty` — `--silent` sets Silent property
- `Cli_Output_LogFlag_WritesOutputToLogFile` — `--log` writes output to file
- `Cli_Output_WriteError_WritesToErrorChannel` — WriteError writes to stderr
- `Cli_Output_WriteError_SetsExitCodeToOne` — WriteError sets ExitCode to 1

### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Cli-Interface` | Interface Scenario | `Cli_Interface_VersionFlag_SetsVersionProperty`, `Cli_Interface_HelpFlag_SetsHelpProperty`, `Cli_Interface_UnknownArgument_ThrowsArgumentException` |
| `ReqStream-Cli-Output` | Output Scenario | `Cli_Output_SilentFlag_SetsSilentProperty`, `Cli_Output_LogFlag_WritesOutputToLogFile` |
| `ReqStream-Cli-StderrRouting` | Output Scenario | `Cli_Output_WriteError_WritesToErrorChannel` |
| `ReqStream-Cli-ExitCodeSignaling` | Output Scenario | `Cli_Output_WriteError_SetsExitCodeToOne` |
| `ReqStream-Cli-MissingArgumentValue` | Interface Scenario | `Cli_Interface_MissingArgumentValue_ThrowsArgumentException` |
| `ReqStream-Cli-InvalidDepthValue` | Interface Scenario | `Cli_Interface_InvalidDepthValue_ThrowsArgumentException` |
| `ReqStream-Cli-LogFileOpenFailure` | Interface Scenario | `Cli_Interface_LogFileOpenFailure_ThrowsArgumentException` |
| `ReqStream-Cli-DepthInheritance` | Interface Scenario | `Cli_Interface_DepthFlag_SetsDefaultForAllReportDepths` |
