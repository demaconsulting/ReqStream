### Section

#### Verification Approach

The Section unit is verified using xUnit integration tests in `SectionTests.cs`. Tests create
YAML requirements files and invoke `Requirements.Load`, then assert on the parsed `Section`
data structure — title, requirements list, and child sections. Export tests in
`RequirementsExportTests.cs` additionally contribute evidence for `ReqStream-Section-Nesting`
by exercising the recursive traversal of child sections during Markdown generation.

#### Test Environment

The Section unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary YAML requirements files are created on disk and deleted on test completion.

#### Acceptance Criteria

The Section unit verification is complete when all xUnit tests in `SectionTests.cs`,
`RequirementsExportTests.cs`, and `RequirementsLoadParsingTests.cs` pass without
uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**Section Container**: Tests verify that a section holds a title, requirements, and child sections
correctly, and that a blank section title is reported as an error with file location. This
scenario is tested by `Section_Load_SimpleRequirement_ParsesCorrectly`,
`Section_Load_NestedSections_ParsesHierarchyCorrectly`, and
`Section_Load_BlankSectionTitle_ReportsErrorWithFileLocation`.

**Section Nesting**: Tests verify that a section correctly parses and holds a hierarchy of child
sections. This scenario is tested by `Section_Load_NestedSections_ParsesHierarchyCorrectly`,
which loads a two-level section hierarchy and asserts that both child section titles and their
requirements are accessible via the `Sections` list.

**Section Title Merging**: Tests verify that when multiple YAML files contribute to the same
hierarchical path, sections with the same title are merged into a single section rather than
duplicated. This scenario is tested by `Requirements_Load_IdenticalSections_MergesCorrectly`,
which loads two files containing identically titled sections and confirms that only one merged
section exists with requirements from both files.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Section-Title` | Section Container Scenario | `Section_Load_SimpleRequirement_ParsesCorrectly` |
| `ReqStream-Section-Title` | Section Nesting | `Section_Load_NestedSections_ParsesHierarchyCorrectly` |
| `ReqStream-Section-RequirementsList` | Section Container Scenario | `Section_Load_SimpleRequirement_ParsesCorrectly` |
| `ReqStream-Section-ChildSections` | Section Nesting | `Section_Load_NestedSections_ParsesHierarchyCorrectly` |
| `ReqStream-Section-Nesting` | Section Nesting | `Section_Load_NestedSections_ParsesHierarchyCorrectly` |
| `ReqStream-Section-Nesting` | Section Nesting | `Requirements_Export_NestedSections_CreatesHierarchy` |
| `ReqStream-Section-TitleMerging` | Section Title Merging | `Requirements_Load_IdenticalSections_MergesCorrectly` |
