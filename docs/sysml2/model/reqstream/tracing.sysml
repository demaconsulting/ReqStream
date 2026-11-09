package ReqStream {
    private import SoftwareItemMetadata::*;

    part def TracingSubsystem {
        doc /* Test result loading and requirement-coverage trace matrix generation. */

        metadata info : SoftwareInfo {
            description = "Test result loading and requirement-coverage trace matrix generation.";
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Tracing/TracingTests.cs");
            designFiles = ("docs/design/reqstream/tracing.md");
            verificationFiles = ("docs/verification/reqstream/tracing.md");
            requirementFiles = ("docs/reqstream/reqstream/tracing.yaml");
        }

        part traceMatrix : TraceMatrix;
    }
}
