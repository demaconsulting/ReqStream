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
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.ReqStream;

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
        // Print validation header
        PrintValidationHeader(context);

        // Create test results collection
        var testResults = new DemaConsulting.TestResults.TestResults();
        testResults.Name = "ReqStream Self-Validation";

        // Run core functionality tests
        RunRequirementsProcessingTest(context, testResults);
        RunTraceMatrixTest(context, testResults);
        RunReportExportTest(context, testResults);

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
        context.WriteLine("# DEMA Consulting ReqStream");
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
        var test = CreateTestResult("RequirementsProcessing");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create a simple requirements file
            var reqFile = Path.Combine(tempDir.Path, "test-requirements.yaml");
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
            var logFile = Path.Combine(tempDir.Path, "requirements-test.log");

            using (new DirectorySwitch(tempDir.Path))
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
                        context.WriteLine("✓ Requirements Processing Test - PASSED");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "Expected output not found in log";
                        context.WriteError("✗ Requirements Processing Test - FAILED: Expected output not found");
                    }
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = $"Program exited with code {exitCode}";
                    context.WriteError($"✗ Requirements Processing Test - FAILED: Exit code {exitCode}");
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "Requirements Processing Test", ex);
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
        var test = CreateTestResult("TraceMatrix");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create requirements and test results files
            var reqFile = Path.Combine(tempDir.Path, "matrix-requirements.yaml");
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
            var trxFile = Path.Combine(tempDir.Path, "test-results.trx");
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

            using (new DirectorySwitch(tempDir.Path))
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
                var matrixFile = Path.Combine(tempDir.Path, "matrix.md");
                if (exitCode == 0 && File.Exists(matrixFile))
                {
                    var matrixContent = File.ReadAllText(matrixFile);
                    if (matrixContent.Contains("MTX-001") && matrixContent.Contains("Test_Matrix_Validation"))
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                        context.WriteLine("✓ Trace Matrix Test - PASSED");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "Matrix file missing expected content";
                        context.WriteError("✗ Trace Matrix Test - FAILED: Matrix file missing expected content");
                    }
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = exitCode != 0 
                        ? $"Program exited with code {exitCode}" 
                        : "Matrix file not created";
                    context.WriteError($"✗ Trace Matrix Test - FAILED: {test.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "Trace Matrix Test", ex);
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
        var test = CreateTestResult("ReportExport");

        try
        {
            using var tempDir = new TemporaryDirectory();

            // Create a requirements file
            var reqFile = Path.Combine(tempDir.Path, "export-requirements.yaml");
            var reqYaml = @"sections:
  - title: Export Test
    requirements:
      - id: EXP-001
        title: Export requirement
";
            File.WriteAllText(reqFile, reqYaml);

            using (new DirectorySwitch(tempDir.Path))
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
                var reportFile = Path.Combine(tempDir.Path, "report.md");
                if (exitCode == 0 && File.Exists(reportFile))
                {
                    var reportContent = File.ReadAllText(reportFile);
                    if (reportContent.Contains("EXP-001") && reportContent.Contains("Export requirement"))
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                        context.WriteLine("✓ Report Export Test - PASSED");
                    }
                    else
                    {
                        test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                        test.ErrorMessage = "Report file missing expected content";
                        context.WriteError("✗ Report Export Test - FAILED: Report file missing expected content");
                    }
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = exitCode != 0 
                        ? $"Program exited with code {exitCode}" 
                        : "Report file not created";
                    context.WriteError($"✗ Report Export Test - FAILED: {test.ErrorMessage}");
                }
            }
        }
        catch (Exception ex)
        {
            HandleTestException(test, context, "Report Export Test", ex);
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
        context.WriteError($"✗ {testName} - FAILED: {ex.Message}");
    }

    /// <summary>
    ///     Represents a temporary directory that is automatically deleted when disposed.
    /// </summary>
    private sealed class TemporaryDirectory : IDisposable
    {
        /// <summary>
        ///     Gets the path to the temporary directory.
        /// </summary>
        public string Path { get; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="TemporaryDirectory"/> class.
        /// </summary>
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"reqstream_validation_{Guid.NewGuid()}");
            Directory.CreateDirectory(Path);
        }

        /// <summary>
        ///     Deletes the temporary directory and all its contents.
        /// </summary>
        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, true);
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
