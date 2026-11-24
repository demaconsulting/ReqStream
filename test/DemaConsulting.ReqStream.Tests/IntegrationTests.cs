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

using System.Globalization;
using DemaConsulting.HelpCards.Catalog;
using DemaConsulting.HelpCards.Testing;
using DemaConsulting.ReqStream.Cli;
using DemaConsulting.ReqStream.Utilities;
namespace DemaConsulting.ReqStream.Tests;

/// <summary>
/// Integration tests for the ReqStream system, exercising the full pipeline across
/// multiple subsystems in end-to-end scenarios.
/// </summary>
public sealed class IntegrationTests : IDisposable
{
    private readonly string _dllPath;
    /// <summary>Temporary directory providing isolated file-system workspace for this test class instance.</summary>
    private readonly TemporaryDirectory _testDirectory = new();

    /// <summary>
    /// Initialize test by locating the DLL and creating a temporary test directory.
    /// </summary>
    public IntegrationTests()
    {
        _dllPath = PathHelpers.SafePathCombine(AppContext.BaseDirectory, "DemaConsulting.ReqStream.dll");
        Assert.True(File.Exists(_dllPath), $"Could not find ReqStream DLL at {_dllPath}");


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
    /// Integration test verifying that a single invocation can process requirements, load test
    /// results, generate a requirements report, justifications, and trace matrix, and enforce
    /// coverage — all subsystems working together correctly.
    /// </summary>
    [Fact]
    public void ReqStream_FullPipeline_WithCoveredRequirements_GeneratesAllReportsAndEnforces()
    {
        // Arrange: create requirements file with one covered requirement
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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
        var trxFile = _testDirectory.GetFilePath("results.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        var reportFile = _testDirectory.GetFilePath("requirements.md");
        var justificationsFile = _testDirectory.GetFilePath("justifications.md");
        var matrixFile = _testDirectory.GetFilePath("matrix.md");

        // Act: run the full pipeline as an external process
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "results.trx",
            "--report", reportFile,
            "--justifications", justificationsFile,
            "--matrix", matrixFile,
            "--enforce");

        // Assert: enforcement passed (exit code 0)
        Assert.Equal(0, exitCode);

        // Assert: all three output files were generated
        Assert.True(File.Exists(reportFile), "Requirements report should be generated.");
        Assert.True(File.Exists(justificationsFile), "Justifications report should be generated.");
        Assert.True(File.Exists(matrixFile), "Trace matrix report should be generated.");

        // Assert: report contains the requirement ID and title
        var reportContent = File.ReadAllText(reportFile);
        Assert.Contains("Integration-System-DoSomethingUseful", reportContent);
        Assert.Contains("The system shall do something useful.", reportContent);

        // Assert: trace matrix contains the satisfied requirement and its covering test
        var matrixContent = File.ReadAllText(matrixFile);
        var normalizedMatrixContent = matrixContent.Replace("\u200B", string.Empty);
        Assert.Contains("Integration-System-DoSomethingUseful", normalizedMatrixContent);
        Assert.Contains("IntegrationTest1", matrixContent);
    }

    /// <summary>
    /// Integration test verifying that enforcement mode causes a non-zero exit code when a
    /// requirement has no passing test evidence, confirming the CI/CD gate operates correctly.
    /// </summary>
    [Fact]
    public void ReqStream_EnforcementMode_RequirementLacksTestEvidence_FailsWithNonZeroExitCode()
    {
        // Arrange: create requirements file with one requirement that has no matching test
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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
        var trxFile = _testDirectory.GetFilePath("empty.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        // Act: run enforcement mode as an external process
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "empty.trx",
            "--enforce");

        // Assert: enforcement failed with a non-zero exit code
        Assert.NotEqual(0, exitCode);
    }

    /// <summary>
    /// Integration test verifying that source-specific test matching restricts coverage evidence
    /// to tests from the named result file, and that enforcement passes when the named source
    /// provides the required passing test.
    /// </summary>
    [Fact]
    public void ReqStream_SourceFilter_NamedSourceInRequirement_MatchesTestsBySourceFile()
    {
        // Arrange: create requirements file with source-specific test reference
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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
        var platformAFile = _testDirectory.GetFilePath("platform-a.trx");
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
        var platformBFile = _testDirectory.GetFilePath("platform-b.trx");
        File.WriteAllText(platformBFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(platformBResults));

        // Act: run enforcement using both result files as external process
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "platform-a.trx",
            "--tests", "platform-b.trx",
            "--enforce");

        // Assert: enforcement passed because platform-a.trx satisfies the source-filtered requirement
        Assert.Equal(0, exitCode);
    }

