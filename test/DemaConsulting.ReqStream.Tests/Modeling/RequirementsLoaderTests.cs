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
/// Unit tests for the RequirementsLoader: verifies that structural issues in requirements
/// YAML files are reported as lint issues when loading via Requirements.Load().
/// </summary>
public sealed class RequirementsLoaderTests : IDisposable
{
    /// <summary>Temporary directory providing isolated file-system workspace for this test class instance.</summary>
    private readonly TemporaryDirectory _testDirectory = new();

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public RequirementsLoaderTests()
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
    /// Test that a valid requirements file produces no issues.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithValidFile_ReportsNoIssues()
    {
        // Arrange: create a valid requirements YAML file
        var reqFile = _testDirectory.GetFilePath("valid.yaml");
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

        // Act: load the requirements file
        var (exitCode, output, errors) = RunLintWithOutput(reqFile);

        // Assert: exit code is 0 and no issues are reported
        Assert.Equal(0, exitCode);
        Assert.Contains("No issues found", output);
        Assert.Equal(string.Empty, errors);
    }

    /// <summary>
    /// Test that an invalid file path (containing null characters) reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithInvalidFilePath_ReportsError()
    {
        // Act: attempt to load a file with a null character in the path (invalid on all platforms)
        var (exitCode, errors) = RunLint("path\0with_null.yaml");

        // Assert: exit code is 1 and error mentions invalid file path
        Assert.Equal(1, exitCode);
        Assert.Contains("error", errors);
        Assert.Contains("Invalid file path", errors);
    }

