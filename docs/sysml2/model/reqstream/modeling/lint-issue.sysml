package ReqStream {
    private import SoftwareItemMetadata::*;

    part def LintIssue {
        doc /* Lint issue severity and data model. */

        metadata info : SoftwareInfo {
            description = "Lint issue severity and data model.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Modeling/LintIssue.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Modeling/LintIssueTests.cs");
            designFiles = ("docs/design/reqstream/modeling/lint-issue.md");
            verificationFiles = ("docs/verification/reqstream/modeling/lint-issue.md");
            requirementFiles = ("docs/reqstream/reqstream/modeling/lint-issue.yaml");
        }
    }
}
