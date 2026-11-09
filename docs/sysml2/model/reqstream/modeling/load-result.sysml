package ReqStream {
    private import SoftwareItemMetadata::*;

    part def LoadResult {
        doc /* Combined result of loading requirements and associated lint issues. */

        metadata info : SoftwareInfo {
            description = "Combined result of loading requirements and associated lint issues.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Modeling/LoadResult.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Modeling/LoadResultTests.cs");
            designFiles = ("docs/design/reqstream/modeling/load-result.md");
            verificationFiles = ("docs/verification/reqstream/modeling/load-result.md");
            requirementFiles = ("docs/reqstream/reqstream/modeling/load-result.yaml");
        }
    }
}
