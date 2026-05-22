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
/// Unit tests for the Section class, proving it correctly holds a title, requirements,
/// and child sections.
/// </summary>
public sealed class SectionTests : IDisposable
{
    /// <summary>Unique temporary directory for this test instance's fixture files.</summary>
    private readonly string _testDirectory;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public SectionTests()
    {
        _testDirectory = PathHelpers.SafePathCombine(Path.GetTempPath(), $"reqstream_section_test_{Guid.NewGuid()}");
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
    /// Test reading a simple YAML file with a single requirement.
    /// </summary>
    [Fact]
    public void Section_Load_SimpleRequirement_ParsesCorrectly()
    {
        // Arrange: create a YAML file with a single requirement
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        var requirements = result.Requirements;

        // Assert: requirement parsed correctly
        Assert.False(result.HasErrors);
        Assert.NotNull(requirements);
        Assert.Single(requirements.Sections);
        Assert.Equal("System Security", requirements.Sections[0].Title);
        Assert.Single(requirements.Sections[0].Requirements);
        Assert.Equal("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.Equal("The system shall support credentials authentication.", requirements.Sections[0].Requirements[0].Title);
    }

    /// <summary>
    /// Test reading nested sections.
    /// </summary>
    [Fact]
    public void Section_Load_NestedSections_ParsesHierarchyCorrectly()
    {
        // Arrange: create a YAML file with nested sections
        var yamlContent = @"---
sections:
  - title: ""Data Management""
    sections:
      - title: ""User Authentication""
        requirements:
          - id: ""AUTH-001""
            title: ""All requests shall be authenticated.""
      - title: ""Logging""
        requirements:
          - id: ""LOG-001""
            title: ""All requests shall be logged.""
";
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        var requirements = result.Requirements;

        // Assert: nested section hierarchy parsed correctly
        Assert.False(result.HasErrors);
        Assert.NotNull(requirements);
        Assert.Single(requirements.Sections);
        Assert.Equal("Data Management", requirements.Sections[0].Title);
        Assert.Equal(2, requirements.Sections[0].Sections.Count);
        Assert.Equal("User Authentication", requirements.Sections[0].Sections[0].Title);
        Assert.Equal("Logging", requirements.Sections[0].Sections[1].Title);
        Assert.Equal("AUTH-001", requirements.Sections[0].Sections[0].Requirements[0].Id);
        Assert.Equal("LOG-001", requirements.Sections[0].Sections[1].Requirements[0].Id);
    }

    /// <summary>
    ///     Test that a blank section title reports an error issue with file location.
    /// </summary>
    [Fact]
    public void Section_Load_BlankSectionTitle_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with a blank section title
        var yamlContent = @"---
sections:
  - title: """"
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var filePath = PathHelpers.SafePathCombine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the blank section title
        Assert.True(result.HasErrors);
        Assert.Null(result.Requirements);
        Assert.Contains(result.Issues, i => i.Description.Contains("Section 'title' cannot be blank"));
        Assert.Contains(result.Issues, i => i.Location.Contains(filePath));
    }
}
