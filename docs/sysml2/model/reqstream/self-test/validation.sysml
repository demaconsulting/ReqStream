package ReqStream {
    private import SoftwareItemMetadata::*;

    part def Validation {
        doc /* Self-validation test runner. */

        metadata info : SoftwareInfo {
            description = "Self-validation test runner.";
            sourceFiles = ("src/DemaConsulting.ReqStream/SelfTest/Validation.cs");
            testFiles = ("test/DemaConsulting.ReqStream.Tests/SelfTest/ValidationTests.cs");
            designFiles = ("docs/design/reqstream/self-test/validation.md");
            verificationFiles = ("docs/verification/reqstream/self-test/validation.md");
            requirementFiles = ("docs/reqstream/reqstream/self-test/validation.yaml");
        }
    }
}
