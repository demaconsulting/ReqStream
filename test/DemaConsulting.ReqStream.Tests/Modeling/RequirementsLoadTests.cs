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
    /// <summary>Temporary directory providing isolated file-system workspace for this test class instance.</summary>
    private readonly TemporaryDirectory _testDirectory = new();

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public RequirementsLoadTests()
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
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
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
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
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
        var filePath = _testDirectory.GetFilePath("malformed.yaml");
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
        var filePath = _testDirectory.GetFilePath("location-test.yaml");
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
        var filePath = _testDirectory.GetFilePath("multiple-issues.yaml");
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

        // Act: load the root file
        var result = Requirements.Load(rootFile);

        // Assert: error from included file is reported
        Assert.Null(result.Requirements);
        Assert.Contains(result.Issues, i => i.Severity == LintSeverity.Error);
        Assert.Contains(result.Issues, i => i.Description.Contains("Unknown field 'unknown_field'"));
    }

    /// <summary>
    ///     Test that FindOrphans returns no orphans when the root-tag set is empty, regardless
    ///     of tree shape (the backward-compatibility no-op path).
    /// </summary>
    [Fact]
    public void Requirements_FindOrphans_EmptyRootTags_ReturnsNoOrphans()
    {
        // Arrange: a fully isolated requirement with no tags, no parents, no children
        var yamlContent = @"---
sections:
  - title: ""Section""
    requirements:
      - id: ""ISOLATED-001""
        title: ""Isolated requirement.""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);
        var requirements = Requirements.Load(filePath).Requirements!;

        // Act: find orphans with an empty root-tag set
        var result = requirements.FindOrphans(new HashSet<string>());

        // Assert: no orphans are ever reported when root tags are not configured
        Assert.Empty(result.OrphanIds);
        Assert.Equal(0, result.TotalRequirements);
    }

    /// <summary>
    ///     Test that a root-tagged requirement with no children is never reported as orphaned.
    /// </summary>
    [Fact]
    public void Requirements_FindOrphans_RequirementTaggedRoot_IsNeverOrphaned()
    {
        // Arrange: a single requirement tagged as a root, with no children
        var yamlContent = @"---
sections:
  - title: ""Section""
    requirements:
      - id: ""ROOT-001""
        title: ""Root requirement.""
        tags: [""product""]
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);
        var requirements = Requirements.Load(filePath).Requirements!;

        // Act: find orphans using the "product" root tag
        var result = requirements.FindOrphans(new HashSet<string> { "product" });

        // Assert: the root-tagged requirement is exempt from being orphaned
        Assert.Empty(result.OrphanIds);
        Assert.Equal(1, result.TotalRequirements);
    }

    /// <summary>
    ///     Test that a child reachable from a root via a single parent link is not orphaned.
    /// </summary>
    [Fact]
    public void Requirements_FindOrphans_ChildReachableFromRoot_IsNotOrphaned()
    {
        // Arrange: a root-tagged requirement with one child
        var yamlContent = @"---
sections:
  - title: ""Section""
    requirements:
      - id: ""ROOT-001""
        title: ""Root requirement.""
        tags: [""product""]
        children: [""CHILD-001""]
      - id: ""CHILD-001""
        title: ""Child requirement.""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);
        var requirements = Requirements.Load(filePath).Requirements!;

        // Act: find orphans using the "product" root tag
        var result = requirements.FindOrphans(new HashSet<string> { "product" });

        // Assert: the child requirement is reachable from the root and not orphaned
        Assert.Empty(result.OrphanIds);
        Assert.Equal(2, result.TotalRequirements);
    }

    /// <summary>
    ///     Test that a requirement referenced as a children entry from two different
    ///     root-reachable parents (a DAG diamond) is visited exactly once - no double
    ///     traversal, no infinite loop.
    /// </summary>
    [Fact]
    public void Requirements_FindOrphans_DiamondMultiParentChild_VisitedOnce_NotOrphaned()
    {
        // Arrange: two root-tagged requirements both referencing the same child
        var yamlContent = @"---
sections:
  - title: ""Section""
    requirements:
      - id: ""ROOT-001""
        title: ""First root.""
        tags: [""product""]
        children: [""SHARED-001""]
      - id: ""ROOT-002""
        title: ""Second root.""
        tags: [""product""]
        children: [""SHARED-001""]
      - id: ""SHARED-001""
        title: ""Shared child requirement.""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);
        var requirements = Requirements.Load(filePath).Requirements!;

        // Act: find orphans using the "product" root tag
        var result = requirements.FindOrphans(new HashSet<string> { "product" });

        // Assert: the shared child is reachable and not double-counted; no orphans
        Assert.Empty(result.OrphanIds);
        Assert.Equal(3, result.TotalRequirements);
    }

    /// <summary>
    ///     Test that a requirement with no tags, no parent references, and no children is
    ///     reported as orphaned when root tags are configured.
    /// </summary>
    [Fact]
    public void Requirements_FindOrphans_IsolatedRequirement_NoTagsNoParentNoChildren_IsOrphaned()
    {
        // Arrange: a root-tagged requirement and a fully-isolated requirement
        var yamlContent = @"---
sections:
  - title: ""Section""
    requirements:
      - id: ""ROOT-001""
        title: ""Root requirement.""
        tags: [""product""]
      - id: ""ISOLATED-001""
        title: ""Isolated requirement.""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);
        var requirements = Requirements.Load(filePath).Requirements!;

        // Act: find orphans using the "product" root tag
        var result = requirements.FindOrphans(new HashSet<string> { "product" });

        // Assert: only the isolated requirement is reported as orphaned
        Assert.Equal(2, result.TotalRequirements);
        Assert.Single(result.OrphanIds);
        Assert.Equal("ISOLATED-001", result.OrphanIds[0]);
    }

    /// <summary>
    ///     Test that every member of a subtree rooted away from any root-tagged requirement is
    ///     reported as orphaned, not just the top of the unreachable subtree.
    /// </summary>
    [Fact]
    public void Requirements_FindOrphans_UnreachableSubtree_AllMembersOrphaned()
    {
        // Arrange: a root-tagged requirement, plus an unreachable parent->child chain
        var yamlContent = @"---
sections:
  - title: ""Section""
    requirements:
      - id: ""ROOT-001""
        title: ""Root requirement.""
        tags: [""product""]
      - id: ""UNREACHED-PARENT""
        title: ""Unreached parent requirement.""
        children: [""UNREACHED-CHILD""]
      - id: ""UNREACHED-CHILD""
        title: ""Unreached child requirement.""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);
        var requirements = Requirements.Load(filePath).Requirements!;

        // Act: find orphans using the "product" root tag
        var result = requirements.FindOrphans(new HashSet<string> { "product" });

        // Assert: both members of the unreachable subtree are reported as orphaned
        Assert.Equal(3, result.TotalRequirements);
        Assert.Equal(2, result.OrphanIds.Count);
        Assert.Contains("UNREACHED-PARENT", result.OrphanIds);
        Assert.Contains("UNREACHED-CHILD", result.OrphanIds);
    }

    /// <summary>
    ///     Test that orphan ids are returned in tree declaration order.
    /// </summary>
    [Fact]
    public void Requirements_FindOrphans_ResultOrder_MatchesDeclarationOrder()
    {
        // Arrange: two orphaned requirements declared in a specific order
        var yamlContent = @"---
sections:
  - title: ""Section""
    requirements:
      - id: ""ROOT-001""
        title: ""Root requirement.""
        tags: [""product""]
      - id: ""ORPHAN-B""
        title: ""Second declared orphan.""
      - id: ""ORPHAN-A""
        title: ""First declared orphan.""
";
        var filePath = _testDirectory.GetFilePath("requirements.yaml");
        File.WriteAllText(filePath, yamlContent);
        var requirements = Requirements.Load(filePath).Requirements!;

        // Act: find orphans using the "product" root tag
        var result = requirements.FindOrphans(new HashSet<string> { "product" });

        // Assert: orphan ids appear in the same order as declared in the YAML
        Assert.Equal(["ORPHAN-B", "ORPHAN-A"], result.OrphanIds);
    }
}
