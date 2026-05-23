// Copyright (c) 2025 DEMA Consulting
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

using DemaConsulting.ReqStream.Utilities;

namespace DemaConsulting.ReqStream.Tests.SelfTest;

/// <summary>
/// Tests for the SelfTest subsystem, proving the Validation class is sufficient to
/// implement the SelfTest subsystem requirements.
/// </summary>
[Collection("Sequential")]
public sealed class SelfTestTests : IDisposable
{
    /// <summary>Temporary directory providing isolated file-system workspace for this test class instance.</summary>
    private readonly TemporaryDirectory _testDirectory = new();

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public SelfTestTests()
    {

    }

    /// <summary>
    /// Clean up test by deleting the temporary test directory.
    /// </summary>
    public void Dispose()
    {
        _testDirectory.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Test that self-validation runs successfully and reports no failures.
    /// </summary>
    [Fact]
    public void SelfTest_Qualification_Run_PassesAllTests()
    {
        // Arrange: create a silent context to suppress console output during validation
        using var context = Context.Create(["--silent"]);

        // Act: run self-validation
        Validation.Run(context);

        // Assert: exit code is 0 indicating all self-validation checks passed
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test that self-validation writes a TRX results file when the results path has a .trx extension.
    /// </summary>
    [Fact]
    public void SelfTest_ResultsOutput_TrxResultsPath_WritesTrxFile()
    {
        // Arrange: define path for the TRX results output file
        var resultsFile = _testDirectory.GetFilePath("validation-results.trx");
        using var context = Context.Create(["--silent", "--results", resultsFile]);

        // Act: run self-validation
        Validation.Run(context);

        // Assert: exit code is 0 and TRX file was created with expected content
        Assert.Equal(0, context.ExitCode);
        Assert.True(File.Exists(resultsFile), $"Expected TRX results file at {resultsFile}");
        var content = File.ReadAllText(resultsFile);
        Assert.Contains("TestRun", content);
    }

    /// <summary>
    /// Test that self-validation writes a JUnit XML results file when the results path has a .xml extension.
    /// </summary>
    [Fact]
    public void SelfTest_ResultsOutput_XmlResultsPath_WritesJUnitFile()
    {
        // Arrange: define path for the JUnit XML results output file
        var resultsFile = _testDirectory.GetFilePath("validation-results.xml");
        using var context = Context.Create(["--silent", "--results", resultsFile]);

        // Act: run self-validation
        Validation.Run(context);

        // Assert: exit code is 0 and JUnit XML file was created with expected content
        Assert.Equal(0, context.ExitCode);
        Assert.True(File.Exists(resultsFile), $"Expected JUnit XML results file at {resultsFile}");
        var content = File.ReadAllText(resultsFile);
        Assert.Contains("testsuite", content);
    }

    /// <summary>
    /// Test that self-validation sets exit code 1 and reports errors when failures are encountered.
    /// </summary>
    [Fact]
    public void SelfTest_FailureReporting_WithErrors_SetsExitCode1()
    {
        // Arrange: create a results file path with an unsupported extension to trigger an error
        var resultsFile = _testDirectory.GetFilePath("validation-results.invalid");
        var logFile = _testDirectory.GetFilePath("failure-test.log");

        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile, "--results", resultsFile]))
        {
            // Act: run self-validation
            Validation.Run(context);
            exitCode = context.ExitCode;
        }

        // Assert: exit code is 1 and error output was written to the log (read after context disposed to flush log)
        Assert.Equal(1, exitCode);
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("Error:", logContent);
    }

    /// <summary>
    /// Test that self-validation sets exit code 1 when genuine internal test failures occur.
    /// Calls <see cref="Validation.ReportSummary"/> directly with a pre-built failed result,
    /// exercising the <c>failedTests &gt; 0</c> branch without manipulating the file system.
    /// </summary>
    [Fact]
    public void SelfTest_FailureReporting_GenuineFailure_SetsExitCode1()
    {
        // Arrange: build a TestResults with one failed outcome to exercise the failedTests > 0 branch
        var testResults = new DemaConsulting.TestResults.TestResults();
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "FakeTest",
            ClassName = "Test",
            Outcome = DemaConsulting.TestResults.TestOutcome.Failed
        });
        using var context = Context.Create(["--silent"]);

        // Act: call ReportSummary directly — no file-system or OS-permission manipulation needed
        Validation.ReportSummary(context, testResults);

        // Assert: exit code is 1 because failedTests > 0 triggered WriteError
        Assert.Equal(1, context.ExitCode);
    }
}
