## Cli Subsystem Verification

### Verification Strategy

The Cli subsystem is verified using xUnit integration tests in `CliTests.cs`. Each test
constructs a `Context` from a specific set of command-line arguments and then asserts on the
combined observable behavior: the relevant flag property, exit code, or (where applicable)
file system state. The tests operate at the subsystem boundary — validating the interaction
between argument parsing (`Context`) and the I/O routing it provides — without mocking any
internal components.

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

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Cli-Interface` | `Cli_Interface_VersionFlag_SetsVersionProperty`, `Cli_Interface_HelpFlag_SetsHelpProperty`, `Cli_Interface_UnknownArgument_ThrowsArgumentException` |
| `ReqStream-Cli-Output` | `Cli_Output_SilentFlag_SetsSilentProperty`, `Cli_Output_LogFlag_WritesOutputToLogFile` |
| `ReqStream-Cli-StderrRouting` | `Cli_Output_WriteError_WritesToErrorChannel` |
| `ReqStream-Cli-ExitCodeSignaling` | `Cli_Output_WriteError_SetsExitCodeToOne` |
| `ReqStream-Cli-MissingArgumentValue` | `Cli_Interface_MissingArgumentValue_ThrowsArgumentException` |
| `ReqStream-Cli-InvalidDepthValue` | `Cli_Interface_InvalidDepthValue_ThrowsArgumentException` |
| `ReqStream-Cli-LogFileOpenFailure` | `Cli_Interface_LogFileOpenFailure_ThrowsArgumentException` |
| `ReqStream-Cli-DepthInheritance` | `Cli_Interface_DepthFlag_SetsDefaultForAllReportDepths` |