    /// <summary>
    /// Test that a file that cannot be read due to an I/O failure reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithIoReadFailure_ReportsError()
    {
        // Skip this test on non-Unix platforms (file permission removal requires Unix)
        if (!OperatingSystem.IsLinux() && !OperatingSystem.IsMacOS())
        {
            Assert.Skip("This test requires Unix file permissions.");
            return;
        }

        // Skip if running as root (root can read any file regardless of permissions)
        if (Environment.IsPrivilegedProcess)
        {
            Assert.Skip("This test cannot run as root.");
            return;
        }

        // Arrange: create a YAML file and remove all permissions so it cannot be read
        var reqFile = _testDirectory.GetFilePath("unreadable.yaml");
        File.WriteAllText(reqFile, "sections: []");
        File.SetUnixFileMode(reqFile, UnixFileMode.None);
        try
        {
            // Act: load the file that exists but cannot be read
            var (exitCode, errors) = RunLint(reqFile);

            // Assert: exit code is 1 and error reports the read failure
            Assert.Equal(1, exitCode);
            Assert.Contains("error", errors);
            Assert.Contains("Failed to read file", errors);
        }
        finally
        {
            // Restore permissions so cleanup can delete the file
            File.SetUnixFileMode(reqFile, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    /// <summary>
    /// Test that a file that doesn't exist reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithMissingFile_ReportsError()
    {
        // Act: attempt to load a file that does not exist
        var (exitCode, errors) = RunLint("/nonexistent/path/missing.yaml");

        // Assert: exit code is 1 and error mentions the file not found
        Assert.Equal(1, exitCode);
        Assert.Contains("error", errors);
        Assert.Contains("File not found", errors);
    }

    /// <summary>
    /// Test that malformed YAML reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithMalformedYaml_ReportsError()
    {
        // Arrange: create a YAML file with invalid syntax
        var reqFile = _testDirectory.GetFilePath("malformed.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Bad
    requirements: [
  invalid yaml here
");

        // Act: load the malformed requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports malformed YAML
        Assert.Equal(1, exitCode);
        Assert.Contains("error", errors);
        Assert.Contains("Malformed YAML", errors);
    }

    /// <summary>
    /// Test that an empty YAML file produces no issues.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithEmptyFile_ReportsNoIssues()
    {
        // Arrange: create an empty YAML file
        var reqFile = _testDirectory.GetFilePath("empty.yaml");
        File.WriteAllText(reqFile, string.Empty);

        // Act: load the empty requirements file
        var (exitCode, output, errors) = RunLintWithOutput(reqFile);

        // Assert: exit code is 0 and no issues are reported
        Assert.Equal(0, exitCode);
        Assert.Contains("No issues found", output);
        Assert.Equal(string.Empty, errors);
    }

    /// <summary>
    /// Test that an unknown field at document root reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithUnknownDocumentField_ReportsError()
    {
        // Arrange: create a YAML file with an unknown field at document root
        var reqFile = _testDirectory.GetFilePath("unknown-field.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test
unknown_field: value
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error names the unknown field
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field'", errors);
    }

    /// <summary>
    /// Test that a section missing the title field reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithSectionMissingTitle_ReportsError()
    {
        // Arrange: create a YAML file with a section that has no title
        var reqFile = _testDirectory.GetFilePath("missing-title.yaml");
        File.WriteAllText(reqFile, @"sections:
  - requirements:
      - id: REQ-001
        title: A requirement
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the missing title field
        Assert.Equal(1, exitCode);
        Assert.Contains("Section missing required field 'title'", errors);
    }

    /// <summary>
    /// Test that a section with a blank title reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithBlankSectionTitle_ReportsError()
    {
        // Arrange: create a YAML file with a section whose title is blank
        var reqFile = _testDirectory.GetFilePath("blank-title.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: ''
    requirements:
      - id: REQ-001
        title: A requirement
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the blank title
        Assert.Equal(1, exitCode);
        Assert.Contains("Section 'title' cannot be blank", errors);
    }

    /// <summary>
    /// Test that a section with an unknown field reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithUnknownSectionField_ReportsError()
    {
        // Arrange: create a YAML file with an unknown field inside a section
        var reqFile = _testDirectory.GetFilePath("unknown-section-field.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test
    unknown_field: value
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error names the unknown section field
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field' in section", errors);
    }

    /// <summary>
    /// Test that a requirement missing the id field reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithRequirementMissingId_ReportsError()
    {
        // Arrange: create a YAML file with a requirement that has no id field
        var reqFile = _testDirectory.GetFilePath("missing-id.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - title: Requirement without ID
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the missing id field
        Assert.Equal(1, exitCode);
        Assert.Contains("Requirement missing required field 'id'", errors);
    }

    /// <summary>
    /// Test that a requirement missing the title field reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithRequirementMissingTitle_ReportsError()
    {
        // Arrange: create a YAML file with a requirement that has no title field
        var reqFile = _testDirectory.GetFilePath("missing-req-title.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the missing title field
        Assert.Equal(1, exitCode);
        Assert.Contains("missing required field 'title'", errors);
    }

    /// <summary>
    /// Test that a requirement with an unknown field reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithUnknownRequirementField_ReportsError()
    {
        // Arrange: create a YAML file with a requirement that has an unknown field
        var reqFile = _testDirectory.GetFilePath("unknown-req-field.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        unknown_field: value
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error names the unknown requirement field
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field' in requirement", errors);
    }

    /// <summary>
    /// Test that duplicate requirement IDs report an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithDuplicateIds_ReportsError()
    {
        // Arrange: create a YAML file with two requirements sharing the same ID
        var reqFile = _testDirectory.GetFilePath("duplicates.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: First requirement
      - id: REQ-001
        title: Duplicate requirement
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the duplicate ID
        Assert.Equal(1, exitCode);
        Assert.Contains("Duplicate requirement ID 'REQ-001'", errors);
    }

    /// <summary>
    /// Test that duplicate IDs across multiple files report an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithDuplicateIdsAcrossFiles_ReportsError()
    {
        // Arrange: create two YAML files that each define the same requirement ID
        var reqFile1 = _testDirectory.GetFilePath("file1.yaml");
        File.WriteAllText(reqFile1, @"sections:
  - title: Section 1
    requirements:
      - id: REQ-001
        title: First requirement
");

        var reqFile2 = _testDirectory.GetFilePath("file2.yaml");
        File.WriteAllText(reqFile2, @"sections:
  - title: Section 2
    requirements:
      - id: REQ-001
        title: Duplicate across files
");

        // Act: load both requirements files together
        var (exitCode, errors) = RunLint(reqFile1, reqFile2);

        // Assert: exit code is 1 and error reports the cross-file duplicate ID
        Assert.Equal(1, exitCode);
        Assert.Contains("Duplicate requirement ID 'REQ-001'", errors);
    }

    /// <summary>
    /// Test that multiple issues are all reported.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithMultipleIssues_ReportsAllIssues()
    {
        // Arrange: create a YAML file with multiple structural errors
        var reqFile = _testDirectory.GetFilePath("multiple-issues.yaml");
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

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and all four errors are reported
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown field 'unknown_section_field' in section", errors);
        Assert.Contains("Requirement missing required field 'id'", errors);
        Assert.Contains("Duplicate requirement ID 'REQ-001'", errors);
        Assert.Contains("Unknown field 'unknown_root_field'", errors);
    }

    /// <summary>
    /// Test that loading follows includes and lints included files.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithIncludes_LintsIncludedFiles()
    {
        // Arrange: create an included YAML file with an unknown field and a root file that includes it
        var includedFile = _testDirectory.GetFilePath("included.yaml");
        File.WriteAllText(includedFile, @"sections:
  - title: Included Section
    requirements:
      - id: INC-001
        title: Included requirement
        unknown_field: bad
");

        var rootFile = _testDirectory.GetFilePath("root.yaml");
        File.WriteAllText(rootFile, $@"includes:
  - included.yaml
sections:
  - title: Root Section
    requirements:
      - id: ROOT-001
        title: Root requirement
");

        // Act: load the root requirements file (which includes the other)
        var (exitCode, errors) = RunLint(rootFile);

        // Assert: exit code is 1 and error from the included file is reported
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field' in requirement", errors);
    }

    /// <summary>
    /// Test that a mapping declared in a parent file targeting a requirement defined in a
    /// file reached via that parent's includes resolves correctly, regardless of the order
    /// the files are processed in.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithMappingToIncludedFile_ResolvesMapping()
    {
        // Arrange: create an included file defining the target requirement, and a root file
        // that includes it and declares a mapping targeting that requirement
        var includedFile = _testDirectory.GetFilePath("included.yaml");
        File.WriteAllText(includedFile, @"sections:
  - title: Included Section
    requirements:
      - id: INC-001
        title: Included requirement
");

        var rootFile = _testDirectory.GetFilePath("root.yaml");
        File.WriteAllText(rootFile, @"includes:
  - included.yaml
sections:
  - title: Root Section
    requirements:
      - id: ROOT-001
        title: Root requirement
mappings:
  - id: INC-001
    tests:
      - Included_Test
");

        // Act: load the root requirements file (which includes the other)
        var result = Requirements.Load(rootFile);

        // Assert: no errors, and the mapping's test resolved against the included requirement
        Assert.False(result.HasErrors);
        var requirements = result.Requirements;
        Assert.NotNull(requirements);

        var includedSection = requirements.Sections.Single(s => s.Title == "Included Section");
        var includedReq = includedSection.Requirements.Single(r => r.Id == "INC-001");
        Assert.Contains("Included_Test", includedReq.Tests);
    }

    /// <summary>
    /// Test that a mapping referencing an id that does not exist anywhere in the full
    /// requirements tree reports an error instead of being silently dropped.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithUnresolvableMappingId_ReportsError()
    {
        // Arrange: create a YAML file with a mapping block that targets a nonexistent id
        var reqFile = _testDirectory.GetFilePath("unresolvable-mapping-id.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
mappings:
  - id: DOES-NOT-EXIST
    tests:
      - SomeTest
");

        // Act: load the requirements file
        var result = Requirements.Load(reqFile);

        // Assert: exit code is 1 and error names the unresolved mapping id
        Assert.True(result.HasErrors);
        Assert.Null(result.Requirements);
        Assert.Contains(result.Issues, i => i.Severity == LintSeverity.Error);
        Assert.Contains(result.Issues,
            i => i.Description.Contains("Mapping references unknown requirement id 'DOES-NOT-EXIST'"));
    }

    /// <summary>
    /// Test that a mapping with an unknown field reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithUnknownMappingField_ReportsError()
    {
        // Arrange: create a YAML file with an unknown field inside a mapping block
        var reqFile = _testDirectory.GetFilePath("unknown-mapping-field.yaml");
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

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error names the unknown mapping field
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown field 'unknown_field' in mapping", errors);
    }

    /// <summary>
    /// Test that a mapping missing id reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithMappingMissingId_ReportsError()
    {
        // Arrange: create a YAML file with a mapping block that has no id field
        var reqFile = _testDirectory.GetFilePath("mapping-missing-id.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
mappings:
  - tests:
      - SomeTest
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the missing mapping id field
        Assert.Equal(1, exitCode);
        Assert.Contains("Mapping missing required field 'id'", errors);
    }

    /// <summary>
    /// Test that a nested section with issues is linted.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithNestedSectionIssues_ReportsError()
    {
        // Arrange: create a YAML file with an issue inside a nested child section
        var reqFile = _testDirectory.GetFilePath("nested.yaml");
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

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error names the unknown nested requirement field
        Assert.Equal(1, exitCode);
        Assert.Contains("Unknown field 'unknown_req_field' in requirement", errors);
    }

    /// <summary>
    /// Test that error format includes file path and line/column info.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_ErrorFormat_IncludesFileAndLocation()
    {
        // Arrange: create a YAML file with a single unknown root field
        var reqFile = _testDirectory.GetFilePath("format-test.yaml");
        File.WriteAllText(reqFile, @"unknown_field: value
");

        // Act: load the requirements file
        var (_, errors) = RunLint(reqFile);

        // Assert: error message includes file path, line/column, and severity
        Assert.Contains(reqFile, errors);
        Assert.Contains("(1,", errors);
        Assert.Contains("error:", errors);
    }

    /// <summary>
    /// Test that a requirement with a blank id reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithBlankRequirementId_ReportsError()
    {
        // Arrange: create a YAML file with a requirement whose id is blank
        var reqFile = _testDirectory.GetFilePath("blank-req-id.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: ''
        title: Test requirement
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the blank id
        Assert.Equal(1, exitCode);
        Assert.Contains("Requirement 'id' cannot be blank", errors);
    }

    /// <summary>
    /// Test that a requirement with a blank title reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithBlankRequirementTitle_ReportsError()
    {
        // Arrange: create a YAML file with a requirement whose title is blank
        var reqFile = _testDirectory.GetFilePath("blank-req-title.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: ''
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the blank title
        Assert.Equal(1, exitCode);
        Assert.Contains("Requirement 'title' cannot be blank", errors);
    }

    /// <summary>
    /// Test that a mapping with a blank id reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithBlankMappingId_ReportsError()
    {
        // Arrange: create a YAML file with a mapping block whose id is blank
        var reqFile = _testDirectory.GetFilePath("blank-mapping-id.yaml");
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

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the blank mapping id
        Assert.Equal(1, exitCode);
        Assert.Contains("Mapping 'id' cannot be blank", errors);
    }

    /// <summary>
    /// Test that a blank test name in a requirement reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithBlankTestName_ReportsError()
    {
        // Arrange: create a YAML file with a requirement that has a blank test name
        var reqFile = _testDirectory.GetFilePath("blank-test-name.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tests:
          - ''
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the blank test name
        Assert.Equal(1, exitCode);
        Assert.Contains("Test name cannot be blank", errors);
    }

    /// <summary>
    /// Test that a blank tag name in a requirement reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithBlankTagName_ReportsError()
    {
        // Arrange: create a YAML file with a requirement that has a blank tag name
        var reqFile = _testDirectory.GetFilePath("blank-tag-name.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tags:
          - ''
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the blank tag name
        Assert.Equal(1, exitCode);
        Assert.Contains("Tag name cannot be blank", errors);
    }

    /// <summary>
    /// Test that a mapping with a blank test name reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithBlankMappingTestName_ReportsError()
    {
        // Arrange: create a YAML file with a mapping block that has a blank test name
        var reqFile = _testDirectory.GetFilePath("blank-mapping-test-name.yaml");
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

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the blank mapping test name
        Assert.Equal(1, exitCode);
        Assert.Contains("Test name cannot be blank in mapping", errors);
    }

    /// <summary>
    /// Test that a requirements file with a non-mapping root (e.g. a top-level sequence) reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithNonMappingRoot_ReportsError()
    {
        // Arrange: create a YAML file whose root is a sequence rather than a mapping
        var reqFile = _testDirectory.GetFilePath("non-mapping-root.yaml");
        File.WriteAllText(reqFile, @"- item1
- item2
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the non-mapping root
        Assert.Equal(1, exitCode);
        Assert.Contains("Document root must be a mapping", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the tests list of a requirement reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithNonScalarTestEntry_ReportsError()
    {
        // Arrange: create a YAML file with a mapping node instead of a scalar in the tests list
        var reqFile = _testDirectory.GetFilePath("non-scalar-test.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tests:
          - key: value
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the non-scalar test entry
        Assert.Equal(1, exitCode);
        Assert.Contains("Test entry must be a scalar value", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the children list of a requirement reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithNonScalarChildEntry_ReportsError()
    {
        // Arrange: create a YAML file with a mapping node instead of a scalar in the children list
        var reqFile = _testDirectory.GetFilePath("non-scalar-child.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        children:
          - key: value
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the non-scalar child entry
        Assert.Equal(1, exitCode);
        Assert.Contains("Child requirement reference must be a scalar string", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the tags list of a requirement reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithNonScalarTagEntry_ReportsError()
    {
        // Arrange: create a YAML file with a mapping node instead of a scalar in the tags list
        var reqFile = _testDirectory.GetFilePath("non-scalar-tag.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test requirement
        tags:
          - key: value
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the non-scalar tag entry
        Assert.Equal(1, exitCode);
        Assert.Contains("Tag entry must be a scalar value", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the tests list of a mapping reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithNonScalarMappingTestEntry_ReportsError()
    {
        // Arrange: create a YAML file with a mapping node instead of a scalar in a mapping tests list
        var reqFile = _testDirectory.GetFilePath("non-scalar-mapping-test.yaml");
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

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the non-scalar mapping test entry
        Assert.Equal(1, exitCode);
        Assert.Contains("Test entry must be a scalar value in mapping", errors);
    }

    /// <summary>
    /// Test that a non-scalar entry in the includes list reports an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithNonScalarIncludeEntry_ReportsError()
    {
        // Arrange: create a YAML file with a mapping node instead of a scalar in the includes list
        var reqFile = _testDirectory.GetFilePath("non-scalar-include.yaml");
        File.WriteAllText(reqFile, @"includes:
  - key: value
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error reports the non-scalar include entry
        Assert.Equal(1, exitCode);
        Assert.Contains("Each 'includes' entry must be a scalar string", errors);
    }

    /// <summary>
    /// Test that multiple cycles in the requirement children graph are all reported.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithMultipleCycles_ReportsAllCycles()
    {
        // Arrange: create a YAML file with two separate back-edges creating two cycles
        var reqFile = _testDirectory.GetFilePath("multiple-cycles.yaml");
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

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and both cycles are individually reported
        Assert.Equal(1, exitCode);

        // Both back-edges (REQ-B->REQ-A and REQ-C->REQ-A) should each be reported exactly once
        var cycleCount = errors.Split(Environment.NewLine)
            .Count(line => line.Contains("Circular requirement reference detected"));
        Assert.Equal(2, cycleCount);
    }

    /// <summary>
    /// Test that a child reference to a non-existent requirement ID is reported as an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithUnknownChildReference_ReportsError()
    {
        // Arrange: create a YAML file with a requirement that references a non-existent child
        var reqFile = _testDirectory.GetFilePath("unknown-child.yaml");
        File.WriteAllText(reqFile, @"sections:
  - title: Test Section
    requirements:
      - id: PARENT
        title: Parent Requirement
        children:
          - NONEXISTENT
");

        // Act: load the requirements file
        var (exitCode, errors) = RunLint(reqFile);

        // Assert: exit code is 1 and error mentions both the parent and the missing child ID
        Assert.Equal(1, exitCode);
        Assert.Contains("PARENT", errors);
        Assert.Contains("NONEXISTENT", errors);
        Assert.Contains("unknown child", errors);
    }

    /// <summary>
    /// Test that a circular file include (file A includes file B which includes file A) is reported as an error.
    /// </summary>
    [Fact]
    public void RequirementsLoader_Load_WithCircularFileInclude_ReportsError()
    {
        // Arrange: create two files that include each other
        var fileA = _testDirectory.GetFilePath("file-a.yaml");
        var fileB = _testDirectory.GetFilePath("file-b.yaml");

        File.WriteAllText(fileA, @"includes:
  - file-b.yaml
sections:
  - title: Section A
    requirements:
      - id: REQ-A
        title: Requirement A
");

        File.WriteAllText(fileB, @"includes:
  - file-a.yaml
sections:
  - title: Section B
    requirements:
      - id: REQ-B
        title: Requirement B
");

        // Act: load the root file which will trigger the circular include
        var (exitCode, errors) = RunLint(fileA);

        // Assert: exit code is 1 and error mentions circular include
        Assert.Equal(1, exitCode);
        Assert.Contains("error", errors);
        Assert.Contains("Circular include", errors);
    }
}
