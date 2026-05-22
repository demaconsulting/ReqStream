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
///     Unit tests for TraceMatrix Markdown export functionality.
/// </summary>
public sealed class TraceMatrixExportTests : IDisposable
{
    /// <summary>Delimiter string used to split export output lines for assertion.</summary>
    private static readonly string[] SplitDelimiter = ["| Test_Credentials |"];

    /// <summary>Unique temporary directory for this test instance's fixture files.</summary>
    private readonly string _testDirectory;

    /// <summary>
    ///     Initialize test by creating a temporary test directory.
    /// </summary>
    public TraceMatrixExportTests()
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
    ///     Test exporting a simple trace matrix to Markdown.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_SimpleTraceMatrix_CreatesMarkdownFile()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

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

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        // Assert:
        Assert.True(File.Exists(mdPath));
        var content = File.ReadAllText(mdPath);
        Assert.Contains("# Summary", content);
        Assert.Contains("1 of 1 requirements are satisfied with tests.", content);
        Assert.Contains("# Requirements", content);
        Assert.Contains("## User Authentication", content);
        Assert.Contains("| ID | Tests Linked | Passed | Failed | Not Executed |", content);
        Assert.Contains("| AUTH-001 | 2 | 2 | 0 | 0 |", content);
        Assert.Contains("# Testing", content);
        Assert.Contains("| Test | Requirement | Passed | Failed |", content);
        Assert.Contains("| Test_Credentials_Invalid | AUTH-001 | 1 | 0 |", content);
        Assert.Contains("| Test_Credentials_Valid | AUTH-001 | 1 | 0 |", content);
    }

    /// <summary>
    ///     Test exporting trace matrix with custom depth.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_WithCustomDepth_UsesCorrectHeaderLevel()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

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

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath, depth: 2);

        // Assert:
        var content = File.ReadAllText(mdPath);
        Assert.Contains("## Summary", content);
        Assert.Contains("## Requirements", content);
        Assert.Contains("### User Authentication", content);
        Assert.Contains("## Testing", content);
    }

    /// <summary>
    ///     Test exporting trace matrix with failed tests.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_WithFailedTests_ShowsFailures()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

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

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        // Assert:
        var content = File.ReadAllText(mdPath);
        Assert.Contains("0 of 1 requirements are satisfied with tests.", content);
        Assert.Contains("| AUTH-001 | 2 | 1 | 1 | 0 |", content);
        Assert.Contains("| Test_Credentials_Invalid | AUTH-001 | 0 | 1 |", content);
        Assert.Contains("| Test_Credentials_Valid | AUTH-001 | 1 | 0 |", content);
    }

    /// <summary>
    ///     Test exporting trace matrix with not executed tests.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_WithNotExecutedTests_ShowsNotExecuted()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

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

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        // Assert:
        var content = File.ReadAllText(mdPath);
        Assert.Contains("0 of 1 requirements are satisfied with tests.", content);
        Assert.Contains("| AUTH-001 | 2 | 1 | 0 | 1 |", content);
        Assert.Contains("| Test_Credentials_Invalid | AUTH-001 | 0 | 0 |", content);
    }

    /// <summary>
    ///     Test exporting trace matrix with nested sections.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_WithNestedSections_CreatesHierarchy()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

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

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        // Assert:
        var content = File.ReadAllText(mdPath);
        Assert.Contains("2 of 2 requirements are satisfied with tests.", content);
        Assert.Contains("## Data Management", content);
        Assert.Contains("### User Authentication", content);
        Assert.Contains("### Logging", content);
        Assert.Contains("| AUTH-001 | 1 | 1 | 0 | 0 |", content);
        Assert.Contains("| LOG-001 | 1 | 1 | 0 | 0 |", content);
    }

    /// <summary>
    ///     Test that export throws exception when file path is null.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_NullFilePath_ThrowsArgumentException()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements);

        // Act / Assert:
        var ex = Assert.Throws<ArgumentException>(() => matrix.Export(null!));
        Assert.Contains("File path cannot be null or empty", ex.Message);
    }

    /// <summary>
    ///     Test that export throws exception when file path is empty.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_EmptyFilePath_ThrowsArgumentException()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements);

        // Act / Assert:
        var ex = Assert.Throws<ArgumentException>(() => matrix.Export(string.Empty));
        Assert.Contains("File path cannot be null or empty", ex.Message);
    }

    /// <summary>
    ///     Test exporting trace matrix with requirements that have child requirements.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_WithChildRequirements_ConsidersChildTests()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

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

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        // Assert:
        var content = File.ReadAllText(mdPath);
        // Both requirements should be satisfied because SYS-SEC-001 has child AUTH-001 which has passing tests
        Assert.Contains("2 of 2 requirements are satisfied with tests.", content);
        Assert.Contains("| SYS-SEC-001 | 0 | 0 | 0 | 0 |", content);
        Assert.Contains("| AUTH-001 | 1 | 1 | 0 | 0 |", content);
    }

    /// <summary>
    ///     Test exporting trace matrix with requirements that have no tests.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_WithNoTests_ShowsNotSatisfied()
    {
        // Arrange:
        // Create requirements with no tests
        var reqYaml = @"---
sections:
  - title: ""User Authentication""
    requirements:
      - id: ""AUTH-001""
        title: ""Validate user credentials""
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Act:
        // Create TraceMatrix with no test results
        var matrix = new TraceMatrix(requirements);

        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        // Assert:
        var content = File.ReadAllText(mdPath);
        Assert.Contains("0 of 1 requirements are satisfied with tests.", content);
        Assert.Contains("| AUTH-001 | 0 | 0 | 0 | 0 |", content);
    }

    /// <summary>
    ///     Test exporting trace matrix where a test maps to multiple requirements.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_TestMapsToMultipleRequirements_ShowsAllMappings()
    {
        // Arrange:
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
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, reqYaml);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

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

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix
        var matrix = new TraceMatrix(requirements, trxPath);

        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        matrix.Export(mdPath);

        // Assert:
        var content = File.ReadAllText(mdPath);
        Assert.Contains("2 of 2 requirements are satisfied with tests.", content);
        // Test should appear twice in the testing section, once for each requirement
        var testCredentialsCount = content.Split(SplitDelimiter, StringSplitOptions.None).Length - 1;
        Assert.Equal(2, testCredentialsCount);
    }

    /// <summary>
    /// Test exporting trace matrix with filter tags.
    /// </summary>
    [Fact]
    public void TraceMatrix_Export_WithFilterTags_ExportsOnlyMatchingRequirements()
    {
        // Arrange:
        var yamlContent = @"sections:
  - title: System Requirements
    requirements:
      - id: REQ-001
        title: Security requirement
        tags:
          - security
        tests:
          - Test_Security
      - id: REQ-002
        title: Performance requirement
        tags:
          - performance
        tests:
          - Test_Performance
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create test results
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "Test_Security",
            ClassName = "SecurityTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "Test_Performance",
            ClassName = "PerformanceTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        // Create TraceMatrix and export with filter
        var matrix = new TraceMatrix(requirements, trxPath);
        var mdPath = PathHelpers.SafePathCombine(_testDirectory, "tracematrix.md");
        var filterTags = new HashSet<string> { "security" };
        matrix.Export(mdPath, filterTags: filterTags);

        // Assert:
        var content = File.ReadAllText(mdPath);

        // Should show 1 of 1 requirements (only security-tagged requirement)
        Assert.Contains("1 of 1 requirements are satisfied with tests.", content);

        // Should contain security requirement but not performance requirement
        Assert.Contains("REQ-001", content);
        Assert.DoesNotContain("REQ-002", content);
        Assert.Contains("Test_Security", content);
        Assert.DoesNotContain("Test_Performance", content);
    }

    /// <summary>
    /// Test that trace matrix filtering affects satisfied requirements count.
    /// </summary>
    [Fact]
    public void TraceMatrix_CalculateSatisfiedRequirements_WithFilterTags_CountsOnlyMatchingRequirements()
    {
        // Arrange:
        var yamlContent = @"sections:
  - title: System Requirements
    requirements:
      - id: REQ-001
        title: Security requirement with tests
        tags:
          - security
        tests:
          - Test_Security
      - id: REQ-002
        title: Security requirement without tests
        tags:
          - security
      - id: REQ-003
        title: Performance requirement with tests
        tags:
          - performance
        tests:
          - Test_Performance
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Create test results
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "Test_Security",
            ClassName = "SecurityTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "Test_Performance",
            ClassName = "PerformanceTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxPath = PathHelpers.SafePathCombine(_testDirectory, "results.trx");
        File.WriteAllText(trxPath, TrxSerializer.Serialize(testResults));

        // Act:
        var matrix = new TraceMatrix(requirements, trxPath);

        // Assert:
        // Without filter: should count all 3 requirements (2 satisfied, 1 unsatisfied)
        var (satisfiedAll, totalAll) = matrix.CalculateSatisfiedRequirements();
        Assert.Equal(2, satisfiedAll);
        Assert.Equal(3, totalAll);

        // With security filter: should count only 2 security requirements (1 satisfied, 1 unsatisfied)
        var filterTags = new HashSet<string> { "security" };
        var (satisfiedFiltered, totalFiltered) = matrix.CalculateSatisfiedRequirements(filterTags);
        Assert.Equal(1, satisfiedFiltered);
        Assert.Equal(2, totalFiltered);
    }

    /// <summary>
    /// Test that trace matrix filtering affects unsatisfied requirements list.
    /// </summary>
    [Fact]
    public void TraceMatrix_GetUnsatisfiedRequirements_WithFilterTags_ReturnsOnlyMatchingRequirements()
    {
        // Arrange:
        var yamlContent = @"sections:
  - title: System Requirements
    requirements:
      - id: REQ-001
        title: Security requirement without tests
        tags:
          - security
      - id: REQ-002
        title: Performance requirement without tests
        tags:
          - performance
";
        var reqPath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var loadResult = Requirements.Load(reqPath);
        Assert.NotNull(loadResult.Requirements);
        var requirements = loadResult.Requirements;

        // Act:
        var matrix = new TraceMatrix(requirements);

        // Assert:
        // Without filter: should return both unsatisfied requirements
        var unsatisfiedAll = matrix.GetUnsatisfiedRequirements();
        Assert.Equal(2, unsatisfiedAll.Count);
        Assert.Contains("REQ-001", unsatisfiedAll);
        Assert.Contains("REQ-002", unsatisfiedAll);

        // With security filter: should return only security requirement
        var filterTags = new HashSet<string> { "security" };
        var unsatisfiedFiltered = matrix.GetUnsatisfiedRequirements(filterTags);
        Assert.Single(unsatisfiedFiltered);
        Assert.Contains("REQ-001", unsatisfiedFiltered);
        Assert.DoesNotContain("REQ-002", unsatisfiedFiltered);
    }
}
