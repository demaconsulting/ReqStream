### Requirements

#### Verification Approach

The Requirements unit is verified using xUnit integration tests across multiple test files:
`RequirementsLoadTests.cs`, `RequirementsLoadParsingTests.cs`, and `RequirementsExportTests.cs`.
Tests create YAML requirements files with various structures, invoke `Requirements.Load`, and
assert on the parsed data model, lint issues, and generated Markdown exports.

#### Test Environment

The Requirements unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary YAML requirements files and Markdown export files are created on disk and deleted on test
completion.

#### Acceptance Criteria

The Requirements unit verification is complete when all xUnit tests across `RequirementsLoadTests.cs`,
`RequirementsLoadParsingTests.cs`, and `RequirementsExportTests.cs` pass without uncaught exceptions
and all assertions succeed. The unit is considered verified when every requirement in the Requirements
Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**YAML Processing**: Tests verify that complex YAML structures are parsed correctly. Note:
section-level tests are covered in Section unit verification. This scenario is tested by
`Requirements_Load_ComplexStructure_ParsesCorrectly`.

**Validation**: Tests verify that invalid YAML content is reported as an error with file location.
Note: section-level tests are covered in Section unit verification. This scenario is tested by
`Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation`.

**Hierarchy**: Tests verify that nested sections are correctly exported. Note: section-level tests
are covered in Section unit verification. This scenario is tested by
`Requirements_Export_NestedSections_CreatesHierarchy`.

**Includes**: Tests verify that included files are merged correctly, that multiple files are all
merged, and that include loops are handled without infinite recursion. This scenario is tested by
`Requirements_Load_WithIncludes_MergesFilesCorrectly`,
`Requirements_Load_MultipleFiles_MergesAllFiles`, and
`Requirements_Load_IncludeLoop_DoesNotCauseInfiniteLoop`.

**Section Merging**: Tests verify that sections with identical titles from the same or different
files are merged correctly. This scenario is tested by
`Requirements_Load_IdenticalSections_MergesCorrectly` and
`Requirements_Load_MultipleFilesWithSameSections_MergesSections`.

**Export**: Tests verify that requirements and justifications are exported to Markdown files with
correct heading levels, tag filtering, and content. This scenario is tested by
`Requirements_Export_SimpleRequirements_CreatesMarkdownFile`,
`Requirements_Export_MultipleSections_ExportsAll`,
`Requirements_Export_EmptyRequirements_CreatesEmptyFile`,
`Requirements_Export_WithCustomDepth_UsesCorrectHeaderLevel`,
`Requirements_Export_WithFilterTags_ExportsOnlyMatchingRequirements`,
`Requirements_Export_WithMultipleFilterTags_ExportsRequirementsMatchingAnyTag`,
`Requirements_ExportJustifications_WithJustifications_CreatesMarkdownFile`,
`Requirements_ExportJustifications_WithoutJustifications_CreatesHeadersOnly`,
`Requirements_ExportJustifications_NestedSections_CreatesHierarchy`,
`Requirements_ExportJustifications_WithCustomDepth_UsesCorrectHeaderLevel`, and
`Requirements_ExportJustifications_WithFilterTags_ExportsOnlyMatchingRequirements`.

**Load Result**: Tests verify the full `Requirements.Load` result for valid files, lint errors,
missing files, malformed YAML, multiple lint errors, included file linting, and issue location
reporting. This scenario is tested by
`Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues`,
`Requirements_Load_WithLintError_ReturnsNullAndIssues`,
`Requirements_Load_MissingFile_ReturnsNullAndIssues`,
`Requirements_Load_MalformedYaml_ReturnsNullAndIssues`,
`Requirements_Load_WithMultipleLintErrors_ReportsAllIssues`,
`Requirements_Load_WithIncludes_LintsIncludedFiles`, and
`Requirements_Load_WithLintError_IssueIncludesLocation`.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Requirements-YamlProcessing` | YAML Processing Scenario | `Requirements_Load_ComplexStructure_ParsesCorrectly` |
| `ReqStream-Requirements-Validation` | Validation Scenario | `Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-YamlErrorReporting` | Validation Scenario | `Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Hierarchy` | Hierarchy Scenario | `Requirements_Export_NestedSections_CreatesHierarchy` |
| `ReqStream-Requirements-Includes` | Includes Scenario | `Requirements_Load_WithIncludes_MergesFilesCorrectly` |
| `ReqStream-Requirements-Includes` | Includes Scenario | `Requirements_Load_MultipleFiles_MergesAllFiles` |
| `ReqStream-Requirements-Includes` | Includes Scenario | `Requirements_Load_IncludeLoop_DoesNotCauseInfiniteLoop` |
| `ReqStream-Requirements-CircularInclude` | Includes Scenario | `Requirements_Load_IncludeLoop_DoesNotCauseInfiniteLoop` |
| `ReqStream-Requirements-SectionMerging` | Section Merging Scenario | `Requirements_Load_IdenticalSections_MergesCorrectly` |
| `ReqStream-Requirements-SectionMerging` | Section Merging Scenario | `Requirements_Load_MultipleFilesWithSameSections_MergesSections` |
| `ReqStream-Report-MarkdownExport` | Export Scenario | `Requirements_Export_SimpleRequirements_CreatesMarkdownFile` |
| `ReqStream-Report-MarkdownExport` | Export Scenario | `Requirements_Export_MultipleSections_ExportsAll` |
| `ReqStream-Report-MarkdownExport` | Export Scenario | `Requirements_Export_EmptyRequirements_CreatesEmptyFile` |
| `ReqStream-Report-HeaderDepth` | Export Scenario | `Requirements_Export_WithCustomDepth_UsesCorrectHeaderLevel` |
| `ReqStream-Report-Justifications` | Export Scenario | `Requirements_ExportJustifications_WithJustifications_CreatesMarkdownFile` |
| `ReqStream-Report-Justifications` | Export Scenario | `Requirements_ExportJustifications_WithoutJustifications_CreatesHeadersOnly` |
| `ReqStream-Report-Justifications` | Export Scenario | `Requirements_ExportJustifications_NestedSections_CreatesHierarchy` |
| `ReqStream-Report-JustificationsDepth` | Export Scenario | `Requirements_ExportJustifications_WithCustomDepth_UsesCorrectHeaderLevel` |
| `ReqStream-Report-TagFilterExport` | Export Scenario | `Requirements_Export_WithFilterTags_ExportsOnlyMatchingRequirements` |
| `ReqStream-Report-TagFilterExport` | Export Scenario | `Requirements_Export_WithMultipleFilterTags_ExportsRequirementsMatchingAnyTag` |
| `ReqStream-Report-TagFilterExport` | Export Scenario | `Requirements_ExportJustifications_WithFilterTags_ExportsOnlyMatchingRequirements` |
