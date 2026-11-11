package ReqStream {
    private import SoftwareItemMetadata::*;

    part def GlobMatcher {
        doc /* Glob-pattern file matching utility. */

        metadata info : SoftwareInfo {
            sourceFiles = ("src/DemaConsulting.ReqStream/Utilities/GlobMatcher.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Utilities/GlobMatcherTests.cs");
            designFiles = ("docs/design/reqstream/utilities/glob-matcher.md");
            verificationFiles = ("docs/verification/reqstream/utilities/glob-matcher.md");
            requirementFiles = ("docs/reqstream/reqstream/utilities/glob-matcher.yaml");
        }
    }
}
