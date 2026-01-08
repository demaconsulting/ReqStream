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

namespace DemaConsulting.ReqStream.Tests;

/// <summary>
/// Unit tests for Requirements Markdown export functionality.
/// </summary>
[TestClass]
public class RequirementsExportTests
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
    /// Test exporting a simple requirements document to Markdown.
    /// </summary>
    [TestMethod]
    public void Export_SimpleRequirements_CreatesMarkdownFile()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
      - id: ""SYS-SEC-002""
        title: ""The system shall enforce password complexity.""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var requirements = Requirements.Read(reqPath);

        var mdPath = Path.Combine(_testDirectory, "requirements.md");
        requirements.Export(mdPath);

        Assert.IsTrue(File.Exists(mdPath));
        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "# System Security");
        StringAssert.Contains(content, "| ID | Title |");
        StringAssert.Contains(content, "| SYS-SEC-001 | The system shall support credentials authentication. |");
        StringAssert.Contains(content, "| SYS-SEC-002 | The system shall enforce password complexity. |");
    }

    /// <summary>
    /// Test exporting requirements with custom depth.
    /// </summary>
    [TestMethod]
    public void Export_WithCustomDepth_UsesCorrectHeaderLevel()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var requirements = Requirements.Read(reqPath);

        var mdPath = Path.Combine(_testDirectory, "requirements.md");
        requirements.Export(mdPath, depth: 3);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "### System Security");
    }

    /// <summary>
    /// Test exporting nested sections with proper hierarchy.
    /// </summary>
    [TestMethod]
    public void Export_NestedSections_CreatesHierarchy()
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
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var requirements = Requirements.Read(reqPath);

        var mdPath = Path.Combine(_testDirectory, "requirements.md");
        requirements.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "# Data Management");
        StringAssert.Contains(content, "## User Authentication");
        StringAssert.Contains(content, "## Logging");
        StringAssert.Contains(content, "| AUTH-001 | All requests shall be authenticated. |");
        StringAssert.Contains(content, "| LOG-001 | All requests shall be logged. |");
    }

    /// <summary>
    /// Test exporting a section with no requirements (only subsections).
    /// </summary>
    [TestMethod]
    public void Export_SectionWithNoRequirements_CreatesHeaderOnly()
    {
        var yamlContent = @"---
sections:
  - title: ""Parent Section""
    sections:
      - title: ""Child Section""
        requirements:
          - id: ""CHILD-001""
            title: ""Child requirement.""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var requirements = Requirements.Read(reqPath);

        var mdPath = Path.Combine(_testDirectory, "requirements.md");
        requirements.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "# Parent Section");
        StringAssert.Contains(content, "## Child Section");
        StringAssert.Contains(content, "| CHILD-001 | Child requirement. |");
    }

    /// <summary>
    /// Test that export throws exception when file path is null.
    /// </summary>
    [TestMethod]
    public void Export_NullFilePath_ThrowsArgumentException()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var requirements = Requirements.Read(reqPath);

        try
        {
            requirements.Export(null!);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException ex)
        {
            StringAssert.Contains(ex.Message, "File path cannot be null or empty");
        }
    }

    /// <summary>
    /// Test that export throws exception when file path is empty.
    /// </summary>
    [TestMethod]
    public void Export_EmptyFilePath_ThrowsArgumentException()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var requirements = Requirements.Read(reqPath);

        try
        {
            requirements.Export(string.Empty);
            Assert.Fail("Expected ArgumentException was not thrown");
        }
        catch (ArgumentException ex)
        {
            StringAssert.Contains(ex.Message, "File path cannot be null or empty");
        }
    }

    /// <summary>
    /// Test exporting multiple sections at the root level.
    /// </summary>
    [TestMethod]
    public void Export_MultipleSections_ExportsAll()
    {
        var yamlContent = @"---
sections:
  - title: ""System Security""
    requirements:
      - id: ""SYS-SEC-001""
        title: ""The system shall support credentials authentication.""
  - title: ""Data Management""
    requirements:
      - id: ""DATA-001""
        title: ""All requests shall be logged.""
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var requirements = Requirements.Read(reqPath);

        var mdPath = Path.Combine(_testDirectory, "requirements.md");
        requirements.Export(mdPath);

        var content = File.ReadAllText(mdPath);
        StringAssert.Contains(content, "# System Security");
        StringAssert.Contains(content, "# Data Management");
        StringAssert.Contains(content, "| SYS-SEC-001 | The system shall support credentials authentication. |");
        StringAssert.Contains(content, "| DATA-001 | All requests shall be logged. |");
    }

    /// <summary>
    /// Test exporting empty requirements document.
    /// </summary>
    [TestMethod]
    public void Export_EmptyRequirements_CreatesEmptyFile()
    {
        var yamlContent = @"---
";
        var reqPath = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqPath, yamlContent);
        var requirements = Requirements.Read(reqPath);

        var mdPath = Path.Combine(_testDirectory, "requirements.md");
        requirements.Export(mdPath);

        Assert.IsTrue(File.Exists(mdPath));
        var content = File.ReadAllText(mdPath);
        Assert.AreEqual(string.Empty, content);
    }
}
