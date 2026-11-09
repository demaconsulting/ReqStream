package ReqStream {
    private import SoftwareItemMetadata::*;

    part def TraceMatrix {
        doc /* Test result loader and requirement-coverage analyzer. */

        metadata info : SoftwareInfo {
            description = "Test result loader and requirement-coverage analyzer.";
            sourceFiles = ("src/DemaConsulting.ReqStream/Tracing/TraceMatrix.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/Tracing/TraceMatrixTests.cs");
            designFiles = ("docs/design/reqstream/tracing/trace-matrix.md");
            verificationFiles = ("docs/verification/reqstream/tracing/trace-matrix.md");
            requirementFiles = ("docs/reqstream/reqstream/tracing/trace-matrix.yaml");
        }
    }
}
