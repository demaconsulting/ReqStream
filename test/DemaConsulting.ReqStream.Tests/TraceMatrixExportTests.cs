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
///     Unit tests for TraceMatrix Markdown export functionality.
/// </summary>
[TestClass]
public class TraceMatrixExportTests
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
    ///     Test exporting a simple trace matrix to Markdown.
    /// </summary>
    [TestMethod]
    public void Export_SimpleTraceMatrix_CreatesMarkdownFile()
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

        // Create TRX file
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

        var mdPath = Path.Combine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        Assert.IsTrue(File.Exists(mdPath));
        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "# Summary");
        StringAssert.Contains(content, "1 of 1 requirements are satisfied with tests.");
        StringAssert.Contains(content, "# Requirements");
        StringAssert.Contains(content, "## User Authentication");
        StringAssert.Contains(content, "| ID | Tests Linked | Passed | Failed | Not Executed |");
        StringAssert.Contains(content, "| AUTH-001 | 2 | 2 | 0 | 0 |");
        StringAssert.Contains(content, "# Testing");
        StringAssert.Contains(content, "| Test | Requirement | Passed | Failed |");
        StringAssert.Contains(content, "| Test_Credentials_Invalid | AUTH-001 | 1 | 0 |");
        StringAssert.Contains(content, "| Test_Credentials_Valid | AUTH-001 | 1 | 0 |");
    }

    /// <summary>
    ///     Test exporting trace matrix with custom depth.
    /// </summary>
    [TestMethod]
    public void Export_WithCustomDepth_UsesCorrectHeaderLevel()
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
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TRX file
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Credentials_Valid",
            ClassName = "AuthTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = Path.Combine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath, depth: 2);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "## Summary");
        StringAssert.Contains(content, "## Requirements");
        StringAssert.Contains(content, "### User Authentication");
        StringAssert.Contains(content, "## Testing");
    }

    /// <summary>
    ///     Test exporting trace matrix with failed tests.
    /// </summary>
    [TestMethod]
    public void Export_WithFailedTests_ShowsFailures()
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

        // Create TRX file with one failure
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
            Outcome = TestOutcome.Failed,
            ErrorMessage = "Test failed",
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = Path.Combine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "0 of 1 requirements are satisfied with tests.");
        StringAssert.Contains(content, "| AUTH-001 | 2 | 1 | 1 | 0 |");
        StringAssert.Contains(content, "| Test_Credentials_Invalid | AUTH-001 | 0 | 1 |");
        StringAssert.Contains(content, "| Test_Credentials_Valid | AUTH-001 | 1 | 0 |");
    }

    /// <summary>
    ///     Test exporting trace matrix with not executed tests.
    /// </summary>
    [TestMethod]
    public void Export_WithNotExecutedTests_ShowsNotExecuted()
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

        // Create TRX file with only one test
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Credentials_Valid",
            ClassName = "AuthTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = Path.Combine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "0 of 1 requirements are satisfied with tests.");
        StringAssert.Contains(content, "| AUTH-001 | 2 | 1 | 0 | 1 |");
        StringAssert.Contains(content, "| Test_Credentials_Invalid | AUTH-001 | 0 | 0 |");
    }

    /// <summary>
    ///     Test exporting trace matrix with nested sections.
    /// </summary>
    [TestMethod]
    public void Export_WithNestedSections_CreatesHierarchy()
    {
        // Create requirements
        var reqYaml = @"---
sections:
  - title: ""Data Management""
    sections:
      - title: ""User Authentication""
        requirements:
          - id: ""AUTH-001""
            title: ""Validate user credentials""
            tests:
              - ""Test_Auth""
      - title: ""Logging""
        requirements:
          - id: ""LOG-001""
            title: ""Log all requests""
            tests:
              - ""Test_Logging""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TRX file
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Auth",
            ClassName = "AuthTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Logging",
            ClassName = "LogTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = Path.Combine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "2 of 2 requirements are satisfied with tests.");
        StringAssert.Contains(content, "## Data Management");
        StringAssert.Contains(content, "### User Authentication");
        StringAssert.Contains(content, "### Logging");
        StringAssert.Contains(content, "| AUTH-001 | 1 | 1 | 0 | 0 |");
        StringAssert.Contains(content, "| LOG-001 | 1 | 1 | 0 | 0 |");
    }

    /// <summary>
    ///     Test that export throws exception when file path is null.
    /// </summary>
    [TestMethod]
    public void Export_NullFilePath_ThrowsArgumentException()
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
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements);

        try
        {
            matrix.Export(null!);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException ex)
        {
            StringAssert.Contains(ex.Message, "File path cannot be null or empty");
        }
    }

    /// <summary>
    ///     Test that export throws exception when file path is empty.
    /// </summary>
    [TestMethod]
    public void Export_EmptyFilePath_ThrowsArgumentException()
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
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements);

        try
        {
            matrix.Export(string.Empty);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException ex)
        {
            StringAssert.Contains(ex.Message, "File path cannot be null or empty");
        }
    }

    /// <summary>
    ///     Test exporting trace matrix with requirements that have child requirements.
    /// </summary>
    [TestMethod]
    public void Export_WithChildRequirements_ConsidersChildTests()
    {
        // Create requirements with children
        var reqYaml = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
        children:
          - ""AUTH-001""
  - title: ""User Authentication""
    requirements:
      - id: ""AUTH-001""
        title: ""Validate user credentials""
        tests:
          - ""Test_Credentials_Valid""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TRX file
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Credentials_Valid",
            ClassName = "AuthTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = Path.Combine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        // Both requirements should be satisfied because SYS-SEC-001 has child AUTH-001 which has passing tests
        StringAssert.Contains(content, "2 of 2 requirements are satisfied with tests.");
        StringAssert.Contains(content, "| SYS-SEC-001 | 0 | 0 | 0 | 0 |");
        StringAssert.Contains(content, "| AUTH-001 | 1 | 1 | 0 | 0 |");
    }

    /// <summary>
    ///     Test exporting trace matrix with requirements that have no tests.
    /// </summary>
    [TestMethod]
    public void Export_WithNoTests_ShowsNotSatisfied()
    {
        // Create requirements with no tests
        var reqYaml = @"---
sections:
  - title: ""User Authentication""
    requirements:
      - id: ""AUTH-001""
        title: ""Validate user credentials""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TraceMatrix with no test results
        var matrix = new TraceMatrix(requirements);

        var mdPath = Path.Combine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "0 of 1 requirements are satisfied with tests.");
        StringAssert.Contains(content, "| AUTH-001 | 0 | 0 | 0 | 0 |");
    }

    /// <summary>
    ///     Test exporting trace matrix where a test maps to multiple requirements.
    /// </summary>
    [TestMethod]
    public void Export_TestMapsToMultipleRequirements_ShowsAllMappings()
    {
        // Create requirements where one test maps to multiple requirements
        var reqYaml = @"---
sections:
  - title: ""User Authentication""
    requirements:
      - id: ""AUTH-001""
        title: ""Validate user credentials""
        tests:
          - ""Test_Credentials""
      - id: ""AUTH-002""
        title: ""Authenticate requests""
        tests:
          - ""Test_Credentials""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var requirements = Requirements.Read(reqPath);

        // Create TRX file
        var testResults = new TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new TestResult
        {
            Name = "Test_Credentials",
            ClassName = "AuthTests",
            CodeBase = "Tests.dll",
            Outcome = TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = Path.Combine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "2 of 2 requirements are satisfied with tests.");
        // Test should appear twice in the testing section, once for each requirement
        var testCredentialsCount = content.Split(new[] { "| Test_Credentials |" }, StringSplitOptions.None).Length - 1;
        Assert.AreEqual(2, testCredentialsCount, "Test_Credentials should appear twice in the testing section");
    }
}
