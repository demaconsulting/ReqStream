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

using DemaConsulting.ReqStream.Cli;
using DemaConsulting.ReqStream.SelfTest;

namespace DemaConsulting.ReqStream.Tests.SelfTest;

/// <summary>
/// Tests for the SelfTest subsystem, proving the Validation class is sufficient to
/// implement the SelfTest subsystem requirements.
/// </summary>
[TestClass]
public class SelfTestTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_self_test_{Guid.NewGuid()}");
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
    /// Test that self-validation runs successfully and reports no failures.
    /// </summary>
    [TestMethod]
    public void SelfTest_Qualification_Run_PassesAllTests()
    {
        // Arrange: create a silent context to suppress console output during validation
        using var context = Context.Create(["--silent"]);

        // Act: run self-validation
        Validation.Run(context);

        // Assert: exit code is 0 indicating all self-validation checks passed
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test that self-validation writes a TRX results file when the results path has a .trx extension.
    /// </summary>
    [TestMethod]
    public void SelfTest_ResultsOutput_TrxResultsPath_WritesTrxFile()
    {
        // Arrange: define path for the TRX results output file
        var resultsFile = Path.Combine(_testDirectory, "validation-results.trx");
        using var context = Context.Create(["--silent", "--results", resultsFile]);

        // Act: run self-validation
        Validation.Run(context);

        // Assert: exit code is 0 and TRX file was created with expected content
        Assert.AreEqual(0, context.ExitCode);
        Assert.IsTrue(File.Exists(resultsFile), $"Expected TRX results file at {resultsFile}");
        var content = File.ReadAllText(resultsFile);
        Assert.Contains("TestRun", content);
    }

    /// <summary>
    /// Test that self-validation writes a JUnit XML results file when the results path has a .xml extension.
    /// </summary>
    [TestMethod]
    public void SelfTest_ResultsOutput_XmlResultsPath_WritesJUnitFile()
    {
        // Arrange: define path for the JUnit XML results output file
        var resultsFile = Path.Combine(_testDirectory, "validation-results.xml");
        using var context = Context.Create(["--silent", "--results", resultsFile]);

        // Act: run self-validation
        Validation.Run(context);

        // Assert: exit code is 0 and JUnit XML file was created with expected content
        Assert.AreEqual(0, context.ExitCode);
        Assert.IsTrue(File.Exists(resultsFile), $"Expected JUnit XML results file at {resultsFile}");
        var content = File.ReadAllText(resultsFile);
        Assert.Contains("testsuite", content);
    }

    /// <summary>
    /// Test that self-validation sets exit code 1 and reports errors when failures are encountered.
    /// </summary>
    [TestMethod]
    public void SelfTest_FailureReporting_WithErrors_SetsExitCode1()
    {
        // Arrange: create a results file path with an unsupported extension to trigger an error
        var resultsFile = Path.Combine(_testDirectory, "validation-results.invalid");
        var logFile = Path.Combine(_testDirectory, "failure-test.log");

        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--results", resultsFile]))
        {
            // Act: run self-validation
            Validation.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert: exit code is 1 and error output was written to the log (read after context disposed to flush log)
        Assert.AreEqual(1, exitCode);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("Error:", logContent);
    }
}
