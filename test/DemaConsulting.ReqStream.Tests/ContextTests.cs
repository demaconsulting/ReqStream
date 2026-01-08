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
    public void Create_NoArguments_ReturnsDefaultContext()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create([], output, error);

        Assert.IsFalse(context.Version);
        Assert.IsFalse(context.Help);
        Assert.IsFalse(context.Silent);
        Assert.IsFalse(context.Validate);
        Assert.AreEqual(0, context.RequirementsFiles.Count);
        Assert.AreEqual(0, context.TestFiles.Count);
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
    public void Create_VersionFlag_SetsVersionProperty()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context1 = Context.Create(["-v"], output, error);
        Assert.IsTrue(context1.Version);
        Assert.AreEqual(0, context1.ExitCode);

        using var context2 = Context.Create(["--version"], output, error);
        Assert.IsTrue(context2.Version);
        Assert.AreEqual(0, context2.ExitCode);
    }

    /// <summary>
    /// Test creating a context with help flags.
    /// </summary>
    [TestMethod]
    public void Create_HelpFlags_SetsHelpProperty()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context1 = Context.Create(["-?"], output, error);
        Assert.IsTrue(context1.Help);
        Assert.AreEqual(0, context1.ExitCode);

        using var context2 = Context.Create(["-h"], output, error);
        Assert.IsTrue(context2.Help);
        Assert.AreEqual(0, context2.ExitCode);

        using var context3 = Context.Create(["--help"], output, error);
        Assert.IsTrue(context3.Help);
        Assert.AreEqual(0, context3.ExitCode);
    }

    /// <summary>
    /// Test creating a context with silent flag.
    /// </summary>
    [TestMethod]
    public void Create_SilentFlag_SetsSilentProperty()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--silent"], output, error);

        Assert.IsTrue(context.Silent);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with validate flag.
    /// </summary>
    [TestMethod]
    public void Create_ValidateFlag_SetsValidateProperty()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--validate"], output, error);

        Assert.IsTrue(context.Validate);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with report depth.
    /// </summary>
    [TestMethod]
    public void Create_ReportDepth_SetsReportDepthProperty()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--report-depth", "3"], output, error);

        Assert.AreEqual(3, context.ReportDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with matrix depth.
    /// </summary>
    [TestMethod]
    public void Create_MatrixDepth_SetsMatrixDepthProperty()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--matrix-depth", "2"], output, error);

        Assert.AreEqual(2, context.MatrixDepth);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with report file.
    /// </summary>
    [TestMethod]
    public void Create_ReportFile_SetsReportProperty()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--report", "report.md"], output, error);

        Assert.AreEqual("report.md", context.RequirementsReport);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with matrix file.
    /// </summary>
    [TestMethod]
    public void Create_MatrixFile_SetsMatrixProperty()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--matrix", "matrix.md"], output, error);

        Assert.AreEqual("matrix.md", context.Matrix);
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with unsupported argument.
    /// </summary>
    [TestMethod]
    public void Create_UnsupportedArgument_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--unsupported"], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: Unsupported argument '--unsupported'");
    }

    /// <summary>
    /// Test creating a context with missing log filename.
    /// </summary>
    [TestMethod]
    public void Create_MissingLogFilename_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--log"], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --log requires a filename argument");
    }

    /// <summary>
    /// Test creating a context with missing report filename.
    /// </summary>
    [TestMethod]
    public void Create_MissingReportFilename_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--report"], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --report requires a filename argument");
    }

    /// <summary>
    /// Test creating a context with missing matrix filename.
    /// </summary>
    [TestMethod]
    public void Create_MissingMatrixFilename_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--matrix"], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --matrix requires a filename argument");
    }

    /// <summary>
    /// Test creating a context with missing report depth.
    /// </summary>
    [TestMethod]
    public void Create_MissingReportDepth_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--report-depth"], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --report-depth requires a depth argument");
    }

    /// <summary>
    /// Test creating a context with missing matrix depth.
    /// </summary>
    [TestMethod]
    public void Create_MissingMatrixDepth_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--matrix-depth"], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --matrix-depth requires a depth argument");
    }

    /// <summary>
    /// Test creating a context with invalid report depth.
    /// </summary>
    [TestMethod]
    public void Create_InvalidReportDepth_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context1 = Context.Create(["--report-depth", "invalid"], output, error);
        Assert.AreEqual(1, context1.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --report-depth requires a positive integer");

        error = new StringWriter();
        using var context2 = Context.Create(["--report-depth", "0"], output, error);
        Assert.AreEqual(1, context2.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --report-depth requires a positive integer");

        error = new StringWriter();
        using var context3 = Context.Create(["--report-depth", "-1"], output, error);
        Assert.AreEqual(1, context3.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --report-depth requires a positive integer");
    }

    /// <summary>
    /// Test creating a context with invalid matrix depth.
    /// </summary>
    [TestMethod]
    public void Create_InvalidMatrixDepth_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context1 = Context.Create(["--matrix-depth", "invalid"], output, error);
        Assert.AreEqual(1, context1.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --matrix-depth requires a positive integer");

        error = new StringWriter();
        using var context2 = Context.Create(["--matrix-depth", "0"], output, error);
        Assert.AreEqual(1, context2.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --matrix-depth requires a positive integer");
    }

    /// <summary>
    /// Test WriteLine writes to output.
    /// </summary>
    [TestMethod]
    public void WriteLine_NormalMode_WritesToOutput()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create([], output, error);
        context.WriteLine("Test message");

        Assert.AreEqual("Test message" + Environment.NewLine, output.ToString());
        Assert.AreEqual(string.Empty, error.ToString());
    }

    /// <summary>
    /// Test WriteLine in silent mode doesn't write to console.
    /// </summary>
    [TestMethod]
    public void WriteLine_SilentMode_DoesNotWriteToConsole()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--silent"], output, error);
        context.WriteLine("Test message");

        Assert.AreEqual(string.Empty, output.ToString());
        Assert.AreEqual(string.Empty, error.ToString());
    }

    /// <summary>
    /// Test WriteError writes to error output.
    /// </summary>
    [TestMethod]
    public void WriteError_NormalMode_WritesToError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create([], output, error);
        context.WriteError("Error message");

        Assert.AreEqual(string.Empty, output.ToString());
        Assert.AreEqual("Error message" + Environment.NewLine, error.ToString());
        Assert.AreEqual(1, context.ExitCode);
    }

    /// <summary>
    /// Test WriteError in silent mode doesn't write to console.
    /// </summary>
    [TestMethod]
    public void WriteError_SilentMode_DoesNotWriteToConsole()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--silent"], output, error);
        context.WriteError("Error message");

        Assert.AreEqual(string.Empty, output.ToString());
        Assert.AreEqual(string.Empty, error.ToString());
        Assert.AreEqual(1, context.ExitCode);
    }

    /// <summary>
    /// Test log file creation and writing.
    /// </summary>
    [TestMethod]
    public void Create_WithLogFile_WritesToLogFile()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var logPath = Path.Combine(_testDirectory, "test.log");

        using (var context = Context.Create(["--log", logPath], output, error))
        {
            context.WriteLine("Normal message");
            context.WriteError("Error message");
        }

        Assert.IsTrue(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        StringAssert.Contains(logContent, "Normal message");
        StringAssert.Contains(logContent, "Error message");
    }

    /// <summary>
    /// Test log file with silent mode still writes to log.
    /// </summary>
    [TestMethod]
    public void Create_WithLogFileAndSilent_WritesToLogOnly()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var logPath = Path.Combine(_testDirectory, "test.log");

        using (var context = Context.Create(["--log", logPath, "--silent"], output, error))
        {
            context.WriteLine("Normal message");
            context.WriteError("Error message");
        }

        Assert.AreEqual(string.Empty, output.ToString());
        Assert.AreEqual(string.Empty, error.ToString());

        Assert.IsTrue(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        StringAssert.Contains(logContent, "Normal message");
        StringAssert.Contains(logContent, "Error message");
    }

    /// <summary>
    /// Test requirements glob pattern expansion.
    /// </summary>
    [TestMethod]
    public void Create_WithRequirementsPattern_ExpandsGlobPattern()
    {
        var output = new StringWriter();
        var error = new StringWriter();

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

            using var context = Context.Create(["--requirements", "*.yaml"], output, error);

            Assert.AreEqual(2, context.RequirementsFiles.Count);
            Assert.IsTrue(context.RequirementsFiles.Any(f => f.EndsWith("req1.yaml")));
            Assert.IsTrue(context.RequirementsFiles.Any(f => f.EndsWith("req2.yaml")));
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
    public void Create_WithTestsPattern_ExpandsGlobPattern()
    {
        var output = new StringWriter();
        var error = new StringWriter();

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

            using var context = Context.Create(["--tests", "*.trx"], output, error);

            Assert.AreEqual(2, context.TestFiles.Count);
            Assert.IsTrue(context.TestFiles.Any(f => f.EndsWith("test1.trx")));
            Assert.IsTrue(context.TestFiles.Any(f => f.EndsWith("test2.trx")));
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
    public void Create_MissingRequirementsPattern_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--requirements"], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --requirements requires a pattern argument");
    }

    /// <summary>
    /// Test missing tests pattern argument.
    /// </summary>
    [TestMethod]
    public void Create_MissingTestsPattern_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(["--tests"], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: --tests requires a pattern argument");
    }

    /// <summary>
    /// Test combining multiple arguments.
    /// </summary>
    [TestMethod]
    public void Create_MultipleArguments_ParsesAllCorrectly()
    {
        var output = new StringWriter();
        var error = new StringWriter();

        using var context = Context.Create(
            ["--version", "--help", "--silent", "--validate", "--report", "out.md", "--report-depth", "2"],
            output,
            error);

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
    public void Dispose_WithLogFile_ClosesLogFile()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var logPath = Path.Combine(_testDirectory, "test.log");

        var context = Context.Create(["--log", logPath], output, error);
        context.WriteLine("Test message");
        context.Dispose();

        // Should be able to delete the file after dispose
        File.Delete(logPath);
        Assert.IsFalse(File.Exists(logPath));
    }

    /// <summary>
    /// Test invalid log file path.
    /// </summary>
    [TestMethod]
    public void Create_InvalidLogPath_ReportsError()
    {
        var output = new StringWriter();
        var error = new StringWriter();
        var invalidPath = Path.Combine(_testDirectory, "nonexistent", "test.log");

        using var context = Context.Create(["--log", invalidPath], output, error);

        Assert.AreEqual(1, context.ExitCode);
        StringAssert.Contains(error.ToString(), "Error: Failed to open log file");
    }
}
