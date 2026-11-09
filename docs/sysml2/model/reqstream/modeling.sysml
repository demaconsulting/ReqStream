package ReqStream {
    private import SoftwareItemMetadata::*;

    part def ModelingSubsystem {
        doc /* Requirements data model, section tree, and YAML load/lint pipeline. */

        metadata info : SoftwareInfo {
            description = "Requirements data model, section tree, and YAML load/lint pipeline.";
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Modeling/ModelingTests.cs");
            designFiles = ("docs/design/reqstream/modeling.md");
            verificationFiles = ("docs/verification/reqstream/modeling.md");
            requirementFiles = ("docs/reqstream/reqstream/modeling.yaml");
        }

        part lintIssue : LintIssue;
        part loadResult : LoadResult;
        part requirementItem : Requirement;
        part requirementsItem : Requirements;
        part requirementsLoader : RequirementsLoader;
        part section : Section;
    }
}
