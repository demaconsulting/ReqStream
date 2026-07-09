package ReqStream {
    part def ModelingSubsystem {
        doc /* Requirements data model, section tree, and YAML load/lint pipeline. */

        comment testRef /* Test: test/DemaConsulting.ReqStream.Tests/Modeling/ModelingTests.cs */
        comment designRef /* Design: docs/design/reqstream/modeling.md */
        comment verificationRef /* Verification: docs/verification/reqstream/modeling.md */
        comment reqRef /* Requirements: docs/reqstream/reqstream/modeling.yaml */

        part lintIssue : LintIssue;
        part loadResult : LoadResult;
        part requirementItem : Requirement;
        part requirementsItem : Requirements;
        part requirementsLoader : RequirementsLoader;
        part section : Section;
    }
}
