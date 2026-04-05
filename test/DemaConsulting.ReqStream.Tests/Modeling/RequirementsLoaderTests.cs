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
/// Unit tests for the RequirementsLoader: verifies that structural issues in requirements
/// YAML files are reported as lint issues when loading via Requirements.Load().
/// </summary>
[TestClass]
public class RequirementsLoaderTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_loader_test_{Guid.NewGuid()}");
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
    /// Helper: load files and return (hasErrors, all issue messages joined).
    /// </summary>
    private static (int exitCode, string errors) RunLint(params string[] files)
    {
        var result = Requirements.Load(files);
        var errors = string.Join(Environment.NewLine, result.Issues.Select(i => i.ToString()));
        var exitCode = result.HasErrors ? 1 : 0;
        return (exitCode, errors);
    }

    /// <summary>
    /// Helper: load files and return (hasErrors, output message, issue messages).
    /// The "output" simulates the success message produced when there are no issues.
    /// </summary>
    private static (int exitCode, string output, string errors) RunLintWithOutput(params string[] files)
    {
        var result = Requirements.Load(files);
        var errors = string.Join(Environment.NewLine, result.Issues.Select(i => i.ToString()));
        var exitCode = result.HasErrors ? 1 : 0;
        var output = exitCode == 0 && files.Length > 0 ? $"{files[0]}: No issues found" : string.Empty;
        return (exitCode, output, errors);
    }

    /// <summary>
    /// Test that loading with no files produces no issues (empty list).
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithNoFiles_PrintsMessage()
    {
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create([]);
            Program.Run(context);

            Assert.AreEqual(0, context.ExitCode);
            Assert.Contains("No requirements files specified", output.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test that a valid requirements file produces no issues.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithValidFile_ReportsNoIssues()
    {
        var reqFile = Path.Combine(_testDirectory, "valid.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tests:
          - SomeTest
        tags:
          - tag1
");

        var (exitCode, output, errors) = RunLintWithOutput(reqFile);

        Assert.AreEqual(0, exitCode);
        Assert.Contains("No issues found", output);
        Assert.AreEqual(string.Empty, errors);
    }

    /// <summary>
    /// Test that a file that doesn't exist reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithMissingFile_ReportsError()
    {
        var (exitCode, errors) = RunLint("/nonexistent/path/missing.yaml");

        Assert.AreEqual(1, exitCode);
        Assert.Contains("error", errors);
        Assert.Contains("File not found", errors);
    }

    /// <summary>
    /// Test that malformed YAML reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithMalformedYaml_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "malformed.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Bad
    requirements: [
  invalid yaml here
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("error", errors);
        Assert.Contains("Malformed YAML", errors);
    }

    /// <summary>
    /// Test that an empty YAML file produces no issues.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithEmptyFile_ReportsNoIssues()
    {
        var reqFile = Path.Combine(_testDirectory, "empty.yaml");
        File.WriteAllText(reqFile, string.Empty);

        var (exitCode, output, errors) = RunLintWithOutput(reqFile);

        Assert.AreEqual(0, exitCode);
        Assert.Contains("No issues found", output);
        Assert.AreEqual(string.Empty, errors);
    }

    /// <summary>
    /// Test that an unknown field at document root reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithUnknownDocumentField_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "unknown-field.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test
unknown_field: value
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field'", errors);
    }

    /// <summary>
    /// Test that a section missing the title field reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithSectionMissingTitle_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "missing-title.yaml");
        File.WriteAllText(reqFile, @"sections:
  - requirements:
      - id: REQ-001
        title: A requirement
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Section missing required field 'title'", errors);
    }

    /// <summary>
    /// Test that a section with a blank title reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithBlankSectionTitle_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "blank-title.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: ''
    requirements:
      - id: REQ-001
        title: A requirement
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Section 'title' cannot be blank", errors);
    }

    /// <summary>
    /// Test that a section with an unknown field reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithUnknownSectionField_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "unknown-section-field.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test
    unknown_field: value
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field' in section", errors);
    }

    /// <summary>
    /// Test that a requirement missing the id field reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithRequirementMissingId_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "missing-id.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - title: Requirement without ID
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Requirement missing required field 'id'", errors);
    }

    /// <summary>
    /// Test that a requirement missing the title field reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithRequirementMissingTitle_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "missing-req-title.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("missing required field 'title'", errors);
    }

    /// <summary>
    /// Test that a requirement with an unknown field reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithUnknownRequirementField_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "unknown-req-field.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        unknown_field: value
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field' in requirement", errors);
    }

    /// <summary>
    /// Test that duplicate requirement IDs report an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithDuplicateIds_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "duplicates.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: First requirement
      - id: REQ-001
        title: Duplicate requirement
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Duplicate requirement ID 'REQ-001'", errors);
    }

    /// <summary>
    /// Test that duplicate IDs across multiple files report an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithDuplicateIdsAcrossFiles_ReportsError()
    {
        var reqFile1 = Path.Combine(_testDirectory, "file1.yaml");
        File.WriteAllText(reqFile1, @"sections:
  - title: Section 1
    requirements:
      - id: REQ-001
        title: First requirement
");

        var reqFile2 = Path.Combine(_testDirectory, "file2.yaml");
        File.WriteAllText(reqFile2, @"sections:
  - title: Section 2
    requirements:
      - id: REQ-001
        title: Duplicate across files
");

        var (exitCode, errors) = RunLint(reqFile1, reqFile2);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Duplicate requirement ID 'REQ-001'", errors);
    }

    /// <summary>
    /// Test that multiple issues are all reported.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithMultipleIssues_ReportsAllIssues()
    {
        var reqFile = Path.Combine(_testDirectory, "multiple-issues.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    unknown_section_field: bad
    requirements:
      - id: REQ-001
        title: Good requirement
      - title: Missing id requirement
      - id: REQ-001
        title: Duplicate id
unknown_root_field: bad
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unknown field 'unknown_section_field' in section", errors);
        Assert.Contains("Requirement missing required field 'id'", errors);
        Assert.Contains("Duplicate requirement ID 'REQ-001'", errors);
        Assert.Contains("Unknown field 'unknown_root_field'", errors);
    }

    /// <summary>
    /// Test that loading follows includes and lints included files.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithIncludes_LintsIncludedFiles()
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

        var (exitCode, errors) = RunLint(rootFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field' in requirement", errors);
    }

    /// <summary>
    /// Test that a mapping with an unknown field reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithUnknownMappingField_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "unknown-mapping-field.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
mappings:
  - id: REQ-001
    tests:
      - SomeTest
    unknown_field: bad
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field' in mapping", errors);
    }

    /// <summary>
    /// Test that a mapping missing id reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithMappingMissingId_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "mapping-missing-id.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
mappings:
  - tests:
      - SomeTest
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Mapping missing required field 'id'", errors);
    }

    /// <summary>
    /// Test that a nested section with issues is linted.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithNestedSectionIssues_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "nested.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Parent Section
    sections:
      - title: Child Section
        requirements:
          - id: REQ-001
            title: Valid requirement
          - id: REQ-002
            unknown_req_field: bad
            title: Bad requirement
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Unknown field 'unknown_req_field' in requirement", errors);
    }

    /// <summary>
    /// Test that error format includes file path and line/column info.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_ErrorFormat_IncludesFileAndLocation()
    {
        var reqFile = Path.Combine(_testDirectory, "format-test.yaml");
        File.WriteAllText(reqFile, @"unknown_field: value
");

        var (_, errors) = RunLint(reqFile);

        // Error should include the file path
        Assert.Contains(reqFile, errors);
        // Error should include line and column in (line,col) format
        Assert.Contains("(1,", errors);
        // Error should include 'error:' severity
        Assert.Contains("error:", errors);
    }

    /// <summary>
    /// Test that a requirement with a blank id reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithBlankRequirementId_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "blank-req-id.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: ''
        title: Test requirement
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Requirement 'id' cannot be blank", errors);
    }

    /// <summary>
    /// Test that a requirement with a blank title reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithBlankRequirementTitle_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "blank-req-title.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: ''
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Requirement 'title' cannot be blank", errors);
    }

    /// <summary>
    /// Test that a mapping with a blank id reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithBlankMappingId_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "blank-mapping-id.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
