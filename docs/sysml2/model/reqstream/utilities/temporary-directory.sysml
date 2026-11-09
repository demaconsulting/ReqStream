package ReqStream {
    private import SoftwareItemMetadata::*;

    part def TemporaryDirectory {
        doc /* Disposable temporary directory for isolated file-system workspaces. */

        metadata info : SoftwareInfo {
            description = "Disposable temporary directory for isolated file-system workspaces.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Utilities/TemporaryDirectory.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Utilities/TemporaryDirectoryTests.cs");
            designFiles = ("docs/design/reqstream/utilities/temporary-directory.md");
            verificationFiles = ("docs/verification/reqstream/utilities/temporary-directory.md");
            requirementFiles = ("docs/reqstream/reqstream/utilities/temporary-directory.yaml");
        }
    }
}
