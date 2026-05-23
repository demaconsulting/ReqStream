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
/// Unit tests for Requirements YAML loading and model parsing functionality.
/// </summary>
public sealed class RequirementsLoadParsingTests : IDisposable
{
    /// <summary>Temporary directory providing isolated file-system workspace for this test class instance.</summary>
    private readonly TemporaryDirectory _testDirectory = new();

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public RequirementsLoadParsingTests()
    {

    }

    /// <summary>
    /// Clean up test by deleting the temporary test directory.
    /// </summary>
    public void Dispose()
    {
        _testDirectory.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Test reading a file with includes.
    /// </summary>
    [Fact]
    public void Requirements_Load_WithIncludes_MergesFilesCorrectly()
    {
        // Arrange: create a main YAML file with an include directive pointing to an additional file
        var mainYaml = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""

includes:
  - ""additional.yaml""
";
        var includedYaml = @"---
sections:
  - title: ""Data Management""
    requirements:
      - id: ""DATA-001""
        title: ""All requests shall be logged.""
";
        var mainPath = _testDirectory.GetFilePath("main.yaml");
        var includedPath = _testDirectory.GetFilePath("additional.yaml");
        File.WriteAllText(mainPath, mainYaml);
        File.WriteAllText(includedPath, includedYaml);

        // Act: load the main requirements file
        var result = Requirements.Load(mainPath);
        Assert.False(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: requirements from both files merged into the tree
        Assert.NotNull(requirements);
        Assert.Equal(2, requirements.Sections.Count);
        Assert.Equal("System Security", requirements.Sections[0].Title);
        Assert.Equal("Data Management", requirements.Sections[1].Title);
        Assert.Equal("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.Equal("DATA-001", requirements.Sections[1].Requirements[0].Id);
    }

    /// <summary>
    /// Test that identical sections are merged.
    /// </summary>
    [Fact]
    public void Requirements_Load_IdenticalSections_MergesCorrectly()
    {
        // Arrange: create two YAML files with the same section title
        var mainYaml = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""

includes:
  - ""additional.yaml""
";
        var includedYaml = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-002""
        title: ""The system shall enforce password complexity.""
";
        var mainPath = _testDirectory.GetFilePath("main.yaml");
        var includedPath = _testDirectory.GetFilePath("additional.yaml");
        File.WriteAllText(mainPath, mainYaml);
        File.WriteAllText(includedPath, includedYaml);

        // Act: load the main requirements file
        var result = Requirements.Load(mainPath);
        Assert.False(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: identical sections merged into one with both requirements
        Assert.NotNull(requirements);
        Assert.Single(requirements.Sections);
        Assert.Equal("System Security", requirements.Sections[0].Title);
        Assert.Equal(2, requirements.Sections[0].Requirements.Count);
        Assert.Equal("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.Equal("SYS-SEC-002", requirements.Sections[0].Requirements[1].Id);
    }

    /// <summary>
    /// Test that include loops are prevented.
    /// </summary>
    [Fact]
    public void Requirements_Load_IncludeLoop_DoesNotCauseInfiniteLoop()
    {
        // Arrange: create two YAML files that include each other
        var fileA = @"---
sections:
  - title: ""File A""
    requirements:
      - id: ""A-001""
        title: ""Requirement from file A.""

includes:
  - ""fileB.yaml""
";
        var fileB = @"---
sections:
  - title: ""File B""
    requirements:
      - id: ""B-001""
        title: ""Requirement from file B.""

includes:
  - ""fileA.yaml""
";
        var pathA = _testDirectory.GetFilePath("fileA.yaml");
        var pathB = _testDirectory.GetFilePath("fileB.yaml");
        File.WriteAllText(pathA, fileA);
        File.WriteAllText(pathB, fileB);

        // Act: load file A (which includes file B, which includes file A)
        var result = Requirements.Load(pathA);

        // Assert: loading completes without infinite loop, circular include is reported as error,
        //         and Requirements is null (errors prevent returning a partial result)
        Assert.True(result.HasErrors);
        Assert.Contains(result.Issues, i => i.Description.Contains("Circular include"));
        Assert.Null(result.Requirements);
    }

    /// <summary>
    /// Test that a missing file reports an error issue.
    /// </summary>
    [Fact]
    public void Requirements_Load_FileNotFound_ReportsError()
    {
        // Arrange: create a path to a file that does not exist
        var nonExistentPath = _testDirectory.GetFilePath("nonexistent.yaml");

        // Act: load the non-existent file
        var result = Requirements.Load(nonExistentPath);

        // Assert: error reported with the missing file location
        Assert.True(result.HasErrors);
        Assert.Null(result.Requirements);
        Assert.Contains(result.Issues, i => i.Description.Contains("File not found"));
        Assert.Contains(result.Issues, i => i.Location.Contains(nonExistentPath));
    }

    /// <summary>
    /// Test that an invalid YAML content (schema error) throws an InvalidOperationException with the file location.
    /// </summary>
    [Fact]
    public void Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with an invalid property name
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        text: ""This uses an invalid property name.""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the unknown field
        Assert.True(result.HasErrors);
        Assert.Null(result.Requirements);
        Assert.Contains(result.Issues, i => i.Description.Contains("Unknown field 'text' in requirement"));
        Assert.Contains(result.Issues, i => i.Location.Contains(filePath));
        Assert.Contains(result.Issues, i => i.Location.Contains($"{filePath}("));
    }

    /// <summary>
    /// Test reading an empty YAML file.
    /// </summary>
    [Fact]
    public void Requirements_Load_EmptyFile_ReturnsEmptyRequirements()
    {
        // Arrange: create an empty YAML file
        var yamlContent = @"---
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        Assert.False(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: empty requirements returned with no sections
        Assert.NotNull(requirements);
        Assert.Empty(requirements.Sections);
        Assert.Empty(requirements.Requirements);
    }

    /// <summary>
    /// Test reading a complex nested structure.
    /// </summary>
    [Fact]
    public void Requirements_Load_ComplexStructure_ParsesCorrectly()
    {
        // Arrange: create a YAML file with a complex nested structure including children, tests, and mappings
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
        children:
          - ""AUTH-001""

  - title: ""Data Management""
    sections:
      - title: ""User Authentication""
        requirements:
          - id: ""AUTH-001""
            title: ""All requests shall have their credentials authenticated.""
            tests:
              - ""Credentials_Valid_Allowed""
              - ""Credentials_Invalid_Refused""
              - ""Credentials_Missing_Refused""

      - title: ""Logging""
        requirements:
          - id: ""DATA-001""
            title: ""All requests shall be logged.""

mappings:
  - id: ""DATA-001""
    tests:
      - ""Logging_ValidRequest_Logged""
      - ""Logging_InvalidRequest_Logged""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        Assert.False(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: complex structure parsed correctly
        Assert.NotNull(requirements);
        Assert.Equal(2, requirements.Sections.Count);

        var sysSec = requirements.Sections[0];
        Assert.Equal("System Security", sysSec.Title);
        Assert.Single(sysSec.Requirements);
        Assert.Equal("SYS-SEC-001", sysSec.Requirements[0].Id);
        Assert.Single(sysSec.Requirements[0].Children);
        Assert.Equal("AUTH-001", sysSec.Requirements[0].Children[0]);

        var dataManagement = requirements.Sections[1];
        Assert.Equal("Data Management", dataManagement.Title);
        Assert.Equal(2, dataManagement.Sections.Count);

        var auth = dataManagement.Sections[0];
        Assert.Equal("User Authentication", auth.Title);
        Assert.Equal("AUTH-001", auth.Requirements[0].Id);
        Assert.Equal(3, auth.Requirements[0].Tests.Count);

        var logging = dataManagement.Sections[1];
        Assert.Equal("Logging", logging.Title);
        Assert.Equal("DATA-001", logging.Requirements[0].Id);
        Assert.Equal(2, logging.Requirements[0].Tests.Count);
        Assert.Equal("Logging_ValidRequest_Logged", logging.Requirements[0].Tests[0]);
    }

    /// <summary>
    ///     Test reading multiple files with params array.
    /// </summary>
    [Fact]
    public void Requirements_Load_MultipleFiles_MergesAllFiles()
    {
        // Arrange: create three YAML files with different sections
        var file1Yaml = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var file2Yaml = @"---
sections:
  - title: ""Data Management""
    requirements:
      - id: ""DATA-001""
        title: ""All requests shall be logged.""
";
        var file3Yaml = @"---
sections:
  - title: ""Performance""
    requirements:
      - id: ""PERF-001""
        title: ""The system shall respond within 100ms.""
";
        var file1Path = _testDirectory.GetFilePath("file1.yaml");
        var file2Path = _testDirectory.GetFilePath("file2.yaml");
        var file3Path = _testDirectory.GetFilePath("file3.yaml");
        File.WriteAllText(file1Path, file1Yaml);
        File.WriteAllText(file2Path, file2Yaml);
        File.WriteAllText(file3Path, file3Yaml);

        // Act: load all three files
        var result = Requirements.Load(file1Path, file2Path, file3Path);
        Assert.False(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: all three sections merged into the requirements tree
        Assert.NotNull(requirements);
        Assert.Equal(3, requirements.Sections.Count);
        Assert.Equal("System Security", requirements.Sections[0].Title);
        Assert.Equal("Data Management", requirements.Sections[1].Title);
        Assert.Equal("Performance", requirements.Sections[2].Title);
        Assert.Equal("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.Equal("DATA-001", requirements.Sections[1].Requirements[0].Id);
        Assert.Equal("PERF-001", requirements.Sections[2].Requirements[0].Id);
    }

    /// <summary>
    ///     Test reading multiple files that merge sections.
    /// </summary>
    [Fact]
    public void Requirements_Load_MultipleFilesWithSameSections_MergesSections()
    {
        // Arrange: create two YAML files with the same section title
        var file1Yaml = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var file2Yaml = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-002""
        title: ""The system shall enforce password complexity.""
";
        var file1Path = _testDirectory.GetFilePath("file1.yaml");
        var file2Path = _testDirectory.GetFilePath("file2.yaml");
        File.WriteAllText(file1Path, file1Yaml);
        File.WriteAllText(file2Path, file2Yaml);

        // Act: load both files
        var result = Requirements.Load(file1Path, file2Path);
        Assert.False(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: sections with the same title merged into one section with both requirements
        Assert.NotNull(requirements);
        Assert.Single(requirements.Sections);
        Assert.Equal("System Security", requirements.Sections[0].Title);
        Assert.Equal(2, requirements.Sections[0].Requirements.Count);
        Assert.Equal("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.Equal("SYS-SEC-002", requirements.Sections[0].Requirements[1].Id);
    }

    /// <summary>
    ///     Test reading single file with params array (backwards compatibility).
    /// </summary>
    [Fact]
    public void Requirements_Load_SingleFileWithParamsArray_WorksCorrectly()
    {
        // Arrange: create a YAML file with one requirement
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        Assert.False(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: requirement loaded correctly
        Assert.NotNull(requirements);
        Assert.Single(requirements.Sections);
        Assert.Equal("System Security", requirements.Sections[0].Title);
        Assert.Single(requirements.Sections[0].Requirements);
        Assert.Equal("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
    }

    /// <summary>
    ///     Test that calling Read with no arguments throws ArgumentException.
    /// </summary>
    [Fact]
    public void Requirements_Load_NoArguments_ThrowsArgumentException()
    {
        // Act + Assert: calling Load with no arguments throws ArgumentException
        var ex = Assert.Throws<ArgumentException>(() => Requirements.Load());
        Assert.Contains("At least one file path must be provided", ex.Message);
    }

    /// <summary>
    ///     Test that calling Read with null throws ArgumentException.
    /// </summary>
    [Fact]
    public void Requirements_Load_NullArgument_ThrowsArgumentException()
    {
        // Act + Assert: calling Load with null throws ArgumentException
        var ex = Assert.Throws<ArgumentException>(() => Requirements.Load(null!));
        Assert.Contains("At least one file path must be provided", ex.Message);
    }
}
