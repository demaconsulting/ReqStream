package ReqStream_ModelingSubsystem {
    public import ReqStream_LintIssue::*;
    public import ReqStream_LoadResult::*;
    public import ReqStream_Requirement::*;
    public import ReqStream_RequirementsLoader::*;
    public import ReqStream_Requirements::*;
    public import ReqStream_Section::*;
    private import SoftwareItemMetadata::*;

    part def ModelingSubsystem {
        doc /* Requirements data model, section tree, and YAML load/lint pipeline. */

        metadata info : SoftwareInfo {
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