    /// <summary>
    /// Integration test verifying that the --lint flag exits silently with code 0 when a valid
    /// requirements file is provided, confirming the no-output-on-success design.
    /// </summary>
    [Fact]
    public void ReqStream_System_Lint_ValidRequirementsFile_ExitsSilentlyWithZero()
    {
        // Arrange: create a structurally valid requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--lint",
            "--requirements", "requirements.yaml");

        // Assert: lint exits with code 0 because no issues were found
        Assert.Equal(0, exitCode);

        // Assert: no output was produced (silent on success)
        Assert.Equal(string.Empty, output.Trim());
    }

    /// <summary>
    /// Integration test verifying that the --lint flag lints requirements files and reports
    /// structural issues in a single invocation, exercising the system-level lint behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_Lint_Flag_ReportsLintIssues()
    {
        // Arrange: create a requirements file with a structural issue (missing title)
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-MissingTitle
            """);

        // Act: run lint as an external process
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--lint",
            "--requirements", "requirements.yaml");

        // Assert: lint exits with a non-zero code because an issue was found
        Assert.NotEqual(0, exitCode);

        // Assert: lint reported an issue about the missing title
        Assert.True(
            output.Contains("Integration-System-MissingTitle") || output.Contains("title"),
            $"Expected lint output to reference the missing-title issue. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that the --validate flag runs the built-in self-test suite
    /// and exits successfully, exercising the system-level validate behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_Validate_Flag_RunsSelfValidation()
    {
        // Act: run validate as an external process
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--validate");

        // Assert: self-validation passes (exit code 0)
        Assert.Equal(0, exitCode);

        // Assert: output contains the validation summary header
        Assert.True(
            output.Contains("Passed") || output.Contains("Total Tests"),
            $"Expected validation output to contain test summary. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that the --validate --results flags write the self-test results
    /// to the specified file, exercising the system-level validate results output behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_ValidateResultsOutput_ResultsFlag_WritesResultsFile()
    {
        // Arrange: create a temporary file path for the results output
        var resultsFile = _testDirectory.GetFilePath("validation.trx");

        // Act: run validate with results flag as an external process
        var exitCode = Runner.Run(
            out _,
            "dotnet",
            _dllPath,
            "--validate",
            "--results", resultsFile);

        // Assert: self-validation passes (exit code 0)
        Assert.Equal(0, exitCode);

        // Assert: results file was created
        Assert.True(File.Exists(resultsFile), "Results file should be created by --results flag.");

        // Assert: results file is non-empty
        Assert.True(new FileInfo(resultsFile).Length > 0, "Results file should be non-empty.");
    }

    /// <summary>
    /// Integration test verifying that the --filter flag restricts requirements output to only
    /// those matching the specified tag, exercising the system-level tag-filter behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_TagFilter_Flag_FiltersRequirements()
    {
        // Arrange: create requirements file with two requirements, each with a different tag
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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

        var reportFile = _testDirectory.GetFilePath("filtered-report.md");

        // Act: run with --filter alpha to export only alpha-tagged requirements
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--report", reportFile,
            "--filter", "alpha");

        // Assert: tool exited successfully
        Assert.Equal(0, exitCode);

        // Assert: report was generated
        Assert.True(File.Exists(reportFile), "Filtered requirements report should be generated.");

        // Assert: report contains the alpha requirement but not the beta requirement
        var reportContent = File.ReadAllText(reportFile);
        Assert.Contains("Integration-System-TaggedAlpha", reportContent);
        Assert.DoesNotContain("Integration-System-TaggedBeta", reportContent);
    }

    /// <summary>
    /// Integration test verifying that the --version flag causes the tool to print version
    /// information and exit with code 0, exercising the system-level CLI interface behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_CliInterface_VersionFlag_PrintsVersion()
    {
        // Act: run with --version flag as an external process
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--version");

        // Assert: tool exits successfully
        Assert.Equal(0, exitCode);

        // Assert: output contains a version string (non-empty, no banner/help)
        Assert.False(string.IsNullOrWhiteSpace(output), "Expected version output to be non-empty.");
        Assert.DoesNotContain("Usage:", output);
    }

    /// <summary>
    /// Integration test verifying that the --help flag causes the tool to print usage information
    /// and exit with code 0, exercising the system-level CLI interface behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_CliInterface_HelpFlag_PrintsHelp()
    {
        // Act: run with --help flag as an external process
        var exitCode = Runner.Run(
            out var output,
            "dotnet",
            _dllPath,
            "--help");

        // Assert: tool exits successfully
        Assert.Equal(0, exitCode);

        // Assert: output contains usage information
        Assert.Contains("Usage:", output);
        Assert.Contains("Options:", output);
    }

    /// <summary>
    /// Integration test verifying that the --log flag routes all output to the specified log file,
    /// exercising the system-level output control behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_OutputControl_LogFlag_WritesOutputToFile()
    {
        // Arrange: create a minimal requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-LogTest
                    title: The system shall do something.
                    tests:
                      - LogTest1
            """);

        var logFile = _testDirectory.GetFilePath("output.log");

        // Act: run with --log flag to route output to a file
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--silent",
            "--log", logFile);

        // Assert: tool exited successfully
        Assert.Equal(0, exitCode);

        // Assert: log file was created with tool output
        Assert.True(File.Exists(logFile), "Log file should have been created.");
        var logContent = File.ReadAllText(logFile);
        Assert.False(string.IsNullOrWhiteSpace(logContent), "Log file should contain output.");
    }

    /// <summary>
    /// Integration test verifying that the --silent flag suppresses console output,
    /// exercising the system-level output control behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_OutputControl_SilentFlag_SuppressesConsoleOutput()
    {
        // Arrange: create a minimal requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
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
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--silent");

        // Assert: tool exited successfully and produced no console output
        Assert.Equal(0, exitCode);
        Assert.True(string.IsNullOrWhiteSpace(output), $"Expected no console output with --silent. Got: {output}");
    }

    /// <summary>
    /// Integration test verifying that requirements files using file includes correctly load
    /// all requirements from included files, exercising the system-level file includes behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_FileIncludes_RequirementsWithIncludes_LoadsAllRequirements()
    {
        // Arrange: create a child requirements file
        var childFile = _testDirectory.GetFilePath("child-requirements.yaml");
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
        var rootFile = _testDirectory.GetFilePath("requirements.yaml");
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

        var reportFile = _testDirectory.GetFilePath("report.md");

        // Act: run with the root requirements file that uses includes
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--report", reportFile);

        // Assert: tool exited successfully
        Assert.Equal(0, exitCode);

        // Assert: report was generated containing requirements from both files
        Assert.True(File.Exists(reportFile), "Report file should have been generated.");
        var reportContent = File.ReadAllText(reportFile);
        Assert.Contains("Integration-Root-Requirement", reportContent);
        Assert.Contains("Integration-Child-Requirement", reportContent);
    }

    /// <summary>
    /// Integration test verifying that the --log flag routes output to a file without
    /// requiring --silent, confirming independent operation of the log flag.
    /// </summary>
    [Fact]
    public void ReqStream_System_OutputControl_LogFlag_WithoutSilent_WritesOutputToFileAndConsole()
    {
        // Arrange: create a minimal requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-LogWithoutSilent
                    title: The system shall do something.
                    tests:
                      - LogWithoutSilentTest1
            """);

        var logFile = _testDirectory.GetFilePath("output.log");

        // Act: run with --log flag but without --silent
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--log", logFile);

        // Assert: tool exited successfully
        Assert.Equal(0, exitCode);

        // Assert: log file was created with tool output
        Assert.True(File.Exists(logFile), "Log file should have been created.");
        var logContent = File.ReadAllText(logFile);
        Assert.False(string.IsNullOrWhiteSpace(logContent), "Log file should contain output.");

        // Assert: console output was also produced (--silent was not specified)
        Assert.False(string.IsNullOrWhiteSpace(output), "Console output should not be suppressed without --silent.");
    }

    /// <summary>
    /// Integration test verifying that the --depth flag controls the Markdown heading level
    /// in the generated requirements report, exercising the system-level report depth behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_ReportDepth_DepthFlag_GeneratesReportWithCorrectHeadingLevel()
    {
        // Arrange: create a minimal requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Depth Test Section
                requirements:
                  - id: Integration-System-DepthTest
                    title: The system shall do something.
                    tests:
                      - DepthTest1
            """);

        var reportFile = _testDirectory.GetFilePath("report.md");

        // Act: run with --depth 3 to use heading level 3 (###)
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--report", reportFile,
            "--depth", "3");

        // Assert: tool exited successfully
        Assert.Equal(0, exitCode);

        // Assert: report was generated with heading level 3
        Assert.True(File.Exists(reportFile), "Report file should have been generated.");
        var reportContent = File.ReadAllText(reportFile);
        Assert.Contains("### Depth Test Section", reportContent);
    }

    /// <summary>
    /// Integration test verifying that circular include references are detected and reported
    /// as errors, exercising the system-level circular include detection behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_CircularIncludeDetection_CircularInclude_ReportsError()
    {
        // Arrange: create two files that include each other (circular)
        var fileA = _testDirectory.GetFilePath("a.yaml");
        var fileB = _testDirectory.GetFilePath("b.yaml");
        File.WriteAllText(fileA, """
            includes:
              - b.yaml
            sections:
              - title: A Requirements
                requirements:
                  - id: Circular-A-Req
                    title: Requirement A.
                    tests:
                      - TestA
            """);
        File.WriteAllText(fileB, """
            includes:
              - a.yaml
            sections:
              - title: B Requirements
                requirements:
                  - id: Circular-B-Req
                    title: Requirement B.
                    tests:
                      - TestB
            """);

        // Act: run lint with the circular-include root file
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--lint",
            "--requirements", "a.yaml");

        // Assert: lint exits with a non-zero code because a circular include was detected
        Assert.NotEqual(0, exitCode);

        // Assert: lint reported a circular-include error
        Assert.True(
            output.Contains("circular") || output.Contains("Circular") || output.Contains("cycle") || output.Contains("already"),
            $"Expected lint output to reference the circular include issue. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that sections with the same title in different included
    /// files are automatically merged into a single section, exercising the system-level
    /// section merging behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_SectionMerging_TwoFilesWithSameSection_ProducesSingleMergedSection()
    {
        // Arrange: create two child files both contributing to the same section title
        var childFileA = _testDirectory.GetFilePath("child-a.yaml");
        File.WriteAllText(childFileA, """
            sections:
              - title: Shared Section
                requirements:
                  - id: Merge-A-Req
                    title: Requirement from file A.
                    tests:
                      - MergeTestA
            """);

        var childFileB = _testDirectory.GetFilePath("child-b.yaml");
        File.WriteAllText(childFileB, """
            sections:
              - title: Shared Section
                requirements:
                  - id: Merge-B-Req
                    title: Requirement from file B.
                    tests:
                      - MergeTestB
            """);

        // Arrange: create root file that includes both children
        var rootFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(rootFile, """
            includes:
              - child-a.yaml
              - child-b.yaml
            """);

        var reportFile = _testDirectory.GetFilePath("report.md");

        // Act: generate a report from the merged requirements
        var exitCode = Runner.RunInDirectory(
            out _,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--report", reportFile);

        // Assert: tool exited successfully
        Assert.Equal(0, exitCode);

        // Assert: report was generated
        Assert.True(File.Exists(reportFile), "Report file should have been generated.");

        // Assert: report contains requirements from both files under the merged section
        var reportContent = File.ReadAllText(reportFile);
        Assert.Contains("Merge-A-Req", reportContent);
        Assert.Contains("Merge-B-Req", reportContent);

        // Assert: the section title appears only once (sections were merged, not duplicated)
        var sectionOccurrences = System.Text.RegularExpressions.Regex.Matches(
            reportContent, @"Shared Section").Count;
        Assert.Equal(1, sectionOccurrences);
    }

    /// <summary>
    /// Integration test verifying that the tool reports a fatal error when a specified test
    /// result file is missing, exercising the system-level test file error handling behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_TestFileErrorHandling_MissingTestFile_ReportsFatalError()
    {
        // Arrange: create a minimal requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-MissingTestFile
                    title: The system shall do something.
                    tests:
                      - SomeTest
            """);

        // Act: run with a non-existent test file path
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "nonexistent.trx",
            "--enforce");

        // Assert: tool exits with a non-zero code due to the missing file
        Assert.NotEqual(0, exitCode);

        // Assert: output contains an error referencing the missing file
        Assert.True(
            output.Contains("nonexistent.trx") || output.Contains("not found") || output.Contains("error") || output.Contains("Error"),
            $"Expected error output referencing the missing test file. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that the tool reports a fatal error when a test result file
    /// cannot be parsed, exercising the system-level test file error handling behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_TestFileErrorHandling_MalformedTestFile_ReportsFatalError()
    {
        // Arrange: create a minimal requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-MalformedTestFile
                    title: The system shall do something.
                    tests:
                      - SomeTest
            """);

        // Arrange: create a malformed test result file (not valid TRX or JUnit XML)
        var malformedFile = _testDirectory.GetFilePath("malformed.trx");
        File.WriteAllText(malformedFile, "this is not valid XML or TRX content <<<");

        // Act: run with the malformed test file path
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--tests", "malformed.trx",
            "--enforce");

        // Assert: tool exits with a non-zero code due to the malformed file
        Assert.NotEqual(0, exitCode);

        // Assert: output contains an error referencing the malformed file
        Assert.True(
            output.Contains("malformed.trx") || output.Contains("error") || output.Contains("Error") || output.Contains("parse"),
            $"Expected error output referencing the malformed test file. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that requesting --matrix without providing any --tests files
    /// reports an error, exercising the system-level matrix error handling behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_MatrixErrorHandling_MatrixWithoutTests_ReportsError()
    {
        // Arrange: create a minimal requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-MatrixNoTests
                    title: The system shall do something.
                    tests:
                      - SomeTest
            """);

        var matrixFile = _testDirectory.GetFilePath("matrix.md");

        // Act: run with --matrix but without --tests
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--matrix", matrixFile);

        // Assert: tool exits with a non-zero code
        Assert.NotEqual(0, exitCode);

        // Assert: output contains an error about missing test files
        Assert.True(
            output.Contains("test") || output.Contains("Test") || output.Contains("matrix") || output.Contains("Matrix") || output.Contains("error") || output.Contains("Error"),
            $"Expected error output about missing test files for matrix generation. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that cyclic references in the child-requirement graph are
    /// detected and reported as errors. This tests children-graph cycles (requirement A has
    /// B as child, B has A as child), which is distinct from circular include detection.
    /// </summary>
    [Fact]
    public void ReqStream_System_CyclicChildDetection_CyclicChildRequirements_ReportsError()
    {
        // Arrange: create a requirements file with a cyclic children graph
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Cyclic-Req-A
                    title: Requirement A references B as child.
                    tests:
                      - TestA
                    children:
                      - Cyclic-Req-B
                  - id: Cyclic-Req-B
                    title: Requirement B references A as child (creating a cycle).
                    tests:
                      - TestB
                    children:
                      - Cyclic-Req-A
            """);

        // Act: run lint with the cyclic requirements file
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--lint",
            "--requirements", "requirements.yaml");

        // Assert: lint exits with a non-zero code because a cycle was detected
        Assert.NotEqual(0, exitCode);

        // Assert: output contains an error referencing the cycle
        Assert.True(
            output.Contains("cycle") || output.Contains("Cycle") || output.Contains("circular") || output.Contains("Circular"),
            $"Expected lint output to reference the cyclic children graph. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that using --enforce without --tests causes the tool to report
    /// a clear error, exercising the system-level enforce-no-tests error handling behavior.
    /// </summary>
    [Fact]
    public void ReqStream_System_EnforceNoTests_EnforceWithoutTests_ReportsError()
    {
        // Arrange: create a minimal requirements file
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-EnforceNoTests
                    title: The system shall do something.
                    tests:
                      - SomeTest
            """);

        // Act: run with --enforce but without --tests
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--enforce");

        // Assert: tool exits with a non-zero code
        Assert.NotEqual(0, exitCode);

        // Assert: output contains an error about missing test files for enforcement
        Assert.True(
            output.Contains("test") || output.Contains("Test") || output.Contains("enforce") || output.Contains("Enforce") || output.Contains("error") || output.Contains("Error"),
            $"Expected error output about missing test files for enforcement. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that a requirement not reachable from any root-tagged
    /// requirement is reported as an orphan, and that --enforce turns the orphan report into a
    /// build-breaking error, exercising the full YAML-to-CLI orphan-checking pipeline.
    /// </summary>
    [Fact]
    public void ReqStream_System_OrphanChecking_RootTagsWithOrphan_EnforceReportsError()
    {
        // Arrange: a root-tagged requirement and an unreachable orphan requirement
        var reqFile = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(reqFile, """
            root-tags: [product]
            sections:
              - title: System Requirements
                requirements:
                  - id: Integration-System-OrphanRoot
                    title: The system shall do the root thing.
                    tags: [product]
                  - id: Integration-System-Orphan
                    title: The system shall do the orphaned thing.
            """);

        // Act: run with --enforce and root-tags configured via YAML, no --tests
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory.DirectoryPath,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--enforce");

        // Assert: tool exits with a non-zero code, reporting the orphan as an error
        Assert.NotEqual(0, exitCode);
        Assert.Contains("orphaned", output, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Integration-System-Orphan", output);
    }

    /// <summary>
    /// Integration test verifying that the help verb lists the topics, answering before the
    /// regular option parser would reject it, and exits with code 0.
    /// </summary>
    [Fact]
    public void ReqStream_System_HelpTopics_HelpCommand_ListsTopics()
    {
        // Act: run the tool with the bare help verb
        var exitCode = Runner.Run(out var output, "dotnet", _dllPath, "help");

        // Assert: the topics are listed and the command succeeded without a banner
        Assert.Equal(0, exitCode);
        Assert.Contains("reqstream help getting-started", output);
        Assert.Contains("reqstream help orphans", output);
        Assert.DoesNotContain("Copyright", output);
    }

    /// <summary>
    /// Integration test verifying that a help card path prints the card with its worked examples.
    /// </summary>
    [Fact]
    public void ReqStream_System_HelpTopics_HelpCard_PrintsCardWithExamples()
    {
        // Act: run the tool for a card that has a child card
        var exitCode = Runner.Run(out var output, "dotnet", _dllPath, "help", "orphans");

        // Assert: the card prints with its examples
        Assert.Equal(0, exitCode);
        Assert.Contains("# Orphans and root tags", output);
        Assert.Contains("## Examples", output);
        Assert.Contains("lint-enforce [command]", output);

        // Act: print one example alone
        exitCode = Runner.Run(out output, "dotnet", _dllPath, "help", "orphans", "--example", "lint-enforce");

        // Assert: only the command line is printed
        Assert.Equal(0, exitCode);
        Assert.Equal("reqstream --requirements requirements.yaml --lint --enforce", output.Trim());
    }

    /// <summary>
    /// Integration test verifying that help search finds the cards that match the words.
    /// </summary>
    [Fact]
    public void ReqStream_System_HelpTopics_HelpSearch_FindsCards()
    {
        // Act: search for the source-filter syntax
        var exitCode = Runner.Run(out var output, "dotnet", _dllPath, "help", "--search", "windows");

        // Assert: the source-filter card is among the results
        Assert.Equal(0, exitCode);
        Assert.Contains("reqstream help test-linking source-filters", output);
    }

    /// <summary>
    /// Integration test verifying that help writes a JSON document for agents with --json, and
    /// that an unknown topic exits with code 2.
    /// </summary>
    [Fact]
    public void ReqStream_System_HelpTopics_HelpJson_WritesJsonDocument()
    {
        // Act: print the topic index as JSON
        var exitCode = Runner.Run(out var output, "dotnet", _dllPath, "help", "--json");

        // Assert: a versioned JSON document that lists the topics
        Assert.Equal(0, exitCode);
        Assert.StartsWith("{", output.Trim());
        Assert.Contains("\"helpSchemaVersion\":1", output);
        Assert.Contains("getting-started", output);

        // Act: ask for an unknown topic
        exitCode = Runner.Run(out output, "dotnet", _dllPath, "help", "no-such-topic");

        // Assert: the usage-error exit code and a hint
        Assert.Equal(2, exitCode);
        Assert.Contains("no-such-topic", output);
    }

    /// <summary>
    /// Integration test verifying that every command example of every help card works: for each card the
    /// source examples are written to a fresh scratch folder and the command examples run against the
    /// built tool with the exit code the card claims.
    /// </summary>
    [Fact]
    public void ReqStream_System_HelpExamples_AllCommandExamples_RunAgainstScratchSet()
    {
        // Arrange: the catalog the tool ships. Command examples are expected to exit with 0 and print
        // nothing about failure unless listed here with the exit code and text they demonstrate.
        var catalog = HelpCommand.CreateCatalog(CultureInfo.InvariantCulture);
        var expectations = new Dictionary<string, (int ExitCode, string OutputContains)>(StringComparer.Ordinal)
        {
            ["lint#lint-broken"] = (1, "Unknown field 'priority' in requirement"),
            ["orphans#lint-warning"] = (0, "Warning: 1 of 3 requirements orphaned"),
            ["orphans#lint-enforce"] = (1, "Error: 1 of 3 requirements orphaned"),
            ["enforcement#enforce-fail"] = (1, "Error: Only 1 of 3 requirements are satisfied with tests."),
            ["enforcement#enforce-nothing"] = (1, "Cannot enforce requirements without test results or root tags"),
            ["reports/filter#enforce-all"] = (1, "Error: Only 1 of 3 requirements are satisfied with tests."),
            ["command-line/self-validation#validate-bad-extension"] = (1, "Unsupported results file format"),
            ["troubleshooting#no-files"] = (0, "No requirements files specified."),
            ["troubleshooting#unknown-option"] = (1, "Unsupported argument '--lint-all'"),
            ["troubleshooting/requirements-errors#lint-broken"] = (1, "Duplicate requirement ID 'Shop-Cart-Total'"),
            ["troubleshooting/requirements-errors#lint-loop"] = (1, "Circular include detected"),
            ["troubleshooting/test-errors#unsatisfied"] = (1, "Error: Only 0 of 1 requirements are satisfied with tests."),
            ["troubleshooting/test-errors#no-result-files"] = (1, "No test result files were provided or matched"),
            ["troubleshooting/test-errors#bad-result-file"] = (1, "Failed to parse test result file")
        };
        var failures = new List<string>();
        var used = new HashSet<string>(StringComparer.Ordinal);
        var commandCount = 0;

        // Act: run the command examples of each card in order, in a scratch folder holding the card's sources
        foreach (var card in catalog.Cards.Where(c => c.Examples.Any(e => e.Kind == HelpExample.CommandKind)))
        {
            using var scratch = new TemporaryDirectory();
            foreach (var source in card.Examples.Where(e => e.Kind != HelpExample.CommandKind))
            {
                var path = Path.Combine(scratch.DirectoryPath, source.FileName!);
                Directory.CreateDirectory(Path.GetDirectoryName(path)!);
                File.WriteAllText(path, source.Content + "\n");
            }

            foreach (var example in card.Examples.Where(e => e.Kind == HelpExample.CommandKind))
            {
                var key = $"{card.Id}#{example.Name}";
                var parsed = ExampleCommandLine.TryParse(example.Content, HelpCommand.ToolName, out var arguments, out var error);
                if (!parsed)
                {
                    failures.Add($"{key}: does not parse: {error}");
                    continue;
                }

                var expected = expectations.TryGetValue(key, out var found) ? found : (ExitCode: 0, OutputContains: string.Empty);
                used.Add(key);
                commandCount++;

                var exitCode = Runner.RunInDirectory(out var output, scratch.DirectoryPath, "dotnet", [_dllPath, .. arguments]);
                if (exitCode != expected.ExitCode)
                {
                    failures.Add($"{key}: expected exit code {expected.ExitCode} but was {exitCode}. Output: {output}");
                }
                else if (!output.Contains(expected.OutputContains, StringComparison.Ordinal))
                {
                    failures.Add($"{key}: output does not contain '{expected.OutputContains}'. Output: {output}");
                }
            }
        }

        // Assert: no example failed, and every expectation names an example that exists
        Assert.True(commandCount > 20, $"Expected more than 20 command examples but ran {commandCount}.");
        failures.AddRange(expectations.Keys.Where(key => !used.Contains(key)).Select(key => $"{key}: expectation names no example"));
        Assert.True(failures.Count == 0, string.Join(Environment.NewLine, failures));
    }
}
