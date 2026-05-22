### Requirements Unit Verification

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

##### YAML Processing Scenario

Test methods:

- `Requirements_Load_ComplexStructure_ParsesCorrectly` — complex structure parsed

Note: Section-level tests are covered in Section unit verification.

##### Validation Scenario

Test methods:

- `Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation` — YAML error → error

Note: Section-level tests are covered in Section unit verification.

##### Hierarchy Scenario

Test methods:

- `Requirements_Export_NestedSections_CreatesHierarchy` — nested sections exported

Note: Section-level tests are covered in Section unit verification.

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

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Requirements-YamlProcessing` | YAML Processing Scenario | `Requirements_Load_ComplexStructure_ParsesCorrectly` |
| `ReqStream-Requirements-Validation` | Validation Scenario | `Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-YamlErrorReporting` | Validation Scenario | `Requirements_Load_InvalidYamlContent_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Hierarchy` | Hierarchy Scenario | `Requirements_Export_NestedSections_CreatesHierarchy` |
| `ReqStream-Requirements-Includes` | Includes Scenario | `Requirements_Load_WithIncludes_MergesFilesCorrectly`, `Requirements_Load_MultipleFiles_MergesAllFiles`, `Requirements_Load_IncludeLoop_DoesNotCauseInfiniteLoop` |
| `ReqStream-Requirements-CircularInclude` | Includes Scenario | `Requirements_Load_IncludeLoop_DoesNotCauseInfiniteLoop` |
| `ReqStream-Requirements-SectionMerging` | Section Merging Scenario | `Requirements_Load_IdenticalSections_MergesCorrectly`, `Requirements_Load_MultipleFilesWithSameSections_MergesSections` |
| `ReqStream-Report-MarkdownExport` | Export Scenario | `Requirements_Export_SimpleRequirements_CreatesMarkdownFile`, `Requirements_Export_MultipleSections_ExportsAll`, `Requirements_Export_EmptyRequirements_CreatesEmptyFile` |
| `ReqStream-Report-HeaderDepth` | Export Scenario | `Requirements_Export_WithCustomDepth_UsesCorrectHeaderLevel` |
| `ReqStream-Report-Justifications` | Export Scenario | `Requirements_ExportJustifications_WithJustifications_CreatesMarkdownFile`, `Requirements_ExportJustifications_WithoutJustifications_CreatesHeadersOnly`, `Requirements_ExportJustifications_NestedSections_CreatesHierarchy` |
| `ReqStream-Report-JustificationsDepth` | Export Scenario | `Requirements_ExportJustifications_WithCustomDepth_UsesCorrectHeaderLevel` |
| `ReqStream-Report-TagFilterExport` | Export Scenario | `Requirements_Export_WithFilterTags_ExportsOnlyMatchingRequirements`, `Requirements_Export_WithMultipleFilterTags_ExportsRequirementsMatchingAnyTag`, `Requirements_ExportJustifications_WithFilterTags_ExportsOnlyMatchingRequirements` |
