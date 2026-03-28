// Copyright (c) 2026 DEMA Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

namespace DemaConsulting.ReqStream.Tests;

/// <summary>
/// Integration tests for the ReqStream system, exercising the full pipeline across
/// multiple subsystems in end-to-end scenarios.
/// </summary>
[TestClass]
public class IntegrationTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Clean up test by deleting the temporary test directory.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    /// <summary>
    /// Integration test verifying that a single invocation can process requirements, load test
    /// results, generate a requirements report, justifications, and trace matrix, and enforce
    /// coverage — all subsystems working together correctly.
    /// </summary>
    [TestMethod]
    public void ReqStream_FullPipeline_GeneratesAllReportsAndEnforces()
    {
        // Arrange: create requirements file with one covered requirement
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-DoSomethingUseful
                    title: The system shall do something useful.
                    justification: |
                      This is a test justification.
                    tests:
                      - IntegrationTest1
            """);

        // Arrange: create TRX file with a passing test result
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "IntegrationRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "IntegrationTest1",
            ClassName = "IntegrationTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxFile = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var reportFile = Path.Combine(_testDirectory, "requirements.md");
        var justificationsFile = Path.Combine(_testDirectory, "justifications.md");
        var matrixFile = Path.Combine(_testDirectory, "matrix.md");

        // Act: run the full pipeline
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create([
                "--requirements", "requirements.yaml",
                "--tests", "results.trx",
                "--report", reportFile,
                "--justifications", justificationsFile,
                "--matrix", matrixFile,
                "--enforce"
            ]);
            Program.Run(context);

            // Assert: enforcement passed (exit code 0)
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        // Assert: all three output files were generated
        Assert.IsTrue(File.Exists(reportFile), "Requirements report should be generated.");
        Assert.IsTrue(File.Exists(justificationsFile), "Justifications report should be generated.");
        Assert.IsTrue(File.Exists(matrixFile), "Trace matrix report should be generated.");

        // Assert: report contains the requirement ID and title
        var reportContent = File.ReadAllText(reportFile);
        Assert.IsTrue(reportContent.Contains("Integration-System-DoSomethingUseful"), "Requirements report should contain the requirement ID.");
        Assert.IsTrue(reportContent.Contains("The system shall do something useful."),
            "Requirements report should contain the requirement title.");

        // Assert: trace matrix contains the satisfied requirement
        var matrixContent = File.ReadAllText(matrixFile);
        Assert.IsTrue(matrixContent.Contains("Integration-System-DoSomethingUseful"), "Trace matrix should contain the requirement ID.");
        Assert.IsTrue(matrixContent.Contains("satisfied with tests"), "Trace matrix should show requirements as satisfied.");
    }

    /// <summary>
    /// Integration test verifying that enforcement mode causes a non-zero exit code when a
    /// requirement has no passing test evidence, confirming the CI/CD gate operates correctly.
    /// </summary>
    [TestMethod]
    public void ReqStream_EnforcementMode_FailsWhenRequirementLacksTestEvidence()
    {
        // Arrange: create requirements file with one requirement that has no matching test
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-Unsatisfied
                    title: The system shall perform an unverified action.
                    justification: |
                      This requirement deliberately has no matching test to verify enforcement failure.
                    tests:
                      - NonExistentTest
            """);

        // Arrange: create TRX file with no tests (empty results)
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "EmptyRun" };
        var trxFile = Path.Combine(_testDirectory, "empty.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        // Act: run enforcement mode
        var originalDir = Directory.GetCurrentDirectory();
        int exitCode;
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create([
                "--requirements", "requirements.yaml",
                "--tests", "empty.trx",
                "--enforce"
            ]);
            Program.Run(context);
            exitCode = context.ExitCode;
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        // Assert: enforcement failed with a non-zero exit code
        Assert.AreNotEqual(0, exitCode, "Enforcement should fail with non-zero exit code when a requirement lacks test evidence.");
    }

    /// <summary>
    /// Integration test verifying that source-specific test matching restricts coverage evidence
    /// to tests from the named result file, and that enforcement passes when the named source
    /// provides the required passing test.
    /// </summary>
    [TestMethod]
    public void ReqStream_SourceFilter_MatchesTestsBySourceFile()
    {
        // Arrange: create requirements file with source-specific test reference
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Platform Requirements
                requirements:
                  - id: Integration-PlatformA-SourceFilter
                    title: The system shall pass the platform-specific test on platform-a.
                    justification: |
                      Platform-specific behavior must be validated on the target platform.
                    tests:
                      - platform-a@PlatformTest1
            """);

        // Arrange: create platform-a.trx with a passing test
        var platformAResults = new DemaConsulting.TestResults.TestResults { Name = "PlatformARun" };
        platformAResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "PlatformTest1",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var platformAFile = Path.Combine(_testDirectory, "platform-a.trx");
        File.WriteAllText(platformAFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(platformAResults));

        // Arrange: create platform-b.trx without the platform-a test
        var platformBResults = new DemaConsulting.TestResults.TestResults { Name = "PlatformBRun" };
        platformBResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            // Include the same test name as in platform-a.trx, but with a failing outcome,
            // so the test validates that the source-specific filter (platform-a@) is honored.
            Name = "PlatformTest1",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Failed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var platformBFile = Path.Combine(_testDirectory, "platform-b.trx");
        File.WriteAllText(platformBFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(platformBResults));

        // Act: run enforcement using both result files
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create([
                "--requirements", "requirements.yaml",
                "--tests", "platform-a.trx",
                "--tests", "platform-b.trx",
                "--enforce"
            ]);
            Program.Run(context);

            // Assert: enforcement passed because platform-a.trx satisfies the source-filtered requirement
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }
}
