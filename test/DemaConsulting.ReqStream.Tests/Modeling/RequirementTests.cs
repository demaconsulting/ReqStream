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
/// Unit tests for the Requirement class, proving it correctly holds its data fields
/// and that invalid values are detected during loading.
/// </summary>
[TestClass]
public class RequirementTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_requirement_test_{Guid.NewGuid()}");
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
    /// Test reading a requirement with tests.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_RequirementWithTests_ParsesTestsCorrectly()
    {
        // Arrange: create a YAML file with a requirement that has test references
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        Assert.IsFalse(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: tests parsed correctly
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
    public void Requirements_Load_RequirementWithChildren_ParsesChildrenCorrectly()
    {
        // Arrange: create a YAML file with a requirement that has child references
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
        children:
          - ""AUTH-001""
          - ""AUTH-002""
      - id: ""AUTH-001""
        title: ""The system shall validate user credentials.""
      - id: ""AUTH-002""
        title: ""The system shall reject invalid credentials.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        Assert.IsFalse(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: children parsed correctly
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
    public void Requirements_Load_RequirementWithJustification_ParsesJustificationCorrectly()
    {
        // Arrange: create a YAML file with a requirement that has a justification
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        Assert.IsFalse(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: justification parsed correctly
        Assert.IsNotNull(requirements);
        var req = requirements.Sections[0].Requirements[0];
        Assert.AreEqual("SYS-SEC-001", req.Id);
        Assert.AreEqual("The system shall support credentials authentication.", req.Title);
        Assert.IsNotNull(req.Justification);
        Assert.Contains("authorized users", req.Justification);
        Assert.Contains("data security", req.Justification);
    }

    /// <summary>
    ///     Test reading a requirement with tags.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_RequirementWithTags_ParsesTagsCorrectly()
    {
        // Arrange: create a YAML file with a requirement that has tags
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        Assert.IsFalse(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: tags parsed correctly
        Assert.IsNotNull(requirements);
        var req = requirements.Sections[0].Requirements[0];
        Assert.AreEqual("SYS-SEC-001", req.Id);
        Assert.HasCount(2, req.Tags);
        Assert.AreEqual("security", req.Tags[0]);
        Assert.AreEqual("critical", req.Tags[1]);
    }

    /// <summary>
    /// Test that duplicate requirement IDs report an error issue.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_DuplicateRequirementId_ReportsError()
    {
        // Arrange: create a YAML file with two requirements sharing the same ID
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported for duplicate ID
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("SYS-SEC-001"), result.Issues);
        Assert.Contains(i => i.Description.Contains("Duplicate requirement ID"), result.Issues);
    }

    /// <summary>
    ///     Test that duplicate requirement ID message includes file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_DuplicateRequirementId_ErrorIncludesFileLocation()
    {
        // Arrange: create a YAML file with two requirements sharing the same ID
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the duplicate ID
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("SYS-SEC-001"), result.Issues);
        Assert.Contains(i => i.Description.Contains("Duplicate requirement ID"), result.Issues);
        Assert.Contains(i => i.Location.Contains(filePath), result.Issues);
    }

    /// <summary>
    ///     Test that a blank requirement ID reports an error issue with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with a blank requirement ID
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: """"
        title: ""The system shall support credentials authentication.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the blank ID
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Requirement 'id' cannot be blank"), result.Issues);
        Assert.Contains(i => i.Location.Contains(filePath), result.Issues);
    }

    /// <summary>
    ///     Test that a blank requirement title reports an error issue with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with a blank requirement title
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: """"
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the blank title
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Requirement 'title' cannot be blank"), result.Issues);
        Assert.Contains(i => i.Location.Contains(filePath), result.Issues);
    }

    /// <summary>
    ///     Test that a blank test name in a requirement reports an error issue with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_BlankTestNameInRequirement_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with a blank test name entry in a requirement
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the blank test name
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Test name cannot be blank"), result.Issues);
        Assert.Contains(i => i.Location.Contains(filePath), result.Issues);
    }

    /// <summary>
    ///     Test that a blank test name in a mapping reports an error issue with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_BlankTestNameInMapping_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with a blank test name in a mapping
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the blank mapping test name
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Test name cannot be blank"), result.Issues);
        Assert.Contains(i => i.Location.Contains(filePath), result.Issues);
    }

    /// <summary>
    ///     Test that a blank mapping ID reports an error issue with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_BlankMappingId_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with a blank mapping ID
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the blank mapping ID
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Mapping 'id' cannot be blank"), result.Issues);
        Assert.Contains(i => i.Location.Contains(filePath), result.Issues);
    }

    /// <summary>
    /// Test reading test mappings that are separate from requirements.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_TestMappings_AppliesMappingsCorrectly()
    {
        // Arrange: create a YAML file with a mapping block that adds tests to an existing requirement
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);
        Assert.IsFalse(result.HasErrors);
        var requirements = result.Requirements;

        // Assert: mappings applied correctly
        Assert.IsNotNull(requirements);
        var req = requirements.Sections[0].Requirements[0];
        Assert.AreEqual("DATA-001", req.Id);
        Assert.HasCount(2, req.Tests);
        Assert.AreEqual("Logging_ValidRequest_Logged", req.Tests[0]);
        Assert.AreEqual("Logging_InvalidRequest_Logged", req.Tests[1]);
    }

    /// <summary>
    ///     Test that circular requirements (A -> B -> A) throw an exception at read time.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_CircularRequirements_ThrowsInvalidOperationException()
    {
        // Arrange: create a YAML file with circular child references
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: circular reference error reported
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Circular requirement reference detected"), result.Issues);
        Assert.Contains(i => i.Description.Contains("REQ-A"), result.Issues);
        Assert.Contains(i => i.Description.Contains("REQ-B"), result.Issues);
    }

    /// <summary>
    ///     Test that a self-referencing requirement (A -> A) reports an error issue at load time.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_SelfReferencingRequirement_ReportsCircularReferenceError()
    {
        // Arrange: create a YAML file with a self-referencing child
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: circular reference error reported
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Circular requirement reference detected"), result.Issues);
        Assert.Contains(i => i.Description.Contains("REQ-A"), result.Issues);
    }

    /// <summary>
    ///     Test that duplicate IDs across multiple files are detected.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_MultipleFilesWithDuplicateIds_ReportsError()
    {
        // Arrange: create two YAML files that share a requirement ID
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

        // Act: load both files
        var result = Requirements.Load(file1Path, file2Path);

        // Assert: error reported for duplicate ID across files
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("SYS-SEC-001"), result.Issues);
        Assert.Contains(i => i.Description.Contains("Duplicate requirement ID"), result.Issues);
        Assert.Contains(i => i.Location.Contains(file2Path), result.Issues);
    }

    /// <summary>
    ///     Test that a blank tag name reports an error issue with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_BlankTagName_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with a blank tag name
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

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the blank tag name
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Tag name cannot be blank"), result.Issues);
        Assert.Contains(i => i.Location.Contains(filePath), result.Issues);
    }

    /// <summary>
    ///     Test that a blank child ID in a requirement reports an error issue with file location.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_BlankChildIdInRequirement_ReportsErrorWithFileLocation()
    {
        // Arrange: create a YAML file with a blank entry in a requirement's children list
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
        children:
          - ""AUTH-001""
          - """"
      - id: ""AUTH-001""
        title: ""The system shall validate user credentials.""
";
        var filePath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(filePath, yamlContent);

        // Act: load the requirements file
        var result = Requirements.Load(filePath);

        // Assert: error reported with file location for the blank child ID
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.Contains(i => i.Description.Contains("Child requirement reference cannot be blank"), result.Issues);
        Assert.Contains(i => i.Location.Contains(filePath), result.Issues);
    }
}
