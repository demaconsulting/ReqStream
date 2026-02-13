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
/// Unit tests for the Context class.
/// </summary>
[TestClass]
public class ContextTests
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
    /// Test creating a context with no arguments.
    /// </summary>
    [TestMethod]
    public void Context_Create_NoArguments_ReturnsDefaultContext()
    {
        using var context = Context.Create([]);

        Assert.IsFalse(context.Version);
        Assert.IsFalse(context.Help);
        Assert.IsFalse(context.Silent);
        Assert.IsFalse(context.Validate);
        Assert.IsEmpty(context.RequirementsFiles);
        Assert.IsEmpty(context.TestFiles);
        Assert.IsNull(context.FilterTags);
        Assert.IsNull(context.RequirementsReport);
        Assert.AreEqual(1, context.ReportDepth);
        Assert.IsNull(context.Matrix);
        Assert.AreEqual(1, context.MatrixDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with version flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_VersionFlag_SetsVersionProperty()
    {
        using var context1 = Context.Create(["-v"]);
        Assert.IsTrue(context1.Version);
        Assert.AreEqual(0, context1.ExitCode);

        using var context2 = Context.Create(["--version"]);
        Assert.IsTrue(context2.Version);
        Assert.AreEqual(0, context2.ExitCode);
    }

    /// <summary>
    /// Test creating a context with help flags.
    /// </summary>
    [TestMethod]
    public void Context_Create_HelpFlags_SetsHelpProperty()
    {
        using var context1 = Context.Create(["-?"]);
        Assert.IsTrue(context1.Help);
        Assert.AreEqual(0, context1.ExitCode);

        using var context2 = Context.Create(["-h"]);
        Assert.IsTrue(context2.Help);
        Assert.AreEqual(0, context2.ExitCode);

        using var context3 = Context.Create(["--help"]);
        Assert.IsTrue(context3.Help);
        Assert.AreEqual(0, context3.ExitCode);
    }

    /// <summary>
    /// Test creating a context with silent flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_SilentFlag_SetsSilentProperty()
    {
        using var context = Context.Create(["--silent"]);

        Assert.IsTrue(context.Silent);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with validate flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_ValidateFlag_SetsValidateProperty()
    {
        using var context = Context.Create(["--validate"]);

        Assert.IsTrue(context.Validate);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with results flag and filename.
    /// </summary>
    [TestMethod]
    public void Context_Create_ResultsFlag_SetsResultsFileProperty()
    {
        using var context = Context.Create(["--results", "results.trx"]);

        Assert.AreEqual("results.trx", context.ResultsFile);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with missing results filename.
    /// </summary>
    [TestMethod]
    public void Context_Create_MissingResultsFilename_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--results"]));
        Assert.Contains("--results requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with enforce flag.
    /// </summary>
    [TestMethod]
    public void Context_Create_EnforceFlag_SetsEnforceProperty()
    {
        using var context = Context.Create(["--enforce"]);

        Assert.IsTrue(context.Enforce);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with report depth.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportDepth_SetsReportDepthProperty()
    {
        using var context = Context.Create(["--report-depth", "3"]);

        Assert.AreEqual(3, context.ReportDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with matrix depth.
    /// </summary>
    [TestMethod]
    public void Context_Create_MatrixDepth_SetsMatrixDepthProperty()
    {
        using var context = Context.Create(["--matrix-depth", "2"]);

        Assert.AreEqual(2, context.MatrixDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with report file.
    /// </summary>
    [TestMethod]
    public void Context_Create_ReportFile_SetsReportProperty()
    {
        using var context = Context.Create(["--report", "report.md"]);

        Assert.AreEqual("report.md", context.RequirementsReport);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with matrix file.
    /// </summary>
    [TestMethod]
    public void Context_Create_MatrixFile_SetsMatrixProperty()
    {
        using var context = Context.Create(["--matrix", "matrix.md"]);

        Assert.AreEqual("matrix.md", context.Matrix);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with unsupported argument.
    /// </summary>
    [TestMethod]
    public void Context_Create_UnsupportedArgument_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--unsupported"]));
        Assert.Contains("Unsupported argument '--unsupported'", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing log filename.
    /// </summary>
    [TestMethod]
    public void Context_Create_MissingLogFilename_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--log"]));
        Assert.Contains("--log requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing report filename.
    /// </summary>
    [TestMethod]
    public void Context_Create_MissingReportFilename_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report"]));
        Assert.Contains("--report requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing matrix filename.
    /// </summary>
    [TestMethod]
    public void Context_Create_MissingMatrixFilename_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--matrix"]));
        Assert.Contains("--matrix requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing report depth.
    /// </summary>
    [TestMethod]
    public void Context_Create_MissingReportDepth_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report-depth"]));
        Assert.Contains("--report-depth requires a depth argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing matrix depth.
    /// </summary>
    [TestMethod]
    public void Context_Create_MissingMatrixDepth_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--matrix-depth"]));
        Assert.Contains("--matrix-depth requires a depth argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with invalid report depth.
    /// </summary>
    [TestMethod]
    public void Context_Create_InvalidReportDepth_ThrowsException()
    {
        var ex1 = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report-depth", "invalid"]));
        Assert.Contains("--report-depth requires a positive integer", ex1.Message);

        var ex2 = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report-depth", "0"]));
        Assert.Contains("--report-depth requires a positive integer", ex2.Message);

        var ex3 = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--report-depth", "-1"]));
        Assert.Contains("--report-depth requires a positive integer", ex3.Message);
    }

    /// <summary>
    /// Test creating a context with invalid matrix depth.
    /// </summary>
    [TestMethod]
    public void Context_Create_InvalidMatrixDepth_ThrowsException()
    {
        var ex1 = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--matrix-depth", "invalid"]));
        Assert.Contains("--matrix-depth requires a positive integer", ex1.Message);

        var ex2 = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--matrix-depth", "0"]));
        Assert.Contains("--matrix-depth requires a positive integer", ex2.Message);
    }

    /// <summary>
    /// Test WriteLine writes to console.
    /// </summary>
    [TestMethod]
    public void Context_WriteLine_NormalMode_WritesToConsole()
    {
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create([]);
            context.WriteLine("Test message");

            Assert.AreEqual("Test message" + Environment.NewLine, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test WriteLine in silent mode doesn't write to console.
    /// </summary>
    [TestMethod]
    public void Context_WriteLine_SilentMode_DoesNotWriteToConsole()
    {
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create(["--silent"]);
            context.WriteLine("Test message");

            Assert.AreEqual(string.Empty, output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test WriteError writes to console.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_NormalMode_WritesToConsole()
    {
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create([]);
            context.WriteError("Error message");

            Assert.AreEqual("Error message" + Environment.NewLine, output.ToString());
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test WriteError in silent mode doesn't write to console.
    /// </summary>
    [TestMethod]
    public void Context_WriteError_SilentMode_DoesNotWriteToConsole()
    {
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create(["--silent"]);
            context.WriteError("Error message");

            Assert.AreEqual(string.Empty, output.ToString());
            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test log file creation and writing.
    /// </summary>
    [TestMethod]
    public void Context_Create_WithLogFile_WritesToLogFile()
    {
        var logPath = Path.Combine(_testDirectory, "test.log");

        using (var context = Context.Create(["--log", logPath, "--silent"]))
        {
            context.WriteLine("Normal message");
            context.WriteError("Error message");
        }

        Assert.IsTrue(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        Assert.Contains("Normal message", logContent);
        Assert.Contains("Error message", logContent);
    }

    /// <summary>
    /// Test log file with silent mode still writes to log.
    /// </summary>
    [TestMethod]
    public void Context_Create_WithLogFileAndSilent_WritesToLogOnly()
    {
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            var logPath = Path.Combine(_testDirectory, "test.log");

            using (var context = Context.Create(["--log", logPath, "--silent"]))
            {
                context.WriteLine("Normal message");
                context.WriteError("Error message");
            }

            Assert.AreEqual(string.Empty, output.ToString());

            Assert.IsTrue(File.Exists(logPath));
            var logContent = File.ReadAllText(logPath);
            Assert.Contains("Normal message", logContent);
            Assert.Contains("Error message", logContent);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test requirements glob pattern expansion.
    /// </summary>
    [TestMethod]
    public void Context_Create_WithRequirementsPattern_ExpandsGlobPattern()
    {
        // Create test files
        var file1 = Path.Combine(_testDirectory, "req1.yaml");
        var file2 = Path.Combine(_testDirectory, "req2.yaml");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create(["--requirements", "*.yaml"]);

            Assert.HasCount(2, context.RequirementsFiles);
            Assert.Contains("req1.yaml", context.RequirementsFiles);
            Assert.Contains("req2.yaml", context.RequirementsFiles);
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test tests glob pattern expansion.
    /// </summary>
    [TestMethod]
    public void Context_Create_WithTestsPattern_ExpandsGlobPattern()
    {
        // Create test files
        var file1 = Path.Combine(_testDirectory, "test1.trx");
        var file2 = Path.Combine(_testDirectory, "test2.trx");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create(["--tests", "*.trx"]);

            Assert.HasCount(2, context.TestFiles);
            Assert.Contains("test1.trx", context.TestFiles);
            Assert.Contains("test2.trx", context.TestFiles);
            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test missing requirements pattern argument.
    /// </summary>
    [TestMethod]
    public void Context_Create_MissingRequirementsPattern_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--requirements"]));
        Assert.Contains("--requirements requires a pattern argument", ex.Message);
    }

    /// <summary>
    /// Test missing tests pattern argument.
    /// </summary>
    [TestMethod]
    public void Context_Create_MissingTestsPattern_ThrowsException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--tests"]));
        Assert.Contains("--tests requires a pattern argument", ex.Message);
    }

    /// <summary>
    /// Test combining multiple arguments.
    /// </summary>
    [TestMethod]
    public void Context_Create_MultipleArguments_ParsesAllCorrectly()
    {
        using var context = Context.Create(
            ["--version", "--help", "--silent", "--validate", "--report", "out.md", "--report-depth", "2"]);

        Assert.IsTrue(context.Version);
        Assert.IsTrue(context.Help);
        Assert.IsTrue(context.Silent);
        Assert.IsTrue(context.Validate);
        Assert.AreEqual("out.md", context.RequirementsReport);
        Assert.AreEqual(2, context.ReportDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test dispose closes log file.
    /// </summary>
    [TestMethod]
    public void Context_Dispose_WithLogFile_ClosesLogFile()
    {
        var logPath = Path.Combine(_testDirectory, "test.log");

        using (var context = Context.Create(["--log", logPath, "--silent"]))
        {
            context.WriteLine("Test message");
        }

        // Should be able to delete the file after dispose
        File.Delete(logPath);
        Assert.IsFalse(File.Exists(logPath));
    }

    /// <summary>
    /// Test invalid log file path.
    /// </summary>
    [TestMethod]
    public void Context_Create_InvalidLogPath_ThrowsException()
    {
        var invalidPath = Path.Combine(_testDirectory, "nonexistent", "test.log");

        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--log", invalidPath]));
        Assert.Contains("Failed to open log file", ex.Message);
    }

    /// <summary>
    /// Test creating a context with filter argument.
    /// </summary>
    [TestMethod]
    public void Context_Create_FilterArgument_ParsesTagsCorrectly()
    {
        using var context = Context.Create(["--filter", "security,critical"]);

        Assert.IsNotNull(context.FilterTags);
        Assert.HasCount(2, context.FilterTags);
        Assert.Contains("security", context.FilterTags);
        Assert.Contains("critical", context.FilterTags);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with filter argument with spaces.
    /// </summary>
    [TestMethod]
    public void Context_Create_FilterArgumentWithSpaces_TrimsAndParsesTagsCorrectly()
    {
        using var context = Context.Create(["--filter", "security, critical, data-integrity"]);

        Assert.IsNotNull(context.FilterTags);
        Assert.HasCount(3, context.FilterTags);
        Assert.Contains("security", context.FilterTags);
        Assert.Contains("critical", context.FilterTags);
        Assert.Contains("data-integrity", context.FilterTags);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with filter argument missing value.
    /// </summary>
    [TestMethod]
    public void Context_Create_FilterArgumentMissingValue_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--filter"]));
        Assert.Contains("--filter requires a comma-separated list of tags", ex.Message);
    }

    /// <summary>
    /// Test creating a context with single tag filter.
    /// </summary>
    [TestMethod]
    public void Context_Create_FilterSingleTag_ParsesCorrectly()
    {
        using var context = Context.Create(["--filter", "security"]);

        Assert.IsNotNull(context.FilterTags);
        Assert.HasCount(1, context.FilterTags);
        Assert.Contains("security", context.FilterTags);
        Assert.AreEqual(0, context.ExitCode);
    }
}
