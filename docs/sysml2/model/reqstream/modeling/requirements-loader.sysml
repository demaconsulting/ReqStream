package ReqStream {
    private import SoftwareItemMetadata::*;

    part def RequirementsLoader {
        doc /* YAML deserializer and lint validator for requirements files. */

        metadata info : SoftwareInfo {
            description = "YAML deserializer and lint validator for requirements files.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Modeling/RequirementsLoader.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Modeling/RequirementsLoaderTests.cs");
            designFiles = ("docs/design/reqstream/modeling/requirements-loader.md");
            verificationFiles = ("docs/verification/reqstream/modeling/requirements-loader.md");
            requirementFiles = ("docs/reqstream/reqstream/modeling/requirements-loader.yaml");
        }
    }
}
