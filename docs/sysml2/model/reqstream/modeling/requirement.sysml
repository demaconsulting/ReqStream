package ReqStream {
    private import SoftwareItemMetadata::*;

    part def Requirement {
        doc /* Single requirement with ID, title, and test links. */

        metadata info : SoftwareInfo {
            description = "Single requirement with ID, title, and test links.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Modeling/Requirement.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Modeling/RequirementTests.cs");
            designFiles = ("docs/design/reqstream/modeling/requirement.md");
            verificationFiles = ("docs/verification/reqstream/modeling/requirement.md");
            requirementFiles = ("docs/reqstream/reqstream/modeling/requirement.yaml");
        }
    }
}
