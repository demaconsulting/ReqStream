package ReqStream {
    private import SoftwareItemMetadata::*;

    import OtsDependencies::*;

    doc
    /*
     * ReqStream is a .NET command-line tool for managing software requirements in YAML
     * format, tracing test results to requirements, and enforcing coverage in CI/CD pipelines.
     */

    part def ReqStreamSystem {
        doc /* The ReqStream system as a whole. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.ReqStream.Tests/IntegrationTests.cs");
            designFiles = ("docs/design/reqstream.md");
            verificationFiles = ("docs/verification/reqstream.md");
            requirementFiles = ("docs/reqstream/reqstream.yaml");
        }

        part program : Program;
        part cli : CliSubsystem;
        part modeling : ModelingSubsystem;
        part tracing : TracingSubsystem;
        part selfTest : SelfTestSubsystem;
        part utilities : UtilitiesSubsystem;

        part yamlDotNet : YamlDotNet;
        part fileSystemGlobbing : FileSystemGlobbing;
        part testResults : TestResults;
    }
}
