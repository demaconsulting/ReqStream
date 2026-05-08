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

using System.Reflection;
using System.Runtime.InteropServices;
using DemaConsulting.ReqStream.Cli;
using DemaConsulting.ReqStream.Utilities;
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.ReqStream.SelfTest;

/// <summary>
///     Provides self-validation functionality for the ReqStream tool.
/// </summary>
public static class Validation
{
    /// <summary>
    ///     Runs self-validation tests and optionally writes results to a file.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    public static void Run(Context context)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(context);

        // Print validation header
        PrintValidationHeader(context);

        // Create test results collection
        var testResults = new DemaConsulting.TestResults.TestResults();
        testResults.Name = "ReqStream Self-Validation";

        // Run core functionality tests
        RunRequirementsProcessingTest(context, testResults);
        RunTraceMatrixTest(context, testResults);
        RunReportExportTest(context, testResults);
        RunTagsFilteringTest(context, testResults);
        RunEnforcementModeTest(context, testResults);
        RunLintTest(context, testResults);

        // Calculate totals
        var totalTests = testResults.Results.Count;
        var passedTests = testResults.Results.Count(t => t.Outcome == DemaConsulting.TestResults.TestOutcome.Passed);
        var failedTests = testResults.Results.Count(t => t.Outcome == DemaConsulting.TestResults.TestOutcome.Failed);

        // Print summary
        context.WriteLine("");
        context.WriteLine($"Total Tests: {totalTests}");
        context.WriteLine($"Passed: {passedTests}");
        if (failedTests > 0)
        {
            context.WriteError($"Failed: {failedTests}");
        }
        else
        {
            context.WriteLine($"Failed: {failedTests}");
        }

        // Write results file if requested
        if (context.ResultsFile != null)
        {
            WriteResultsFile(context, testResults);
        }
    }

    /// <summary>
    ///     Prints the validation header with system information.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintValidationHeader(Context context)
    {
        context.WriteLine($"{new string('#', context.Depth)} DEMA Consulting ReqStream");
        context.WriteLine("");
        context.WriteLine("| Information         | Value                                              |");
        context.WriteLine("| :------------------ | :------------------------------------------------- |");
        context.WriteLine($"| ReqStream Version   | {Program.Version,-50} |");
        context.WriteLine($"| Machine Name        | {Environment.MachineName,-50} |");
        context.WriteLine($"| OS Version          | {RuntimeInformation.OSDescription,-50} |");
        context.WriteLine($"| DotNet Runtime      | {RuntimeInformation.FrameworkDescription,-50} |");
        context.WriteLine($"| Time Stamp          | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC{"",-29} |");
        context.WriteLine("");
    }

    /// <summary>
    ///     Runs a test for requirements processing functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunRequirementsProcessingTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReqStream_RequirementsProcessing");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create a simple requirements file
            var reqFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "test-requirements.yaml");
            var yaml = @"sections:
  - title: Test Requirements
    requirements:
      - id: TEST-001
        title: Test requirement one
      - id: TEST-002
        title: Test requirement two
";
            File.WriteAllText(reqFile, yaml);

            // Create a log file to capture output
            var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "requirements-test.log");

            using (new DirectorySwitch(tempDir.DirectoryPath))
            {
                // Run the program with requirements file (using relative pattern)
                int exitCode;
                using (var testContext = Context.Create(["--silent", "--log", "requirements-test.log", "--requirements", "*.yaml"]))
                {
                    Program.Run(testContext);
                    exitCode = testContext.ExitCode;
                }

                // Check if execution succeeded
                if (exitCode == 0)
                {
                    // Verify log contains expected output (read after context is disposed to ensure log is flushed)
                    var logContent = File.ReadAllText(logFile);

                    if (logContent.Contains("Reading 1 requirements file(s)") &&
                        logContent.Contains("Requirements loaded successfully"))
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                        context.WriteLine("✓ ReqStream_RequirementsProcessing - Passed");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "Expected output not found in log";
                        context.WriteError("✗ ReqStream_RequirementsProcessing - Failed: Expected output not found");
                    }
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = $"Program exited with code {exitCode}";
                    context.WriteError($"✗ ReqStream_RequirementsProcessing - Failed: Exit code {exitCode}");
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReqStream_RequirementsProcessing", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for trace matrix functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunTraceMatrixTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReqStream_TraceMatrix");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create requirements and test results files
            var reqFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "matrix-requirements.yaml");
            var reqYaml = @"sections:
  - title: Matrix Test
    requirements:
      - id: MTX-001
        title: Matrix requirement
        tests:
          - Test_Matrix_Validation
