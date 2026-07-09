package ReqStream {
    part def UtilitiesSubsystem {
        doc /* Glob-pattern matching, safe path handling, and temporary workspace helpers. */

        comment designRef /* Design: docs/design/reqstream/utilities.md */
        comment verificationRef /* Verification: docs/verification/reqstream/utilities.md */
        comment reqRef /* Requirements: docs/reqstream/reqstream/utilities.yaml */

        part globMatcher : GlobMatcher;
        part pathHelpers : PathHelpers;
        part temporaryDirectory : TemporaryDirectory;
    }
}
