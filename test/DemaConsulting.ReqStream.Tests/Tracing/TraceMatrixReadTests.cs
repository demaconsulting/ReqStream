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
using DemaConsulting.TestResults;
using DemaConsulting.TestResults.IO;
using TestResult = DemaConsulting.TestResults.TestResult;

namespace DemaConsulting.ReqStream.Tests.Tracing;

/// <summary>
///     Unit tests for TraceMatrix reading functionality.
/// </summary>
[TestClass]
public class TraceMatrixReadTests
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
    ///     Test TraceMatrix with a TRX test result file.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithTrxFile_ParsesCorrectly()
    {
        // Create requirements
        var reqYaml = @"---
sections:
  - title: ""User Authentication""
    requirements:
      - id: ""AUTH-001""
        title: ""Validate user credentials""
        tests:
          - ""Test_Credentials_Valid""
          - ""Test_Credentials_Invalid""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TRX file using the TestResults library
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Credentials_Valid",
            ClassName = "AuthTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Credentials_Invalid",
            ClassName = "AuthTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        // Verify results
        var result1 = matrix.GetTestResult("Test_Credentials_Valid");
        Assert.IsNotNull(result1);
        Assert.AreEqual(1, result1.Executed);
        Assert.AreEqual(1, result1.Passes);

        var result2 = matrix.GetTestResult("Test_Credentials_Invalid");
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result2.Executed);
        Assert.AreEqual(1, result2.Passes);
    }

    /// <summary>
    ///     Test TraceMatrix with multiple test result files (matrix testing scenario).
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithMultipleFiles_AggregatesResults()
    {
        // Create requirements
        var reqYaml = @"---
sections:
  - title: ""Cross-Platform Tests""
    requirements:
      - id: ""PLAT-001""
        title: ""Run on multiple platforms""
        tests:
          - ""Test_PlatformBasic""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create first TRX file (Windows, passed)
        var testResults1 = new TestResults.TestResults { Name = "WindowsRun" };
        testResults1.Results.Add(new TestResult
        {
            Name = "Test_PlatformBasic",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trx1Path = Path.Combine(_testDirectory, "windows-results.trx");
        File.WriteAllText(trx1Path, TrxSerializer.Serialize(testResults1));

        // Create second TRX file (Linux, passed)
        var testResults2 = new TestResults.TestResults { Name = "LinuxRun" };
        testResults2.Results.Add(new TestResult
        {
            Name = "Test_PlatformBasic",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trx2Path = Path.Combine(_testDirectory, "linux-results.trx");
        File.WriteAllText(trx2Path, TrxSerializer.Serialize(testResults2));

        // Create third TRX file (macOS, failed)
        var testResults3 = new TestResults.TestResults { Name = "MacOSRun" };
        testResults3.Results.Add(new TestResult
        {
            Name = "Test_PlatformBasic",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Failed,
            ErrorMessage = "Test failed on macOS",
            Duration = TimeSpan.FromSeconds(1)
        });
        var trx3Path = Path.Combine(_testDirectory, "macos-results.trx");
        File.WriteAllText(trx3Path, TrxSerializer.Serialize(testResults3));

        // Create TraceMatrix with all three files
        var matrix = new TraceMatrix(requirements, trx1Path, trx2Path, trx3Path);

        // Verify aggregated results
        var result = matrix.GetTestResult("Test_PlatformBasic");
        Assert.IsNotNull(result);
        Assert.AreEqual(3, result.Executed, "Test should have been executed 3 times");
        Assert.AreEqual(2, result.Passes, "Test should have passed 2 times");
    }

    /// <summary>
    ///     Test that extra tests (beyond those in requirements) are ignored.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithExtraTests_IgnoresUnreferencedTests()
    {
        // Create requirements with only one test
        var reqYaml = @"---
sections:
  - title: ""Security Tests""
    requirements:
      - id: ""SEC-001""
        title: ""Authentication required""
        tests:
          - ""Test_Auth_Valid""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TRX with multiple tests
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Auth_Valid",
            ClassName = "SecurityTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_ExtraNotInRequirements",
            ClassName = "SecurityTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_AnotherExtra",
            ClassName = "SecurityTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        // Verify only the referenced test is tracked
        var result1 = matrix.GetTestResult("Test_Auth_Valid");
        Assert.IsNotNull(result1);
        Assert.AreEqual(1, result1.Executed);
        Assert.AreEqual(1, result1.Passes);

        // Extra tests are now tracked (all tests captured)
        var result2 = matrix.GetTestResult("Test_ExtraNotInRequirements");
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result2.Executed);
        Assert.AreEqual(1, result2.Passes);

        var result3 = matrix.GetTestResult("Test_AnotherExtra");
        Assert.IsNotNull(result3);
        Assert.AreEqual(1, result3.Executed);
        Assert.AreEqual(1, result3.Passes);

        // GetAllTestResults only returns tests referenced in requirements
        var allResults = matrix.GetAllTestResults();
        Assert.HasCount(1, allResults);
        Assert.IsTrue(allResults.ContainsKey("Test_Auth_Valid"));
        Assert.IsFalse(allResults.ContainsKey("Test_ExtraNotInRequirements"));
        Assert.IsFalse(allResults.ContainsKey("Test_AnotherExtra"));
    }

    /// <summary>
    ///     Test that null requirements throws ArgumentNullException.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_NullRequirements_ThrowsArgumentNullException()
    {
        var ex = Assert.ThrowsExactly<ArgumentNullException>(() => _ = new TraceMatrix(null!, Array.Empty<string>()));
        Assert.Contains("requirements", ex.Message);
    }

    /// <summary>
    ///     Test that missing test result file throws FileNotFoundException.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_MissingFile_ThrowsFileNotFoundException()
    {
        // Create minimal requirements
        var reqYaml = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""TEST-001""
        title: ""Test requirement""
        tests:
          - ""SomeTest""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.trx");

        var ex = Assert.ThrowsExactly<FileNotFoundException>(() => _ = new TraceMatrix(requirements, nonExistentPath));
        Assert.Contains("Test result file not found", ex.Message);
    }

    /// <summary>
    ///     Test TraceMatrix with failed tests.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithFailedTests_TracksFailures()
    {
        // Create requirements
        var reqYaml = @"---
sections:
  - title: ""Failure Tests""
    requirements:
      - id: ""FAIL-001""
        title: ""Test failures""
        tests:
          - ""Test_Passing""
          - ""Test_Failing""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TRX with passed and failed tests
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Passing",
            ClassName = "FailureTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Failing",
            ClassName = "FailureTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Failed,
            ErrorMessage = "Assertion failed",
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        // Verify passing test
        var result1 = matrix.GetTestResult("Test_Passing");
        Assert.IsNotNull(result1);
        Assert.AreEqual(1, result1.Executed);
        Assert.AreEqual(1, result1.Passes);

        // Verify failing test
        var result2 = matrix.GetTestResult("Test_Failing");
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result2.Executed);
        Assert.AreEqual(0, result2.Passes, "Failed test should have 0 passes");
    }

    /// <summary>
    ///     Test TraceMatrix with no test result files.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithNoFiles_CreatesEmptyMatrix()
    {
        // Create requirements
        var reqYaml = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""TEST-001""
        title: ""Test requirement""
        tests:
          - ""SomeTest""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TraceMatrix with no files
        var matrix = new TraceMatrix(requirements);

        // Verify no results
        var allResults = matrix.GetAllTestResults();
        Assert.IsEmpty(allResults);

        var result = matrix.GetTestResult("SomeTest");
        Assert.AreEqual(0, result.Executed);
    }

    /// <summary>
    ///     Test TraceMatrix with a JUnit test result file.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithJUnitFile_ParsesCorrectly()
    {
        // Create requirements
        var reqYaml = @"---
sections:
  - title: ""Data Validation""
    requirements:
      - id: ""DATA-001""
        title: ""Validate input data""
        tests:
          - ""Test_ValidData""
          - ""Test_InvalidData""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create JUnit file using the TestResults library
        var testResults = new TestResults.TestResults { Name = "DataTests" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_ValidData",
            ClassName = "DataTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1.2)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_InvalidData",
            ClassName = "DataTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1.3)
        });

        var junitPath = Path.Combine(_testDirectory, "results.xml");
        File.WriteAllText(junitPath, JUnitSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, junitPath);

        // Verify results
        var result1 = matrix.GetTestResult("Test_ValidData");
        Assert.IsNotNull(result1);
        Assert.AreEqual(1, result1.Executed);
        Assert.AreEqual(1, result1.Passes);

        var result2 = matrix.GetTestResult("Test_InvalidData");
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result2.Executed);
        Assert.AreEqual(1, result2.Passes);
    }

    /// <summary>
    ///     Test TraceMatrix with mixed TRX and JUnit files.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithMixedFormats_ProcessesBoth()
    {
        // Create requirements
        var reqYaml = @"---
sections:
  - title: ""Mixed Format Tests""
    requirements:
      - id: ""MIX-001""
        title: ""Test with mixed formats""
        tests:
          - ""Test_TrxFormat""
          - ""Test_JUnitFormat""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TRX file
        var trxResults = new TestResults.TestResults { Name = "TrxRun" };
        trxResults.Results.Add(new TestResult
        {
            Name = "Test_TrxFormat",
            ClassName = "MixedTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(trxResults));

        // Create JUnit file
        var junitResults = new TestResults.TestResults { Name = "JUnitRun" };
        junitResults.Results.Add(new TestResult
        {
            Name = "Test_JUnitFormat",
            ClassName = "MixedTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1.5)
        });
        var junitPath = Path.Combine(_testDirectory, "results.xml");
        File.WriteAllText(junitPath, JUnitSerializer.Serialize(junitResults));

        // Create TraceMatrix with both files
        var matrix = new TraceMatrix(requirements, trxPath, junitPath);

        // Verify results from TRX
        var result1 = matrix.GetTestResult("Test_TrxFormat");
        Assert.IsNotNull(result1);
        Assert.AreEqual(1, result1.Executed);
        Assert.AreEqual(1, result1.Passes);

        // Verify results from JUnit
        var result2 = matrix.GetTestResult("Test_JUnitFormat");
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result2.Executed);
        Assert.AreEqual(1, result2.Passes);
    }

    /// <summary>
    ///     Test TraceMatrix with JUnit file containing failed tests.
    /// </summary>
    [TestMethod]
    public void TraceMatrix_WithJUnitFailedTests_TracksFailures()
    {
        // Create requirements
        var reqYaml = @"---
sections:
  - title: ""JUnit Failure Tests""
    requirements:
      - id: ""JUNIT-FAIL-001""
        title: ""Test JUnit failures""
        tests:
          - ""Test_JUnit_Passing""
          - ""Test_JUnit_Failing""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create JUnit file with passed and failed tests
        var testResults = new TestResults.TestResults { Name = "JUnitTestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_JUnit_Passing",
            ClassName = "JUnitFailureTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_JUnit_Failing",
            ClassName = "JUnitFailureTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Failed,
            ErrorMessage = "Assertion failed in JUnit",
            Duration = TimeSpan.FromSeconds(1)
        });

        var junitPath = Path.Combine(_testDirectory, "results.xml");
        File.WriteAllText(junitPath, JUnitSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, junitPath);

        // Verify passing test
        var result1 = matrix.GetTestResult("Test_JUnit_Passing");
        Assert.IsNotNull(result1);
        Assert.AreEqual(1, result1.Executed);
        Assert.AreEqual(1, result1.Passes);

        // Verify failing test
        var result2 = matrix.GetTestResult("Test_JUnit_Failing");
        Assert.IsNotNull(result2);
        Assert.AreEqual(1, result2.Executed);
        Assert.AreEqual(0, result2.Passes, "Failed test should have 0 passes");
    }
}
