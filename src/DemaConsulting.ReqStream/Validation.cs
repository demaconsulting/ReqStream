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
        RunYamlParsingTest(context, testResults);
        RunRequirementsReadTest(context, testResults);
        RunTestResultsParsingTest(context, testResults);

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
        var assembly = typeof(Validation).Assembly;
        var version = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                      ?? assembly.GetName().Version?.ToString()
                      ?? "Unknown";

        context.WriteLine("# DEMA Consulting ReqStream");
        context.WriteLine("");
        context.WriteLine("| Information         | Value                                              |");
        context.WriteLine("| :------------------ | :------------------------------------------------- |");
        context.WriteLine($"| ReqStream Version   | {version,-50} |");
        context.WriteLine($"| Machine Name        | {Environment.MachineName,-50} |");
        context.WriteLine($"| OS Version          | {RuntimeInformation.OSDescription,-50} |");
        context.WriteLine($"| DotNet Runtime      | {RuntimeInformation.FrameworkDescription,-50} |");
        context.WriteLine($"| Time Stamp          | {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC{"",-29} |");
        context.WriteLine("");
    }

    /// <summary>
    ///     Runs a test for YAML parsing functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunYamlParsingTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = new DemaConsulting.TestResults.TestResult
        {
            Name = "YamlParsing",
            ClassName = "Validation",
            CodeBase = "ReqStream"
        };

        try
        {
            // Create a simple YAML document in memory
            var yaml = @"
sections:
  - title: Test Requirements
    requirements:
      - id: TEST-001
        title: Test requirement
";
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, yaml);
                var requirements = Requirements.Read(tempFile);
                
                if (requirements.Sections.Count == 1 && 
                    requirements.Sections[0].Requirements.Count == 1 &&
                    requirements.Sections[0].Requirements[0].Id == "TEST-001")
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                    context.WriteLine("✓ YAML Parsing Test - PASSED");
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Parsed requirements do not match expected values";
                    context.WriteError("✗ YAML Parsing Test - FAILED: Parsed requirements do not match expected values");
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch (Exception ex)
        {
            test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
            test.ErrorMessage = $"Exception: {ex.Message}";
            context.WriteError($"✗ YAML Parsing Test - FAILED: {ex.Message}");
        }

        test.Duration = DateTime.UtcNow - startTime;
        testResults.Results.Add(test);
    }

    /// <summary>
    ///     Runs a test for requirements reading functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunRequirementsReadTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = new DemaConsulting.TestResults.TestResult
        {
            Name = "RequirementsRead",
            ClassName = "Validation",
            CodeBase = "ReqStream"
        };

        try
        {
            // Create a YAML document with nested sections
            var yaml = @"
sections:
  - title: Section 1
    requirements:
      - id: REQ-001
        title: Requirement 1
      - id: REQ-002
        title: Requirement 2
";
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, yaml);
                var requirements = Requirements.Read(tempFile);
                
                if (requirements.Sections.Count == 1 && 
                    requirements.Sections[0].Requirements.Count == 2)
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                    context.WriteLine("✓ Requirements Read Test - PASSED");
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Requirements structure does not match expected values";
                    context.WriteError("✗ Requirements Read Test - FAILED: Requirements structure does not match expected values");
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch (Exception ex)
        {
            test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
            test.ErrorMessage = $"Exception: {ex.Message}";
            context.WriteError($"✗ Requirements Read Test - FAILED: {ex.Message}");
        }

        test.Duration = DateTime.UtcNow - startTime;
        testResults.Results.Add(test);
    }

    /// <summary>
    ///     Runs a test for test results parsing functionality.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="testResults">The test results collection.</param>
    private static void RunTestResultsParsingTest(Context context, DemaConsulting.TestResults.TestResults testResults)
    {
        var startTime = DateTime.UtcNow;
        var test = new DemaConsulting.TestResults.TestResult
        {
            Name = "TestResultsParsing",
            ClassName = "Validation",
            CodeBase = "ReqStream"
        };

        try
        {
            // Create a simple TRX document - use the library's serializer instead
            var testData = new DemaConsulting.TestResults.TestResults { Name = "ValidationTest" };
            testData.Results.Add(new DemaConsulting.TestResults.TestResult
            {
                Name = "TestMethod1",
                ClassName = "ValidationTests",
                CodeBase = "Validation.dll",
                Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
                Duration = TimeSpan.FromSeconds(1)
            });
            
            var trxContent = TrxSerializer.Serialize(testData);
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, trxContent);
                var content = File.ReadAllText(tempFile);
                var results = TrxSerializer.Deserialize(content);
                
                if (results.Results.Count == 1 && results.Results[0].Outcome == DemaConsulting.TestResults.TestOutcome.Passed)
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Passed;
                    context.WriteLine("✓ Test Results Parsing Test - PASSED");
                }
                else
                {
                    test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
                    test.ErrorMessage = "Parsed test results do not match expected values";
                    context.WriteError("✗ Test Results Parsing Test - FAILED: Parsed test results do not match expected values");
                }
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }
        catch (Exception ex)
        {
            test.Outcome = DemaConsulting.TestResults.TestOutcome.Failed;
            test.ErrorMessage = $"Exception: {ex.Message}";
            context.WriteError($"✗ Test Results Parsing Test - FAILED: {ex.Message}");
        }

        test.Duration = DateTime.UtcNow - startTime;
        testResults.Results.Add(test);
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
}