";
            File.WriteAllText(reqFile, reqYaml);

            // Create a simple TRX file
            var trxFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "test-results.trx");
            var testData = new DemaConsulting.TestResults.TestResults { Name = "ValidationTests" };
            testData.Results.Add(new DemaConsulting.TestResults.TestResult
            {
                Name = "Test_Matrix_Validation",
                ClassName = "MatrixTests",
                CodeBase = "Tests.dll",
                Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
                Duration = TimeSpan.FromSeconds(1)
            });
            File.WriteAllText(trxFile, TrxSerializer.Serialize(testData));

            using (new DirectorySwitch(tempDir.DirectoryPath))
            {
                // Run the program with trace matrix (using relative paths)
                int exitCode;
                using (var testContext = Context.Create(["--silent", "--log", "matrix-test.log", "--requirements", "*-requirements.yaml",
                                                          "--tests", "*.trx", "--matrix", "matrix.md"]))
                {
                    Program.Run(testContext);
                    exitCode = testContext.ExitCode;
                }

                // Check if execution succeeded and matrix file was created (check after context disposed)
                var matrixFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "matrix.md");
                if (exitCode == 0 && File.Exists(matrixFile))
                {
                    var matrixContent = File.ReadAllText(matrixFile);
                    if (matrixContent.Contains("MTX-001") && matrixContent.Contains("Test_Matrix_Validation"))
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                        context.WriteLine("✓ ReqStream_TraceMatrix - Passed");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "Matrix file missing expected content";
                        context.WriteError("✗ ReqStream_TraceMatrix - Failed: Matrix file missing expected content");
                    }
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = exitCode != 0
                        ? $"Program exited with code {exitCode}"
                        : "Matrix file not created";
                    context.WriteError($"✗ ReqStream_TraceMatrix - Failed: {test.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReqStream_TraceMatrix", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for report export functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunReportExportTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReqStream_ReportExport");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create a requirements file
            var reqFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "export-requirements.yaml");
            var reqYaml = @"sections:
  - title: Export Test
    requirements:
      - id: EXP-001
        title: Export requirement
";
            File.WriteAllText(reqFile, reqYaml);

            using (new DirectorySwitch(tempDir.DirectoryPath))
            {
                // Run the program with report export (using relative paths)
                int exitCode;
                using (var testContext = Context.Create(["--silent", "--log", "export-test.log", "--requirements", "*-requirements.yaml",
                                                          "--report", "report.md"]))
                {
                    Program.Run(testContext);
                    exitCode = testContext.ExitCode;
                }

                // Check if execution succeeded and report file was created (check after context disposed)
                var reportFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "report.md");
                if (exitCode == 0 && File.Exists(reportFile))
                {
                    var reportContent = File.ReadAllText(reportFile);
                    if (reportContent.Contains("EXP-001") && reportContent.Contains("Export requirement"))
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                        context.WriteLine("✓ ReqStream_ReportExport - Passed");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "Report file missing expected content";
                        context.WriteError("✗ ReqStream_ReportExport - Failed: Report file missing expected content");
                    }
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = exitCode != 0
                        ? $"Program exited with code {exitCode}"
                        : "Report file not created";
                    context.WriteError($"✗ ReqStream_ReportExport - Failed: {test.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReqStream_ReportExport", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for requirement tags filtering functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunTagsFilteringTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReqStream_TagsFiltering");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create requirements file with tagged requirements
            var reqFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "requirements.yaml");
            var reqYaml = @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Tagged requirement
        tags:
          - test-tag
      - id: REQ-002
        title: Untagged requirement
";
            File.WriteAllText(reqFile, reqYaml);

            using (new DirectorySwitch(tempDir.DirectoryPath))
            {
                // Test filtering with --filter argument
                int exitCode;
                using (var testContext = Context.Create(
                    ["--silent", "--log", "filter-test.log", "--requirements", "*.yaml", "--filter", "test-tag", "--report", "filtered.md"]))
                {
                    Program.Run(testContext);
                    exitCode = testContext.ExitCode;
                }

                // Check if execution succeeded and filtered report was created
                var filteredReportPath = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "filtered.md");
                if (exitCode == 0 && File.Exists(filteredReportPath))
                {
                    var reportContent = File.ReadAllText(filteredReportPath);

                    // Verify filtered report contains only tagged requirement
                    if (reportContent.Contains("REQ-001") && !reportContent.Contains("REQ-002"))
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                        context.WriteLine("✓ ReqStream_TagsFiltering - Passed");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "Filtered report did not contain expected requirements";
                        context.WriteError("✗ ReqStream_TagsFiltering - Failed: Filtering not working correctly");
                    }
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = $"Program exited with code {exitCode} or report file not created";
                    context.WriteError($"✗ ReqStream_TagsFiltering - Failed: {test.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReqStream_TagsFiltering", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for enforcement mode functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunEnforcementModeTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReqStream_EnforcementMode");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create a requirements file with a requirement linked to a test
            var reqFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "enforce-requirements.yaml");
            var reqYaml = @"sections:
  - title: Enforce Test
    requirements:
      - id: ENF-001
        title: Enforcement requirement
        tests:
          - Test_Enforce_Validation
