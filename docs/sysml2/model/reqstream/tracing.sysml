package ReqStream {
    part def TracingSubsystem {
        doc /* Test result loading and requirement-coverage trace matrix generation. */

        comment testRef /* Test: test/DemaConsulting.ReqStream.Tests/Tracing/TracingTests.cs */
        comment designRef /* Design: docs/design/reqstream/tracing.md */
        comment verificationRef /* Verification: docs/verification/reqstream/tracing.md */
        comment reqRef /* Requirements: docs/reqstream/reqstream/tracing.yaml */

        part traceMatrix : TraceMatrix;
    }
}
