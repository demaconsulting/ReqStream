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
/// Unit tests for the Validation class.
/// </summary>
[TestClass]
public class ValidationTests
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
    /// Test that Run throws ArgumentNullException when context is null.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange - nothing to arrange; null is the input

        // Act + Assert - calling Run with null should throw ArgumentNullException
        Assert.ThrowsExactly<ArgumentNullException>(() => Validation.Run(null!));
    }

    /// <summary>
    /// Test that Run completes successfully with a silent context and log file.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithSilentContext_CompletesSuccessfully()
    {
        // Arrange - create a log file path and a silent context
        var logFile = Path.Combine(_testDirectory, "validation.log");

        // Act - run validation and dispose context to flush the log file
        using (var context = Context.Create(["--silent", "--log", logFile]))
        {
            Validation.Run(context);

            // Validation should succeed with exit code 0
            Assert.AreEqual(0, context.ExitCode);
        }

        // Assert - log file must exist and contain expected validation output
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
        Assert.Contains("Failed: 0", logContent);
    }

    /// <summary>
    /// Test that Run writes a TRX results file when the results path has a .trx extension.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithTrxResultsFile_WritesTrxFile()
    {
        // Arrange - create a results file path with .trx extension and a silent context
        var resultsFile = Path.Combine(_testDirectory, "validation-results.trx");

        // Act - run validation and dispose context to flush output
        using (var context = Context.Create(["--silent", "--results", resultsFile]))
        {
            Validation.Run(context);

            // Validation should succeed with exit code 0
            Assert.AreEqual(0, context.ExitCode);
        }

        // Assert - TRX file must exist and contain valid TRX XML content
        Assert.IsTrue(File.Exists(resultsFile), "TRX results file should exist");
        var trxContent = File.ReadAllText(resultsFile);
        Assert.StartsWith("<?xml", trxContent);
        Assert.Contains("TestRun", trxContent);
    }

    /// <summary>
    /// Test that Run writes a JUnit XML results file when the results path has a .xml extension.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithXmlResultsFile_WritesXmlFile()
    {
        // Arrange - create a results file path with .xml extension and a silent context
        var resultsFile = Path.Combine(_testDirectory, "validation-results.xml");

        // Act - run validation and dispose context to flush output
        using (var context = Context.Create(["--silent", "--results", resultsFile]))
        {
            Validation.Run(context);

            // Validation should succeed with exit code 0
            Assert.AreEqual(0, context.ExitCode);
        }

        // Assert - XML file must exist and contain valid JUnit XML content
        Assert.IsTrue(File.Exists(resultsFile), "JUnit XML results file should exist");
        var xmlContent = File.ReadAllText(resultsFile);
        Assert.StartsWith("<?xml", xmlContent);
        Assert.Contains("testsuite", xmlContent);
    }

    /// <summary>
    /// Test that Run reports an error when the results file has an unsupported extension.
    /// </summary>
    [TestMethod]
    public void Validation_Run_WithInvalidResultsExtension_ReportsError()
    {
        // Arrange - create a results file path with an unsupported .invalid extension
        var resultsFile = Path.Combine(_testDirectory, "validation-results.invalid");

        // Act - run validation and dispose context to flush output
        using var context = Context.Create(["--silent", "--results", resultsFile]);
        Validation.Run(context);

        // Assert - exit code must be 1 indicating an error was reported for the unsupported format
        Assert.AreEqual(1, context.ExitCode);
    }
}
