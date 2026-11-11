package ReqStream {
    private import SoftwareItemMetadata::*;

    part def SelfTestSubsystem {
        doc /* Self-validation test runner exercised at startup for OTS/BuildMark self-check evidence. */

        metadata info : SoftwareInfo {
            testFiles = ("test/DemaConsulting.ReqStream.Tests/SelfTest/SelfTestTests.cs");
            designFiles = ("docs/design/reqstream/self-test.md");
            verificationFiles = ("docs/verification/reqstream/self-test.md");
            requirementFiles = ("docs/reqstream/reqstream/self-test.yaml");
        }

        part validation : Validation;
    }
}
