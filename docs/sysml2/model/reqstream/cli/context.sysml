package ReqStream {
    private import SoftwareItemMetadata::*;

    part def Context {
        doc /* Command-line argument parser, root-tag configuration, and I/O owner. */

        metadata info : SoftwareInfo {
            description = "Command-line argument parser, root-tag configuration, and I/O owner.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Cli/Context.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Cli/ContextTests.cs");
            designFiles = ("docs/design/reqstream/cli/context.md");
            verificationFiles = ("docs/verification/reqstream/cli/context.md");
            requirementFiles = ("docs/reqstream/reqstream/cli/context.yaml");
        }
    }
}
