package OtsDependencies {
    doc /* Off-the-shelf (OTS) dependencies used by ReqStream. */

    part def YamlDotNet {
        doc /* OTS: YamlDotNet. */

        comment designRef /* Design: docs/design/ots/yamldotnet.md */
        comment verificationRef /* Verification: docs/verification/ots/yamldotnet.md */
        comment reqRef /* Requirements: docs/reqstream/ots/yamldotnet.yaml */
    }

    part def FileSystemGlobbing {
        doc /* OTS: Microsoft.Extensions.FileSystemGlobbing. */

        comment designRef /* Design: docs/design/ots/microsoft-extensions-file-system-globbing.md */
        comment verificationRef /* Verification: docs/verification/ots/microsoft-extensions-file-system-globbing.md */
        comment reqRef /* Requirements: docs/reqstream/ots/microsoft-extensions-file-system-globbing.yaml */
    }

    part def TestResults {
        doc /* OTS: DemaConsulting.TestResults. */

        comment designRef /* Design: docs/design/ots/dema-consulting-test-results.md */
        comment verificationRef /* Verification: docs/verification/ots/dema-consulting-test-results.md */
        comment reqRef /* Requirements: docs/reqstream/ots/dema-consulting-test-results.yaml */
    }
}
