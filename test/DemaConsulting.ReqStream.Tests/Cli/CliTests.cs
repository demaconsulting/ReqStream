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

namespace DemaConsulting.ReqStream.Tests.Cli;

/// <summary>
/// Tests for the Cli subsystem, proving the Context class is sufficient to implement
/// the Cli subsystem requirements.
/// </summary>
public sealed class CliTests : IDisposable
{
    /// <summary>Unique temporary directory for this test instance's fixture files.</summary>
    private readonly string _testDirectory;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public CliTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_cli_{Guid.NewGuid()}");
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
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Test that the --version flag sets the Version property on the context.
    /// </summary>
    [Fact]
    public void Cli_Interface_VersionFlag_SetsVersionProperty()
    {
        // Arrange: nothing to arrange - the --version flag alone is the input

        // Act: create a context with the --version flag
        using var context = Context.Create(["--version"]);

        // Assert: the Version property is true
        Assert.True(context.Version);
    }

    /// <summary>
    /// Test that the --help flag sets the Help property on the context.
    /// </summary>
    [Fact]
    public void Cli_Interface_HelpFlag_SetsHelpProperty()
    {
        // Arrange: nothing to arrange - the --help flag alone is the input

        // Act: create a context with the --help flag
        using var context = Context.Create(["--help"]);

        // Assert: the Help property is true
        Assert.True(context.Help);
    }

    /// <summary>
    /// Test that an unrecognized argument throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_Interface_UnknownArgument_ThrowsArgumentException()
    {
        // Arrange: nothing to arrange - the unknown argument is the input

        // Act + Assert: creating a context with an unknown argument throws ArgumentException
        Assert.Throws<ArgumentException>(() => Context.Create(["--unknown-argument-xyz"]));
    }

    /// <summary>
    /// Test that the --silent flag sets the Silent property on the context.
    /// </summary>
    [Fact]
    public void Cli_Output_SilentFlag_SetsSilentProperty()
    {
        // Arrange: nothing to arrange - the --silent flag alone is the input

        // Act: create a context with the --silent flag
        using var context = Context.Create(["--silent"]);

        // Assert: the Silent property is true
        Assert.True(context.Silent);
    }

    /// <summary>
    /// Test that the --log flag causes output to be written to the specified file.
    /// </summary>
    [Fact]
    public void Cli_Output_LogFlag_WritesOutputToLogFile()
    {
        // Arrange: define path for the log output file
        var logFile = Path.Combine(_testDirectory, "output.log");

        // Act: create a context with the --log flag, write a message, then dispose to flush
        using (var context = Context.Create(["--silent", "--log", logFile]))
        {
            context.WriteLine("test output message");
        }

        // Assert: log file exists and contains the written message
        Assert.True(File.Exists(logFile), $"Expected log file at {logFile}");
        var content = File.ReadAllText(logFile);
        Assert.Contains("test output message", content);
    }

    /// <summary>
    /// Test that --log without a filename throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_Interface_MissingArgumentValue_ThrowsArgumentException()
    {
        // Arrange: nothing to arrange - the missing value is the input

        // Act + Assert: creating a context with --log but no filename throws ArgumentException
        Assert.Throws<ArgumentException>(() => Context.Create(["--log"]));
    }

    /// <summary>
    /// Test that an invalid depth value throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_Interface_InvalidDepthValue_ThrowsArgumentException()
    {
        // Arrange: nothing to arrange - the invalid depth is the input

        // Act + Assert: creating a context with a non-integer depth throws ArgumentException
        Assert.Throws<ArgumentException>(() => Context.Create(["--depth", "not-a-number"]));
    }

    /// <summary>
    /// Test that a log file path that cannot be opened throws an ArgumentException.
    /// </summary>
    [Fact]
    public void Cli_Interface_LogFileOpenFailure_ThrowsArgumentException()
    {
        // Arrange: use a path inside a directory that does not exist
        var invalidLogPath = Path.Combine(_testDirectory, "nonexistent-subdir", "output.log");

        // Act + Assert: creating a context with an inaccessible log file throws ArgumentException
        Assert.Throws<ArgumentException>(() => Context.Create(["--log", invalidLogPath]));
    }

    /// <summary>
    /// Test that WriteError writes to the error channel, not standard output.
    /// </summary>
    [Fact]
    public void Cli_Output_WriteError_WritesToErrorChannel()
    {
        // Arrange: redirect both stdout and stderr to capture writes separately
        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var stdoutCapture = new StringWriter();
        using var stderrCapture = new StringWriter();
        Console.SetOut(stdoutCapture);
        Console.SetError(stderrCapture);

        try
        {
            // Act: create a context and write an error message
            using var context = Context.Create([]);
            context.WriteError("error message");

            // Assert: the error went to stderr, not stdout
            Assert.Equal(string.Empty, stdoutCapture.ToString());
            Assert.Contains("error message", stderrCapture.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    /// <summary>
    /// Test that ExitCode returns 1 after WriteError is called.
    /// </summary>
    [Fact]
    public void Cli_Output_WriteError_SetsExitCodeToOne()
    {
        // Arrange: redirect stderr to suppress console noise during the test
        var originalError = Console.Error;
        Console.SetError(TextWriter.Null);

        try
        {
            // Act: create a context and write an error
            using var context = Context.Create([]);
            context.WriteError("error message");

            // Assert: exit code is 1 after an error is reported
            Assert.Equal(1, context.ExitCode);
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    /// Test that --depth sets the default for all per-report depth options.
    /// </summary>
    [Fact]
    public void Cli_Interface_DepthFlag_SetsDefaultForAllReportDepths()
    {
        // Arrange: nothing to arrange - the --depth flag alone is the input

        // Act: create a context with only --depth 3 (no per-report overrides)
        using var context = Context.Create(["--depth", "3"]);

        // Assert: all per-report depth properties inherit the --depth value
        Assert.Equal(3, context.ReportDepth);
        Assert.Equal(3, context.MatrixDepth);
        Assert.Equal(3, context.JustificationsDepth);
    }
}
