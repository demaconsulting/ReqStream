package ReqStream_CliSubsystem {
    public import ReqStream_Context::*;
    public import ReqStream_HelpCommand::*;
    private import SoftwareItemMetadata::*;

    part def CliSubsystem {
        doc /* Command-line argument parsing and I/O ownership. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Cli/CliTests.cs");
            designFiles = ("docs/design/reqstream/cli.md");
            verificationFiles = ("docs/verification/reqstream/cli.md");
            requirementFiles = ("docs/reqstream/reqstream/cli.yaml");
        }

        part context : Context;
        part helpCommand : HelpCommand;
    }
}
