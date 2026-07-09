package ReqStream {
    part def SelfTestSubsystem {
        doc /* Self-validation test runner exercised at startup for OTS/BuildMark self-check evidence. */

        comment testRef /* Test: test/DemaConsulting.ReqStream.Tests/SelfTest/SelfTestTests.cs */
        comment designRef /* Design: docs/design/reqstream/self-test.md */
        comment verificationRef /* Verification: docs/verification/reqstream/self-test.md */
        comment reqRef /* Requirements: docs/reqstream/reqstream/self-test.yaml */

        part validation : Validation;
    }
}
