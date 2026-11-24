package ReqStream_HelpCommand {
    private import SoftwareItemMetadata::*;

    part def HelpCommand {
        doc /* Built-in help command: answers the help verb from embedded help cards before option parsing. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.ReqStream/Cli/HelpCommand.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Cli/HelpCommandTests.cs");
            designFiles = ("docs/design/reqstream/cli/help-command.md");
            verificationFiles = ("docs/verification/reqstream/cli/help-command.md");
            requirementFiles = ("docs/reqstream/reqstream/cli/help-command.yaml");
        }
    }
}
