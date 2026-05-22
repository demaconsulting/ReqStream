### Section

#### Verification Approach

The Section unit is verified using xUnit integration tests in `SectionTests.cs`. Tests create
YAML requirements files and invoke `Requirements.Load`, then assert on the parsed `Section`
data structure — title, requirements list, and child sections.

#### Test Environment

The Section unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary YAML requirements files are created on disk and deleted on test completion.

#### Acceptance Criteria

The Section unit verification is complete when all xUnit tests in `SectionTests.cs` pass without
uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage table is mapped to at least one passing test method.

#### Test Scenarios

**Section Container**: Tests verify that a section holds a title, requirements, and child sections
correctly, and that a blank section title is reported as an error with file location. This
scenario is tested by `Section_Load_SimpleRequirement_ParsesCorrectly`,
`Section_Load_NestedSections_ParsesHierarchyCorrectly`, and
`Section_Load_BlankSectionTitle_ReportsErrorWithFileLocation`.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Section-Container` | Section Container Scenario | `Section_Load_SimpleRequirement_ParsesCorrectly` |
| `ReqStream-Section-Container` | Section Container Scenario | `Section_Load_NestedSections_ParsesHierarchyCorrectly` |
| `ReqStream-Section-Nesting` | Section Container Scenario | `Section_Load_NestedSections_ParsesHierarchyCorrectly` |
| `ReqStream-Section-Nesting` | Section Container Scenario | `Requirements_Export_NestedSections_CreatesHierarchy` |
| `ReqStream-Section-TitleMerging` | Section Container Scenario | `Requirements_Load_IdenticalSections_MergesCorrectly` |
