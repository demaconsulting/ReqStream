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
using DemaConsulting.ReqStream.Modeling;
using DemaConsulting.ReqStream.Utilities;

namespace DemaConsulting.ReqStream.Tests.Modeling;

/// <summary>
/// Unit tests for the LoadResult class, proving it correctly encapsulates load outcomes
/// and routes lint issues to the appropriate context output streams.
/// </summary>
public sealed class LoadResultTests : IDisposable
{
    private readonly string _testDirectory;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public LoadResultTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reqstream_load_result_test_{Guid.NewGuid()}");
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
    /// Test that ReportIssues routes error-level issues to the context error output.
    /// </summary>
    [Fact]
    public void LoadResult_ReportIssues_ErrorIssue_SetsContextError()
    {
        // Arrange: load a file with a lint error
        var yamlContent = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""REQ-001""
        title: ""A requirement.""
        unknown_field: bad
";
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        // Act: report the issues via context
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "report-issues-error.log");
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile]))
        {
            result.ReportIssues(context);
            exitCode = context.ExitCode;
        }

        // Assert: error context exit code set and log contains issue
        Assert.Equal(1, exitCode);
        var log = File.ReadAllText(logFile);
        Assert.Contains("unknown_field", log);
    }

    /// <summary>
    /// Test that ReportIssues routes warning-level issues to context normal output.
    /// </summary>
    [Fact]
    public void LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError()
    {
        // Arrange: create a load result with a single warning issue
        var warningResult = new LoadResult(
            new Requirements(),
            [new LintIssue("file.yaml", LintSeverity.Warning, "A warning")]);

        // Act: report issues via context
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "report-issues-warning.log");
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile]))
        {
            warningResult.ReportIssues(context);
            exitCode = context.ExitCode;
        }

        // Assert: no error exit code and warning written to log
        Assert.Equal(0, exitCode);
        var log = File.ReadAllText(logFile);
        Assert.Contains("A warning", log);
    }

    /// <summary>
    /// Test that ReportIssues produces no output when there are no issues.
    /// </summary>
    [Fact]
    public void LoadResult_ReportIssues_NoIssues_ProducesNoOutput()
    {
        // Arrange: load a valid file with no issues
        var yamlContent = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""REQ-001""
        title: ""A valid requirement.""
";
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        // Act: report issues via context
        var logFile = PathHelpers.SafePathCombine(_testDirectory, "report-issues-none.log");
        int exitCode;
        using (var context = Context.Create(["--silent", "--log", logFile]))
        {
            result.ReportIssues(context);
            exitCode = context.ExitCode;
        }

        // Assert: no output produced and exit code zero
        Assert.Equal(0, exitCode);
        Assert.Equal(string.Empty, File.ReadAllText(logFile));
    }

    /// <summary>
    /// Test that HasErrors is false when there are only warnings.
    /// </summary>
    [Fact]
    public void LoadResult_HasErrors_WithOnlyWarnings_ReturnsFalse()
    {
        // Arrange: create a load result with a single warning issue
        var result = new LoadResult(
            new Requirements(),
            [new LintIssue("file.yaml", LintSeverity.Warning, "A warning")]);

        // Assert: HasErrors is false and Requirements is not null for warnings-only results
        Assert.False(result.HasErrors);
        Assert.NotNull(result.Requirements);
    }

    /// <summary>
    /// Test that HasErrors is true when there are error-level issues.
    /// </summary>
    [Fact]
    public void LoadResult_HasErrors_WithErrorIssue_ReturnsTrue()
    {
        // Arrange: create a load result with an error issue and null requirements
        var result = new LoadResult(
            null,
            [new LintIssue("file.yaml", LintSeverity.Error, "An error")]);

        // Assert: HasErrors is true and Requirements is null for error results
        Assert.True(result.HasErrors);
        Assert.Null(result.Requirements);
    }
}