";
            File.WriteAllText(reqFile, reqYaml);

            // Create a TRX file with a passing test
            var trxFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "test-results.trx");
            var testData = new DemaConsulting.TestResults.TestResults { Name = "EnforceTests" };
            testData.Results.Add(new DemaConsulting.TestResults.TestResult
            {
                Name = "Test_Enforce_Validation",
                ClassName = "EnforceTests",
                CodeBase = "Tests.dll",
                Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
                Duration = TimeSpan.FromSeconds(1)
            });
            File.WriteAllText(trxFile, TrxSerializer.Serialize(testData));

            using (new DirectorySwitch(tempDir.DirectoryPath))
            {
                // Verify that --enforce succeeds when all requirements are satisfied
                int exitCode;
                using (var testContext = Context.Create(["--silent", "--requirements", "*-requirements.yaml",
                                                          "--tests", "*.trx", "--enforce"]))
                {
                    Program.Run(testContext);
                    exitCode = testContext.ExitCode;
                }

                if (exitCode != 0)
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = $"Enforcement with satisfied requirements should succeed, but exited with code {exitCode}";
                    context.WriteError($"✗ ReqStream_EnforcementMode - Failed: {test.ErrorMessage}");
                }
                else
                {
                    // Create an unsatisfied requirements file for the second check
                    var unsatisfiedReqFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "unsatisfied-requirements.yaml");
                    var unsatisfiedYaml = @"sections:
  - title: Unsatisfied Test
    requirements:
      - id: UNS-001
        title: Unsatisfied requirement
        tests:
          - Test_NonExistent
";
                    File.WriteAllText(unsatisfiedReqFile, unsatisfiedYaml);

                    // Verify that --enforce fails when requirements are not satisfied
                    using (var testContext = Context.Create(["--silent", "--requirements", "unsatisfied-requirements.yaml",
                                                              "--tests", "*.trx", "--enforce"]))
                    {
                        Program.Run(testContext);
                        exitCode = testContext.ExitCode;
                    }

                    if (exitCode == 0)
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "Enforcement with unsatisfied requirements should fail, but succeeded";
                        context.WriteError($"✗ ReqStream_EnforcementMode - Failed: {test.ErrorMessage}");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                        context.WriteLine("✓ ReqStream_EnforcementMode - Passed");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReqStream_EnforcementMode", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Runs a test for lint functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunLintTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = CreateTestResult("ReqStream_Lint");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create a valid requirements file
            var reqFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "lint-requirements.yaml");
            var reqYaml = @"sections:
  - title: Lint Test
    requirements:
      - id: LNT-001
        title: Lint requirement
";
            File.WriteAllText(reqFile, reqYaml);

            // Create a requirements file with a known issue (duplicate ID)
            var badReqFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "bad-requirements.yaml");
            var badReqYaml = @"sections:
  - title: Bad Lint Test
    requirements:
      - id: LNT-001
        title: Duplicate requirement ID
