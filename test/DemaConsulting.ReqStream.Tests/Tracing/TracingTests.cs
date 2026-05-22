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

using DemaConsulting.ReqStream.Modeling;
using DemaConsulting.ReqStream.Tracing;
using DemaConsulting.ReqStream.Utilities;
using DemaConsulting.TestResults;
using DemaConsulting.TestResults.IO;
using TestResult = DemaConsulting.TestResults.TestResult;

namespace DemaConsulting.ReqStream.Tests.Tracing;

/// <summary>
/// Tests for the Tracing subsystem, proving the TraceMatrix class is sufficient to
/// implement the Tracing subsystem requirements.
/// </summary>
public sealed class TracingTests : IDisposable
{
    /// <summary>Absolute path to the per-test temporary directory created in the constructor and deleted in Dispose.</summary>
    private readonly string _testDirectory;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public TracingTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reqstream_tracing_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Clean up test by deleting the temporary test directory.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Test that a TRX results file is loaded and its test results are accessible via the trace matrix.
    /// </summary>
    [Fact]
    public void Tracing_TestResults_TrxFile_LoadsTestResults()
    {
        // Arrange: create a requirements file with one traceable requirement
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
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
        var loadResult = Requirements.Load(reqFile);
        Assert.NotNull(loadResult.Requirements);

        // Arrange: create a TRX file with a passing test result
        var testResults = new TestResults.TestResults { Name = "TracingRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "TracingTest1",
            ClassName = "TracingTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxFile = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxFile, TrxSerializer.Serialize(testResults));

        // Act: create a trace matrix loading the TRX file
        var matrix = new TraceMatrix(loadResult.Requirements, trxFile);

        // Assert: the test result was loaded with one pass and zero fails
        var result = matrix.GetTestResult("TracingTest1");
        Assert.Equal(1, result.Passes);
        Assert.Equal(0, result.Fails);
    }

    /// <summary>
    /// Test that a JUnit XML results file is loaded and its test results are accessible via the trace matrix.
    /// </summary>
    [Fact]
    public void Tracing_TestResults_JUnitFile_LoadsTestResults()
    {
        // Arrange: create a requirements file with one traceable requirement
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Tracing Test Requirements
                requirements:
                  - id: Tracing-Test-Req2
                    title: The system shall be traced by JUnit tests.
                    justification: Tracing JUnit test justification.
                    tests:
                      - TracingJUnitTest1
            """);
        var loadResult = Requirements.Load(reqFile);
        Assert.NotNull(loadResult.Requirements);

        // Arrange: create a JUnit XML file with a passing test result
        var testResults = new TestResults.TestResults { Name = "TracingJUnitRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "TracingJUnitTest1",
            ClassName = "TracingTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var junitFile = PathHelpers.SafePathCombine(_testDirectory, "results.xml");
        File.WriteAllText(junitFile, JUnitSerializer.Serialize(testResults));

        // Act: create a trace matrix loading the JUnit XML file
        var matrix = new TraceMatrix(loadResult.Requirements, junitFile);

        // Assert: the test result was loaded with one pass and zero fails
        var result = matrix.GetTestResult("TracingJUnitTest1");
        Assert.Equal(1, result.Passes);
        Assert.Equal(0, result.Fails);
    }

    /// <summary>
    /// Test that all requirements are satisfied when every required test has a passing result.
    /// </summary>
    [Fact]
    public void Tracing_Coverage_WithPassingTests_AllRequirementsSatisfied()
    {
        // Arrange: create a requirements file with one requirement to be satisfied
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
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
        var loadResult = Requirements.Load(reqFile);
        Assert.NotNull(loadResult.Requirements);

        // Arrange: create a TRX file with a passing test result matching the requirement
        var testResults = new TestResults.TestResults { Name = "EnforcementRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "EnforcementTest1",
            ClassName = "EnforcementTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxFile = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxFile, TrxSerializer.Serialize(testResults));

        // Act: build the trace matrix and check unsatisfied requirements
        var matrix = new TraceMatrix(loadResult.Requirements, trxFile);
        var unsatisfied = matrix.GetUnsatisfiedRequirements();

        // Assert: no unsatisfied requirements
        Assert.Empty(unsatisfied);
    }

    /// <summary>
    /// Test that a requirement is unsatisfied when its required test has no matching result.
    /// </summary>
    [Fact]
    public void Tracing_Coverage_WithMissingTests_RequirementIsUnsatisfied()
    {
        // Arrange: create a requirements file with one requirement whose test will not be present
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
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
        var loadResult = Requirements.Load(reqFile);
        Assert.NotNull(loadResult.Requirements);

        // Arrange: create a TRX file with no test results (empty run)
        var testResults = new TestResults.TestResults { Name = "EmptyRun" };
        var trxFile = PathHelpers.SafePathCombine(_testDirectory, "empty.trx");
        File.WriteAllText(trxFile, TrxSerializer.Serialize(testResults));

        // Act: build the trace matrix and check unsatisfied requirements
        var matrix = new TraceMatrix(loadResult.Requirements, trxFile);
        var unsatisfied = matrix.GetUnsatisfiedRequirements();

        // Assert: the requirement is listed as unsatisfied
        Assert.Single(unsatisfied);
        Assert.Contains("Tracing-Enforce-Unsatisfied", unsatisfied);
    }

    /// <summary>
    /// Test that constructing a TraceMatrix with a non-existent file path throws FileNotFoundException.
    /// </summary>
    [Fact]
    public void Tracing_FileLoading_NonExistentFile_ThrowsFileNotFoundException()
    {
        // Arrange: create a requirements object and a path to a file that does not exist
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Error Test Requirements
                requirements:
                  - id: Tracing-Error-Req1
                    title: The system shall handle missing result files.
                    justification: Error handling test justification.
                    tests:
                      - ErrorTest1
            """);
        var loadResult = Requirements.Load(reqFile);
        Assert.NotNull(loadResult.Requirements);
        var missingFile = PathHelpers.SafePathCombine(_testDirectory, "does-not-exist.trx");

        // Act and Assert: constructing a TraceMatrix with a missing file throws FileNotFoundException
        Assert.Throws<FileNotFoundException>(() =>
            _ = new TraceMatrix(loadResult.Requirements, missingFile));
    }

