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

        var (requirements, issues) = Requirements.Load(filePath);

        Assert.IsNotNull(requirements);
        Assert.HasCount(0, issues);
        Assert.HasCount(1, requirements.Sections);
        Assert.AreEqual("REQ-001", requirements.Sections[0].Requirements[0].Id);
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

        var (requirements, issues) = Requirements.Load(filePath);

        Assert.IsNull(requirements);
        Assert.IsTrue(issues.Count > 0);
        Assert.IsTrue(issues.Any(i => i.Severity == LintSeverity.Error));
        Assert.IsTrue(issues.Any(i => i.Description.Contains("Unknown field 'unknown_field'")));
    }

    /// <summary>
    /// Test that loading a missing file returns null requirements and an error issue.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_MissingFile_ReturnsNullAndIssues()
    {
        var (requirements, issues) = Requirements.Load("/nonexistent/path/missing.yaml");

        Assert.IsNull(requirements);
        Assert.IsTrue(issues.Count > 0);
        Assert.IsTrue(issues.Any(i => i.Severity == LintSeverity.Error));
        Assert.IsTrue(issues.Any(i => i.Description.Contains("File not found")));
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

        var (requirements, issues) = Requirements.Load(filePath);

        Assert.IsNull(requirements);
        Assert.IsTrue(issues.Count > 0);
        Assert.IsTrue(issues.Any(i => i.Severity == LintSeverity.Error));
        Assert.IsTrue(issues.Any(i => i.Description.Contains("Malformed YAML")));
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

        var (_, issues) = Requirements.Load(filePath);

        Assert.IsTrue(issues.Count > 0);
        var issue = issues[0];
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

        var (requirements, issues) = Requirements.Load(filePath);

        Assert.IsNull(requirements);
        Assert.IsTrue(issues.Count >= 4);
        Assert.IsTrue(issues.Any(i => i.Description.Contains("Unknown field 'unknown_section_field'")));
        Assert.IsTrue(issues.Any(i => i.Description.Contains("Requirement missing required field 'id'")));
        Assert.IsTrue(issues.Any(i => i.Description.Contains("Duplicate requirement ID 'REQ-001'")));
        Assert.IsTrue(issues.Any(i => i.Description.Contains("Unknown field 'unknown_root_field'")));
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

        var (requirements, issues) = Requirements.Load(rootFile);

        Assert.IsNull(requirements);
        Assert.IsTrue(issues.Any(i => i.Severity == LintSeverity.Error));
        Assert.IsTrue(issues.Any(i => i.Description.Contains("Unknown field 'unknown_field'")));
    }
}
