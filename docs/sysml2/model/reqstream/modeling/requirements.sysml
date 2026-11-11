package ReqStream {
    private import SoftwareItemMetadata::*;

    part def Requirements {
        doc /* Parsed requirements document with section tree, root-tag set, and orphan detection. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.ReqStream/Modeling/Requirements.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Modeling/RequirementsLoadTests.cs");
            designFiles = ("docs/design/reqstream/modeling/requirements.md");
            verificationFiles = ("docs/verification/reqstream/modeling/requirements.md");
            requirementFiles = ("docs/reqstream/reqstream/modeling/requirements.yaml");
        }
    }
}
