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

using DemaConsulting.ReqStream;
using DemaConsulting.ReqStream.Cli;
using DemaConsulting.ReqStream.Utilities;

namespace DemaConsulting.ReqStream.Tests;

/// <summary>
/// Unit tests for the Program class Run method.
/// </summary>
public sealed class ProgramTests : IDisposable
{
    private readonly string _testDirectory;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public ProgramTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reqstream_test_{Guid.NewGuid()}");
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
    /// Test Run with version flag prints version information.
    /// </summary>
    [Fact]
    public void Program_Run_WithVersionFlag_PrintsVersion()
    {
        // Arrange: create log file path to capture output
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "version.log");

        // Act: run with version flag, capturing output to log file
        using (var context = Context.Create(["--version", "--log", logFile]))
        {
            Program.Run(context);

            // Assert: version string is printed without banner or help
            Assert.Equal(0, context.ExitCode);
        }

        // Assert: log file contains version output (read after context disposal to ensure flush)
        var outputText = File.ReadAllText(logFile).Trim();
        Assert.False(string.IsNullOrWhiteSpace(outputText));
        Assert.DoesNotContain("Copyright", outputText);
        Assert.DoesNotContain("Usage", outputText);
    }

    /// <summary>
    /// Test Run with help flag prints help information.
    /// </summary>
    [Fact]
    public void Program_Run_WithHelpFlag_PrintsHelp()
    {
        // Arrange: create log file path to capture output
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "help.log");

        // Act: run with help flag, capturing output to log file
        using (var context = Context.Create(["--help", "--log", logFile]))
        {
            Program.Run(context);
        }

        // Assert: banner and usage information are printed
        var outputText = File.ReadAllText(logFile);
        Assert.Contains("ReqStream version", outputText);
        Assert.Contains("Copyright", outputText);
        Assert.Contains("Usage:", outputText);
        Assert.Contains("Options:", outputText);
    }

    /// <summary>
    /// Test running the program with validate flag.
    /// </summary>
    [Fact]
    public void Program_Run_WithValidateFlag_RunsValidation()
    {
        // Arrange: set up log file path for validation output
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "validation.log");

        // Act: run with validate flag, capturing output to log file
        using (var context = Context.Create(["--validate", "--silent", "--log", logFile]))
        {
            Program.Run(context);

            // Assert: validation succeeds with exit code 0
            Assert.Equal(0, context.ExitCode);
        }

        // Assert: log file contains expected validation output (after context is disposed to flush log)
        Assert.True(File.Exists(logFile), "Log file should exist");
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("DEMA Consulting ReqStream", logContent);
        Assert.Contains("ReqStream Version", logContent);
        Assert.Contains("ReqStream_RequirementsProcessing - Passed", logContent);
        Assert.Contains("ReqStream_TraceMatrix - Passed", logContent);
        Assert.Contains("ReqStream_ReportExport - Passed", logContent);
        Assert.Contains("ReqStream_TagsFiltering - Passed", logContent);
        Assert.Contains("ReqStream_EnforcementMode - Passed", logContent);
        Assert.Contains("ReqStream_Lint - Passed", logContent);
        Assert.Contains("Total Tests: 6", logContent);
        Assert.Contains("Passed: 6", logContent);
        Assert.Contains("Failed: 0", logContent);
    }

    /// <summary>
    /// Test running the program with validate flag and results file.
    /// </summary>
    [Fact]
    public void Program_Run_WithValidateAndResults_WritesResultsFile()
    {
        // Arrange: set up log file and results file paths
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "validation.log");
        var resultsFile = PathHelpers.SafePathCombine(_testDirectory, "validation-results.trx");

        // Act: run with validate and results flags
        using (var context = Context.Create(["--validate", "--silent", "--log", logFile, "--results", resultsFile]))
        {
            Program.Run(context);

            // Assert: validation succeeds with exit code 0
            Assert.Equal(0, context.ExitCode);
        }

        // Assert: results file was created with expected content
        Assert.True(File.Exists(resultsFile));

        // Assert: results file is valid TRX
        var trxContent = File.ReadAllText(resultsFile);
        Assert.Contains("TestRun", trxContent);
        Assert.Contains("RequirementsProcessing", trxContent);
        Assert.Contains("TraceMatrix", trxContent);
        Assert.Contains("ReportExport", trxContent);
        Assert.Contains("outcome=\"Passed\"", trxContent);

        // Assert: log confirms results were written
        var logContent = File.ReadAllText(logFile);
        Assert.Contains($"Results written to {resultsFile}", logContent);
    }

    /// <summary>
    /// Test running the program with validate flag and JUnit results file.
    /// </summary>
    [Fact]
    public void Program_Run_WithValidateAndJUnitResults_WritesJUnitFile()
    {
        // Arrange: set up log file and JUnit results file paths
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "validation.log");
        var resultsFile = PathHelpers.SafePathCombine(_testDirectory, "validation-results.xml");

        // Act: run with validate flag and JUnit results file path
        using (var context = Context.Create(["--validate", "--silent", "--log", logFile, "--results", resultsFile]))
        {
            Program.Run(context);

            // Assert: validation succeeds with exit code 0
            Assert.Equal(0, context.ExitCode);
        }

        // Assert: JUnit results file was created with expected content
        Assert.True(File.Exists(resultsFile));

        // Assert: JUnit results file contains expected test names
        var xmlContent = File.ReadAllText(resultsFile);
        Assert.Contains("<testsuite", xmlContent);
        Assert.Contains("RequirementsProcessing", xmlContent);
        Assert.Contains("TraceMatrix", xmlContent);
        Assert.Contains("ReportExport", xmlContent);

        // Assert: log confirms results were written
        var logContent = File.ReadAllText(logFile);
        Assert.Contains($"Results written to {resultsFile}", logContent);
    }

    /// <summary>
    /// Test Run with no requirements files prints an informational message.
    /// </summary>
    [Fact]
    public void Program_Run_WithNoFiles_PrintsMessage()
    {
        // Arrange: create log file path to capture output
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "no-files.log");

        // Act: run program with no arguments, capturing output to log file
        using (var context = Context.Create(["--log", logFile]))
        {
            Program.Run(context);

            // Assert: exit code is 0
            Assert.Equal(0, context.ExitCode);
        }

        // Assert: message includes "No requirements files specified"
        var outputText = File.ReadAllText(logFile);
        Assert.Contains("No requirements files specified", outputText);
    }

    /// <summary>
    /// Test Run with no requirements files shows message.
    /// </summary>
    [Fact]
    public void Program_Run_WithNoRequirementsFiles_ShowsMessage()
    {
        // Arrange: create log file path to capture output
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "no-req-files.log");

        // Act: run with no arguments, capturing output to log file
        using (var context = Context.Create(["--log", logFile]))
        {
            Program.Run(context);

            // Assert: completes without errors
            Assert.Equal(0, context.ExitCode);
        }

        // Assert: message indicates no requirements files specified
        var output = File.ReadAllText(logFile);
        Assert.Contains("No requirements files specified.", output);
    }

    /// <summary>
    /// Test Run with requirements files processes them successfully.
    /// </summary>
    [Fact]
    public void Program_Run_WithRequirementsFiles_ProcessesSuccessfully()
    {
        // Arrange: create a test requirements file in the temp directory
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements file glob
            using var context = Context.Create(["--requirements", "*.yaml"]);
            Program.Run(context);

            // Assert: requirements processed successfully
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with requirements export generates report file.
    /// </summary>
    [Fact]
    public void Program_Run_WithRequirementsExport_GeneratesReport()
    {
        // Arrange: create a test requirements file and set report output path
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var reportFile = PathHelpers.SafePathCombine(_testDirectory, "report.md");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements and report flags
            using var context = Context.Create(["--requirements", "*.yaml", "--report", reportFile]);
            Program.Run(context);

            // Assert: report file was generated with expected content
            Assert.Equal(0, context.ExitCode);
            Assert.True(File.Exists(reportFile));

            var reportContent = File.ReadAllText(reportFile);
            Assert.Contains("Test Section", reportContent);
            Assert.Contains("REQ-001", reportContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with trace matrix export generates matrix file.
    /// </summary>
    [Fact]
    public void Program_Run_WithTraceMatrixExport_GeneratesMatrix()
    {
        // Arrange: create requirements file and TRX test results file
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        tests:
          - TestMethod1
");

        // Create a test TRX file using TestResults library
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TestMethod1",
            ClassName = "TestClass",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxFile = PathHelpers.SafePathCombine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var matrixFile = PathHelpers.SafePathCombine(_testDirectory, "matrix.md");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements, tests, and matrix flags
            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--tests", "*.trx",
                "--matrix", matrixFile
            ]);
            Program.Run(context);

            // Assert: matrix file was generated with expected content
            Assert.Equal(0, context.ExitCode);
            Assert.True(File.Exists(matrixFile));

            var matrixContent = File.ReadAllText(matrixFile);
            Assert.Contains("Summary", matrixContent);
            Assert.Contains("REQ-001", matrixContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test priority order: version takes precedence over help.
    /// </summary>
    [Fact]
    public void Program_Run_WithVersionAndHelp_ProcessesVersionFirst()
    {
        // Arrange: create log file path to capture output
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "version-and-help.log");

        // Act: run with both version and help flags, capturing output to log file
        using (var context = Context.Create(["--version", "--help", "--log", logFile]))
        {
            Program.Run(context);
        }

        // Assert: only version string is printed (help is skipped)
        var outputText = File.ReadAllText(logFile).Trim();
        Assert.False(string.IsNullOrWhiteSpace(outputText));
        Assert.DoesNotContain("Usage:", outputText);
        Assert.DoesNotContain("Copyright", outputText);
    }

    /// <summary>
    /// Test priority order: help takes precedence over validate.
    /// </summary>
    [Fact]
    public void Program_Run_WithHelpAndValidate_ProcessesHelpFirst()
    {
        // Arrange: create log file path to capture output
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "help-and-validate.log");

        // Act: run with both help and validate flags, capturing output to log file
        using (var context = Context.Create(["--help", "--validate", "--log", logFile]))
        {
            Program.Run(context);
        }

        // Assert: help is printed (validation is skipped)
        var outputText = File.ReadAllText(logFile);
        Assert.Contains("Usage:", outputText);
        Assert.DoesNotContain("Self-validation", outputText);
    }

    /// <summary>
    /// Test enforcement with fully satisfied requirements succeeds.
    /// </summary>
    [Fact]
    public void Program_Run_WithEnforcementAndFullySatisfiedRequirements_Succeeds()
    {
        // Arrange: create requirements file and TRX with all requirements covered by passing tests
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        tests:
          - TestMethod1
");

        // Create a test TRX file with passing test using TestResults library
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TestMethod1",
            ClassName = "TestClass",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxFile = PathHelpers.SafePathCombine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements, tests, and enforce flags
            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--tests", "*.trx",
                "--enforce"
            ]);
            Program.Run(context);

            // Assert: enforcement passes when all requirements are satisfied
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test enforcement with unsatisfied requirements fails.
    /// </summary>
    [Fact]
    public void Program_Run_WithEnforcementAndUnsatisfiedRequirements_Fails()
    {
        // Arrange: create requirements file with one tested and one untested requirement, and a passing TRX
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Tested Requirement
        tests:
          - TestMethod1
      - id: REQ-002
        title: Untested Requirement
");

        // Create a test TRX file with passing test using TestResults library
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TestMethod1",
            ClassName = "TestClass",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxFile = PathHelpers.SafePathCombine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "enforcement-test.log");

        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements, tests, and enforce flags
            int exitCode;
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--tests", "*.trx",
                "--enforce",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                exitCode = context.ExitCode;
            }

            // Assert: enforcement fails with unsatisfied requirement listed
            Assert.Equal(1, exitCode);

            // Verify error message includes the unsatisfied requirement via log file
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Only 1 of 2 requirements are satisfied", logContent);
            Assert.Contains("Unsatisfied requirements:", logContent);
            Assert.Contains("REQ-002", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test enforcement without test files fails.
    /// </summary>
    [Fact]
    public void Program_Run_WithEnforcementAndNoTests_Fails()
    {
        // Arrange: create a requirements file with no test TRX
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements and enforce flags but no test files
            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--enforce"
            ]);
            Program.Run(context);

            // Assert: enforcement fails when no test results are provided
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with lint flag lints requirements files.
    /// </summary>
    [Fact]
    public void Program_Run_WithLintFlag_RunsLinter()
    {
        // Arrange: create a valid requirements file with no issues
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var logFile = PathHelpers.SafePathCombine(_testDirectory, "lint.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with lint flag against a clean requirements file
            using var context = Context.Create(["--lint", "--requirements", "*.yaml", "--silent", "--log", logFile]);
            Program.Run(context);

            // Assert: lint succeeds with exit code 0
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        // Assert: no output is produced when lint finds no issues (no banner, no summary line)
        Assert.True(File.Exists(logFile), "Log file should exist");
        var logContent = File.ReadAllText(logFile);
        Assert.Equal(string.Empty, logContent.Trim());
    }

    /// <summary>
    /// Test Run with lint flag does not print the banner.
    /// </summary>
    [Fact]
    public void Program_Run_WithLintFlag_SuppressesBanner()
    {
        // Arrange: create a valid requirements file and log file to capture output
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var logFile = PathHelpers.SafePathCombine(_testDirectory, "lint-banner.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with lint flag, capturing output to log file
            using var context = Context.Create(["--lint", "--requirements", "*.yaml", "--log", logFile]);
            Program.Run(context);

            // Assert: lint succeeds with no output
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        // Assert: banner and summary are not printed during lint
        var outputText = File.ReadAllText(logFile);
        Assert.DoesNotContain("ReqStream version", outputText);
        Assert.DoesNotContain("Copyright", outputText);
        Assert.DoesNotContain("No issues found", outputText);
        Assert.Equal(string.Empty, outputText.Trim());
    }

    /// <summary>
    /// Test Run with lint flag only outputs issue lines (no banner, no summary) when issues are found.
    /// </summary>
    [Fact]
    public void Program_Run_WithLintFlag_OnlyOutputsIssues()
    {
        // Arrange: create a valid requirements file and a second file with a duplicate ID
        var validFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(validFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        // Create a second file with a duplicate ID to cause a lint issue
        var badFile = PathHelpers.SafePathCombine(_testDirectory, "bad-requirements.yaml");
        File.WriteAllText(badFile, @"
sections:
  - title: Bad Section
    requirements:
      - id: REQ-001
        title: Duplicate Requirement
");

        var logFile = PathHelpers.SafePathCombine(_testDirectory, "lint-issues.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run lint across both files
            using var context = Context.Create([
                "--lint",
                "--requirements", "requirements.yaml",
                "--requirements", "bad-requirements.yaml",
                "--silent",
                "--log", logFile]);
            Program.Run(context);

            // Assert: lint fails due to duplicate requirement ID
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        // Assert: log contains the issue but not banner or summary
        Assert.True(File.Exists(logFile), "Log file should exist");
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("REQ-001", logContent);
        Assert.DoesNotContain("ReqStream version", logContent);
        Assert.DoesNotContain("No issues found", logContent);
    }

    /// <summary>
    /// Test Run with enforcement mode and failed tests fails.
    /// </summary>
    [Fact]
    public void Program_Run_WithEnforcementAndFailedTests_Fails()
    {
        // Arrange: create requirements file and TRX with a failed test
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        tests:
          - TestMethod1
");

        // Create a test TRX file with failing test using TestResults library
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TestMethod1",
            ClassName = "TestClass",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Failed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxFile = PathHelpers.SafePathCombine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements, tests, and enforce flags
            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--tests", "*.trx",
                "--enforce"
            ]);
            Program.Run(context);

            // Assert: enforcement fails when linked test is failed
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with lint flag and no requirements files prints an informational message.
    /// </summary>
    [Fact]
    public void Program_Run_WithLintAndNoRequirements_PrintsInformationalMessage()
    {
        // Arrange: create log file path to capture output
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "lint-no-req.log");

        // Act: run with lint flag but no requirements files
        using (var context = Context.Create(["--lint", "--log", logFile]))
        {
            Program.Run(context);

            // Assert: completes without error exit code
            Assert.Equal(0, context.ExitCode);
        }

        // Assert: informational message is present
        var output = File.ReadAllText(logFile);
        Assert.Contains("No requirements files specified.", output);
    }

    /// <summary>
    /// Test Run with justifications export generates a justifications report file.
    /// </summary>
    [Fact]
    public void Program_Run_WithJustificationsExport_GeneratesJustificationsReport()
    {
        // Arrange: create a test requirements file with justification text and set report output path
        var reqFile = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        justification: This requirement exists to test the justifications export feature.
");

        var justificationsFile = PathHelpers.SafePathCombine(_testDirectory, "justifications.md");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements and justifications flags
            using var context = Context.Create(["--requirements", "*.yaml", "--justifications", justificationsFile]);
            Program.Run(context);

            // Assert: justifications file was generated with requirement content
            Assert.Equal(0, context.ExitCode);
            Assert.True(File.Exists(justificationsFile));
            var justificationsContent = File.ReadAllText(justificationsFile);
            Assert.Contains("REQ-001", justificationsContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }
}
