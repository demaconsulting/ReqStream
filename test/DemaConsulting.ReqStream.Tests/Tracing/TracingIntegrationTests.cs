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

namespace DemaConsulting.ReqStream.Tests.Tracing;

/// <summary>
/// Integration tests for the Tracing subsystem, testing test result loading, trace matrix
/// generation, and enforcement through the full tool executable.
/// </summary>
[TestClass]
public class TracingIntegrationTests
{
    private string _dllPath = string.Empty;
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by locating the DLL and creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _dllPath = Path.Combine(AppContext.BaseDirectory, "DemaConsulting.ReqStream.dll");
        Assert.IsTrue(File.Exists(_dllPath), $"Could not find ReqStream DLL at {_dllPath}");

        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_tracing_{Guid.NewGuid()}");
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
    /// Integration test verifying that a trace matrix Markdown file is generated correctly
    /// from requirements and TRX test results.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_TraceMatrix_GeneratesMarkdown()
    {
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Tracing Test Requirements
                requirements:
                  - id: Tracing-Test-Req1
                    title: The system shall be traced by tests.
                    justification: Tracing test justification.
                    tests:
                      - TracingTest1
            """);

        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TracingRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TracingTest1",
            ClassName = "TracingTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxFile = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var matrixFile = Path.Combine(_testDirectory, "matrix.md");
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "results.trx",
            "--matrix", matrixFile);

        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
        Assert.IsTrue(File.Exists(matrixFile), "Trace matrix report should be generated.");

        var content = File.ReadAllText(matrixFile);
        Assert.Contains("Tracing-Test-Req1", content);
    }

    /// <summary>
    /// Integration test verifying that enforcement mode passes when all requirements have
    /// passing test evidence.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_EnforcementMode_PassesWithTests()
    {
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Enforcement Test Requirements
                requirements:
                  - id: Tracing-Enforce-Req1
                    title: The system shall be verified by a passing test.
                    justification: Enforcement test justification.
                    tests:
                      - EnforcementTest1
            """);

        var testResults = new DemaConsulting.TestResults.TestResults { Name = "EnforcementRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "EnforcementTest1",
            ClassName = "EnforcementTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxFile = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "results.trx",
            "--enforce");

        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that enforcement mode fails when a requirement has no
    /// passing test evidence.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_EnforcementMode_FailsWithoutTests()
    {
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Enforcement Test Requirements
                requirements:
                  - id: Tracing-Enforce-Unsatisfied
                    title: The system shall have an unverified requirement.
                    justification: Enforcement test justification.
                    tests:
                      - MissingTest1
            """);

        var testResults = new DemaConsulting.TestResults.TestResults { Name = "EmptyRun" };
        var trxFile = Path.Combine(_testDirectory, "empty.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "empty.trx",
            "--enforce");

        Assert.AreNotEqual(0, exitCode, $"Expected non-zero exit code but got 0. Output: {output}");
    }
}
