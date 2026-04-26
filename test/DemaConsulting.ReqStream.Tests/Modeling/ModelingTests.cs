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
/// Tests for the Modeling subsystem, proving the Requirements class is sufficient to
/// implement the Modeling subsystem requirements.
/// </summary>
[TestClass]
public class ModelingTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_modeling_{Guid.NewGuid()}");
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
    /// Test that loading a valid YAML file produces a requirements model with no errors.
    /// </summary>
    [TestMethod]
    public void Modeling_YamlParsing_ValidFile_LoadsRequirements()
    {
        // Arrange: create a valid requirements YAML file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Modeling Test Requirements
                requirements:
                  - id: Modeling-Test-Req1
                    title: The system shall have a testable requirement.
                    justification: Test justification.
                    tests:
                      - SomeTest
            """);

        // Act: load the requirements file
        var result = Requirements.Load(reqFile);

        // Assert: requirements loaded successfully with no errors
        Assert.IsNotNull(result.Requirements);
        Assert.IsFalse(result.HasErrors);
        Assert.HasCount(1, result.Requirements.Sections);
        Assert.AreEqual("Modeling-Test-Req1", result.Requirements.Sections[0].Requirements[0].Id);
    }

    /// <summary>
    /// Test that loading a valid YAML file produces no lint issues.
    /// </summary>
    [TestMethod]
    public void Modeling_YamlParsing_ValidFile_ReturnsNoLintIssues()
    {
        // Arrange: create a structurally valid requirements YAML file with no duplicate IDs
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Lint Test Requirements
                requirements:
                  - id: Lint-Test-Req1
                    title: The system shall have a valid requirement.
                    justification: Lint test justification.
                    tests:
                      - LintTest1
            """);

        // Act: load the requirements file
        var result = Requirements.Load(reqFile);

        // Assert: no lint issues reported
        Assert.IsFalse(result.HasErrors);
        Assert.HasCount(0, result.Issues);
    }

    /// <summary>
    /// Test that loading a YAML file with duplicate requirement IDs reports a lint error.
    /// </summary>
    [TestMethod]
    public void Modeling_YamlParsing_DuplicateIds_DetectsLintError()
    {
        // Arrange: create a requirements YAML file containing duplicate requirement IDs
        var reqFile = Path.Combine(_testDirectory, "invalid.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Duplicate ID Test
                requirements:
                  - id: Lint-Duplicate-Req
                    title: The first requirement.
                    justification: First.
                    tests:
                      - Test1
                  - id: Lint-Duplicate-Req
                    title: The second requirement with duplicate ID.
                    justification: Second.
                    tests:
                      - Test2
            """);

        // Act: load the requirements file
        var result = Requirements.Load(reqFile);

        // Assert: an error-level lint issue was detected
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Issues.Count > 0, "Expected at least one lint issue to be reported.");
        Assert.IsTrue(result.Issues.Any(i => i.Severity == LintSeverity.Error), "Expected at least one Error-severity lint issue.");
    }

    /// <summary>
    /// Test that a requirements Markdown report is generated correctly.
    /// </summary>
    [TestMethod]
    public void Modeling_Export_Requirements_GeneratesMarkdownFile()
    {
        // Arrange: create a requirements file with one testable requirement
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Modeling Test Requirements
                requirements:
                  - id: Modeling-Test-Req1
                    title: The system shall have a testable requirement.
                    justification: Test justification.
                    tests:
                      - SomeTest
            """);
        var loadResult = Requirements.Load(reqFile);
        Assert.IsNotNull(loadResult.Requirements);

        var reportFile = Path.Combine(_testDirectory, "requirements.md");

        // Act: export the requirements to a Markdown file
        loadResult.Requirements.Export(reportFile);

        // Assert: report file exists and contains the requirement ID and title
        Assert.IsTrue(File.Exists(reportFile), "Requirements report should be generated.");
        var content = File.ReadAllText(reportFile);
        Assert.Contains("Modeling-Test-Req1", content);
        Assert.Contains("The system shall have a testable requirement.", content);
    }

    /// <summary>
    /// Test that a justifications Markdown report is generated correctly.
    /// </summary>
    [TestMethod]
    public void Modeling_Export_Justifications_GeneratesMarkdownFile()
    {
        // Arrange: create a requirements file with one justified requirement
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Modeling Test Requirements
                requirements:
                  - id: Modeling-Test-Req2
                    title: The system shall have a justified requirement.
                    justification: This justification explains why the requirement is needed.
                    tests:
                      - SomeTest
            """);
        var loadResult = Requirements.Load(reqFile);
        Assert.IsNotNull(loadResult.Requirements);

        var justificationsFile = Path.Combine(_testDirectory, "justifications.md");

        // Act: export the justifications to a Markdown file
        loadResult.Requirements.ExportJustifications(justificationsFile);

        // Assert: report file exists and contains the requirement ID and justification text
        Assert.IsTrue(File.Exists(justificationsFile), "Justifications report should be generated.");
        var content = File.ReadAllText(justificationsFile);
        Assert.Contains("Modeling-Test-Req2", content);
        Assert.Contains("This justification explains why the requirement is needed.", content);
    }

    /// <summary>
    /// Test that the Modeling subsystem detects an error when loading a malformed YAML file.
    /// </summary>
    [TestMethod]
    public void Modeling_Linting_MalformedYaml_DetectsError()
    {
        // Arrange: create a requirements file containing malformed YAML
        var reqFile = Path.Combine(_testDirectory, "malformed.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Bad Requirements
                requirements:
                  - id: [unclosed bracket
            """);

        // Act: load the malformed requirements file through the Modeling subsystem entry point
        var result = Requirements.Load(reqFile);

        // Assert: an error-level lint issue is reported and requirements are null
        Assert.IsTrue(result.HasErrors);
        Assert.IsNull(result.Requirements);
        Assert.IsTrue(result.Issues.Count > 0, "Expected at least one lint issue to be reported.");
        Assert.IsTrue(result.Issues.Any(i => i.Severity == LintSeverity.Error), "Expected at least one Error-severity lint issue.");
    }

    /// <summary>
    /// Test that the Modeling subsystem reports no issues when loading a valid requirements file.
    /// </summary>
    [TestMethod]
    public void Modeling_Linting_ValidFile_ReturnsNoIssues()
    {
        // Arrange: create a structurally valid requirements YAML file
        var reqFile = Path.Combine(_testDirectory, "valid.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Valid Linting Requirements
                requirements:
                  - id: Modeling-Lint-Valid-Req1
                    title: The system shall satisfy this requirement.
                    justification: Justification for the requirement.
                    tests:
                      - Modeling_Linting_ValidFile_ReturnsNoIssues
            """);

        // Act: load the valid requirements file through the Modeling subsystem entry point
        var result = Requirements.Load(reqFile);

        // Assert: no lint issues are reported
        Assert.IsFalse(result.HasErrors);
        Assert.HasCount(0, result.Issues);
    }

    /// <summary>
    /// Test that the Modeling subsystem reports ALL lint issues when multiple independent
    /// lint conditions are present in one load, not just the first one encountered.
    /// </summary>
    [TestMethod]
    public void Modeling_LintingReporting_MultipleConditions_ReportsAllIssues()
    {
        // Arrange: create a requirements file with two independent lint errors:
        //   (1) a section missing its title field
        //   (2) a requirement missing its title field
        var reqFile = Path.Combine(_testDirectory, "multi_lint.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - requirements:
                  - id: Modeling-MultiLint-Req1
              - title: Second Section
                requirements:
                  - id: Modeling-MultiLint-Req2
            """);

        // Act: load through the Modeling subsystem entry point
        var result = Requirements.Load(reqFile);

        // Assert: both lint issues are reported (not just HasErrors == true)
        Assert.IsTrue(result.HasErrors);
        Assert.IsTrue(result.Issues.Count >= 2,
            $"Expected at least 2 lint issues but got {result.Issues.Count}.");
        Assert.IsTrue(result.Issues.All(i => i.Severity == LintSeverity.Error),
            "All reported issues should be Error severity.");
    }
}