mappings:
  - id: ''
    tests:
      - SomeTest
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Mapping 'id' cannot be blank", errors);
    }

    /// <summary>
    /// Test that a blank test name in a requirement reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithBlankTestName_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "blank-test-name.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tests:
          - ''
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Test name cannot be blank", errors);
    }

    /// <summary>
    /// Test that a blank tag name in a requirement reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithBlankTagName_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "blank-tag-name.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tags:
          - ''
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Tag name cannot be blank", errors);
    }

    /// <summary>
    /// Test that a mapping with a blank test name reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithBlankMappingTestName_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "blank-mapping-test-name.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
mappings:
  - id: REQ-001
    tests:
      - ''
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Test name cannot be blank in mapping", errors);
    }

    /// <summary>
    /// Test that a requirements file with a non-mapping root (e.g. a top-level sequence) reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithNonMappingRoot_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "non-mapping-root.yaml");
        File.WriteAllText(reqFile, @"- item1
- item2
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Document root must be a mapping", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the tests list of a requirement reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithNonScalarTestEntry_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "non-scalar-test.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tests:
          - key: value
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Test entry must be a scalar value", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the children list of a requirement reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithNonScalarChildEntry_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "non-scalar-child.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        children:
          - key: value
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Child requirement reference must be a scalar string", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the tags list of a requirement reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithNonScalarTagEntry_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "non-scalar-tag.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tags:
          - key: value
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Tag entry must be a scalar value", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the tests list of a mapping reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithNonScalarMappingTestEntry_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "non-scalar-mapping-test.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
mappings:
  - id: REQ-001
    tests:
      - key: value
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Test entry must be a scalar value in mapping", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the includes list reports an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithNonScalarIncludeEntry_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "non-scalar-include.yaml");
        File.WriteAllText(reqFile, @"includes:
  - key: value
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.Contains("Each 'includes' entry must be a scalar string", errors);
    }

    /// <summary>
    /// Test that multiple cycles in the requirement children graph are all reported.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithMultipleCycles_ReportsAllCycles()
    {
        var reqFile = Path.Combine(_testDirectory, "multiple-cycles.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-A
        title: Requirement A
        children:
          - REQ-B
          - REQ-C
      - id: REQ-B
        title: Requirement B
        children:
          - REQ-A
      - id: REQ-C
        title: Requirement C
        children:
          - REQ-A
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);

        // Both back-edges (REQ-B->REQ-A and REQ-C->REQ-A) should each be reported exactly once
        var cycleCount = errors.Split(Environment.NewLine)
            .Count(line => line.Contains("Circular requirement reference detected"));
        Assert.AreEqual(2, cycleCount, $"Expected exactly 2 cycle errors, got {cycleCount}: {errors}");
    }

    /// <summary>
    /// Test that a child reference to a non-existent requirement ID is reported as an error.
    /// </summary>
    [TestMethod]
    public void RequirementsLoader_Load_WithUnknownChildReference_ReportsError()
    {
        var reqFile = Path.Combine(_testDirectory, "unknown-child.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: PARENT
        title: Parent Requirement
        children:
          - NONEXISTENT
");

        var (exitCode, errors) = RunLint(reqFile);

        Assert.AreEqual(1, exitCode);
        Assert.IsTrue(errors.Contains("PARENT"), $"Expected 'PARENT' in errors: {errors}");
        Assert.IsTrue(errors.Contains("NONEXISTENT"), $"Expected 'NONEXISTENT' in errors: {errors}");
        Assert.IsTrue(errors.Contains("unknown child"), $"Expected 'unknown child' in errors: {errors}");
    }
}
