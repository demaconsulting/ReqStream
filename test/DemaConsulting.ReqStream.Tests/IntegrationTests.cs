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

namespace DemaConsulting.ReqStream.Tests;

/// <summary>
/// Integration tests for the ReqStream system, exercising the full pipeline across
/// multiple subsystems in end-to-end scenarios.
/// </summary>
[TestClass]
public class IntegrationTests
{
    private string _dllPath = string.Empty;
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by locating the DLL and creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _dllPath = Path.Combine(AppContext.BaseDirectory, "DemaConsulting.ReqStream.dll");
        Assert.IsTrue(File.Exists(_dllPath), $"Could not find ReqStream DLL at {_dllPath}");

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
    /// Integration test verifying that a single invocation can process requirements, load test
    /// results, generate a requirements report, justifications, and trace matrix, and enforce
    /// coverage — all subsystems working together correctly.
    /// </summary>
    [TestMethod]
    public void ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces()
    {
        // Arrange: create requirements file with one covered requirement
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-DoSomethingUseful
                    title: The system shall do something useful.
                    justification: |
                      This is a test justification.
                    tests:
                      - IntegrationTest1
            """);

        // Arrange: create TRX file with a passing test result
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "IntegrationRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "IntegrationTest1",
            ClassName = "IntegrationTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var trxFile = Path.Combine(_testDirectory, "results.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var reportFile = Path.Combine(_testDirectory, "requirements.md");
        var justificationsFile = Path.Combine(_testDirectory, "justifications.md");
        var matrixFile = Path.Combine(_testDirectory, "matrix.md");

        // Act: run the full pipeline as an external process
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "results.trx",
            "--report", reportFile,
            "--justifications", justificationsFile,
            "--matrix", matrixFile,
            "--enforce");

        // Assert: enforcement passed (exit code 0)
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");

        // Assert: all three output files were generated
        Assert.IsTrue(File.Exists(reportFile), "Requirements report should be generated.");
        Assert.IsTrue(File.Exists(justificationsFile), "Justifications report should be generated.");
        Assert.IsTrue(File.Exists(matrixFile), "Trace matrix report should be generated.");

        // Assert: report contains the requirement ID and title
        var reportContent = File.ReadAllText(reportFile);
        Assert.Contains("Integration-System-DoSomethingUseful", reportContent);
        Assert.Contains("The system shall do something useful.", reportContent);

        // Assert: trace matrix contains the satisfied requirement and its covering test
        var matrixContent = File.ReadAllText(matrixFile);
        Assert.Contains("Integration-System-DoSomethingUseful", matrixContent);
        Assert.Contains("IntegrationTest1", matrixContent);
    }

    /// <summary>
    /// Integration test verifying that enforcement mode causes a non-zero exit code when a
    /// requirement has no passing test evidence, confirming the CI/CD gate operates correctly.
    /// </summary>
    [TestMethod]
    public void ReqStream_EnforcementMode_RequirementLacksTestEvidence_FailsWithNonZeroExitCode()
    {
        // Arrange: create requirements file with one requirement that has no matching test
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-Unsatisfied
                    title: The system shall perform an unverified action.
                    justification: |
                      This requirement deliberately has no matching test to verify enforcement failure.
                    tests:
                      - NonExistentTest
            """);

