## Requirement Unit Verification

### Verification Strategy

The Requirement unit is verified using xUnit integration tests in `RequirementTests.cs`. Tests
create YAML requirements files with various field combinations and assert on the parsed
`Requirement` data model properties and any lint issues reported.

### Test Scenarios

#### Properties Scenario

Tests verify that requirement properties are parsed correctly from YAML.

Test methods:

- `Requirement_Properties_DefaultValues` — default property values are correct
- `Requirements_Load_RequirementWithTests_ParsesTestsCorrectly` — tests list parsed
- `Requirements_Load_RequirementWithTags_ParsesTagsCorrectly` — tags list parsed
- `Requirements_Load_RequirementWithJustification_ParsesJustificationCorrectly` — justification parsed
- `Requirements_Load_RequirementWithChildren_ParsesChildrenCorrectly` — children list parsed

#### Validation Scenario

Tests verify that missing or invalid fields are reported as lint errors.

Test methods:

- `Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation` — blank ID → error
- `Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation` — blank title → error
- `Requirements_Load_DuplicateRequirementId_ReportsError` — duplicate ID → error
- `Requirements_Load_DuplicateRequirementId_ErrorIncludesFileLocation` — error includes file location
- `Requirements_Load_MultipleFilesWithDuplicateIds_ReportsError` — cross-file duplicate ID → error
- `Requirements_Load_BlankTagName_ReportsErrorWithFileLocation` — blank tag → error
- `Requirements_Load_BlankChildIdInRequirement_ReportsErrorWithFileLocation` — blank child ID → error
- `Requirements_Load_BlankTestNameInRequirement_ReportsErrorWithFileLocation` — blank test name → error
- `Requirements_Load_BlankTestNameInMapping_ReportsErrorWithFileLocation` — blank mapping test → error
- `Requirements_Load_BlankMappingId_ReportsErrorWithFileLocation` — blank mapping ID → error
- `Requirements_Load_TestMappings_AppliesMappingsCorrectly` — test mappings applied correctly

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Requirements-UniqueIds` | `Requirements_Load_DuplicateRequirementId_ReportsError`, `Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation`, `Requirements_Load_MultipleFilesWithDuplicateIds_ReportsError` |
| `ReqStream-Requirements-RequiredTitle` | `Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-ParentChild` | `Requirements_Load_RequirementWithChildren_ParsesChildrenCorrectly`, `Requirements_Load_BlankChildIdInRequirement_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Tags` | `Requirements_Load_RequirementWithTags_ParsesTagsCorrectly`, `Requirements_Load_BlankTagName_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Justification` | `Requirements_Load_RequirementWithJustification_ParsesJustificationCorrectly` |
| `ReqStream-Requirements-TestMappings` | `Requirements_Load_RequirementWithTests_ParsesTestsCorrectly`, `Requirements_Load_BlankTestNameInRequirement_ReportsErrorWithFileLocation`, `Requirements_Load_TestMappings_AppliesMappingsCorrectly`, `Requirements_Load_BlankTestNameInMapping_ReportsErrorWithFileLocation`, `Requirements_Load_BlankMappingId_ReportsErrorWithFileLocation` |
