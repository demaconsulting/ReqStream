package ReqStream {
    private import SoftwareItemMetadata::*;

    part def Program {
        doc /* Entry point, execution orchestrator, and orphan-check reporting/enforcement. */

        metadata info : SoftwareInfo {
            description = "Entry point, execution orchestrator, and orphan-check reporting/enforcement.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Program.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/ProgramTests.cs");
            designFiles = ("docs/design/reqstream/program.md");
            verificationFiles = ("docs/verification/reqstream/program.md");
            requirementFiles = ("docs/reqstream/reqstream/program.yaml");
        }
    }
}
