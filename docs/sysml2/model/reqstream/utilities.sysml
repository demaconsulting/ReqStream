package ReqStream {
    private import SoftwareItemMetadata::*;

    part def UtilitiesSubsystem {
        doc /* Glob-pattern matching, safe path handling, and temporary workspace helpers. */

        metadata info : SoftwareInfo {
            description = "Glob-pattern matching, safe path handling, and temporary workspace helpers.";
            designFiles = ("docs/design/reqstream/utilities.md");
            verificationFiles = ("docs/verification/reqstream/utilities.md");
            requirementFiles = ("docs/reqstream/reqstream/utilities.yaml");
        }

        part globMatcher : GlobMatcher;
        part pathHelpers : PathHelpers;
        part temporaryDirectory : TemporaryDirectory;
    }
}
