package ReqStream {
    import OtsDependencies::*;

    doc
    /*
     * ReqStream is a .NET command-line tool for managing software requirements in YAML
     * format, tracing test results to requirements, and enforcing coverage in CI/CD pipelines.
     */

    part def ReqStreamSystem {
        doc /* The ReqStream system as a whole. */

        comment testRef /* Test: test/DemaConsulting.ReqStream.Tests/IntegrationTests.cs */
        comment designRef /* Design: docs/design/reqstream.md */
        comment verificationRef /* Verification: docs/verification/reqstream.md */
        comment reqRef /* Requirements: docs/reqstream/reqstream.yaml */

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
