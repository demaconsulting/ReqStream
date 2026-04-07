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
[TestClass]
public class CliTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_cli_{Guid.NewGuid()}");
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
    /// Test that the --version flag sets the Version property on the context.
    /// </summary>
    [TestMethod]
    public void Cli_Interface_VersionFlag_SetsVersionProperty()
    {
        // Arrange: nothing to arrange - the --version flag alone is the input

        // Act: create a context with the --version flag
        using var context = Context.Create(["--version"]);

        // Assert: the Version property is true
        Assert.IsTrue(context.Version);
    }

    /// <summary>
    /// Test that the --help flag sets the Help property on the context.
    /// </summary>
    [TestMethod]
    public void Cli_Interface_HelpFlag_SetsHelpProperty()
    {
        // Arrange: nothing to arrange - the --help flag alone is the input

        // Act: create a context with the --help flag
        using var context = Context.Create(["--help"]);

        // Assert: the Help property is true
        Assert.IsTrue(context.Help);
    }

    /// <summary>
    /// Test that an unrecognized argument throws an ArgumentException.
    /// </summary>
    [TestMethod]
    public void Cli_Interface_UnknownArgument_ThrowsArgumentException()
    {
        // Arrange: nothing to arrange - the unknown argument is the input

        // Act + Assert: creating a context with an unknown argument throws ArgumentException
        Assert.ThrowsExactly<ArgumentException>(() => Context.Create(["--unknown-argument-xyz"]));
    }

    /// <summary>
    /// Test that the --silent flag sets the Silent property on the context.
    /// </summary>
    [TestMethod]
    public void Cli_Output_SilentFlag_SetsSilentProperty()
    {
        // Arrange: nothing to arrange - the --silent flag alone is the input

        // Act: create a context with the --silent flag
        using var context = Context.Create(["--silent"]);

        // Assert: the Silent property is true
        Assert.IsTrue(context.Silent);
    }

    /// <summary>
    /// Test that the --log flag causes output to be written to the specified file.
    /// </summary>
    [TestMethod]
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
        Assert.IsTrue(File.Exists(logFile), $"Expected log file at {logFile}");
        var content = File.ReadAllText(logFile);
        Assert.Contains("test output message", content);
    }
}
