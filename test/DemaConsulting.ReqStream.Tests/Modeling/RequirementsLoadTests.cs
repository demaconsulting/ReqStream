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

namespace DemaConsulting.ReqStream.Tests.Modeling;

/// <summary>
/// Unit tests for Requirements.Load() unified loading with linting.
/// </summary>
[TestClass]
public class RequirementsLoadTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_load_test_{Guid.NewGuid()}");
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
    /// Test that loading a valid file returns requirements and no issues.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues()
    {
        var yamlContent = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""REQ-001""
        title: ""A valid requirement.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        Assert.IsNotNull(result.Requirements);
        Assert.HasCount(0, result.Issues);
        Assert.HasCount(1, result.Requirements.Sections);
        Assert.AreEqual("REQ-001", result.Requirements.Sections[0].Requirements[0].Id);
    }

    /// <summary>
    /// Test that loading a file with a lint error returns null requirements and issues.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_WithLintError_ReturnsNullAndIssues()
    {
        var yamlContent = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""REQ-001""
        title: ""A requirement.""
        unknown_field: bad
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        Assert.IsNull(result.Requirements);
        Assert.IsTrue(result.Issues.Count > 0);
        Assert.IsTrue(result.Issues.Any(i => i.Severity == LintSeverity.Error));
        Assert.IsTrue(result.Issues.Any(i => i.Description.Contains("Unknown field 'unknown_field'")));
    }

    /// <summary>
    /// Test that loading a missing file returns null requirements and an error issue.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_MissingFile_ReturnsNullAndIssues()
    {
        var result = Requirements.Load("/nonexistent/path/missing.yaml");

        Assert.IsNull(result.Requirements);
        Assert.IsTrue(result.Issues.Count > 0);
        Assert.IsTrue(result.Issues.Any(i => i.Severity == LintSeverity.Error));
        Assert.IsTrue(result.Issues.Any(i => i.Description.Contains("File not found")));
    }

    /// <summary>
    /// Test that loading a file with malformed YAML returns null requirements and an error issue.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_MalformedYaml_ReturnsNullAndIssues()
    {
        var yamlContent = @"sections:
  - title: Bad
    requirements: [
  invalid yaml here
";
        var filePath = Path.Combine(_testDirectory, "malformed.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        Assert.IsNull(result.Requirements);
        Assert.IsTrue(result.Issues.Count > 0);
        Assert.IsTrue(result.Issues.Any(i => i.Severity == LintSeverity.Error));
        Assert.IsTrue(result.Issues.Any(i => i.Description.Contains("Malformed YAML")));
    }

    /// <summary>
    /// Test that lint issues contain location information.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_WithLintError_IssueIncludesLocation()
    {
        var yamlContent = @"unknown_field: value
";
        var filePath = Path.Combine(_testDirectory, "location-test.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        Assert.IsTrue(result.Issues.Count > 0);
        var issue = result.Issues[0];
        StringAssert.Contains(issue.Location, filePath);
        StringAssert.Contains(issue.ToString(), "error:");
    }

    /// <summary>
    /// Test that LintIssue.ToString() formats as "location: severity: description".
    /// </summary>
    [TestMethod]
    public void LintIssue_ToString_FormatsCorrectly()
    {
        var errorIssue = new LintIssue("file.yaml(3,5)", LintSeverity.Error, "Some error");
        var warningIssue = new LintIssue("file.yaml", LintSeverity.Warning, "Some warning");

        Assert.AreEqual("file.yaml(3,5): error: Some error", errorIssue.ToString());
        Assert.AreEqual("file.yaml: warning: Some warning", warningIssue.ToString());
    }

    /// <summary>
    /// Test that loading a file with multiple lint errors reports all of them.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_WithMultipleLintErrors_ReportsAllIssues()
    {
        var yamlContent = @"sections:
  - title: Test Section
    unknown_section_field: bad
    requirements:
      - id: REQ-001
        title: Good requirement
      - title: Missing id requirement
      - id: REQ-001
        title: Duplicate id
unknown_root_field: bad
";
        var filePath = Path.Combine(_testDirectory, "multiple-issues.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        Assert.IsNull(result.Requirements);
        Assert.IsTrue(result.Issues.Count >= 4);
        Assert.IsTrue(result.Issues.Any(i => i.Description.Contains("Unknown field 'unknown_section_field'")));
        Assert.IsTrue(result.Issues.Any(i => i.Description.Contains("Requirement missing required field 'id'")));
        Assert.IsTrue(result.Issues.Any(i => i.Description.Contains("Duplicate requirement ID 'REQ-001'")));
        Assert.IsTrue(result.Issues.Any(i => i.Description.Contains("Unknown field 'unknown_root_field'")));
    }

    /// <summary>
    /// Test that loading with no files throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_NoFiles_ThrowsArgumentException()
    {
        Assert.ThrowsExactly<ArgumentException>(() => Requirements.Load());
    }

    /// <summary>
    /// Test that loading follows includes and lints included files.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_WithIncludes_LintsIncludedFiles()
    {
        var includedFile = Path.Combine(_testDirectory, "included.yaml");
        File.WriteAllText(includedFile, @"sections:
  - title: Included Section
    requirements:
      - id: INC-001
        title: Included requirement
        unknown_field: bad
");

        var rootFile = Path.Combine(_testDirectory, "root.yaml");
        File.WriteAllText(rootFile, $@"includes:
  - included.yaml
sections:
  - title: Root Section
    requirements:
      - id: ROOT-001
        title: Root requirement
");

        var result = Requirements.Load(rootFile);

        Assert.IsNull(result.Requirements);
        Assert.IsTrue(result.Issues.Any(i => i.Severity == LintSeverity.Error));
        Assert.IsTrue(result.Issues.Any(i => i.Description.Contains("Unknown field 'unknown_field'")));
    }

    /// <summary>
    /// Test that ReportIssues routes error-level issues to the context error output.
    /// </summary>
    [TestMethod]
    public void LoadResult_ReportIssues_ErrorIssue_SetsContextError()
    {
        var yamlContent = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""REQ-001""
        title: ""A requirement.""
        unknown_field: bad
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        var logFile = Path.Combine(_testDirectory, "report-issues-error.log");
        using var context = Context.Create(["--silent", "--log", logFile]);
        result.ReportIssues(context);

        Assert.AreEqual(1, context.ExitCode);
        var log = File.ReadAllText(logFile);
        Assert.IsTrue(log.Contains("unknown_field"));
    }

    /// <summary>
    /// Test that ReportIssues routes warning-level issues to context normal output.
    /// </summary>
    [TestMethod]
    public void LoadResult_ReportIssues_WarningIssue_DoesNotSetContextError()
    {
        var warningResult = new LoadResult(
            new Requirements(),
            [new LintIssue("file.yaml", LintSeverity.Warning, "A warning")]);

        var logFile = Path.Combine(_testDirectory, "report-issues-warning.log");
        using var context = Context.Create(["--silent", "--log", logFile]);
        warningResult.ReportIssues(context);

        Assert.AreEqual(0, context.ExitCode);
        var log = File.ReadAllText(logFile);
        Assert.IsTrue(log.Contains("A warning"));
    }

    /// <summary>
    /// Test that ReportIssues produces no output when there are no issues.
    /// </summary>
    [TestMethod]
    public void LoadResult_ReportIssues_NoIssues_ProducesNoOutput()
    {
        var yamlContent = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""REQ-001""
        title: ""A valid requirement.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var result = Requirements.Load(filePath);

        var logFile = Path.Combine(_testDirectory, "report-issues-none.log");
        using var context = Context.Create(["--silent", "--log", logFile]);
        result.ReportIssues(context);

        Assert.AreEqual(0, context.ExitCode);
        Assert.AreEqual(string.Empty, File.ReadAllText(logFile));
    }

    /// <summary>
    /// Test that HasErrors is false when there are only warnings.
    /// </summary>
    [TestMethod]
    public void LoadResult_HasErrors_WithOnlyWarnings_ReturnsFalse()
    {
        var result = new LoadResult(
            new Requirements(),
            [new LintIssue("file.yaml", LintSeverity.Warning, "A warning")]);

        Assert.IsFalse(result.HasErrors);
        Assert.IsNotNull(result.Requirements);
    }

    /// <summary>
    /// Test that HasErrors is true when there are error-level issues.
    /// </summary>
    [TestMethod]
    public void LoadResult_HasErrors_WithErrorIssue_ReturnsTrue()
    {
        var result = new LoadResult(
            null,
            [new LintIssue("file.yaml", LintSeverity.Error, "An error")]);

        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
    }
}
