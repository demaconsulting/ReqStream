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
/// Unit tests for Requirements YAML reading functionality.
/// </summary>
[TestClass]
public class RequirementsReadTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_test_{Guid.NewGuid()}");
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
    /// Test reading a simple YAML file with a single requirement.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_SimpleRequirement_ParsesCorrectly()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        Assert.HasCount(1, requirements.Sections);
        Assert.AreEqual("System Security", requirements.Sections[0].Title);
        Assert.HasCount(1, requirements.Sections[0].Requirements);
        Assert.AreEqual("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.AreEqual("The system shall support credentials authentication.", requirements.Sections[0].Requirements[0].Title);
    }

    /// <summary>
    /// Test reading a requirement with tests.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_RequirementWithTests_ParsesTestsCorrectly()
    {
        var yamlContent = @"---
sections:
  - title: ""User Authentication""
    requirements:
      - id: ""AUTH-001""
        title: ""All requests shall have their credentials authenticated.""
        tests:
          - ""Credentials_Valid_Allowed""
          - ""Credentials_Invalid_Refused""
          - ""Credentials_Missing_Refused""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        var req = requirements.Sections[0].Requirements[0];
        Assert.AreEqual("AUTH-001", req.Id);
        Assert.HasCount(3, req.Tests);
        Assert.AreEqual("Credentials_Valid_Allowed", req.Tests[0]);
        Assert.AreEqual("Credentials_Invalid_Refused", req.Tests[1]);
        Assert.AreEqual("Credentials_Missing_Refused", req.Tests[2]);
    }

    /// <summary>
    /// Test reading a requirement with child requirements.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_RequirementWithChildren_ParsesChildrenCorrectly()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
        children:
          - ""AUTH-001""
          - ""AUTH-002""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        var req = requirements.Sections[0].Requirements[0];
        Assert.AreEqual("SYS-SEC-001", req.Id);
        Assert.HasCount(2, req.Children);
        Assert.AreEqual("AUTH-001", req.Children[0]);
        Assert.AreEqual("AUTH-002", req.Children[1]);
    }

    /// <summary>
    /// Test reading a requirement with justification.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_RequirementWithJustification_ParsesJustificationCorrectly()
    {
        var yamlContent = @"---
sections:
  - title: System Security
    requirements:
      - id: SYS-SEC-001
        title: The system shall support credentials authentication.
        justification: |
          This requirement is necessary to ensure that only authorized users
          can access the system and to maintain data security and integrity.
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        var req = requirements.Sections[0].Requirements[0];
        Assert.AreEqual("SYS-SEC-001", req.Id);
        Assert.AreEqual("The system shall support credentials authentication.", req.Title);
        Assert.IsNotNull(req.Justification);
        Assert.Contains("authorized users", req.Justification);
        Assert.Contains("data security", req.Justification);
    }

    /// <summary>
    /// Test reading nested sections.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_NestedSections_ParsesHierarchyCorrectly()
    {
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
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        Assert.HasCount(1, requirements.Sections);
        Assert.AreEqual("Data Management", requirements.Sections[0].Title);
        Assert.HasCount(2, requirements.Sections[0].Sections);
        Assert.AreEqual("User Authentication", requirements.Sections[0].Sections[0].Title);
        Assert.AreEqual("Logging", requirements.Sections[0].Sections[1].Title);
        Assert.AreEqual("AUTH-001", requirements.Sections[0].Sections[0].Requirements[0].Id);
        Assert.AreEqual("LOG-001", requirements.Sections[0].Sections[1].Requirements[0].Id);
    }

    /// <summary>
    /// Test reading test mappings that are separate from requirements.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_TestMappings_AppliesMappingsCorrectly()
    {
        var yamlContent = @"---
sections:
  - title: ""System""
    requirements:
      - id: ""DATA-001""
        title: ""All requests shall be logged.""

mappings:
  - id: ""DATA-001""
    tests:
      - ""Logging_ValidRequest_Logged""
      - ""Logging_InvalidRequest_Logged""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        var req = requirements.Sections[0].Requirements[0];
        Assert.AreEqual("DATA-001", req.Id);
        Assert.HasCount(2, req.Tests);
        Assert.AreEqual("Logging_ValidRequest_Logged", req.Tests[0]);
        Assert.AreEqual("Logging_InvalidRequest_Logged", req.Tests[1]);
    }

    /// <summary>
    /// Test reading a file with includes.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_WithIncludes_MergesFilesCorrectly()
    {
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
        var mainPath = Path.Combine(_testDirectory, "main.yaml");
        var includedPath = Path.Combine(_testDirectory, "additional.yaml");
        File.WriteAllText(mainPath, mainYaml);
        File.WriteAllText(includedPath, includedYaml);

        var requirements = Requirements.Read(mainPath);

        Assert.IsNotNull(requirements);
        Assert.HasCount(2, requirements.Sections);
        Assert.AreEqual("System Security", requirements.Sections[0].Title);
        Assert.AreEqual("Data Management", requirements.Sections[1].Title);
        Assert.AreEqual("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.AreEqual("DATA-001", requirements.Sections[1].Requirements[0].Id);
    }

    /// <summary>
    /// Test that identical sections are merged.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_IdenticalSections_MergesCorrectly()
    {
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
        var mainPath = Path.Combine(_testDirectory, "main.yaml");
        var includedPath = Path.Combine(_testDirectory, "additional.yaml");
        File.WriteAllText(mainPath, mainYaml);
        File.WriteAllText(includedPath, includedYaml);

        var requirements = Requirements.Read(mainPath);

        Assert.IsNotNull(requirements);
        Assert.HasCount(1, requirements.Sections);
        Assert.AreEqual("System Security", requirements.Sections[0].Title);
        Assert.HasCount(2, requirements.Sections[0].Requirements);
        Assert.AreEqual("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.AreEqual("SYS-SEC-002", requirements.Sections[0].Requirements[1].Id);
    }

    /// <summary>
    /// Test that duplicate requirement IDs throw an exception.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_DuplicateRequirementId_ThrowsException()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
      - id: ""SYS-SEC-001""
        title: ""Duplicate ID requirement.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("SYS-SEC-001", ex.Message);
        Assert.Contains("Duplicate requirement ID", ex.Message);
    }

    /// <summary>
    /// Test that include loops are prevented.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_IncludeLoop_DoesNotCauseInfiniteLoop()
    {
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
        var pathA = Path.Combine(_testDirectory, "fileA.yaml");
        var pathB = Path.Combine(_testDirectory, "fileB.yaml");
        File.WriteAllText(pathA, fileA);
        File.WriteAllText(pathB, fileB);

        var requirements = Requirements.Read(pathA);

        Assert.IsNotNull(requirements);
        Assert.HasCount(2, requirements.Sections);
    }

    /// <summary>
    /// Test that file not found throws an exception.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_FileNotFound_ThrowsException()
    {
        var nonExistentPath = Path.Combine(_testDirectory, "nonexistent.yaml");

        var ex = Assert.ThrowsExactly<FileNotFoundException>(() => Requirements.Read(nonExistentPath));
        Assert.Contains("Requirements file not found", ex.Message);
    }

    /// <summary>
    /// Test that a malformed YAML file throws an InvalidOperationException with the file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_MalformedYaml_ThrowsExceptionWithFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        text: ""This uses an invalid property name.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("YAML formatting error", ex.Message);
        Assert.Contains(filePath, ex.Message);
        Assert.Contains("line", ex.Message);
        Assert.Contains("col", ex.Message);
    }

    /// <summary>
    /// Test reading an empty YAML file.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_EmptyFile_ReturnsEmptyRequirements()
    {
        var yamlContent = @"---
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        Assert.IsEmpty(requirements.Sections);
        Assert.IsEmpty(requirements.Requirements);
    }

    /// <summary>
    /// Test reading a complex nested structure.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_ComplexStructure_ParsesCorrectly()
    {
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
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        Assert.HasCount(2, requirements.Sections);

        var sysSec = requirements.Sections[0];
        Assert.AreEqual("System Security", sysSec.Title);
        Assert.HasCount(1, sysSec.Requirements);
        Assert.AreEqual("SYS-SEC-001", sysSec.Requirements[0].Id);
        Assert.HasCount(1, sysSec.Requirements[0].Children);
        Assert.AreEqual("AUTH-001", sysSec.Requirements[0].Children[0]);

        var dataManagement = requirements.Sections[1];
        Assert.AreEqual("Data Management", dataManagement.Title);
        Assert.HasCount(2, dataManagement.Sections);

        var auth = dataManagement.Sections[0];
        Assert.AreEqual("User Authentication", auth.Title);
        Assert.AreEqual("AUTH-001", auth.Requirements[0].Id);
        Assert.HasCount(3, auth.Requirements[0].Tests);

        var logging = dataManagement.Sections[1];
        Assert.AreEqual("Logging", logging.Title);
        Assert.AreEqual("DATA-001", logging.Requirements[0].Id);
        Assert.HasCount(2, logging.Requirements[0].Tests);
        Assert.AreEqual("Logging_ValidRequest_Logged", logging.Requirements[0].Tests[0]);
    }

    /// <summary>
    ///     Test that blank requirement ID throws an exception with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_BlankRequirementId_ThrowsExceptionWithFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: """"
        title: ""The system shall support credentials authentication.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Requirement ID cannot be blank", ex.Message);
        Assert.Contains("System Security", ex.Message);
        Assert.Contains(filePath, ex.Message);
    }

    /// <summary>
    ///     Test that blank requirement title throws an exception with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_BlankRequirementTitle_ThrowsExceptionWithFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: """"
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Requirement title cannot be blank", ex.Message);
        Assert.Contains("SYS-SEC-001", ex.Message);
        Assert.Contains(filePath, ex.Message);
    }

    /// <summary>
    ///     Test that blank section title throws an exception with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_BlankSectionTitle_ThrowsExceptionWithFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: """"
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Section title cannot be blank", ex.Message);
        Assert.Contains(filePath, ex.Message);
    }

    /// <summary>
    ///     Test that blank test name in requirement throws an exception with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_BlankTestNameInRequirement_ThrowsExceptionWithFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
        tests:
          - ""ValidTest""
          - """"
          - ""AnotherTest""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Test name cannot be blank", ex.Message);
        Assert.Contains("SYS-SEC-001", ex.Message);
        Assert.Contains(filePath, ex.Message);
    }

    /// <summary>
    ///     Test that blank test name in mapping throws an exception with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_BlankTestNameInMapping_ThrowsExceptionWithFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""

mappings:
  - id: ""SYS-SEC-001""
    tests:
      - ""ValidTest""
      - """"
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Test name cannot be blank", ex.Message);
        Assert.Contains("SYS-SEC-001", ex.Message);
        Assert.Contains(filePath, ex.Message);
    }

    /// <summary>
    ///     Test that blank mapping ID throws an exception with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_BlankMappingId_ThrowsExceptionWithFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""