    /// <summary>
    /// Test that constructing a TraceMatrix with a malformed result file throws InvalidOperationException
    /// containing the offending file path in the message.
    /// </summary>
    [Fact]
    public void Tracing_FileLoading_MalformedFile_ThrowsInvalidOperationException()
    {
        // Arrange: create a requirements object and a file with invalid (non-XML) content
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Error Test Requirements
                requirements:
                  - id: Tracing-Error-Req2
                    title: The system shall handle malformed result files.
                    justification: Error handling test justification.
                    tests:
                      - ErrorTest2
            """);
        var loadResult = Requirements.Load(reqFile);
        Assert.NotNull(loadResult.Requirements);
        var malformedFile = PathHelpers.SafePathCombine(_testDirectory, "malformed.trx");
        File.WriteAllText(malformedFile, "this is not valid xml or json content @@##!!");

        // Act and Assert: constructing a TraceMatrix with a malformed file throws InvalidOperationException
        // with the offending path in the message
        var ex = Assert.Throws<InvalidOperationException>(() =>
            _ = new TraceMatrix(loadResult.Requirements, malformedFile));
        Assert.Contains(malformedFile, ex.Message);
    }

    /// <summary>
    /// Test that the Tracing subsystem exports a trace matrix report to a Markdown file.
    /// </summary>
    [Fact]
    public void Tracing_Reporting_SimpleMatrix_CreatesMarkdownFile()
    {
        // Arrange: create a requirements file with one traceable requirement
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Reporting Test Requirements
                requirements:
                  - id: Tracing-Report-Req1
                    title: The system shall be verified by a passing test.
                    justification: Reporting test justification.
                    tests:
                      - Tracing_Reporting_Test1
            """);
        var loadResult = Requirements.Load(reqFile);
        Assert.NotNull(loadResult.Requirements);

        // Arrange: create a TRX file with a passing test result
        var testResults = new TestResults.TestResults { Name = "ReportingRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Tracing_Reporting_Test1",
            ClassName = "TracingTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxFile = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxFile, TrxSerializer.Serialize(testResults));

        // Act: build the trace matrix and export the report
        var matrix = new TraceMatrix(loadResult.Requirements, trxFile);
        var mdFile = PathHelpers.SafePathCombine(_testDirectory, "trace-matrix.md");
        matrix.Export(mdFile);

        // Assert: the Markdown report file exists and contains required sections
        Assert.True(File.Exists(mdFile));
        var content = File.ReadAllText(mdFile);
        Assert.Contains("# Summary", content);
        Assert.Contains("1 of 1 requirements are satisfied with tests.", content);
        Assert.Contains("# Requirements", content);
        Assert.Contains("# Testing", content);
    }
}
