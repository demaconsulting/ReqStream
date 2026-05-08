### Requirements Unit Verification

#### Verification Strategy

The Requirements unit is verified using xUnit integration tests across multiple test files:
`RequirementsLoadTests.cs`, `RequirementsLoadParsingTests.cs`, and `RequirementsExportTests.cs`.
Tests create YAML requirements files with various structures, invoke `Requirements.Load`, and
assert on the parsed data model, lint issues, and generated Markdown exports.

#### Test Scenarios

##### YAML Processing Scenario

Test methods:

- `Section_Load_SimpleRequirement_ParsesCorrectly` — single requirement parsed
- `Requirements_Load_ComplexStructure_ParsesCorrectly` — complex structure parsed

##### Validation Scenario

Test methods:

- `Section_Load_BlankSectionTitle_ReportsErrorWithFileLocation` — blank section title → error
- `Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation` — blank req ID → error
- `Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation` — blank req title → error
- `Requirements_Load_DuplicateRequirementId_ReportsError` — duplicate ID → error
- `Requirements_Load_DuplicateRequirementId_ErrorIncludesFileLocation` — error includes location
- `Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation` — YAML error → error

##### Hierarchy Scenario

Test methods:

- `Section_Load_NestedSections_ParsesHierarchyCorrectly` — nested sections parsed
- `Requirements_Export_NestedSections_CreatesHierarchy` — nested sections exported

##### Includes Scenario

Test methods:

- `Requirements_Load_WithIncludes_MergesFilesCorrectly` — includes merged
- `Requirements_Load_MultipleFiles_MergesAllFiles` — multiple files merged
- `Requirements_Load_IncludeLoop_DoesNotCauseInfiniteLoop` — include loops handled

##### Section Merging Scenario

Test methods:

- `Requirements_Load_IdenticalSections_MergesCorrectly` — same-title sections merged
- `Requirements_Load_MultipleFilesWithSameSections_MergesSections` — cross-file merging

##### Export Scenario

Test methods:

- `Requirements_Export_SimpleRequirements_CreatesMarkdownFile` — simple export
- `Requirements_Export_MultipleSections_ExportsAll` — all sections exported
- `Requirements_Export_EmptyRequirements_CreatesEmptyFile` — empty → empty file
- `Requirements_Export_WithCustomDepth_UsesCorrectHeaderLevel` — custom depth applied
- `Requirements_Export_WithFilterTags_ExportsOnlyMatchingRequirements` — tag filter applied
- `Requirements_Export_WithMultipleFilterTags_ExportsRequirementsMatchingAnyTag` — multiple tags
- `Requirements_ExportJustifications_WithJustifications_CreatesMarkdownFile` — justifications export
- `Requirements_ExportJustifications_WithoutJustifications_CreatesHeadersOnly` — no justifications
- `Requirements_ExportJustifications_NestedSections_CreatesHierarchy` — nested justifications
- `Requirements_ExportJustifications_WithCustomDepth_UsesCorrectHeaderLevel` — custom depth
- `Requirements_ExportJustifications_WithFilterTags_ExportsOnlyMatchingRequirements` — tag filter

##### Load Result Scenario

Test methods:

- `Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues` — valid → requirements and no issues
- `Requirements_Load_WithLintError_ReturnsNullAndIssues` — error → null and issues
- `Requirements_Load_MissingFile_ReturnsNullAndIssues` — missing → null and issues
- `Requirements_Load_MalformedYaml_ReturnsNullAndIssues` — malformed → null and issues
- `Requirements_Load_WithMultipleLintErrors_ReportsAllIssues` — multiple errors all reported
- `Requirements_Load_WithIncludes_LintsIncludedFiles` — included files linted
- `Requirements_Load_WithLintError_IssueIncludesLocation` — issue includes location

#### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Requirements-YamlProcessing` | `Section_Load_SimpleRequirement_ParsesCorrectly`, `Requirements_Load_ComplexStructure_ParsesCorrectly` |
| `ReqStream-Requirements-Validation` | `Section_Load_BlankSectionTitle_ReportsErrorWithFileLocation`, `Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation`, `Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation`, `Requirements_Load_DuplicateRequirementId_ReportsError`, `Requirements_Load_DuplicateRequirementId_ErrorIncludesFileLocation` |
| `ReqStream-Requirements-YamlErrorReporting` | `Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Hierarchy` | `Section_Load_NestedSections_ParsesHierarchyCorrectly`, `Requirements_Export_NestedSections_CreatesHierarchy` |
| `ReqStream-Requirements-Includes` | `Requirements_Load_WithIncludes_MergesFilesCorrectly`, `Requirements_Load_MultipleFiles_MergesAllFiles`, `Requirements_Load_IncludeLoop_DoesNotCauseInfiniteLoop` |
| `ReqStream-Requirements-SectionMerging` | `Requirements_Load_IdenticalSections_MergesCorrectly`, `Requirements_Load_MultipleFilesWithSameSections_MergesSections` |
| `ReqStream-Report-MarkdownExport` | `Requirements_Export_SimpleRequirements_CreatesMarkdownFile`, `Requirements_Export_MultipleSections_ExportsAll`, `Requirements_Export_EmptyRequirements_CreatesEmptyFile` |
| `ReqStream-Report-HeaderDepth` | `Requirements_Export_WithCustomDepth_UsesCorrectHeaderLevel` |
| `ReqStream-Report-Justifications` | `Requirements_ExportJustifications_WithJustifications_CreatesMarkdownFile`, `Requirements_ExportJustifications_WithoutJustifications_CreatesHeadersOnly`, `Requirements_ExportJustifications_NestedSections_CreatesHierarchy` |
| `ReqStream-Report-JustificationsDepth` | `Requirements_ExportJustifications_WithCustomDepth_UsesCorrectHeaderLevel` |
| `ReqStream-Report-TagFilterExport` | `Requirements_Export_WithFilterTags_ExportsOnlyMatchingRequirements`, `Requirements_Export_WithMultipleFilterTags_ExportsRequirementsMatchingAnyTag`, `Requirements_ExportJustifications_WithFilterTags_ExportsOnlyMatchingRequirements` |
| `ReqStream-Requirements-UnifiedLoad` | `Requirements_Load_ValidFile_ReturnsRequirementsAndNoIssues`, `Requirements_Load_WithLintError_ReturnsNullAndIssues`, `Requirements_Load_MissingFile_ReturnsNullAndIssues`, `Requirements_Load_MalformedYaml_ReturnsNullAndIssues`, `Requirements_Load_WithMultipleLintErrors_ReportsAllIssues`, `Requirements_Load_WithIncludes_LintsIncludedFiles`, `Requirements_Load_WithLintError_IssueIncludesLocation` |
