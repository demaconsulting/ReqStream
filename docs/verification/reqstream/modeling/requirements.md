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

**Root Tags Parsing**: Tests verify that `root-tags:` is parsed from a single file, combined
across multiple included files, is a no-op when absent, and is validated with the same
non-scalar/blank-entry checks applied to other list fields. This scenario is tested by
`Requirements_Load_WithRootTagsInSingleFile_PopulatesRootTags`,
`Requirements_Load_WithRootTagsAcrossIncludedFiles_UnionsAllValues`,
`Requirements_Load_WithNoRootTagsDeclared_RootTagsIsEmpty`,
`Requirements_Load_WithNonScalarRootTagEntry_ReportsError`,
`Requirements_Load_WithBlankRootTagEntry_ReportsError`, and
`Requirements_Load_WithRootTagsKey_DoesNotReportUnknownField`.

**Orphan Detection (`FindOrphans`)**: Tests verify the downward-reachability flood-fill
algorithm: an empty root-tag set is a no-op, a root-tagged requirement is never orphaned
regardless of children, a simple parent/child pair is fully reachable, a diamond-shaped
multi-parent DAG is visited exactly once per node with no infinite loop, a fully isolated
requirement is reported as orphaned, an entire unreachable subtree is reported as orphaned, and
orphan IDs are returned in declaration order. This scenario is tested by
`Requirements_FindOrphans_EmptyRootTags_ReturnsNoOrphans`,
`Requirements_FindOrphans_RequirementTaggedRoot_IsNeverOrphaned`,
`Requirements_FindOrphans_ChildReachableFromRoot_IsNotOrphaned`,
`Requirements_FindOrphans_DiamondMultiParentChild_VisitedOnce_NotOrphaned`,
`Requirements_FindOrphans_IsolatedRequirement_NoTagsNoParentNoChildren_IsOrphaned`,
`Requirements_FindOrphans_UnreachableSubtree_AllMembersOrphaned`, and
`Requirements_FindOrphans_ResultOrder_MatchesDeclarationOrder`.

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
| `ReqStream-Requirements-RootTags` | Root Tags Parsing Scenario | `Requirements_Load_WithRootTagsInSingleFile_PopulatesRootTags` |
| `ReqStream-Requirements-RootTags` | Root Tags Parsing Scenario | `Requirements_Load_WithRootTagsAcrossIncludedFiles_UnionsAllValues` |
| `ReqStream-Requirements-OrphanReachability` | Orphan Detection Scenario | `Requirements_FindOrphans_ChildReachableFromRoot_IsNotOrphaned` |
| `ReqStream-Requirements-OrphanReachability` | Orphan Detection Scenario | `Requirements_FindOrphans_DiamondMultiParentChild_VisitedOnce_NotOrphaned` |
| `ReqStream-Requirements-OrphanRootExemption` | Orphan Detection Scenario | `Requirements_FindOrphans_RequirementTaggedRoot_IsNeverOrphaned` |
| `ReqStream-Requirements-OrphanIsolated` | Orphan Detection Scenario | `Requirements_FindOrphans_IsolatedRequirement_NoTagsNoParentNoChildren_IsOrphaned` |
| `ReqStream-Requirements-OrphanIsolated` | Orphan Detection Scenario | `Requirements_FindOrphans_UnreachableSubtree_AllMembersOrphaned` |
| `ReqStream-Requirements-OrphanNoRootTags` | Orphan Detection Scenario | `Requirements_FindOrphans_EmptyRootTags_ReturnsNoOrphans` |
