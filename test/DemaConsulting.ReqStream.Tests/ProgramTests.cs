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

namespace DemaConsulting.ReqStream.Tests;

/// <summary>
/// Unit tests for the Program class Run method.
/// </summary>
[TestClass]
public class ProgramTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_test_{Guid.NewGuid()}");
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
    /// Test Run with version flag prints version information.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithVersionFlag_PrintsVersion()
    {
        // Arrange: redirect stdout to capture output
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act: run with version flag
            using var context = Context.Create(["--version"]);
            Program.Run(context);

            // Assert: version string is printed without banner or help
            var outputText = output.ToString().Trim();
            Assert.IsFalse(string.IsNullOrWhiteSpace(outputText));
            Assert.DoesNotContain("Copyright", outputText);
            Assert.DoesNotContain("Usage", outputText);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test Run with help flag prints help information.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithHelpFlag_PrintsHelp()
    {
        // Arrange: redirect stdout to capture output
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act: run with help flag
            using var context = Context.Create(["--help"]);
            Program.Run(context);

            // Assert: banner and usage information are printed
            var outputText = output.ToString();
            Assert.Contains("ReqStream version", outputText);
            Assert.Contains("Copyright", outputText);
            Assert.Contains("Usage:", outputText);
            Assert.Contains("Options:", outputText);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test running the program with validate flag.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithValidateFlag_RunsValidation()
    {
        // Arrange: set up log file path for validation output
        var logFile = Path.Combine(_testDirectory, "validation.log");

        // Act: run with validate flag, capturing output to log file
        using (var context = Context.Create(["--validate", "--silent", "--log", logFile]))
        {
            Program.Run(context);

            // Assert: validation succeeds with exit code 0
            Assert.AreEqual(0, context.ExitCode);
        }

        // Assert: log file contains expected validation output (after context is disposed to flush log)
        Assert.IsTrue(File.Exists(logFile), "Log file should exist");
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
    [TestMethod]
    public void Program_Run_WithValidateAndResults_WritesResultsFile()
    {
        // Arrange: set up log file and results file paths
        var logFile = Path.Combine(_testDirectory, "validation.log");
        var resultsFile = Path.Combine(_testDirectory, "validation-results.trx");

        // Act: run with validate and results flags
        using (var context = Context.Create(["--validate", "--silent", "--log", logFile, "--results", resultsFile]))
        {
            Program.Run(context);

            // Assert: validation succeeds with exit code 0
            Assert.AreEqual(0, context.ExitCode);
        }

        // Assert: results file was created with expected content
        Assert.IsTrue(File.Exists(resultsFile));

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
    [TestMethod]
    public void Program_Run_WithValidateAndJUnitResults_WritesJUnitFile()
    {
        // Arrange: set up log file and JUnit results file paths
        var logFile = Path.Combine(_testDirectory, "validation.log");
        var resultsFile = Path.Combine(_testDirectory, "validation-results.xml");

        // Act: run with validate flag and JUnit results file path
        using (var context = Context.Create(["--validate", "--silent", "--log", logFile, "--results", resultsFile]))
        {
            Program.Run(context);

            // Assert: validation succeeds with exit code 0
            Assert.AreEqual(0, context.ExitCode);
        }

        // Assert: JUnit results file was created with expected content
        Assert.IsTrue(File.Exists(resultsFile));

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
    /// Test Run with no requirements files shows message.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithNoRequirementsFiles_ShowsMessage()
    {
        // Act: run with no arguments
        using var context = Context.Create([]);
        Program.Run(context);

        // Assert: completes without errors
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test Run with requirements files processes them successfully.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithRequirementsFiles_ProcessesSuccessfully()
    {
        // Arrange: create a test requirements file in the temp directory
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
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
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with requirements export generates report file.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithRequirementsExport_GeneratesReport()
    {
        // Arrange: create a test requirements file and set report output path
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var reportFile = Path.Combine(_testDirectory, "report.md");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements and report flags
            using var context = Context.Create(["--requirements", "*.yaml", "--report", reportFile]);
            Program.Run(context);

            // Assert: report file was generated with expected content
            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(reportFile));

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
    [TestMethod]
    public void Program_Run_WithTraceMatrixExport_GeneratesMatrix()
    {
        // Arrange: create requirements file and TRX test results file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
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
        var trxFile = Path.Combine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var matrixFile = Path.Combine(_testDirectory, "matrix.md");

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
            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(matrixFile));

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
    [TestMethod]
    public void Program_Run_WithVersionAndHelp_ProcessesVersionFirst()
    {
        // Arrange: redirect stdout to capture output
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act: run with both version and help flags
            using var context = Context.Create(["--version", "--help"]);
            Program.Run(context);

            // Assert: only version string is printed (help is skipped)
            var outputText = output.ToString().Trim();
            Assert.IsFalse(string.IsNullOrWhiteSpace(outputText));
            Assert.DoesNotContain("Usage:", outputText);
            Assert.DoesNotContain("Copyright", outputText);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test priority order: help takes precedence over validate.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithHelpAndValidate_ProcessesHelpFirst()
    {
        // Arrange: redirect stdout to capture output
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            // Act: run with both help and validate flags
            using var context = Context.Create(["--help", "--validate"]);
            Program.Run(context);

            // Assert: help is printed (validation is skipped)
            var outputText = output.ToString();
            Assert.Contains("Usage:", outputText);
            Assert.DoesNotContain("Self-validation", outputText);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test enforcement with fully satisfied requirements succeeds.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithEnforcementAndFullySatisfiedRequirements_Succeeds()
    {
        // Arrange: create requirements file and TRX with all requirements covered by passing tests
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
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

        var trxFile = Path.Combine(_testDirectory, "tests.trx");
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
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test enforcement with unsatisfied requirements fails.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithEnforcementAndUnsatisfiedRequirements_Fails()
    {
        // Arrange: create requirements file with one tested and one untested requirement, and a passing TRX
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
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

        var trxFile = Path.Combine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = Path.Combine(_testDirectory, "enforcement-test.log");

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
            Assert.AreEqual(1, exitCode);

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
    [TestMethod]
    public void Program_Run_WithEnforcementAndNoTests_Fails()
    {
        // Arrange: create a requirements file with no test TRX
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
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
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with lint flag lints requirements files.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_RunsLinter()
    {
        // Arrange: create a valid requirements file with no issues
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var logFile = Path.Combine(_testDirectory, "lint.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with lint flag against a clean requirements file
            using var context = Context.Create(["--lint", "--requirements", "*.yaml", "--silent", "--log", logFile]);
            Program.Run(context);

            // Assert: lint succeeds with exit code 0
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        // Assert: no output is produced when lint finds no issues (no banner, no summary line)
        Assert.IsTrue(File.Exists(logFile), "Log file should exist");
        var logContent = File.ReadAllText(logFile);
        Assert.AreEqual(string.Empty, logContent.Trim(), "Lint with no issues should produce no output");
    }

    /// <summary>
    /// Test Run with lint flag does not print the banner.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_SuppressesBanner()
    {
        // Arrange: create a valid requirements file and redirect stdout to capture output
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with lint flag
            using var context = Context.Create(["--lint", "--requirements", "*.yaml"]);
            Program.Run(context);

            // Assert: lint succeeds with no output
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Console.SetOut(originalOut);
        }

        // Assert: banner and summary are not printed during lint
        var outputText = output.ToString();
        Assert.DoesNotContain("ReqStream version", outputText, "Banner should be suppressed during lint");
        Assert.DoesNotContain("Copyright", outputText, "Banner should be suppressed during lint");
        Assert.DoesNotContain("No issues found", outputText, "Summary line should be suppressed during lint");
        Assert.AreEqual(string.Empty, outputText.Trim(), "Output should be empty for clean lint");
    }

    /// <summary>
    /// Test Run with lint flag only outputs issue lines (no banner, no summary) when issues are found.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintFlag_OnlyOutputsIssues()
    {
        // Arrange: create a valid requirements file and a second file with a duplicate ID
        var validFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(validFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        // Create a second file with a duplicate ID to cause a lint issue
        var badFile = Path.Combine(_testDirectory, "bad-requirements.yaml");
        File.WriteAllText(badFile, @"
sections:
  - title: Bad Section
    requirements:
      - id: REQ-001
        title: Duplicate Requirement
");

        var logFile = Path.Combine(_testDirectory, "lint-issues.log");

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
            Assert.AreEqual(1, context.ExitCode, "Lint with duplicate IDs should fail");
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        // Assert: log contains the issue but not banner or summary
        Assert.IsTrue(File.Exists(logFile), "Log file should exist");
        var logContent = File.ReadAllText(logFile);
        Assert.Contains("REQ-001", logContent, "Issue about duplicate ID should appear in output");
        Assert.DoesNotContain("ReqStream version", logContent, "Banner should not appear in lint output");
        Assert.DoesNotContain("No issues found", logContent, "Summary line should not appear in lint output");
    }

    /// <summary>
    /// Test Run with enforcement mode and failed tests fails.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithEnforcementAndFailedTests_Fails()
    {
        // Arrange: create requirements file and TRX with a failed test
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
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

        var trxFile = Path.Combine(_testDirectory, "tests.trx");
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
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with lint flag and no requirements files prints an informational message.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithLintAndNoRequirements_PrintsError()
    {
        // Act: run with lint flag but no requirements files
        using var context = Context.Create(["--lint"]);
        Program.Run(context);

        // Assert: completes without error exit code
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test Run with justifications export generates a justifications report file.
    /// </summary>
    [TestMethod]
    public void Program_Run_WithJustificationsExport_GeneratesJustificationsReport()
    {
        // Arrange: create a test requirements file with justification text and set report output path
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        justification: This requirement exists to test the justifications export feature.
");

        var justificationsFile = Path.Combine(_testDirectory, "justifications.md");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: run with requirements and justifications flags
            using var context = Context.Create(["--requirements", "*.yaml", "--justifications", justificationsFile]);
            Program.Run(context);

            // Assert: justifications file was generated with requirement content
            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(justificationsFile));
            var justificationsContent = File.ReadAllText(justificationsFile);
            Assert.Contains("REQ-001", justificationsContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }
}
