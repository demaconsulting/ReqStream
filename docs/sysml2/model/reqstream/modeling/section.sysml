package ReqStream {
    private import SoftwareItemMetadata::*;

    part def Section {
        doc /* Named group of requirements within a document. */

        metadata info : SoftwareInfo {
            description = "Named group of requirements within a document.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Modeling/Section.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Modeling/SectionTests.cs");
            designFiles = ("docs/design/reqstream/modeling/section.md");
            verificationFiles = ("docs/verification/reqstream/modeling/section.md");
            requirementFiles = ("docs/reqstream/reqstream/modeling/section.yaml");
        }
    }
}
