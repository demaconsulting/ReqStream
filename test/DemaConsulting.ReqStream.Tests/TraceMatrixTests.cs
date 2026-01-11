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
        Assert.AreEqual(1, windowsResult.Passed);

        // Verify Linux test is tracked separately
        var linuxResult = matrix.GetTestResult("ubuntu-latest@Test_PlatformBasic");
        Assert.IsNotNull(linuxResult);
        Assert.AreEqual(1, linuxResult.Executed);
        Assert.AreEqual(1, linuxResult.Passed);

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
        Assert.IsNull(result);
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
        Assert.AreEqual(2, result.Passed);
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
        Assert.AreEqual(2, commonResult.Passed);

        // Verify Windows-specific test
        var windowsResult = matrix.GetTestResult("windows@Test_WindowsSpecific");
        Assert.IsNotNull(windowsResult);
        Assert.AreEqual(1, windowsResult.Executed);
        Assert.AreEqual(1, windowsResult.Passed);

        // Verify Linux-specific test
        var linuxResult = matrix.GetTestResult("linux@Test_LinuxSpecific");
        Assert.IsNotNull(linuxResult);
        Assert.AreEqual(1, linuxResult.Executed);
        Assert.AreEqual(1, linuxResult.Passed);

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
        Assert.AreEqual(1, result.Passed);
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
        Assert.AreEqual(1, result.Passed);
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
        Assert.AreEqual(1, windowsResult.Passed);

        var dotnet8Result = matrix.GetTestResult("dotnet8.x@Test_Platform");
        Assert.IsNotNull(dotnet8Result);
        Assert.AreEqual(1, dotnet8Result.Executed);
        Assert.AreEqual(1, dotnet8Result.Passed);

        // Verify both requirements would be satisfied
        var (satisfied, total) = matrix.CalculateSatisfiedRequirements();
        Assert.AreEqual(2, satisfied);
        Assert.AreEqual(2, total);
    }
}
