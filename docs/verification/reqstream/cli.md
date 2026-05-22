## Cli

### Verification Approach

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
requirement in the Requirements Coverage is mapped to at least one passing test method.

### Test Scenarios

**Interface**: Tests verify that the Cli subsystem correctly parses flags and rejects unknown
arguments. This scenario is tested by `Cli_Interface_VersionFlag_SetsVersionProperty`, which
verifies `--version` sets the Version property; `Cli_Interface_HelpFlag_SetsHelpProperty`, which
verifies `--help` sets the Help property;
`Cli_Interface_UnknownArgument_ThrowsArgumentException`, which verifies an unknown argument throws
`ArgumentException`; `Cli_Interface_MissingArgumentValue_ThrowsArgumentException`, which verifies
a missing value throws `ArgumentException`;
`Cli_Interface_InvalidDepthValue_ThrowsArgumentException`, which verifies a non-integer depth
throws `ArgumentException`; `Cli_Interface_LogFileOpenFailure_ThrowsArgumentException`, which
verifies an inaccessible log path throws `ArgumentException`; and
`Cli_Interface_DepthFlag_SetsDefaultForAllReportDepths`, which verifies `--depth` sets all
per-report depths.

**Output**: Tests verify that output is correctly routed to the console and/or log file. This
scenario is tested by `Cli_Output_SilentFlag_SetsSilentProperty`, which verifies `--silent` sets
the Silent property; `Cli_Output_LogFlag_WritesOutputToLogFile`, which verifies `--log` writes
output to a file; `Cli_Output_WriteError_WritesToErrorChannel`, which verifies `WriteError`
writes to stderr; and `Cli_Output_WriteError_SetsExitCodeToOne`, which verifies `WriteError`
sets `ExitCode` to 1.

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
