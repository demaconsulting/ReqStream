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
using DemaConsulting.ReqStream.Utilities;

namespace DemaConsulting.ReqStream.Tests.Modeling;

/// <summary>
/// Unit tests for Requirements.Load() unified loading with linting.
/// </summary>
public sealed class RequirementsLoadTests : IDisposable
{
    /// <summary>Unique temporary directory for this test instance's fixture files.</summary>
    private readonly string _testDirectory;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public RequirementsLoadTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reqstream_load_test_{Guid.NewGuid()}");
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
    /// Test that loading a valid file returns requirements and no issues.
    /// </summary>
    [Fact]
    public void Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues()
    {
        // Arrange: create a valid YAML file
        var yamlContent = @"---
sections:
  - title: ""Test Section""
    requirements:
      - id: ""REQ-001""
        title: ""A valid requirement.""
";
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: requirements loaded with no issues
        Assert.NotNull(result.Requirements);
        Assert.Empty(result.Issues);
        Assert.Single(result.Requirements.Sections);
        Assert.Equal("REQ-001", result.Requirements.Sections[0].Requirements[0].Id);
    }

    /// <summary>
    /// Test that loading a file with a lint error returns null requirements and issues.
    /// </summary>
    [Fact]
    public void Requirements_Load_WithLintError_ReturnsNullAndIssues()
    {
        // Arrange: create a YAML file with an unknown field
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: null requirements returned with error issues
        Assert.Null(result.Requirements);
        Assert.NotEmpty(result.Issues);
        Assert.Contains(result.Issues, i => i.Severity == LintSeverity.Error);
        Assert.Contains(result.Issues, i => i.Description.Contains("Unknown field 'unknown_field'"));
    }

    /// <summary>
    /// Test that loading a missing file returns null requirements and an error issue.
    /// </summary>
    [Fact]
    public void Requirements_Load_MissingFile_ReturnsNullAndIssues()
    {
        // Act: load a non-existent file
        var result = Requirements.Load("/nonexistent/path/missing.yaml");

        // Assert: null requirements returned with File not found error
        Assert.Null(result.Requirements);
        Assert.NotEmpty(result.Issues);
        Assert.Contains(result.Issues, i => i.Severity == LintSeverity.Error);
        Assert.Contains(result.Issues, i => i.Description.Contains("File not found"));
    }

    /// <summary>
    /// Test that loading a file with malformed YAML returns null requirements and an error issue.
    /// </summary>
    [Fact]
    public void Requirements_Load_MalformedYaml_ReturnsNullAndIssues()
    {
        // Arrange: create a YAML file with invalid syntax
        var yamlContent = @"sections:
  - title: Bad
    requirements: [
  invalid yaml here
";
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "malformed.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the malformed file
        var result = Requirements.Load(filePath);

        // Assert: null requirements returned with malformed YAML error
        Assert.Null(result.Requirements);
        Assert.NotEmpty(result.Issues);
        Assert.Contains(result.Issues, i => i.Severity == LintSeverity.Error);
        Assert.Contains(result.Issues, i => i.Description.Contains("Malformed YAML"));
    }

    /// <summary>
    /// Test that lint issues contain location information.
    /// </summary>
    [Fact]
    public void Requirements_Load_WithLintError_IssueIncludesLocation()
    {
        // Arrange: create a YAML file with an unknown field at root level
        var yamlContent = @"unknown_field: value
";
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "location-test.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: issue location includes the file path and severity text
        Assert.NotEmpty(result.Issues);
        var issue = result.Issues[0];
        Assert.Contains(filePath, issue.Location);
        Assert.Contains("error:", issue.ToString());
    }

    /// <summary>
    /// Test that loading a file with multiple lint errors reports all of them.
    /// </summary>
    [Fact]
    public void Requirements_Load_WithMultipleLintErrors_ReportsAllIssues()
    {
        // Arrange: create a YAML file with multiple structural issues
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
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "multiple-issues.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: all issues reported
        Assert.Null(result.Requirements);
        Assert.True(result.Issues.Count >= 4);
        Assert.Contains(result.Issues, i => i.Description.Contains("Unknown field 'unknown_section_field'"));
        Assert.Contains(result.Issues, i => i.Description.Contains("Requirement missing required field 'id'"));
        Assert.Contains(result.Issues, i => i.Description.Contains("Duplicate requirement ID 'REQ-001'"));
        Assert.Contains(result.Issues, i => i.Description.Contains("Unknown field 'unknown_root_field'"));
    }

    /// <summary>
    /// Test that loading with no files throws ArgumentException.
    /// </summary>
    [Fact]
    public void Requirements_Load_NoFiles_ThrowsArgumentException()
    {
        // Act + Assert: calling Load with no arguments throws ArgumentException
        Assert.Throws<ArgumentException>(() => Requirements.Load());
    }

    /// <summary>
    /// Test that loading follows includes and lints included files.
    /// </summary>
    [Fact]
    public void Requirements_Load_WithIncludes_LintsIncludedFiles()
    {
        // Arrange: create a root YAML file and an included file with a lint error
        var includedFile = PathHelpers.SafePathCombine(_testDirectory, "included.yaml");
        File.WriteAllText(includedFile, @"sections:
  - title: Included Section
    requirements:
      - id: INC-001
        title: Included requirement
        unknown_field: bad
");

        var rootFile = PathHelpers.SafePathCombine(_testDirectory, "root.yaml");
        File.WriteAllText(rootFile, $@"includes:
  - included.yaml
sections:
  - title: Root Section
    requirements:
      - id: ROOT-001
        title: Root requirement
");

        // Act: load the root file
        var result = Requirements.Load(rootFile);

        // Assert: error from included file is reported
        Assert.Null(result.Requirements);
        Assert.Contains(result.Issues, i => i.Severity == LintSeverity.Error);
        Assert.Contains(result.Issues, i => i.Description.Contains("Unknown field 'unknown_field'"));
    }

}
