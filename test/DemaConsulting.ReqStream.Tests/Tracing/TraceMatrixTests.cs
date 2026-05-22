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
///     Unit tests for TraceMatrix functionality.
/// </summary>
public sealed class TraceMatrixTests : IDisposable
{
    /// <summary>Unique temporary directory for this test instance's fixture files.</summary>
    private readonly string _testDirectory;

    /// <summary>
    ///     Initialize test by creating a temporary test directory.
    /// </summary>
    public TraceMatrixTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reqstream_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    ///     Clean up test by deleting the temporary test directory.
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
    ///     Test source-specific test matching with filepart@testname pattern.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesCorrectly()
    {
        // Arrange:
        // Create requirements with source-specific test names
        var reqYaml = @"---
sections:
  - title: ""Platform Support""
    requirements:
      - id: ""PLAT-001""
        title: ""Shall support Windows""
        tests:
          - ""windows-latest@Test_PlatformBasic""
      - id: ""PLAT-002""
        title: ""Shall support Linux""
        tests:
          - ""ubuntu-latest@Test_PlatformBasic""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create Windows test results
        var windowsResults = new TestResults.TestResults { Name = "WindowsRun" };
        windowsResults.Results.Add(new TestResult
        {
            Name = "Test_PlatformBasic",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var windowsPath = PathHelpers.SafePathCombine(_testDirectory, "test-results-windows-latest.trx");
        File.WriteAllText(windowsPath, TrxSerializer.Serialize(windowsResults));

        // Create Linux test results
        var linuxResults = new TestResults.TestResults { Name = "LinuxRun" };
        linuxResults.Results.Add(new TestResult
        {
            Name = "Test_PlatformBasic",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var linuxPath = PathHelpers.SafePathCombine(_testDirectory, "test-results-ubuntu-latest.trx");
        File.WriteAllText(linuxPath, TrxSerializer.Serialize(linuxResults));

        // Act:
        // Create TraceMatrix with both files
        var matrix = new TraceMatrix(requirements, windowsPath, linuxPath);

        // Assert:
        // Verify Windows test is tracked separately
        var windowsResult = matrix.GetTestResult("windows-latest@Test_PlatformBasic");
        Assert.NotNull(windowsResult);
        Assert.Equal(1, windowsResult.Executed);
        Assert.Equal(1, windowsResult.Passes);

        // Verify Linux test is tracked separately
        var linuxResult = matrix.GetTestResult("ubuntu-latest@Test_PlatformBasic");
        Assert.NotNull(linuxResult);
        Assert.Equal(1, linuxResult.Executed);
        Assert.Equal(1, linuxResult.Passes);

        // Verify only 2 test results are tracked (not aggregated)
        var allResults = matrix.GetAllTestResults();
        Assert.Equal(2, allResults.Count);
    }

    /// <summary>
    ///     Test that source-specific test names only match their specified source.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithSourceSpecificTests_DoesNotMatchOtherSources()
    {
        // Arrange:
        // Create requirements with Windows-specific test name
        var reqYaml = @"---
sections:
  - title: ""Windows Only""
    requirements:
      - id: ""WIN-001""
        title: ""Windows specific test""
        tests:
          - ""windows@Test_WindowsOnly""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create Linux test results (should not match)
        var linuxResults = new TestResults.TestResults { Name = "LinuxRun" };
        linuxResults.Results.Add(new TestResult
        {
            Name = "Test_WindowsOnly",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var linuxPath = PathHelpers.SafePathCombine(_testDirectory, "test-results-ubuntu-latest.trx");
        File.WriteAllText(linuxPath, TrxSerializer.Serialize(linuxResults));

        // Act:
        // Create TraceMatrix with Linux file only
        var matrix = new TraceMatrix(requirements, linuxPath);

        // Assert:
        // Verify Windows-specific test is not tracked from Linux file
        var result = matrix.GetTestResult("windows@Test_WindowsOnly");
        Assert.Equal(0, result.Executed);
    }

    /// <summary>
    ///     Test that plain test names match all sources.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithPlainTestNames_MatchesAllSources()
    {
        // Arrange:
        // Create requirements with plain test name
        var reqYaml = @"---
sections:
  - title: ""Cross Platform""
    requirements:
      - id: ""CP-001""
        title: ""Works on all platforms""
        tests:
          - ""Test_CrossPlatform""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create Windows test results
        var windowsResults = new TestResults.TestResults { Name = "WindowsRun" };
        windowsResults.Results.Add(new TestResult
        {
            Name = "Test_CrossPlatform",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var windowsPath = PathHelpers.SafePathCombine(_testDirectory, "windows-results.trx");
        File.WriteAllText(windowsPath, TrxSerializer.Serialize(windowsResults));

        // Create Linux test results
        var linuxResults = new TestResults.TestResults { Name = "LinuxRun" };
        linuxResults.Results.Add(new TestResult
        {
            Name = "Test_CrossPlatform",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var linuxPath = PathHelpers.SafePathCombine(_testDirectory, "linux-results.trx");
        File.WriteAllText(linuxPath, TrxSerializer.Serialize(linuxResults));

        // Act:
        // Create TraceMatrix with both files
        var matrix = new TraceMatrix(requirements, windowsPath, linuxPath);

        // Assert:
        // Verify test is aggregated from both sources
        var result = matrix.GetTestResult("Test_CrossPlatform");
        Assert.NotNull(result);
        Assert.Equal(2, result.Executed);
        Assert.Equal(2, result.Passes);
    }

    /// <summary>
    ///     Test mixed source-specific and plain test names in the same requirement.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithMixedTestNames_MatchesAppropriately()
    {
        // Arrange:
        // Create requirements with both plain and source-specific test names
        var reqYaml = @"---
sections:
  - title: ""Mixed Tests""
    requirements:
      - id: ""MIX-001""
        title: ""Has both types of tests""
        tests:
          - ""Test_Common""
          - ""windows@Test_WindowsSpecific""
          - ""linux@Test_LinuxSpecific""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create Windows test results
        var windowsResults = new TestResults.TestResults { Name = "WindowsRun" };
        windowsResults.Results.Add(new TestResult
        {
            Name = "Test_Common",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        windowsResults.Results.Add(new TestResult
        {
            Name = "Test_WindowsSpecific",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var windowsPath = PathHelpers.SafePathCombine(_testDirectory, "test-windows.trx");
        File.WriteAllText(windowsPath, TrxSerializer.Serialize(windowsResults));

        // Create Linux test results
        var linuxResults = new TestResults.TestResults { Name = "LinuxRun" };
        linuxResults.Results.Add(new TestResult
        {
            Name = "Test_Common",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        linuxResults.Results.Add(new TestResult
        {
            Name = "Test_LinuxSpecific",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var linuxPath = PathHelpers.SafePathCombine(_testDirectory, "test-linux.trx");
        File.WriteAllText(linuxPath, TrxSerializer.Serialize(linuxResults));

        // Act:
        // Create TraceMatrix with both files
        var matrix = new TraceMatrix(requirements, windowsPath, linuxPath);

        // Assert:
        // Verify common test is aggregated
        var commonResult = matrix.GetTestResult("Test_Common");
        Assert.NotNull(commonResult);
        Assert.Equal(2, commonResult.Executed);
        Assert.Equal(2, commonResult.Passes);

        // Verify Windows-specific test
        var windowsResult = matrix.GetTestResult("windows@Test_WindowsSpecific");
        Assert.NotNull(windowsResult);
        Assert.Equal(1, windowsResult.Executed);
        Assert.Equal(1, windowsResult.Passes);

        // Verify Linux-specific test
        var linuxResult = matrix.GetTestResult("linux@Test_LinuxSpecific");
        Assert.NotNull(linuxResult);
        Assert.Equal(1, linuxResult.Executed);
        Assert.Equal(1, linuxResult.Passes);

        // Verify total number of tracked tests
        var allResults = matrix.GetAllTestResults();
        Assert.Equal(3, allResults.Count);
    }

    /// <summary>
    ///     Test case-insensitive matching of file parts.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithSourceSpecificTests_IsCaseInsensitive()
    {
        // Arrange:
        // Create requirements with lowercase file part
        var reqYaml = @"---
sections:
  - title: ""Case Test""
    requirements:
      - id: ""CASE-001""
        title: ""Case insensitive file matching""
        tests:
          - ""windows@Test_CaseSensitive""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create test results with uppercase WINDOWS in filename
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_CaseSensitive",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var testPath = PathHelpers.SafePathCombine(_testDirectory, "test-results-WINDOWS-latest.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Assert:
        // Verify test is matched despite case difference
        var result = matrix.GetTestResult("windows@Test_CaseSensitive");
        Assert.NotNull(result);
        Assert.Equal(1, result.Executed);
        Assert.Equal(1, result.Passes);
    }

    /// <summary>
    ///     Test partial file name matching (filepart can match anywhere in base name).
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithSourceSpecificTests_MatchesPartialFilename()
    {
        // Arrange:
        // Create requirements with partial file name
        var reqYaml = @"---
sections:
  - title: ""Partial Match Test""
    requirements:
      - id: ""PART-001""
        title: ""Partial filename matching""
        tests:
          - ""ubuntu@Test_Partial""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create test results with full filename containing ubuntu
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Partial",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var testPath = PathHelpers.SafePathCombine(_testDirectory, "test-results-ubuntu-22.04-latest.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Assert:
        // Verify test is matched with partial filename
        var result = matrix.GetTestResult("ubuntu@Test_Partial");
        Assert.NotNull(result);
        Assert.Equal(1, result.Executed);
        Assert.Equal(1, result.Passes);
    }

    /// <summary>
    ///     Test that a single test result can match multiple source-specific requirement tests.
    ///     This occurs when a filename contains multiple matching source specifiers.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithMultipleSourceSpecifiers_MatchesAllRequirements()
    {
        // Arrange:
        // Create requirements with multiple source-specific tests for the same test name
        var reqYaml = @"---
sections:
  - title: ""Multiple Source Specifiers Test""
    requirements:
      - id: ""MULTI-001""
        title: ""Windows platform support""
        tests:
          - ""windows@Test_Platform""
      - id: ""MULTI-002""
        title: ""dotnet8 runtime support""
        tests:
          - ""dotnet8.x@Test_Platform""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create test results with filename containing both windows and dotnet8
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Platform",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var testPath = PathHelpers.SafePathCombine(_testDirectory, "integration-test-windows-latest-dotnet8.x.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Assert:
        // Verify the test matches both source-specific requirements
        var windowsResult = matrix.GetTestResult("windows@Test_Platform");
        Assert.NotNull(windowsResult);
        Assert.Equal(1, windowsResult.Executed);
        Assert.Equal(1, windowsResult.Passes);

        var dotnet8Result = matrix.GetTestResult("dotnet8.x@Test_Platform");
        Assert.NotNull(dotnet8Result);
        Assert.Equal(1, dotnet8Result.Executed);
        Assert.Equal(1, dotnet8Result.Passes);

        // Verify both requirements would be satisfied
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.Equal(2, satisfied);
        Assert.Equal(2, total);
    }

    /// <summary>
    ///     Test that a test referenced in multiple requirements (some with file-filter, some without)
    ///     is correctly detected for all requirements.
    ///     This is a regression test for the issue where a test with source-specific format would
    ///     prevent matching the same test with plain format in the same file.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithMixedFilterAndPlainReferences_MatchesBoth()
    {
        // Arrange:
        // Create requirements where the same test is referenced with and without file filter
        var reqYaml = @"---
sections:
  - title: ""Mixed References Test""
    requirements:
      - id: ""REQ-001""
        title: ""Platform-specific requirement""
        tests:
          - ""windows@Test_SharedTest""
      - id: ""REQ-002""
        title: ""General requirement (no filter)""
        tests:
          - ""Test_SharedTest""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create a Windows test result file containing the shared test
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_SharedTest",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var testPath = PathHelpers.SafePathCombine(_testDirectory, "test-results-windows-latest.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Assert:
        // Verify the source-specific test is tracked
        var sourceSpecificResult = matrix.GetTestResult("windows@Test_SharedTest");
        Assert.NotNull(sourceSpecificResult);
        Assert.Equal(1, sourceSpecificResult.Executed);
        Assert.Equal(1, sourceSpecificResult.Passes);

        // Verify the plain test is ALSO tracked (regression: was not tracked before fix)
        var plainResult = matrix.GetTestResult("Test_SharedTest");
        Assert.NotNull(plainResult);
        Assert.Equal(1, plainResult.Executed);
        Assert.Equal(1, plainResult.Passes);

        // Verify both requirements are satisfied
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.Equal(2, satisfied);
        Assert.Equal(2, total);
    }

    /// <summary>
    ///     Test that non-executed tests are ignored and don't affect execution counts.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithNotExecutedTests_IgnoresNonExecutedTests()
    {
        // Arrange:
        // Create requirements with test references
        var reqYaml = @"---
sections:
  - title: ""Test Requirements""
    requirements:
      - id: ""REQ-001""
        title: ""Test requirement""
        tests:
          - ""Test_ExecutedTest""
          - ""Test_NotExecutedTest""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create test results with one executed and one not-executed test
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_ExecutedTest",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_NotExecutedTest",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.NotExecuted,
            Duration = TimeSpan.Zero
        });
        var testPath = PathHelpers.SafePathCombine(_testDirectory, "test-results.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Assert:
        // Verify executed test is tracked
        var executedResult = matrix.GetTestResult("Test_ExecutedTest");
        Assert.NotNull(executedResult);
        Assert.Equal(1, executedResult.Executed);
        Assert.Equal(1, executedResult.Passes);

        // Verify not-executed test is NOT tracked
        var notExecutedResult = matrix.GetTestResult("Test_NotExecutedTest");
        Assert.Equal(0, notExecutedResult.Executed);

        // Verify requirement is not satisfied (has a test reference without execution)
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.Equal(0, satisfied);
        Assert.Equal(1, total);
    }

    /// <summary>
    ///     Test that requirements with only non-executed tests are treated as having no test coverage.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithOnlyNotExecutedTests_TreatsAsNoTests()
    {
        // Arrange:
        // Create requirements with test references
        var reqYaml = @"---
sections:
  - title: ""Test Requirements""
    requirements:
      - id: ""REQ-001""
        title: ""Requirement with only not-executed tests""
        tests:
          - ""Test_NotExecuted1""
          - ""Test_NotExecuted2""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create test results with only not-executed tests
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_NotExecuted1",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.NotExecuted,
            Duration = TimeSpan.Zero
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_NotExecuted2",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.NotExecuted,
            Duration = TimeSpan.Zero
        });
        var testPath = PathHelpers.SafePathCombine(_testDirectory, "test-results.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Assert:
        // Verify no tests are tracked
        var result1 = matrix.GetTestResult("Test_NotExecuted1");
        Assert.Equal(0, result1.Executed);

        var result2 = matrix.GetTestResult("Test_NotExecuted2");
        Assert.Equal(0, result2.Executed);

        // Verify requirement is not satisfied (has no executed tests)
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.Equal(0, satisfied);
        Assert.Equal(1, total);
    }

    /// <summary>
    ///     Test that non-executed tests are properly handled in mixed outcome scenarios.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetTestResult_WithMixedOutcomes_OnlyCountsExecutedTests()
    {
        // Arrange:
        // Create requirements with test references
        var reqYaml = @"---
sections:
  - title: ""Test Requirements""
    requirements:
      - id: ""REQ-001""
        title: ""Test requirement with mixed outcomes""
        tests:
          - ""Test_Passed""
          - ""Test_Failed""
          - ""Test_NotExecuted""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create test results with passed, failed, and not-executed tests
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Passed",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Failed",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Failed,
            Duration = TimeSpan.FromSeconds(1),
            ErrorMessage = "Test failed"
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_NotExecuted",
            ClassName = "Tests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.NotExecuted,
            Duration = TimeSpan.Zero
        });
        var testPath = PathHelpers.SafePathCombine(_testDirectory, "test-results.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Assert:
        // Verify passed test is tracked
        var passedResult = matrix.GetTestResult("Test_Passed");
        Assert.NotNull(passedResult);
        Assert.Equal(1, passedResult.Executed);
        Assert.Equal(1, passedResult.Passes);

        // Verify failed test is tracked
        var failedResult = matrix.GetTestResult("Test_Failed");
        Assert.NotNull(failedResult);
        Assert.Equal(1, failedResult.Executed);
        Assert.Equal(0, failedResult.Passes);

        // Verify not-executed test is NOT tracked
        var notExecutedResult = matrix.GetTestResult("Test_NotExecuted");
        Assert.Equal(0, notExecutedResult.Executed);

        // Verify requirement is not satisfied (has a failed test)
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.Equal(0, satisfied);
        Assert.Equal(1, total);
    }
}
