package ReqStream {
    private import SoftwareItemMetadata::*;

    part def PathHelpers {
        doc /* Safe path-combination utility with traversal protection. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.ReqStream/Utilities/PathHelpers.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Utilities/PathHelpersTests.cs");
            designFiles = ("docs/design/reqstream/utilities/path-helpers.md");
            verificationFiles = ("docs/verification/reqstream/utilities/path-helpers.md");
            requirementFiles = ("docs/reqstream/reqstream/utilities/path-helpers.yaml");
        }
    }
}
