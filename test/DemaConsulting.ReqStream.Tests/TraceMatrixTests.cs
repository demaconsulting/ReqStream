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

using DemaConsulting.TestResults;
using DemaConsulting.TestResults.IO;
using TestResult = DemaConsulting.TestResults.TestResult;

namespace DemaConsulting.ReqStream.Tests;

/// <summary>
///     Unit tests for TraceMatrix functionality.
/// </summary>
[TestClass]
public class TraceMatrixTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    ///     Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    ///     Clean up test by deleting the temporary test directory.
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
    ///     Test source-specific test matching with filepart@testname pattern.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithSourceSpecificTests_MatchesCorrectly()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var windowsPath = Path.Combine(_testDirectory, "test-results-windows-latest.trx");
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
        var linuxPath = Path.Combine(_testDirectory, "test-results-ubuntu-latest.trx");
        File.WriteAllText(linuxPath, TrxSerializer.Serialize(linuxResults));

        // Create TraceMatrix with both files
        var matrix = new TraceMatrix(requirements, windowsPath, linuxPath);

        // Verify Windows test is tracked separately
        var windowsResult = matrix.GetTestResult("windows-latest@Test_PlatformBasic");
        Assert.IsNotNull(windowsResult);
        Assert.AreEqual(1, windowsResult.Executed);
        Assert.AreEqual(1, windowsResult.Passes);

        // Verify Linux test is tracked separately
        var linuxResult = matrix.GetTestResult("ubuntu-latest@Test_PlatformBasic");
        Assert.IsNotNull(linuxResult);
        Assert.AreEqual(1, linuxResult.Executed);
        Assert.AreEqual(1, linuxResult.Passes);

        // Verify only 2 test results are tracked (not aggregated)
        var allResults = matrix.GetAllTestResults();
        Assert.HasCount(2, allResults);
    }

    /// <summary>
    ///     Test that source-specific test names only match their specified source.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithSourceSpecificTests_DoesNotMatchOtherSources()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var linuxPath = Path.Combine(_testDirectory, "test-results-ubuntu-latest.trx");
        File.WriteAllText(linuxPath, TrxSerializer.Serialize(linuxResults));

        // Create TraceMatrix with Linux file only
        var matrix = new TraceMatrix(requirements, linuxPath);

        // Verify Windows-specific test is not tracked from Linux file
        var result = matrix.GetTestResult("windows@Test_WindowsOnly");
        Assert.AreEqual(0, result.Executed);
    }

    /// <summary>
    ///     Test that plain test names match all sources.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithPlainTestNames_MatchesAllSources()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var windowsPath = Path.Combine(_testDirectory, "windows-results.trx");
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
        var linuxPath = Path.Combine(_testDirectory, "linux-results.trx");
        File.WriteAllText(linuxPath, TrxSerializer.Serialize(linuxResults));

        // Create TraceMatrix with both files
        var matrix = new TraceMatrix(requirements, windowsPath, linuxPath);

        // Verify test is aggregated from both sources
        var result = matrix.GetTestResult("Test_CrossPlatform");
        Assert.IsNotNull(result);
        Assert.AreEqual(2, result.Executed, "Should aggregate from both sources");
        Assert.AreEqual(2, result.Passes);
    }

    /// <summary>
    ///     Test mixed source-specific and plain test names in the same requirement.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithMixedTestNames_MatchesAppropriately()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var windowsPath = Path.Combine(_testDirectory, "test-windows.trx");
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
        var linuxPath = Path.Combine(_testDirectory, "test-linux.trx");
        File.WriteAllText(linuxPath, TrxSerializer.Serialize(linuxResults));

        // Create TraceMatrix with both files
        var matrix = new TraceMatrix(requirements, windowsPath, linuxPath);

        // Verify common test is aggregated
        var commonResult = matrix.GetTestResult("Test_Common");
        Assert.IsNotNull(commonResult);
        Assert.AreEqual(2, commonResult.Executed);
        Assert.AreEqual(2, commonResult.Passes);

        // Verify Windows-specific test
        var windowsResult = matrix.GetTestResult("windows@Test_WindowsSpecific");
        Assert.IsNotNull(windowsResult);
        Assert.AreEqual(1, windowsResult.Executed);
        Assert.AreEqual(1, windowsResult.Passes);

        // Verify Linux-specific test
        var linuxResult = matrix.GetTestResult("linux@Test_LinuxSpecific");
        Assert.IsNotNull(linuxResult);
        Assert.AreEqual(1, linuxResult.Executed);
        Assert.AreEqual(1, linuxResult.Passes);

        // Verify total number of tracked tests
        var allResults = matrix.GetAllTestResults();
        Assert.HasCount(3, allResults);
    }

    /// <summary>
    ///     Test case-insensitive matching of file parts.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithSourceSpecificTests_IsCaseInsensitive()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var testPath = Path.Combine(_testDirectory, "test-results-WINDOWS-latest.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Verify test is matched despite case difference
        var result = matrix.GetTestResult("windows@Test_CaseSensitive");
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Executed);
        Assert.AreEqual(1, result.Passes);
    }

    /// <summary>
    ///     Test partial file name matching (filepart can match anywhere in base name).
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithSourceSpecificTests_MatchesPartialFilename()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var testPath = Path.Combine(_testDirectory, "test-results-ubuntu-22.04-latest.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Verify test is matched with partial filename
        var result = matrix.GetTestResult("ubuntu@Test_Partial");
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Executed);
        Assert.AreEqual(1, result.Passes);
    }

    /// <summary>
    ///     Test that a single test result can match multiple source-specific requirement tests.
    ///     This occurs when a filename contains multiple matching source specifiers.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithMultipleSourceSpecifiers_MatchesAllRequirements()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var testPath = Path.Combine(_testDirectory, "integration-test-windows-latest-dotnet8.x.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Verify the test matches both source-specific requirements
        var windowsResult = matrix.GetTestResult("windows@Test_Platform");
        Assert.IsNotNull(windowsResult);
        Assert.AreEqual(1, windowsResult.Executed);
        Assert.AreEqual(1, windowsResult.Passes);

        var dotnet8Result = matrix.GetTestResult("dotnet8.x@Test_Platform");
        Assert.IsNotNull(dotnet8Result);
        Assert.AreEqual(1, dotnet8Result.Executed);
        Assert.AreEqual(1, dotnet8Result.Passes);

        // Verify both requirements would be satisfied
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.AreEqual(2, satisfied);
        Assert.AreEqual(2, total);
    }

    /// <summary>
    ///     Test that a test referenced in multiple requirements (some with file-filter, some without)
    ///     is correctly detected for all requirements.
    ///     This is a regression test for the issue where a test with source-specific format would
    ///     prevent matching the same test with plain format in the same file.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithMixedFilterAndPlainReferences_MatchesBoth()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var testPath = Path.Combine(_testDirectory, "test-results-windows-latest.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Verify the source-specific test is tracked
        var sourceSpecificResult = matrix.GetTestResult("windows@Test_SharedTest");
        Assert.IsNotNull(sourceSpecificResult, "Source-specific test should be tracked");
        Assert.AreEqual(1, sourceSpecificResult.Executed);
        Assert.AreEqual(1, sourceSpecificResult.Passes);

        // Verify the plain test is ALSO tracked (this is the bug - it won't be tracked)
        var plainResult = matrix.GetTestResult("Test_SharedTest");
        Assert.IsNotNull(plainResult, "Plain test name should also be tracked from the same file");
        Assert.AreEqual(1, plainResult.Executed);
        Assert.AreEqual(1, plainResult.Passes);

        // Verify both requirements are satisfied
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.AreEqual(2, satisfied, "Both requirements should be satisfied");
        Assert.AreEqual(2, total);
    }

    /// <summary>
    ///     Test that non-executed tests are ignored and don't affect execution counts.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithNotExecutedTests_IgnoresNonExecutedTests()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var testPath = Path.Combine(_testDirectory, "test-results.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Verify executed test is tracked
        var executedResult = matrix.GetTestResult("Test_ExecutedTest");
        Assert.IsNotNull(executedResult, "Executed test should be tracked");
        Assert.AreEqual(1, executedResult.Executed);
        Assert.AreEqual(1, executedResult.Passes);

        // Verify not-executed test is NOT tracked
        var notExecutedResult = matrix.GetTestResult("Test_NotExecutedTest");
        Assert.AreEqual(0, notExecutedResult.Executed, "Not-executed test should not be tracked");

        // Verify requirement is not satisfied (has a test reference without execution)
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.AreEqual(0, satisfied, "Requirement should not be satisfied when a referenced test is not executed");
        Assert.AreEqual(1, total);
    }

    /// <summary>
    ///     Test that requirements with only non-executed tests are treated as having no test coverage.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithOnlyNotExecutedTests_TreatsAsNoTests()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var testPath = Path.Combine(_testDirectory, "test-results.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Verify no tests are tracked
        var result1 = matrix.GetTestResult("Test_NotExecuted1");
        Assert.AreEqual(0, result1.Executed, "Not-executed test should not be tracked");

        var result2 = matrix.GetTestResult("Test_NotExecuted2");
        Assert.AreEqual(0, result2.Executed, "Not-executed test should not be tracked");

        // Verify requirement is not satisfied (has no executed tests)
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.AreEqual(0, satisfied, "Requirement should not be satisfied when all tests are not executed");
        Assert.AreEqual(1, total);
    }

    /// <summary>
    ///     Test that non-executed tests are properly handled in mixed outcome scenarios.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithMixedOutcomes_OnlyCountsExecutedTests()
    {
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

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
        var testPath = Path.Combine(_testDirectory, "test-results.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, testPath);

        // Verify passed test is tracked
        var passedResult = matrix.GetTestResult("Test_Passed");
        Assert.IsNotNull(passedResult);
        Assert.AreEqual(1, passedResult.Executed);
        Assert.AreEqual(1, passedResult.Passes);

        // Verify failed test is tracked
        var failedResult = matrix.GetTestResult("Test_Failed");
        Assert.IsNotNull(failedResult);
        Assert.AreEqual(1, failedResult.Executed);
        Assert.AreEqual(0, failedResult.Passes);

        // Verify not-executed test is NOT tracked
        var notExecutedResult = matrix.GetTestResult("Test_NotExecuted");
        Assert.AreEqual(0, notExecutedResult.Executed, "Not-executed test should not be tracked");

        // Verify requirement is not satisfied (has a failed test)
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.AreEqual(0, satisfied, "Requirement should not be satisfied when a test fails");
        Assert.AreEqual(1, total);
    }

    /// <summary>
    ///     Test that circular requirements (A -> B -> A) throw an InvalidOperationException.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithCircularRequirements_ThrowsInvalidOperationException()
    {
        // Create requirements with a circular reference: REQ-A -> REQ-B -> REQ-A
        var reqYaml = @"---
sections:
  - title: ""Cyclic Section""
    requirements:
      - id: ""REQ-A""
        title: ""Requirement A""
        children:
          - ""REQ-B""
      - id: ""REQ-B""
        title: ""Requirement B""
        children:
          - ""REQ-A""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create an empty TRX file
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        var testPath = Path.Combine(_testDirectory, "test-results.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        // Creating the TraceMatrix triggers satisfaction calculation which recurses into children
        var matrix = new TraceMatrix(requirements, testPath);

        // CalculateSatisfiedRequirements triggers the cycle detection
        Assert.ThrowsExactly<InvalidOperationException>(() => matrix.CalculateSatisfiedRequirements());
    }

    /// <summary>
    ///     Test that a self-referencing requirement (A -> A) throws an InvalidOperationException.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithSelfReferencingRequirement_ThrowsInvalidOperationException()
    {
        // Create a requirement that references itself: REQ-A -> REQ-A
        var reqYaml = @"---
sections:
  - title: ""Cyclic Section""
    requirements:
      - id: ""REQ-A""
        title: ""Requirement A""
        children:
          - ""REQ-A""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create an empty TRX file
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        var testPath = Path.Combine(_testDirectory, "test-results.trx");
        File.WriteAllText(testPath, TrxSerializer.Serialize(testResults));

        var matrix = new TraceMatrix(requirements, testPath);

        // CalculateSatisfiedRequirements triggers the cycle detection
        Assert.ThrowsExactly<InvalidOperationException>(() => matrix.CalculateSatisfiedRequirements());
    }
}