        // Arrange: create TRX file with no tests (empty results)
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "EmptyRun" };
        var trxFile = Path.Combine(_testDirectory, "empty.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        // Act: run enforcement mode as an external process
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "empty.trx",
            "--enforce");

        // Assert: enforcement failed with a non-zero exit code
        Assert.AreNotEqual(0, exitCode, $"Enforcement should fail with non-zero exit code when a requirement lacks test evidence. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that source-specific test matching restricts coverage evidence
    /// to tests from the named result file, and that enforcement passes when the named source
    /// provides the required passing test.
    /// </summary>
    [TestMethod]
    public void ReqStream_SourceFilter_NamedSourceInRequirement_MatchesTestsBySourceFile()
    {
        // Arrange: create requirements file with source-specific test reference
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Platform Requirements
                requirements:
                  - id: Integration-PlatformA-SourceFilter
                    title: The system shall pass the platform-specific test on platform-a.
                    justification: |
                      Platform-specific behavior must be validated on the target platform.
                    tests:
                      - platform-a@PlatformTest1
            """);

        // Arrange: create platform-a.trx with a passing test
        var platformAResults = new DemaConsulting.TestResults.TestResults { Name = "PlatformARun" };
        platformAResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "PlatformTest1",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var platformAFile = Path.Combine(_testDirectory, "platform-a.trx");
        File.WriteAllText(platformAFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(platformAResults));

        // Arrange: create platform-b.trx with a failing test (same test name, different source)
        var platformBResults = new DemaConsulting.TestResults.TestResults { Name = "PlatformBRun" };
        platformBResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "PlatformTest1",
            ClassName = "PlatformTests",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Failed,
            Duration = TimeSpan.FromSeconds(1)
        });
        var platformBFile = Path.Combine(_testDirectory, "platform-b.trx");
        File.WriteAllText(platformBFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(platformBResults));

        // Act: run enforcement using both result files as external process
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "platform-a.trx",
            "--tests", "platform-b.trx",
            "--enforce");

        // Assert: enforcement passed because platform-a.trx satisfies the source-filtered requirement
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that the --lint flag exits silently with code 0 when a valid
    /// requirements file is provided, confirming the no-output-on-success design.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_Lint_ValidRequirementsFile_ExitsSilentlyWithZero()
    {
        // Arrange: create a structurally valid requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-ValidLintRequirement
                    title: The system shall perform a lint-clean operation.
                    tests:
                      - LintTest1
            """);

        // Act: run lint as an external process
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--lint",
            "--requirements", "requirements.yaml");

        // Assert: lint exits with code 0 because no issues were found
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 from lint on valid file, but got {exitCode}. Output: {output}");

        // Assert: no output was produced (silent on success)
        Assert.AreEqual(string.Empty, output.Trim(), $"Expected no output from lint on valid file, but got: {output}");
    }

    /// <summary>
    /// Integration test verifying that the --lint flag lints requirements files and reports
    /// structural issues in a single invocation, exercising the system-level lint behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_Lint_Flag_ReportsLintIssues()
    {
        // Arrange: create a requirements file with a structural issue (missing title)
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-MissingTitle
            """);

        // Act: run lint as an external process
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--lint",
            "--requirements", "requirements.yaml");

        // Assert: lint exits with a non-zero code because an issue was found
        Assert.AreNotEqual(0, exitCode, $"Expected non-zero exit code from lint, but got {exitCode}. Output: {output}");

        // Assert: lint reported an issue about the missing title
        Assert.IsTrue(
            output.Contains("Integration-System-MissingTitle") || output.Contains("title"),
            $"Expected lint output to reference the missing-title issue. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that the --validate flag runs the built-in self-test suite
    /// and exits successfully, exercising the system-level validate behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_Validate_Flag_RunsSelfValidation()
    {
        // Act: run validate as an external process
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate");

        // Assert: self-validation passes (exit code 0)
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 from --validate, but got {exitCode}. Output: {output}");

        // Assert: output contains the validation summary header
        Assert.IsTrue(
            output.Contains("Passed") || output.Contains("Total Tests"),
            $"Expected validation output to contain test summary. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that the --validate --results flags write the self-test results
    /// to the specified file, exercising the system-level validate results output behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_ValidateResultsOutput_ResultsFlag_WritesResultsFile()
    {
        // Arrange: create a temporary file path for the results output
        var resultsFile = Path.Combine(_testDirectory, "validation.trx");

        // Act: run validate with results flag as an external process
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate",
            "--results", resultsFile);

        // Assert: self-validation passes (exit code 0)
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 from --validate --results, but got {exitCode}. Output: {output}");

        // Assert: results file was created
        Assert.IsTrue(File.Exists(resultsFile), "Results file should be created by --results flag.");

        // Assert: results file is non-empty
        Assert.IsTrue(new FileInfo(resultsFile).Length > 0, "Results file should be non-empty.");
    }

    /// <summary>
    /// Integration test verifying that the --filter flag restricts requirements output to only
    /// those matching the specified tag, exercising the system-level tag-filter behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_TagFilter_Flag_FiltersRequirements()
    {
        // Arrange: create requirements file with two requirements, each with a different tag
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-TaggedAlpha
                    title: The system shall satisfy alpha requirements.
                    justification: Alpha requirement for tag filtering test.
                    tags:
                      - alpha
                    tests:
                      - FilterTest_Alpha
                  - id: Integration-System-TaggedBeta
                    title: The system shall satisfy beta requirements.
                    justification: Beta requirement for tag filtering test.
                    tags:
                      - beta
                    tests:
                      - FilterTest_Beta
            """);

        var reportFile = Path.Combine(_testDirectory, "filtered-report.md");

        // Act: run with --filter alpha to export only alpha-tagged requirements
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--report", reportFile,
            "--filter", "alpha");

        // Assert: tool exited successfully
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");

        // Assert: report was generated
        Assert.IsTrue(File.Exists(reportFile), "Filtered requirements report should be generated.");

        // Assert: report contains the alpha requirement but not the beta requirement
        var reportContent = File.ReadAllText(reportFile);
        Assert.IsTrue(
            reportContent.Contains("Integration-System-TaggedAlpha"),
            "Filtered report should contain the alpha-tagged requirement.");
        Assert.IsFalse(
            reportContent.Contains("Integration-System-TaggedBeta"),
            "Filtered report should not contain the beta-tagged requirement.");
    }

    /// <summary>
    /// Integration test verifying that the --version flag causes the tool to print version
    /// information and exit with code 0, exercising the system-level CLI interface behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_CliInterface_VersionFlag_PrintsVersion()
    {
        // Act: run with --version flag as an external process
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--version");

        // Assert: tool exits successfully
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");

        // Assert: output contains a version string (non-empty, no banner/help)
        Assert.IsFalse(string.IsNullOrWhiteSpace(output), "Expected version output to be non-empty.");
        Assert.IsFalse(output.Contains("Usage:"), "Version output should not contain usage help.");
    }

    /// <summary>
    /// Integration test verifying that the --help flag causes the tool to print usage information
    /// and exit with code 0, exercising the system-level CLI interface behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_CliInterface_HelpFlag_PrintsHelp()
    {
        // Act: run with --help flag as an external process
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--help");

        // Assert: tool exits successfully
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");

        // Assert: output contains usage information
        Assert.IsTrue(output.Contains("Usage:"), $"Expected help output to contain 'Usage:'. Output: {output}");
        Assert.IsTrue(output.Contains("Options:"), $"Expected help output to contain 'Options:'. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that the --log flag routes all output to the specified log file,
    /// exercising the system-level output control behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_OutputControl_LogFlag_WritesOutputToFile()
    {
        // Arrange: create a minimal requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-LogTest
                    title: The system shall do something.
                    tests:
                      - LogTest1
            """);

        var logFile = Path.Combine(_testDirectory, "output.log");

        // Act: run with --log flag to route output to a file
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--silent",
            "--log", logFile);

        // Assert: tool exited successfully
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}.");

        // Assert: log file was created with tool output
        Assert.IsTrue(File.Exists(logFile), "Log file should have been created.");
        var logContent = File.ReadAllText(logFile);
        Assert.IsFalse(string.IsNullOrWhiteSpace(logContent), "Log file should contain output.");
    }

    /// <summary>
    /// Integration test verifying that the --silent flag suppresses console output,
    /// exercising the system-level output control behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_OutputControl_SilentFlag_SuppressesConsoleOutput()
    {
        // Arrange: create a minimal requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-SilentTest
                    title: The system shall do something.
                    tests:
                      - SilentTest1
            """);

        // Act: run with --silent flag and capture output
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--silent");

        // Assert: tool exited successfully and produced no console output
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
        Assert.IsTrue(string.IsNullOrWhiteSpace(output), $"Expected no console output with --silent. Got: {output}");
    }

    /// <summary>
    /// Integration test verifying that requirements files using file includes correctly load
    /// all requirements from included files, exercising the system-level file includes behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_FileIncludes_RequirementsWithIncludes_LoadsAllRequirements()
    {
        // Arrange: create a child requirements file
        var childFile = Path.Combine(_testDirectory, "child-requirements.yaml");
        File.WriteAllText(childFile, """
            sections:
              - title: Child Requirements
                requirements:
                  - id: Integration-Child-Requirement
                    title: The system shall have a child requirement.
                    tests:
                      - ChildTest1
            """);

        // Arrange: create a root requirements file that includes the child file
        var rootFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(rootFile, """
            includes:
              - child-requirements.yaml
            sections:
              - title: Root Requirements
                requirements:
                  - id: Integration-Root-Requirement
                    title: The system shall have a root requirement.
                    tests:
                      - RootTest1
            """);

        var reportFile = Path.Combine(_testDirectory, "report.md");

        // Act: run with the root requirements file that uses includes
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--report", reportFile);

        // Assert: tool exited successfully
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");

        // Assert: report was generated containing requirements from both files
        Assert.IsTrue(File.Exists(reportFile), "Report file should have been generated.");
        var reportContent = File.ReadAllText(reportFile);
        Assert.IsTrue(
            reportContent.Contains("Integration-Root-Requirement"),
            "Report should contain the root requirement.");
        Assert.IsTrue(
            reportContent.Contains("Integration-Child-Requirement"),
            "Report should contain the included child requirement.");
    }

    /// <summary>
    /// Integration test verifying that the --log flag routes output to a file without
    /// requiring --silent, confirming independent operation of the log flag.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_OutputControl_LogFlag_WithoutSilent_WritesOutputToFileAndConsole()
    {
        // Arrange: create a minimal requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-LogWithoutSilent
                    title: The system shall do something.
                    tests:
                      - LogWithoutSilentTest1
            """);

        var logFile = Path.Combine(_testDirectory, "output.log");

        // Act: run with --log flag but without --silent
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--log", logFile);

        // Assert: tool exited successfully
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}.");

        // Assert: log file was created with tool output
        Assert.IsTrue(File.Exists(logFile), "Log file should have been created.");
        var logContent = File.ReadAllText(logFile);
        Assert.IsFalse(string.IsNullOrWhiteSpace(logContent), "Log file should contain output.");

        // Assert: console output was also produced (--silent was not specified)
        Assert.IsFalse(string.IsNullOrWhiteSpace(output), "Console output should not be suppressed without --silent.");
    }

    /// <summary>
    /// Integration test verifying that the --depth flag controls the Markdown heading level
    /// in the generated requirements report, exercising the system-level report depth behavior.
    /// </summary>
    [TestMethod]
    public void ReqStream_System_ReportDepth_DepthFlag_GeneratesReportWithCorrectHeadingLevel()
    {
        // Arrange: create a minimal requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Depth Test Section
                requirements:
                  - id: Integration-System-DepthTest
                    title: The system shall do something.
                    tests:
                      - DepthTest1
            """);

        var reportFile = Path.Combine(_testDirectory, "report.md");

        // Act: run with --depth 3 to use heading level 3 (###)
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--report", reportFile,
            "--depth", "3");

        // Assert: tool exited successfully
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");

        // Assert: report was generated with heading level 3
        Assert.IsTrue(File.Exists(reportFile), "Report file should have been generated.");
        var reportContent = File.ReadAllText(reportFile);
        Assert.Contains("### Depth Test Section", reportContent);
    }
}
