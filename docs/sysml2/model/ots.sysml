package OtsDependencies {
    private import SoftwareItemMetadata::*;

    doc /* Off-the-shelf (OTS) dependencies used by ReqStream. */

    part def YamlDotNet {
        doc /* OTS: YamlDotNet. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/yamldotnet.md");
            verificationFiles = ("docs/verification/ots/yamldotnet.md");
            requirementFiles = ("docs/reqstream/ots/yamldotnet.yaml");
        }
    }

    part def FileSystemGlobbing {
        doc /* OTS: Microsoft.Extensions.FileSystemGlobbing. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/microsoft-extensions-file-system-globbing.md");
            verificationFiles = ("docs/verification/ots/microsoft-extensions-file-system-globbing.md");
            requirementFiles = ("docs/reqstream/ots/microsoft-extensions-file-system-globbing.yaml");
        }
    }

    part def TestResults {
        doc /* OTS: DemaConsulting.TestResults. */

        metadata info : SoftwareInfo {
            designFiles = ("docs/design/ots/dema-consulting-test-results.md");
            verificationFiles = ("docs/verification/ots/dema-consulting-test-results.md");
            requirementFiles = ("docs/reqstream/ots/dema-consulting-test-results.yaml");
        }
    }
}
