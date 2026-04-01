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

namespace DemaConsulting.ReqStream.Tests.Cli;

/// <summary>
/// Integration tests for the Cli subsystem, exercising command-line interface features
/// through the full tool executable.
/// </summary>
[TestClass]
public class CliIntegrationTests
{
    private string _dllPath = string.Empty;
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by locating the DLL and creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _dllPath = Path.Combine(AppContext.BaseDirectory, "DemaConsulting.ReqStream.dll");
        Assert.IsTrue(File.Exists(_dllPath), $"Could not find ReqStream DLL at {_dllPath}");

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
    /// Integration test verifying that --version outputs a version string and exits with code 0.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_VersionFlag_OutputsVersion()
    {
        // Arrange: no setup needed; --version requires no input files

        // Act: invoke the tool with --version flag
        var exitCode = Runner.Run(out var output, "dotnet", _dllPath, "--version");

        // Assert: exit code is 0 and output contains a version string
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
        Assert.IsFalse(string.IsNullOrWhiteSpace(output), "Expected non-empty version output.");
    }

    /// <summary>
    /// Integration test verifying that --help outputs usage information and exits with code 0.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_HelpFlag_OutputsUsageInformation()
    {
        // Arrange: no setup needed; --help requires no input files

        // Act: invoke the tool with --help flag
        var exitCode = Runner.Run(out var output, "dotnet", _dllPath, "--help");

        // Assert: exit code is 0 and output contains usage information
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
        Assert.Contains("Usage:", output);
        Assert.Contains("--version", output);
    }

    /// <summary>
    /// Integration test verifying that --silent suppresses all output.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_SilentFlag_SuppressesOutput()
    {
        // Arrange: no setup needed; --silent suppresses output without requiring input files

        // Act: invoke the tool with --silent flag
        var exitCode = Runner.Run(out var output, "dotnet", _dllPath, "--silent");

        // Assert: exit code is 0 and no output is produced
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
        Assert.IsTrue(string.IsNullOrWhiteSpace(output), $"Expected no output with --silent but got: {output}");
    }

    /// <summary>
    /// Integration test verifying that an unknown argument causes a non-zero exit code.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_UnknownArgument_ReturnsError()
    {
        // Arrange: no setup needed; an unrecognized argument should trigger an error response

        // Act: invoke the tool with an unrecognized argument
        var exitCode = Runner.Run(out var output, "dotnet", _dllPath, "--unknown-argument-xyz");

        // Assert: exit code is non-zero indicating the argument was rejected
        Assert.AreNotEqual(0, exitCode, $"Expected non-zero exit code for unknown argument. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that --log writes output to the specified file.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_LogFlag_WritesOutputToFile()
    {
        // Arrange: define path for the log output file
        var logFile = Path.Combine(_testDirectory, "output.log");

        // Act: invoke the tool with --log flag pointing to the target file
        var exitCode = Runner.Run(out var _, "dotnet", _dllPath, "--log", logFile);

        // Assert: exit code is 0 and the log file was created
        Assert.AreEqual(0, exitCode);
        Assert.IsTrue(File.Exists(logFile), $"Expected log file at {logFile}");
    }
}
