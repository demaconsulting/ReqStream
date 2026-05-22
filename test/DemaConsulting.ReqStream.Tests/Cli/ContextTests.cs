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

using DemaConsulting.ReqStream.Cli;
using DemaConsulting.ReqStream.Utilities;

namespace DemaConsulting.ReqStream.Tests.Cli;

/// <summary>
/// Unit tests for the Context class.
/// </summary>
public sealed class ContextTests : IDisposable
{
    private readonly string _testDirectory;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public ContextTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reqstream_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Clean up test by deleting the temporary test directory.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    /// <summary>
    /// Test creating a context with no arguments.
    /// </summary>
    [Fact]
    public void Context_Create_NoArguments_ReturnsDefaultContext()
    {
        // Act: create context with no arguments
        using var context = Context.Create([]);

        // Assert: all properties have default values
        Assert.False(context.Version);
        Assert.False(context.Help);
        Assert.False(context.Silent);
        Assert.False(context.Validate);
        Assert.False(context.Lint);
        Assert.Empty(context.RequirementsFiles);
        Assert.Empty(context.TestFiles);
        Assert.Null(context.FilterTags);
        Assert.Null(context.ResultsFile);
        Assert.False(context.Enforce);
        Assert.Null(context.RequirementsReport);
        Assert.Equal(1, context.Depth);
        Assert.Equal(1, context.ReportDepth);
        Assert.Null(context.Matrix);
        Assert.Equal(1, context.MatrixDepth);
        Assert.Null(context.JustificationsFile);
        Assert.Equal(1, context.JustificationsDepth);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with version flag.
    /// </summary>
    [Fact]
    public void Context_Create_VersionFlag_SetsVersionProperty()
    {
        // Act: create context with short version flag (-v)
        using var context1 = Context.Create(["-v"]);

        // Assert: Version property is true
        Assert.True(context1.Version);
        Assert.Equal(0, context1.ExitCode);

        // Act: create context with long version flag (--version)
        using var context2 = Context.Create(["--version"]);

        // Assert: Version property is true
        Assert.True(context2.Version);
        Assert.Equal(0, context2.ExitCode);
    }

    /// <summary>
    /// Test creating a context with help flags.
    /// </summary>
    [Fact]
    public void Context_Create_HelpFlags_SetsHelpProperty()
    {
        // Act: create context with short help flag (-?)
        using var context1 = Context.Create(["-?"]);

        // Assert: Help property is true
        Assert.True(context1.Help);
        Assert.Equal(0, context1.ExitCode);

        // Act: create context with short help flag (-h)
        using var context2 = Context.Create(["-h"]);

        // Assert: Help property is true
        Assert.True(context2.Help);
        Assert.Equal(0, context2.ExitCode);

        // Act: create context with long help flag (--help)
        using var context3 = Context.Create(["--help"]);

        // Assert: Help property is true
        Assert.True(context3.Help);
        Assert.Equal(0, context3.ExitCode);
    }

    /// <summary>
    /// Test creating a context with silent flag.
    /// </summary>
    [Fact]
    public void Context_Create_SilentFlag_SetsSilentProperty()
    {
        // Act: create context with silent flag
        using var context = Context.Create(["--silent"]);

        // Assert: Silent property is true
        Assert.True(context.Silent);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with validate flag.
    /// </summary>
    [Fact]
    public void Context_Create_ValidateFlag_SetsValidateProperty()
    {
        // Act: create context with validate flag
        using var context = Context.Create(["--validate"]);

        // Assert: Validate property is true
        Assert.True(context.Validate);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with results flag and filename.
    /// </summary>
    [Fact]
    public void Context_Create_ResultsFlag_SetsResultsFileProperty()
    {
        // Act: create context with results flag and filename
        using var context = Context.Create(["--results", "results.trx"]);

        // Assert: ResultsFile property is set to the specified path
        Assert.Equal("results.trx", context.ResultsFile);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with result flag (alias) and filename.
    /// </summary>
    [Fact]
    public void Context_Create_ResultFlag_SetsResultsFileProperty()
    {
        // Act: create context with result alias flag and filename
        using var context = Context.Create(["--result", "results.trx"]);

        // Assert: ResultsFile property is set to the specified path
        Assert.Equal("results.trx", context.ResultsFile);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with missing results filename.
    /// </summary>
    [Fact]
    public void Context_Create_MissingResultsFilename_ThrowsException()
    {
        // Act: create context with --results and no following filename (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--results"]));

        // Assert: exception message identifies the missing argument
        Assert.Contains("--results requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing result (alias) filename.
    /// </summary>
    [Fact]
    public void Context_Create_MissingResultFilename_ThrowsException()
    {
        // Act: create context with --result alias and no following filename (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--result"]));

        // Assert: exception message identifies the missing argument
        Assert.Contains("--result requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with enforce flag.
    /// </summary>
    [Fact]
    public void Context_Create_EnforceFlag_SetsEnforceProperty()
    {
        // Act: create context with enforce flag
        using var context = Context.Create(["--enforce"]);

        // Assert: Enforce property is true
        Assert.True(context.Enforce);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with report depth.
    /// </summary>
    [Fact]
    public void Context_Create_ReportDepth_SetsReportDepthProperty()
    {
        // Act: create context with report-depth flag set to 3
        using var context = Context.Create(["--report-depth", "3"]);

        // Assert: ReportDepth property is set to 3
        Assert.Equal(3, context.ReportDepth);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with matrix depth.
    /// </summary>
    [Fact]
    public void Context_Create_MatrixDepth_SetsMatrixDepthProperty()
    {
        // Act: create context with matrix-depth flag set to 2
        using var context = Context.Create(["--matrix-depth", "2"]);

        // Assert: MatrixDepth property is set to 2
        Assert.Equal(2, context.MatrixDepth);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with report file.
    /// </summary>
    [Fact]
    public void Context_Create_ReportFile_SetsReportProperty()
    {
        // Act: create context with report flag and filename
        using var context = Context.Create(["--report", "report.md"]);

        // Assert: RequirementsReport property is set to the specified path
        Assert.Equal("report.md", context.RequirementsReport);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with matrix file.
    /// </summary>
    [Fact]
    public void Context_Create_MatrixFile_SetsMatrixProperty()
    {
        // Act: create context with matrix flag and filename
        using var context = Context.Create(["--matrix", "matrix.md"]);

        // Assert: Matrix property is set to the specified path
        Assert.Equal("matrix.md", context.Matrix);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with unsupported argument.
    /// </summary>
    [Fact]
    public void Context_Create_UnsupportedArgument_ThrowsException()
    {
        // Act: create context with an unrecognized argument (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--unsupported"]));

        // Assert: exception message identifies the unsupported argument
        Assert.Contains("Unsupported argument '--unsupported'", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing log filename.
    /// </summary>
    [Fact]
    public void Context_Create_MissingLogFilename_ThrowsException()
    {
        // Act: create context with --log and no following filename (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--log"]));

        // Assert: exception message identifies the missing argument
        Assert.Contains("--log requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing report filename.
    /// </summary>
    [Fact]
    public void Context_Create_MissingReportFilename_ThrowsException()
    {
        // Act: create context with --report and no following filename (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--report"]));

        // Assert: exception message identifies the missing argument
        Assert.Contains("--report requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing matrix filename.
    /// </summary>
    [Fact]
    public void Context_Create_MissingMatrixFilename_ThrowsException()
    {
        // Act: create context with --matrix and no following filename (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--matrix"]));

        // Assert: exception message identifies the missing argument
        Assert.Contains("--matrix requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing report depth.
    /// </summary>
    [Fact]
    public void Context_Create_MissingReportDepth_ThrowsException()
    {
        // Act: create context with --report-depth and no following value (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--report-depth"]));

        // Assert: exception message identifies the missing depth argument
        Assert.Contains("--report-depth requires a depth argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with missing matrix depth.
    /// </summary>
    [Fact]
    public void Context_Create_MissingMatrixDepth_ThrowsException()
    {
        // Act: create context with --matrix-depth and no following value (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--matrix-depth"]));

        // Assert: exception message identifies the missing depth argument
        Assert.Contains("--matrix-depth requires a depth argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with invalid report depth.
    /// </summary>
    [Fact]
    public void Context_Create_InvalidReportDepth_ThrowsException()
    {
        // Act: create context with non-numeric report-depth (combined with assertion)
        var ex1 = Assert.Throws<ArgumentException>(() => Context.Create(["--report-depth", "invalid"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--report-depth requires a positive integer", ex1.Message);

        // Act: create context with zero report-depth (combined with assertion)
        var ex2 = Assert.Throws<ArgumentException>(() => Context.Create(["--report-depth", "0"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--report-depth requires a positive integer", ex2.Message);

        // Act: create context with negative report-depth (combined with assertion)
        var ex3 = Assert.Throws<ArgumentException>(() => Context.Create(["--report-depth", "-1"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--report-depth requires a positive integer", ex3.Message);
    }

    /// <summary>
    /// Test creating a context with invalid matrix depth.
    /// </summary>
    [Fact]
    public void Context_Create_InvalidMatrixDepth_ThrowsException()
    {
        // Act: create context with non-numeric matrix-depth (combined with assertion)
        var ex1 = Assert.Throws<ArgumentException>(() => Context.Create(["--matrix-depth", "invalid"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--matrix-depth requires a positive integer", ex1.Message);

        // Act: create context with zero matrix-depth (combined with assertion)
        var ex2 = Assert.Throws<ArgumentException>(() => Context.Create(["--matrix-depth", "0"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--matrix-depth requires a positive integer", ex2.Message);

        // Act: create context with negative matrix-depth (combined with assertion)
        var ex3 = Assert.Throws<ArgumentException>(() => Context.Create(["--matrix-depth", "-1"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--matrix-depth requires a positive integer", ex3.Message);
    }

    /// <summary>
    /// Test WriteLine writes to the log file.
    /// </summary>
    [Fact]
    public void Context_WriteLine_NormalMode_WritesToLogFile()
    {
        // Arrange: set up a log file to capture written messages
        var logPath = PathHelpers.SafePathCombine(_testDirectory, "output-normal.log");

        // Act: create context in normal mode with log file, write a message, then dispose
        using (var context = Context.Create(["--log", logPath]))
        {
            context.WriteLine("Test message");
        }

        // Assert: message was captured in the log file
        Assert.True(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        Assert.Contains("Test message", logContent);
    }

    /// <summary>
    /// Test WriteLine in silent mode still writes to the log file.
    /// </summary>
    [Fact]
    public void Context_WriteLine_SilentMode_WritesToLogFile()
    {
        // Arrange: set up a log file to observe output in silent mode
        var logPath = PathHelpers.SafePathCombine(_testDirectory, "output-silent.log");

        // Act: create context in silent mode with log file, write a message, then dispose
        using (var context = Context.Create(["--silent", "--log", logPath]))
        {
            context.WriteLine("Test message");
        }

        // Assert: log file still receives the message even in silent mode
        Assert.True(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        Assert.Contains("Test message", logContent);
    }

    /// <summary>
    /// Test WriteError in normal mode writes to the log file.
    /// </summary>
    [Fact]
    public void Context_WriteError_NormalMode_WritesToLogFile()
    {
        // Arrange: set up a log file to capture error messages
        var logPath = PathHelpers.SafePathCombine(_testDirectory, "error-normal.log");
        int exitCode;

        // Act: create context in normal mode with log file, write an error, then dispose
        using (var context = Context.Create(["--log", logPath]))
        {
            context.WriteError("Error message");
            exitCode = context.ExitCode;
        }

        // Assert: error was captured in the log file and exit code reflects failure
        Assert.True(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        Assert.Contains("Error message", logContent);
        Assert.Equal(1, exitCode);
    }

    /// <summary>
    /// Test WriteError in normal mode routes the message to stderr (Console.Error).
    /// </summary>
    [Fact]
    public void Context_WriteError_NormalMode_WritesToStderr()
    {
        // Arrange: redirect Console.Error to a StringWriter so the output can be inspected
        var originalError = Console.Error;
        var errorWriter = new StringWriter();
        Console.SetError(errorWriter);
        try
        {
            // Act: create context in normal mode and write an error message
            using var context = Context.Create([]);
            context.WriteError("stderr message");

            // Assert: the error message was written to the captured stderr stream
            Assert.Contains("stderr message", errorWriter.ToString());
        }
        finally
        {
            // Restore original Console.Error to avoid affecting other tests
            Console.SetError(originalError);
        }
    }

    /// <summary>
    /// Test WriteError in silent mode still writes to the log file.
    /// </summary>
    [Fact]
    public void Context_WriteError_SilentMode_WritesToLogFile()
    {
        // Arrange: set up a log file to observe output in silent mode
        var logPath = PathHelpers.SafePathCombine(_testDirectory, "error-silent.log");
        int exitCode;

        // Act: create context in silent mode with log file, write an error, then dispose
        using (var context = Context.Create(["--silent", "--log", logPath]))
        {
            context.WriteError("Error message");
            exitCode = context.ExitCode;
        }

        // Assert: log file still receives the error and exit code reflects failure
        Assert.True(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        Assert.Contains("Error message", logContent);
        Assert.Equal(1, exitCode);
    }

    /// <summary>
    /// Test that ExitCode returns 0 before any errors and 1 after WriteError.
    /// </summary>
    [Fact]
    public void Context_ExitCode_AfterWriteError_ReturnsOne()
    {
        // Arrange: set up a log file to suppress console noise during the test
        var logPath = PathHelpers.SafePathCombine(_testDirectory, "exit-test.log");

        // Act: create context, check initial exit code, call WriteError, check again
        using var context = Context.Create(["--silent", "--log", logPath]);
        var initialExitCode = context.ExitCode;
        context.WriteError("error");
        var exitCodeAfterError = context.ExitCode;

        // Assert: exit code starts at 0 and becomes 1 after WriteError
        Assert.Equal(0, initialExitCode);
        Assert.Equal(1, exitCodeAfterError);
    }

    /// <summary>
    /// Test log file creation and writing.
    /// </summary>
    [Fact]
    public void Context_Create_WithLogFile_WritesToLogFile()
    {
        // Arrange: set up the log file path in the test directory
        var logPath = PathHelpers.SafePathCombine(_testDirectory, "test.log");

        // Act: create context with log file, write normal and error messages, then dispose
        using (var context = Context.Create(["--log", logPath, "--silent"]))
        {
            context.WriteLine("Normal message");
            context.WriteError("Error message");
        }

        // Assert: log file was created and contains both messages
        Assert.True(File.Exists(logPath));
        var logContent = File.ReadAllText(logPath);
        Assert.Contains("Normal message", logContent);
        Assert.Contains("Error message", logContent);
    }

    /// <summary>
    /// Test log file with silent mode still writes to log.
    /// </summary>
    [Fact]
    public void Context_Create_WithLogFileAndSilent_WritesToLogOnly()
    {
        // Arrange: set up the log file path
        var logPath = PathHelpers.SafePathCombine(_testDirectory, "silent_output.log");

        // Act: create context with log file and silent flag, write messages, then dispose
        int exitCode;
        using (var context = Context.Create(["--log", logPath, "--silent"]))
        {
            context.WriteLine("Silent normal message");
            context.WriteError("Silent error message");
            exitCode = context.ExitCode;
        }

        // Assert: log file contains both messages and exit code reflects the error
        Assert.Equal(1, exitCode);
        Assert.True(File.Exists(logPath));
        var lines = File.ReadAllLines(logPath);
        Assert.Equal(2, lines.Length);
        Assert.Contains("Silent normal message", lines[0]);
        Assert.Contains("Silent error message", lines[1]);
    }

    /// <summary>
    /// Test requirements glob pattern expansion.
    /// </summary>
    [Fact]
    public void Context_Create_WithRequirementsPattern_ExpandsGlobPattern()
    {
        // Arrange: create test YAML files and change working directory to the test directory
        var file1 = PathHelpers.SafePathCombine(_testDirectory, "req1.yaml");
        var file2 = PathHelpers.SafePathCombine(_testDirectory, "req2.yaml");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: create context with a glob pattern for requirements
            using var context = Context.Create(["--requirements", "*.yaml"]);

            // Assert: both YAML files are resolved and present in RequirementsFiles
            Assert.Equal(2, context.RequirementsFiles.Count);
            Assert.Single(context.RequirementsFiles, f => f.EndsWith("req1.yaml"));
            Assert.Single(context.RequirementsFiles, f => f.EndsWith("req2.yaml"));
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test tests glob pattern expansion.
    /// </summary>
    [Fact]
    public void Context_Create_WithTestsPattern_ExpandsGlobPattern()
    {
        // Arrange: create test TRX files and change working directory to the test directory
        var file1 = PathHelpers.SafePathCombine(_testDirectory, "test1.trx");
        var file2 = PathHelpers.SafePathCombine(_testDirectory, "test2.trx");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            // Act: create context with a glob pattern for test files
            using var context = Context.Create(["--tests", "*.trx"]);

            // Assert: both TRX files are resolved and present in TestFiles
            Assert.Equal(2, context.TestFiles.Count);
            Assert.Single(context.TestFiles, f => f.EndsWith("test1.trx"));
            Assert.Single(context.TestFiles, f => f.EndsWith("test2.trx"));
            Assert.Equal(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test missing requirements pattern argument.
    /// </summary>
    [Fact]
    public void Context_Create_MissingRequirementsPattern_ThrowsException()
    {
        // Act: create context with --requirements and no following pattern (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--requirements"]));

        // Assert: exception message identifies the missing pattern argument
        Assert.Contains("--requirements requires a pattern argument", ex.Message);
    }

    /// <summary>
    /// Test missing tests pattern argument.
    /// </summary>
    [Fact]
    public void Context_Create_MissingTestsPattern_ThrowsException()
    {
        // Act: create context with --tests and no following pattern (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--tests"]));

        // Assert: exception message identifies the missing pattern argument
        Assert.Contains("--tests requires a pattern argument", ex.Message);
    }

    /// <summary>
    /// Test combining multiple arguments.
    /// </summary>
    [Fact]
    public void Context_Create_MultipleArguments_ParsesAllCorrectly()
    {
        // Act: create context with several flags combined
        using var context = Context.Create(
            ["--version", "--help", "--silent", "--validate", "--report", "out.md", "--report-depth", "2"]);

        // Assert: all specified properties are correctly set
        Assert.True(context.Version);
        Assert.True(context.Help);
        Assert.True(context.Silent);
        Assert.True(context.Validate);
        Assert.Equal("out.md", context.RequirementsReport);
        Assert.Equal(2, context.ReportDepth);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test dispose closes log file.
    /// </summary>
    [Fact]
    public void Context_Dispose_WithLogFile_ClosesLogFile()
    {
        // Arrange: set up the log file path in the test directory
        var logPath = PathHelpers.SafePathCombine(_testDirectory, "test.log");

        // Act: create context with log file, write a message, then dispose
        using (var context = Context.Create(["--log", logPath, "--silent"]))
        {
            context.WriteLine("Test message");
        }

        // Assert: log file handle is released and the file can be deleted
        File.Delete(logPath);
        Assert.False(File.Exists(logPath));
    }

    /// <summary>
    /// Test invalid log file path.
    /// </summary>
    [Fact]
    public void Context_Create_InvalidLogPath_ThrowsException()
    {
        // Arrange: construct a path whose parent directory does not exist
        var invalidPath = PathHelpers.SafePathCombine(PathHelpers.SafePathCombine(_testDirectory, "nonexistent"), "test.log");

        // Act: create context with the invalid log path (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--log", invalidPath]));

        // Assert: exception message identifies the failure to open the log file
        Assert.Contains("Failed to open log file", ex.Message);
    }

    /// <summary>
    /// Test creating a context with filter argument.
    /// </summary>
    [Fact]
    public void Context_Create_FilterArgument_ParsesTagsCorrectly()
    {
        // Act: create context with a comma-separated filter value
        using var context = Context.Create(["--filter", "security,critical"]);

        // Assert: FilterTags contains both parsed tags
        Assert.NotNull(context.FilterTags);
        Assert.Equal(2, context.FilterTags.Count);
        Assert.Contains("security", context.FilterTags);
        Assert.Contains("critical", context.FilterTags);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with filter argument with spaces.
    /// </summary>
    [Fact]
    public void Context_Create_FilterArgumentWithSpaces_TrimsAndParsesTagsCorrectly()
    {
        // Act: create context with a comma-separated filter value containing spaces
        using var context = Context.Create(["--filter", "security, critical, data-integrity"]);

        // Assert: FilterTags contains all three tags with whitespace trimmed
        Assert.NotNull(context.FilterTags);
        Assert.Equal(3, context.FilterTags.Count);
        Assert.Contains("security", context.FilterTags);
        Assert.Contains("critical", context.FilterTags);
        Assert.Contains("data-integrity", context.FilterTags);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with filter argument missing value.
    /// </summary>
    [Fact]
    public void Context_Create_FilterArgumentMissingValue_ThrowsException()
    {
        // Act: create context with --filter and no following value (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--filter"]));

        // Assert: exception message identifies the missing tag list
        Assert.Contains("--filter requires a comma-separated list of tags", ex.Message);
    }

    /// <summary>
    /// Test creating a context with single tag filter.
    /// </summary>
    [Fact]
    public void Context_Create_FilterSingleTag_ParsesCorrectly()
    {
        // Act: create context with a single tag filter value
        using var context = Context.Create(["--filter", "security"]);

        // Assert: FilterTags contains exactly the one specified tag
        Assert.NotNull(context.FilterTags);
        Assert.Single(context.FilterTags);
        Assert.Contains("security", context.FilterTags);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with multiple --filter arguments merges into one set.
    /// </summary>
    [Fact]
    public void Context_Create_MultipleFilterArguments_MergesIntoSingleSet()
    {
        // Act: create context with two separate --filter arguments
        using var context = Context.Create(["--filter", "tag1", "--filter", "tag2"]);

        // Assert: both tags are merged into a single FilterTags set
        Assert.NotNull(context.FilterTags);
        Assert.Equal(2, context.FilterTags.Count);
        Assert.Contains("tag1", context.FilterTags);
        Assert.Contains("tag2", context.FilterTags);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with lint flag.
    /// </summary>
    [Fact]
    public void Context_Create_LintFlag_SetsLintProperty()
    {
        // Act: create context with lint flag
        using var context = Context.Create(["--lint"]);

        // Assert: Lint property is true
        Assert.True(context.Lint);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with justifications file.
    /// </summary>
    [Fact]
    public void Context_Create_JustificationsFile_SetsJustificationsFileProperty()
    {
        // Act: create context with justifications flag and filename
        using var context = Context.Create(["--justifications", "justifications.md"]);

        // Assert: JustificationsFile property is set to the specified path
        Assert.Equal("justifications.md", context.JustificationsFile);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with missing justifications filename.
    /// </summary>
    [Fact]
    public void Context_Create_MissingJustificationsFilename_ThrowsException()
    {
        // Act: create context with --justifications and no following filename (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--justifications"]));

        // Assert: exception message identifies the missing argument
        Assert.Contains("--justifications requires a filename argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with justifications depth.
    /// </summary>
    [Fact]
    public void Context_Create_JustificationsDepth_SetsJustificationsDepthProperty()
    {
        // Act: create context with justifications-depth flag set to 3
        using var context = Context.Create(["--justifications-depth", "3"]);

        // Assert: JustificationsDepth property is set to 3
        Assert.Equal(3, context.JustificationsDepth);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with missing justifications depth argument.
    /// </summary>
    [Fact]
    public void Context_Create_MissingJustificationsDepth_ThrowsException()
    {
        // Act: create context with --justifications-depth and no following value (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--justifications-depth"]));

        // Assert: exception message identifies the missing depth argument
        Assert.Contains("--justifications-depth requires a depth argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with invalid justifications depth.
    /// </summary>
    [Fact]
    public void Context_Create_InvalidJustificationsDepth_ThrowsException()
    {
        // Act: create context with non-numeric justifications-depth (combined with assertion)
        var ex1 = Assert.Throws<ArgumentException>(() => Context.Create(["--justifications-depth", "invalid"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--justifications-depth requires a positive integer", ex1.Message);

        // Act: create context with zero justifications-depth (combined with assertion)
        var ex2 = Assert.Throws<ArgumentException>(() => Context.Create(["--justifications-depth", "0"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--justifications-depth requires a positive integer", ex2.Message);

        // Act: create context with negative justifications-depth (combined with assertion)
        var ex3 = Assert.Throws<ArgumentException>(() => Context.Create(["--justifications-depth", "-1"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--justifications-depth requires a positive integer", ex3.Message);
    }

    /// <summary>
    /// Test creating a context with depth flag sets all report depths.
    /// </summary>
    [Fact]
    public void Context_Create_Depth_SetsAllDepths()
    {
        // Act: create context with depth flag set to 2
        using var context = Context.Create(["--depth", "2"]);

        // Assert: all report depth properties inherit the specified default depth
        Assert.Equal(2, context.Depth);
        Assert.Equal(2, context.ReportDepth);
        Assert.Equal(2, context.MatrixDepth);
        Assert.Equal(2, context.JustificationsDepth);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test that specific depth flags override the default depth.
    /// </summary>
    [Fact]
    public void Context_Create_SpecificDepthOverridesDefaultDepth()
    {
        // Act: create context with a default depth of 2 and a report-specific depth of 3
        using var context = Context.Create(["--depth", "2", "--report-depth", "3"]);

        // Assert: report depth uses the override value and other depths inherit the default
        Assert.Equal(2, context.Depth);
        Assert.Equal(3, context.ReportDepth);
        Assert.Equal(2, context.MatrixDepth);
        Assert.Equal(2, context.JustificationsDepth);
        Assert.Equal(0, context.ExitCode);
    }

    /// <summary>
    /// Test creating a context with missing depth argument.
    /// </summary>
    [Fact]
    public void Context_Create_MissingDepth_ThrowsException()
    {
        // Act: create context with --depth and no following value (combined with assertion)
        var ex = Assert.Throws<ArgumentException>(() => Context.Create(["--depth"]));

        // Assert: exception message identifies the missing depth argument
        Assert.Contains("--depth requires a depth argument", ex.Message);
    }

    /// <summary>
    /// Test creating a context with invalid depth.
    /// </summary>
    [Fact]
    public void Context_Create_InvalidDepth_ThrowsException()
    {
        // Act: create context with non-numeric depth (combined with assertion)
        var ex1 = Assert.Throws<ArgumentException>(() => Context.Create(["--depth", "invalid"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--depth requires a positive integer", ex1.Message);

        // Act: create context with zero depth (combined with assertion)
        var ex2 = Assert.Throws<ArgumentException>(() => Context.Create(["--depth", "0"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--depth requires a positive integer", ex2.Message);

        // Act: create context with negative depth (combined with assertion)
        var ex3 = Assert.Throws<ArgumentException>(() => Context.Create(["--depth", "-1"]));

        // Assert: exception message indicates a positive integer is required
        Assert.Contains("--depth requires a positive integer", ex3.Message);
    }
}
