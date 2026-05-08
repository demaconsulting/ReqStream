### Section Unit Verification

#### Verification Strategy

The Section unit is verified using xUnit integration tests in `SectionTests.cs`. Tests create
YAML requirements files and invoke `Requirements.Load`, then assert on the parsed `Section`
data structure — title, requirements list, and child sections.

#### Test Scenarios

##### Section Container Scenario

Tests verify that a section holds a title, requirements, and child sections correctly.

Test methods:

- `Section_Load_SimpleRequirement_ParsesCorrectly` — single requirement in a section
- `Section_Load_NestedSections_ParsesHierarchyCorrectly` — nested child sections
- `Section_Load_BlankSectionTitle_ReportsErrorWithFileLocation` — blank title → error with location

#### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Section-Container` | `Section_Load_SimpleRequirement_ParsesCorrectly`, `Section_Load_NestedSections_ParsesHierarchyCorrectly` |
| `ReqStream-Section-Nesting` | `Section_Load_NestedSections_ParsesHierarchyCorrectly`, `Requirements_Export_NestedSections_CreatesHierarchy` |
| `ReqStream-Section-TitleMerging` | `Requirements_Load_IdenticalSections_MergesCorrectly` |