mappings:
  - id: """"
    tests:
      - ""ValidTest""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Mapping requirement ID cannot be blank", ex.Message);
        Assert.Contains(filePath, ex.Message);
    }

    /// <summary>
    ///     Test that duplicate requirement ID message includes file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_DuplicateRequirementId_ExceptionIncludesFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
      - id: ""SYS-SEC-001""
        title: ""Duplicate ID requirement.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("SYS-SEC-001", ex.Message);
        Assert.Contains("Duplicate requirement ID", ex.Message);
        Assert.Contains(filePath, ex.Message);
    }

    /// <summary>
    ///     Test reading multiple files with params array.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_MultipleFiles_MergesAllFiles()
    {
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
        var file1Path = Path.Combine(_testDirectory, "file1.yaml");
        var file2Path = Path.Combine(_testDirectory, "file2.yaml");
        var file3Path = Path.Combine(_testDirectory, "file3.yaml");
        File.WriteAllText(file1Path, file1Yaml);
        File.WriteAllText(file2Path, file2Yaml);
        File.WriteAllText(file3Path, file3Yaml);

        var requirements = Requirements.Read(file1Path, file2Path, file3Path);

        Assert.IsNotNull(requirements);
        Assert.HasCount(3, requirements.Sections);
        Assert.AreEqual("System Security", requirements.Sections[0].Title);
        Assert.AreEqual("Data Management", requirements.Sections[1].Title);
        Assert.AreEqual("Performance", requirements.Sections[2].Title);
        Assert.AreEqual("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.AreEqual("DATA-001", requirements.Sections[1].Requirements[0].Id);
        Assert.AreEqual("PERF-001", requirements.Sections[2].Requirements[0].Id);
    }

    /// <summary>
    ///     Test reading multiple files that merge sections.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_MultipleFilesWithSameSections_MergesSections()
    {
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
        var file1Path = Path.Combine(_testDirectory, "file1.yaml");
        var file2Path = Path.Combine(_testDirectory, "file2.yaml");
        File.WriteAllText(file1Path, file1Yaml);
        File.WriteAllText(file2Path, file2Yaml);

        var requirements = Requirements.Read(file1Path, file2Path);

        Assert.IsNotNull(requirements);
        Assert.HasCount(1, requirements.Sections);
        Assert.AreEqual("System Security", requirements.Sections[0].Title);
        Assert.HasCount(2, requirements.Sections[0].Requirements);
        Assert.AreEqual("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
        Assert.AreEqual("SYS-SEC-002", requirements.Sections[0].Requirements[1].Id);
    }

    /// <summary>
    ///     Test reading single file with params array (backwards compatibility).
    /// </summary>
    [TestMethod]
    public void Requirements_Read_SingleFileWithParamsArray_WorksCorrectly()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        Assert.HasCount(1, requirements.Sections);
        Assert.AreEqual("System Security", requirements.Sections[0].Title);
        Assert.HasCount(1, requirements.Sections[0].Requirements);
        Assert.AreEqual("SYS-SEC-001", requirements.Sections[0].Requirements[0].Id);
    }

    /// <summary>
    ///     Test that calling Read with no arguments throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_NoArguments_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Requirements.Read());
        Assert.Contains("At least one file path must be provided", ex.Message);
    }

    /// <summary>
    ///     Test that calling Read with null throws ArgumentException.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_NullArgument_ThrowsArgumentException()
    {
        var ex = Assert.ThrowsExactly<ArgumentException>(() => Requirements.Read(null!));
        Assert.Contains("At least one file path must be provided", ex.Message);
    }

    /// <summary>
    ///     Test that duplicate IDs across multiple files are detected.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_MultipleFilesWithDuplicateIds_ThrowsException()
    {
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
      - id: ""SYS-SEC-001""
        title: ""Duplicate requirement with same ID.""
";
        var file1Path = Path.Combine(_testDirectory, "file1.yaml");
        var file2Path = Path.Combine(_testDirectory, "file2.yaml");
        File.WriteAllText(file1Path, file1Yaml);
        File.WriteAllText(file2Path, file2Yaml);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(file1Path, file2Path));
        Assert.Contains("SYS-SEC-001", ex.Message);
        Assert.Contains("Duplicate requirement ID", ex.Message);
        Assert.Contains(file2Path, ex.Message);
    }

    /// <summary>
    ///     Test reading a requirement with tags.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_RequirementWithTags_ParsesTagsCorrectly()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
        tags:
          - ""security""
          - ""critical""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var requirements = Requirements.Read(filePath);

        Assert.IsNotNull(requirements);
        var req = requirements.Sections[0].Requirements[0];
        Assert.AreEqual("SYS-SEC-001", req.Id);
        Assert.HasCount(2, req.Tags);
        Assert.AreEqual("security", req.Tags[0]);
        Assert.AreEqual("critical", req.Tags[1]);
    }

    /// <summary>
    ///     Test that blank tag name throws an exception with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_BlankTagName_ThrowsExceptionWithFileLocation()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
        tags:
          - ""security""
          - """"
          - ""critical""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Tag name cannot be blank", ex.Message);
        Assert.Contains("SYS-SEC-001", ex.Message);
        Assert.Contains(filePath, ex.Message);
    }

    /// <summary>
    ///     Test that circular requirements (A -> B -> A) throw an exception at read time.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_CircularRequirements_ThrowsInvalidOperationException()
    {
        var yamlContent = @"---
sections:
  - title: ""Cyclic Section""
    requirements:
      - id: ""REQ-A""
        title: ""Requirement A""
        children:
          - ""REQ-B""
      - id: ""REQ-B""
        title: ""Requirement B""
        children:
          - ""REQ-A""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Circular requirement reference detected", ex.Message);
        Assert.Contains("REQ-A", ex.Message);
        Assert.Contains("REQ-B", ex.Message);
    }

    /// <summary>
    ///     Test that a self-referencing requirement (A -> A) throws an exception at read time.
    /// </summary>
    [TestMethod]
    public void Requirements_Read_SelfReferencingRequirement_ThrowsInvalidOperationException()
    {
        var yamlContent = @"---
sections:
  - title: ""Cyclic Section""
    requirements:
      - id: ""REQ-A""
        title: ""Requirement A""
        children:
          - ""REQ-A""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() => Requirements.Read(filePath));
        Assert.Contains("Circular requirement reference detected", ex.Message);
        Assert.Contains("REQ-A", ex.Message);
    }
}
