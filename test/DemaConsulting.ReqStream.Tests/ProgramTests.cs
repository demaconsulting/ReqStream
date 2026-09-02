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
/// Collection definition to disable parallel execution for ProgramTests.
/// </summary>
[CollectionDefinition("Sequential", DisableParallelization = true)]
public sealed class SequentialCollectionDefinition;

/// <summary>
/// Unit tests for the Program class Run method.
/// </summary>
[Collection("Sequential")]
public sealed class ProgramTests : IDisposable
{
    /// <summary>Temporary directory providing isolated file-system workspace for this test class instance.</summary>
    private readonly TemporaryDirectory _testDirectory = new();

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public ProgramTests()
    {
    }

    /// <summary>
    /// Clean up test by deleting the temporary test directory.
    /// </summary>
    public void Dispose()
    {
        _testDirectory.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Test Run with version flag prints version information.
    /// </summary>
    [Fact]
    public void Program_Run_WithVersionFlag_PrintsVersion()
    {
        // Arrange: create log file path to capture output
        var logFile = _testDirectory.GetFilePath("version.log");

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
        Assert.Contains(Program.Version, outputText);
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
        var logFile = _testDirectory.GetFilePath("help.log");

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
        var logFile = _testDirectory.GetFilePath("validation.log");

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
        Assert.Contains("ReqStream_OrphanDetection - Passed", logContent);
        Assert.Contains("ReqStream_Lint - Passed", logContent);
        Assert.Contains("Total Tests: 7", logContent);
        Assert.Contains("Passed: 7", logContent);
        Assert.Contains("Failed: 0", logContent);
    }

    /// <summary>
    /// Test running the program with validate flag and results file.
    /// </summary>
    [Fact]
    public void Program_Run_WithValidateAndResults_WritesResultsFile()
    {
        // Arrange: set up log file and results file paths
        var logFile = _testDirectory.GetFilePath("validation.log");
        var resultsFile = _testDirectory.GetFilePath("validation-results.trx");

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
        var logFile = _testDirectory.GetFilePath("validation.log");
        var resultsFile = _testDirectory.GetFilePath("validation-results.xml");

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
    /// Test Run with no requirements files shows message.
    /// </summary>
    [Fact]
    public void Program_Run_WithNoRequirementsFiles_ShowsMessage()
    {
        // Arrange: create log file path to capture output
        var logFile = _testDirectory.GetFilePath("no-req-files.log");

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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var reportFile = _testDirectory.GetFilePath("report.md");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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
        var trxFile = _testDirectory.GetFilePath("tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var matrixFile = _testDirectory.GetFilePath("matrix.md");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
        var logFile = _testDirectory.GetFilePath("version-and-help.log");

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
        var logFile = _testDirectory.GetFilePath("help-and-validate.log");

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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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

        var trxFile = _testDirectory.GetFilePath("tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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

        var trxFile = _testDirectory.GetFilePath("tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("enforcement-test.log");

        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
    /// Test that orphaned requirements produce a non-fatal warning (not an error) when
    /// --enforce is not active.
    /// </summary>
    [Fact]
    public void Program_Run_WithRootTagsAndOrphans_PrintsWarningWithoutFailing()
    {
        // Arrange: a root-tagged requirement and an unreachable orphan requirement
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
      - id: ORPHAN-001
        title: Orphaned Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("orphan-warning.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with requirements only, no --enforce
            int exitCode;
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                exitCode = context.ExitCode;
            }

            // Assert: exit code unaffected, warning text (not error text) printed with the orphan id
            Assert.Equal(0, exitCode);
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Warning: 1 of 2 requirements orphaned", logContent);
            Assert.Contains("ORPHAN-001", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test that no warning is printed when root tags are configured but the tree is fully
    /// reachable (no orphans).
    /// </summary>
    [Fact]
    public void Program_Run_WithRootTagsNoOrphans_NoWarningPrinted()
    {
        // Arrange: a root-tagged requirement whose only child is reachable
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
        children: [CHILD-001]
      - id: CHILD-001
        title: Child Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("orphan-no-warning.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with requirements only, no --enforce
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                Assert.Equal(0, context.ExitCode);
            }

            // Assert: no orphan warning text appears anywhere in the output
            var logContent = File.ReadAllText(logFile);
            Assert.DoesNotContain("orphaned", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test that --enforce reports orphans as a build-breaking error even when no --tests
    /// were supplied at all, proving orphan enforcement is independent of test-coverage
    /// enforcement.
    /// </summary>
    [Fact]
    public void Program_Run_WithEnforcementRootTagsAndOrphansNoTests_FailsEvenWithoutTests()
    {
        // Arrange: a root-tagged requirement and an orphan, with no test files
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
      - id: ORPHAN-001
        title: Orphaned Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("orphan-enforce.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with requirements and --enforce, no --tests at all
            int exitCode;
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--enforce",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                exitCode = context.ExitCode;
            }

            // Assert: non-zero exit code with the orphan Error: block, despite no --tests
            Assert.Equal(1, exitCode);
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Error: 1 of 2 requirements orphaned", logContent);
            Assert.Contains("ORPHAN-001", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test that --enforce reports both the missing-test-coverage error block and the
    /// orphan error block together in the same invocation, when both conditions apply.
    /// </summary>
    [Fact]
    public void Program_Run_WithEnforcementOrphansAndMissingCoverage_ReportsBothErrorBlocks()
    {
        // Arrange: a root-tagged tested requirement, an untested reachable requirement,
        // plus one orphaned requirement
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
        children: [CHILD-001]
        tests:
          - TestMethod1
      - id: CHILD-001
        title: Untested Child Requirement
      - id: ORPHAN-001
        title: Orphaned Requirement
");

        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TestMethod1",
            ClassName = "TestClass",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxFile = _testDirectory.GetFilePath("tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("orphan-and-coverage.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with requirements, tests, and --enforce
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

            // Assert: both the coverage-failure block and the orphan-failure block are present
            Assert.Equal(1, exitCode);
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Only 1 of 3 requirements are satisfied", logContent);
            Assert.Contains("CHILD-001", logContent);
            Assert.Contains("Error: 1 of 3 requirements orphaned", logContent);
            Assert.Contains("ORPHAN-001", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Regression test: --enforce with neither test files nor root tags configured still
    /// reports the original "nothing to enforce" error.
    /// </summary>
    [Fact]
    public void Program_Run_WithEnforcementNoTestsNoRootTags_ReportsNothingToEnforceError()
    {
        // Arrange: a requirements file with no root-tags and no test files
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("nothing-to-enforce.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with requirements and --enforce, no --tests, no root-tags
            int exitCode;
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--enforce",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                exitCode = context.ExitCode;
            }

            // Assert: the original "nothing to enforce" error still fires
            Assert.Equal(1, exitCode);
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Cannot enforce requirements without test results or root tags", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test that orphan-checking runs against the full, unfiltered requirement graph and is
    /// unaffected by a --filter argument that would exclude the orphan from filtered reports.
    /// </summary>
    [Fact]
    public void Program_Run_WithFilterAndRootTagsOrphans_OrphanCheckIgnoresFilter()
    {
        // Arrange: a root-tagged requirement and an orphan tagged with a tag excluded by --filter
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
      - id: ORPHAN-001
        title: Orphaned Requirement
        tags: [excluded]
");

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("filter-independence.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with a --filter that would exclude ORPHAN-001 from filtered report output
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--filter", "product",
                "--enforce",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                Assert.Equal(1, context.ExitCode);
            }

            // Assert: the orphan is still detected and reported despite the --filter
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Error: 1 of 2 requirements orphaned", logContent);
            Assert.Contains("ORPHAN-001", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test that a YAML-only root-tags declaration (no --root-tags CLI flag) is sufficient to
    /// trigger orphan checking.
    /// </summary>
    [Fact]
    public void Program_Run_WithRootTagsDeclaredOnlyInYaml_NoCliFlag_StillChecksOrphans()
    {
        // Arrange: root-tags declared only in YAML
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
      - id: ORPHAN-001
        title: Orphaned Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("yaml-only-root-tags.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run without any --root-tags CLI flag
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                Assert.Equal(0, context.ExitCode);
            }

            // Assert: the orphan warning still appears, driven solely by the YAML declaration
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Warning: 1 of 2 requirements orphaned", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test that a CLI-only --root-tags flag (no YAML root-tags: declaration) is sufficient to
    /// trigger orphan checking.
    /// </summary>
    [Fact]
    public void Program_Run_WithCliRootTagsFlagOnly_NoYamlDeclaration_StillChecksOrphans()
    {
        // Arrange: no root-tags: key in YAML at all
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
      - id: ORPHAN-001
        title: Orphaned Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("cli-only-root-tags.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with --root-tags on the CLI only
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--root-tags", "product",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                Assert.Equal(0, context.ExitCode);
            }

            // Assert: the orphan warning still appears, driven solely by the CLI flag
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("Warning: 1 of 2 requirements orphaned", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test that with no root tags configured anywhere (neither YAML nor CLI), orphan checking
    /// is skipped entirely - no warning/error text related to orphans appears, confirming full
    /// backward compatibility.
    /// </summary>
    [Fact]
    public void Program_Run_WithNoRootTagsAnywhere_SkipsOrphanCheckEntirely()
    {
        // Arrange: a requirements file with an otherwise-orphan-shaped tree, but no root-tags
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Standalone Requirement
");

        var originalDir = Directory.GetCurrentDirectory();
        var logFile = _testDirectory.GetFilePath("no-root-tags.log");
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with no root-tags configured anywhere
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                Assert.Equal(0, context.ExitCode);
            }

            // Assert: no orphan-related text appears at all
            var logContent = File.ReadAllText(logFile);
            Assert.DoesNotContain("orphaned", logContent);
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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var logFile = _testDirectory.GetFilePath("lint.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var logFile = _testDirectory.GetFilePath("lint-banner.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
        var validFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(validFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        // Create a second file with a duplicate ID to cause a lint issue
        var badFile = _testDirectory.GetFilePath("bad-requirements.yaml");
        File.WriteAllText(badFile, @"
sections:
  - title: Bad Section
    requirements:
      - id: REQ-001
        title: Duplicate Requirement
");

        var logFile = _testDirectory.GetFilePath("lint-issues.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
    /// Test Run with lint flag and root tags configured reports orphans as a non-fatal
    /// warning, so --lint alone surfaces orphan-shaped requirements without needing a
    /// separate full requirements-processing invocation (e.g. --report/--matrix).
    /// </summary>
    [Fact]
    public void Program_Run_WithLintFlagAndRootTags_ReportsOrphansAsWarning()
    {
        // Arrange: a root-tagged requirement and an unreachable orphan requirement
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
      - id: ORPHAN-001
        title: Orphaned Requirement
");

        var logFile = _testDirectory.GetFilePath("lint-orphan-warning.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with --lint only, no --enforce
            using var context = Context.Create(["--lint", "--requirements", "*.yaml", "--silent", "--log", logFile]);
            Program.Run(context);

            // Assert: exit code unaffected, warning text (not error text) printed with the orphan id
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        var logContent = File.ReadAllText(logFile);
        Assert.Contains("Warning: 1 of 2 requirements is orphaned", logContent);
        Assert.Contains("ORPHAN-001", logContent);
    }

    /// <summary>
    /// Test Run with lint flag combined with --enforce fails when root tags are configured
    /// and orphans are present, matching the failure behavior of --requirements --enforce.
    /// </summary>
    [Fact]
    public void Program_Run_WithLintFlagAndEnforce_FailsOnOrphans()
    {
        // Arrange: a root-tagged requirement and an unreachable orphan requirement
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
      - id: ORPHAN-001
        title: Orphaned Requirement
");

        var logFile = _testDirectory.GetFilePath("lint-orphan-error.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with --lint and --enforce together
            using var context = Context.Create(["--lint", "--enforce", "--requirements", "*.yaml", "--silent", "--log", logFile]);
            Program.Run(context);

            // Assert: exit code reflects failure, error text (not warning text) printed with the orphan id
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        var logContent = File.ReadAllText(logFile);
        Assert.Contains("Error: 1 of 2 requirements is orphaned", logContent);
        Assert.Contains("ORPHAN-001", logContent);
    }

    /// <summary>
    /// Test Run with lint flag and root tags configured but no orphans present produces no
    /// orphan-related output.
    /// </summary>
    [Fact]
    public void Program_Run_WithLintFlagAndRootTagsNoOrphans_ProducesNoOrphanOutput()
    {
        // Arrange: a root-tagged requirement with no orphans
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
");

        var logFile = _testDirectory.GetFilePath("lint-no-orphans.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with --lint only, root tags configured, no orphans
            using var context = Context.Create(["--lint", "--requirements", "*.yaml", "--silent", "--log", logFile]);
            Program.Run(context);

            // Assert: lint succeeds with no orphan-related output
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }

        var logContent = File.ReadAllText(logFile);
        Assert.DoesNotContain("orphaned", logContent);
    }

    /// <summary>
    /// Test Run with enforcement mode and failed tests fails.
    /// </summary>
    [Fact]
    public void Program_Run_WithEnforcementAndFailedTests_Fails()
    {
        // Arrange: create requirements file and TRX with a failed test
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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

        var trxFile = _testDirectory.GetFilePath("tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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
        var logFile = _testDirectory.GetFilePath("lint-no-req.log");

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
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        justification: This requirement exists to test the justifications export feature.
");

        var justificationsFile = _testDirectory.GetFilePath("justifications.md");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

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

    /// <summary>
    /// Test Run with --matrix but no test files reports an error.
    /// </summary>
    [Fact]
    public void Program_Run_WithMatrixButNoTestFiles_ReportsError()
    {
        // Arrange: create a requirements file but no test result files
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var matrixFile = _testDirectory.GetFilePath("matrix.md");
        var logFile = _testDirectory.GetFilePath("matrix-no-tests.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with requirements and matrix flags but no test files
            int exitCode;
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--matrix", matrixFile,
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                exitCode = context.ExitCode;
            }

            // Assert: exits with error code and matrix file is not created
            Assert.Equal(1, exitCode);
            Assert.False(File.Exists(matrixFile));

            // Assert: error message explains the problem
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("No test result files were provided or matched", logContent);
            Assert.Contains("--tests", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with --matrix and --tests pattern that matches no files reports an error.
    /// </summary>
    [Fact]
    public void Program_Run_WithMatrixAndUnmatchedTestsPattern_ReportsError()
    {
        // Arrange: create a requirements file but no test result files matching the pattern
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var matrixFile = _testDirectory.GetFilePath("matrix.md");
        var logFile = _testDirectory.GetFilePath("matrix-unmatched-tests.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with requirements, matrix, and --tests pointing at a pattern that matches nothing
            int exitCode;
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--matrix", matrixFile,
                "--tests", "nonexistent-*.xml",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                exitCode = context.ExitCode;
            }

            // Assert: exits with error code and matrix file is not created
            Assert.Equal(1, exitCode);
            Assert.False(File.Exists(matrixFile));

            // Assert: error message explains the problem
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("No test result files were provided or matched", logContent);
            Assert.Contains("--tests", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Regression test for a bug where the "--matrix requested but no test files matched" guard
    /// used to <c>return;</c> before reaching the <c>--enforce</c> check, silently skipping
    /// orphan-freedom enforcement in a combined <c>--matrix</c>+<c>--enforce</c> invocation even
    /// when root tags were configured. Verifies that both the matrix "no test files" error and
    /// the orphan-enforcement error are reported together, with a single non-zero exit code.
    /// </summary>
    [Fact]
    public void Program_Run_WithMatrixNoMatchAndEnforceRootTagsOrphan_ReportsBothMatrixAndOrphanErrors()
    {
        // Arrange: root-tagged requirement plus an orphan, and a --tests pattern matching nothing
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, @"
root-tags: [product]
sections:
  - title: Test Section
    requirements:
      - id: ROOT-001
        title: Root Requirement
        tags: [product]
      - id: ORPHAN-001
        title: Orphaned Requirement
");

        var matrixFile = _testDirectory.GetFilePath("matrix.md");
        var logFile = _testDirectory.GetFilePath("matrix-enforce-orphan.log");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: run with --matrix, a --tests pattern matching no files, --enforce, and root tags
            int exitCode;
            using (var context = Context.Create([
                "--requirements", "*.yaml",
                "--matrix", matrixFile,
                "--tests", "nonexistent-*.xml",
                "--enforce",
                "--silent",
                "--log", logFile
            ]))
            {
                Program.Run(context);
                exitCode = context.ExitCode;
            }

            // Assert: single non-zero exit code, matrix file not created
            Assert.Equal(1, exitCode);
            Assert.False(File.Exists(matrixFile));

            // Assert: both the matrix "no test files" error AND the orphan-enforcement error
            // are reported - before the fix, the orphan block was silently skipped
            var logContent = File.ReadAllText(logFile);
            Assert.Contains("No test result files were provided or matched", logContent);
            Assert.Contains("Error: 1 of 2 requirements orphaned", logContent);
            Assert.Contains("ORPHAN-001", logContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }
}
