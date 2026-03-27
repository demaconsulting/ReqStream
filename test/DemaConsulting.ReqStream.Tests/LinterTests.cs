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
/// Unit tests for the Linter class.
/// </summary>
[TestClass]
public class LinterTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_lint_test_{Guid.NewGuid()}");
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
    /// Helper to run linter and capture error output.
    /// </summary>
    private static (int exitCode, string errors) RunLint(params string[] files)
    {
        var originalError = Console.Error;
        using var errorOutput = new StringWriter();
        Console.SetError(errorOutput);

        try
        {
            using var context = Context.Create([]);
            Linter.Lint(context, files.ToList());
            return (context.ExitCode, errorOutput.ToString());
        }
        finally
        {
            Console.SetError(originalError);
        }
    }

    /// <summary>
    /// Helper to run linter and capture all output.
    /// </summary>
    private static (int exitCode, string output, string errors) RunLintWithOutput(params string[] files)
    {
        var originalOut = Console.Out;
        var originalError = Console.Error;
        using var stdOutput = new StringWriter();
        using var errorOutput = new StringWriter();
        Console.SetOut(stdOutput);
        Console.SetError(errorOutput);

        try
        {
            using var context = Context.Create([]);
            Linter.Lint(context, files.ToList());
            return (context.ExitCode, stdOutput.ToString(), errorOutput.ToString());
        }
        finally
        {
            Console.SetOut(originalOut);
            Console.SetError(originalError);
        }
    }

    /// <summary>
    /// Test that linting with no files prints appropriate message.
    /// </summary>
    [TestMethod]
    public void Linter_Lint_WithNoFiles_PrintsMessage()
    {
        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create([]);
            Linter.Lint(context, []);

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
    public void Linter_Lint_WithValidFile_ReportsNoIssues()
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
    public void Linter_Lint_WithMissingFile_ReportsError()
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
    public void Linter_Lint_WithMalformedYaml_ReportsError()
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
    public void Linter_Lint_WithEmptyFile_ReportsNoIssues()
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
    public void Linter_Lint_WithUnknownDocumentField_ReportsError()
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
    public void Linter_Lint_WithSectionMissingTitle_ReportsError()
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
    public void Linter_Lint_WithBlankSectionTitle_ReportsError()
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
    public void Linter_Lint_WithUnknownSectionField_ReportsError()
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
    public void Linter_Lint_WithRequirementMissingId_ReportsError()
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
    public void Linter_Lint_WithRequirementMissingTitle_ReportsError()
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
    public void Linter_Lint_WithUnknownRequirementField_ReportsError()
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
    public void Linter_Lint_WithDuplicateIds_ReportsError()
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
    public void Linter_Lint_WithDuplicateIdsAcrossFiles_ReportsError()
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
    public void Linter_Lint_WithMultipleIssues_ReportsAllIssues()
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
    /// Test that linting follows includes.
    /// </summary>
    [TestMethod]
    public void Linter_Lint_WithIncludes_LintsIncludedFiles()
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
    /// Test that --lint via Program.Run works correctly.
    /// </summary>
    [TestMethod]
    public void Linter_ProgramRun_WithLintFlag_RunsLinter()
    {
        var reqFile = Path.Combine(_testDirectory, "valid.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
");

        var originalOut = Console.Out;
        using var output = new StringWriter();
        Console.SetOut(output);

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create(["--lint", "--requirements", "*.yaml"]);
            Program.Run(context);

            Assert.AreEqual(0, context.ExitCode);
            Assert.Contains("No issues found", output.ToString());
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test that a mapping with an unknown field reports an error.
    /// </summary>
    [TestMethod]
    public void Linter_Lint_WithUnknownMappingField_ReportsError()
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
    public void Linter_Lint_WithMappingMissingId_ReportsError()
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
    public void Linter_Lint_WithNestedSectionIssues_ReportsError()
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
    public void Linter_Lint_ErrorFormat_IncludesFileAndLocation()
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
    public void Linter_Lint_WithBlankRequirementId_ReportsError()
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
    public void Linter_Lint_WithBlankRequirementTitle_ReportsError()
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
    public void Linter_Lint_WithBlankMappingId_ReportsError()
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
    public void Linter_Lint_WithBlankTestName_ReportsError()
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
    public void Linter_Lint_WithBlankTagName_ReportsError()
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
}
