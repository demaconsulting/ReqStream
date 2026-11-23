package ReqStream_UtilitiesSubsystem {
    public import ReqStream_GlobMatcher::*;
    public import ReqStream_PathHelpers::*;
    public import ReqStream_TemporaryDirectory::*;
    private import SoftwareItemMetadata::*;

    part def UtilitiesSubsystem {
        doc /* Glob-pattern matching, safe path handling, and temporary workspace helpers. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/reqstream/utilities.md");
            verificationFiles = ("docs/verification/reqstream/utilities.md");
            requirementFiles = ("docs/reqstream/reqstream/utilities.yaml");
        }

        part globMatcher : GlobMatcher;
        part pathHelpers : PathHelpers;
        part temporaryDirectory : TemporaryDirectory;
    }
}