";
            File.WriteAllText(badReqFile, badReqYaml);

            using (new DirectorySwitch(tempDir.DirectoryPath))
            {
                // Test 1: Lint a valid file - should succeed with no issues
                int exitCode;
                string logContent;
                var logFile = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "lint-test.log");

                using (var testContext = Context.Create(["--silent", "--log", "lint-test.log", "--lint",
                                                          "--requirements", "lint-requirements.yaml"]))
                {
                    Program.Run(testContext);
                    exitCode = testContext.ExitCode;
                }

                logContent = File.ReadAllText(logFile);

                if (exitCode != 0 || !string.IsNullOrEmpty(logContent.Trim()))
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Lint of valid file should succeed with no output";
                    context.WriteError($"✗ ReqStream_Lint - Failed: {test.ErrorMessage}");
                    FinalizeTestResult(test, startTime, testResults);
                    return;
                }

                // Test 2: Lint a file with a duplicate ID - should fail
                var logFile2 = PathHelpers.SafePathCombine(tempDir.DirectoryPath, "lint-test2.log");
                using (var testContext = Context.Create(["--silent", "--log", "lint-test2.log", "--lint",
                                                          "--requirements", "lint-requirements.yaml",
                                                          "--requirements", "bad-requirements.yaml"]))
                {
                    Program.Run(testContext);
                    exitCode = testContext.ExitCode;
                }

                var logContent2 = File.ReadAllText(logFile2);

                if (exitCode == 0 || !logContent2.Contains("Duplicate requirement ID 'LNT-001'"))
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Lint of file with duplicate ID should fail";
                    context.WriteError($"✗ ReqStream_Lint - Failed: {test.ErrorMessage}");
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                    context.WriteLine("✓ ReqStream_Lint - Passed");
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "ReqStream_Lint", ex);
        }

        FinalizeTestResult(test, startTime, testResults);
    }

    /// <summary>
    ///     Writes test results to a file in TRX or JUnit format.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results to write.</param>
    private static void WriteResultsFile(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        // Defensive guard: the only call site already checks context.ResultsFile != null,
        // but this guard protects against future callers that may not.
        if (context.ResultsFile == null)
        {
            return;
        }

        try
        {
            var extension = Path.GetExtension(context.ResultsFile).ToLowerInvariant();
            string content;

            if (extension == ".trx")
            {
                content = TrxSerializer.Serialize(testResults);
            }
            else if (extension == ".xml")
            {
                // Assume JUnit format for .xml extension
                content = JUnitSerializer.Serialize(testResults);
            }
            else
            {
                context.WriteError($"Error: Unsupported results file format '{extension}'. Use .trx or .xml extension.");
                return;
            }

            File.WriteAllText(context.ResultsFile, content);
            context.WriteLine($"Results written to {context.ResultsFile}");
        }
        catch (Exception ex)
        {
            context.WriteError($"Error: Failed to write results file: {ex.Message}");
        }
    }

    /// <summary>
    ///     Creates a new test result object with common properties.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <returns>A new test result object.</returns>
    private static DemaConsulting.TestResults.TestResult CreateTestResult(string testName)
    {
        return new DemaConsulting.TestResults.TestResult
        {
            Name = testName,
            ClassName = "Validation",
            CodeBase = "ReqStream"
        };
    }

    /// <summary>
    ///     Finalizes a test result by setting its duration and adding it to the collection.
    /// </summary>
    /// <param name="test">The test result to finalize.</param>
    /// <param name="startTime">The start time of the test.</param>
    /// <param name="testResults">The test results collection to add to.</param>
    private static void FinalizeTestResult(
        DemaConsulting.TestResults.TestResult test,
        DateTime startTime,
        DemaConsulting.TestResults.TestResults testResults)
    {
        test.Duration = DateTime.UtcNow - startTime;
        testResults.Results.Add(test);
    }

    /// <summary>
    ///     Handles test exceptions by setting failure information and logging the error.
    /// </summary>
    /// <param name="test">The test result to update.</param>
    /// <param name="context">The context for output.</param>
    /// <param name="testName">The name of the test for error messages.</param>
    /// <param name="ex">The exception that occurred.</param>
    private static void HandleTestException(
        DemaConsulting.TestResults.TestResult test,
        Context context,
        string testName,
        Exception ex)
    {
        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
        test.ErrorMessage = $"Exception: {ex.Message}";
        context.WriteError($"✗ {testName} - Failed: {ex.Message}");
    }

    /// <summary>
    ///     Represents a temporary directory that is automatically deleted when disposed.
    /// </summary>
    private sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        ///     Gets the path to the temporary directory.
        /// </summary>
        public string DirectoryPath { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
        /// </summary>
        public TemporaryDirectory()
        {
            DirectoryPath = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reqstream_validation_{Guid.NewGuid()}");
            Directory.CreateDirectory(DirectoryPath);
        }

        /// <summary>
        ///     Deletes the temporary directory and all its contents.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(DirectoryPath, true);
            }
        }
    }

    /// <summary>
    ///     Represents a directory switch that restores the original directory when disposed.
    /// </summary>
    private sealed class DirectorySwitch : IDisposable
    {
        private readonly string _originalDirectory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DirectorySwitch"/> class.
        /// </summary>
        /// <param name="newDirectory">The directory to switch to.</param>
        public DirectorySwitch(string newDirectory)
        {
            _originalDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(newDirectory);
        }

        /// <summary>
        ///     Restores the original directory.
        /// </summary>
        public void Dispose()
        {
            Directory.SetCurrentDirectory(_originalDirectory);
        }
    }
}
