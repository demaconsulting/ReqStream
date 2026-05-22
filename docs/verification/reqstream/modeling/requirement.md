### Requirement Unit Verification

#### Verification Approach

The Requirement unit is verified using xUnit integration tests in `RequirementTests.cs`. Tests
create YAML requirements files with various field combinations and assert on the parsed
`Requirement` data model properties and any lint issues reported.

#### Test Environment

The Requirement unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
Temporary YAML requirements files are created on disk and deleted on test completion.

#### Acceptance Criteria

The Requirement unit verification is complete when all xUnit tests in `RequirementTests.cs` pass
without uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage table is mapped to at least one passing test method.

#### Test Scenarios

##### Properties Scenario

Tests verify that requirement properties are parsed correctly from YAML.

Test methods:

- `Requirement_Properties_DefaultValues` — default property values are correct
- `Requirements_Load_RequirementWithTests_ParsesTestsCorrectly` — tests list parsed
- `Requirements_Load_RequirementWithTags_ParsesTagsCorrectly` — tags list parsed
- `Requirements_Load_RequirementWithJustification_ParsesJustificationCorrectly` — justification parsed
- `Requirements_Load_RequirementWithChildren_ParsesChildrenCorrectly` — children list parsed

##### Validation Scenario

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

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Requirements-UniqueIds` | Validation Scenario | `Requirements_Load_DuplicateRequirementId_ReportsError`, `Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation`, `Requirements_Load_MultipleFilesWithDuplicateIds_ReportsError` |
| `ReqStream-Requirements-RequiredTitle` | Validation Scenario | `Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-ParentChild` | Properties Scenario | `Requirements_Load_RequirementWithChildren_ParsesChildrenCorrectly` |
| `ReqStream-Requirements-BlankChildId` | Validation Scenario | `Requirements_Load_BlankChildIdInRequirement_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Tags` | Properties Scenario | `Requirements_Load_RequirementWithTags_ParsesTagsCorrectly` |
| `ReqStream-Requirements-BlankTagName` | Validation Scenario | `Requirements_Load_BlankTagName_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Justification` | Properties Scenario | `Requirements_Load_RequirementWithJustification_ParsesJustificationCorrectly` |
| `ReqStream-Requirements-TestMappings` | Properties Scenario | `Requirements_Load_RequirementWithTests_ParsesTestsCorrectly`, `Requirements_Load_BlankTestNameInRequirement_ReportsErrorWithFileLocation`, `Requirements_Load_TestMappings_AppliesMappingsCorrectly`, `Requirements_Load_BlankTestNameInMapping_ReportsErrorWithFileLocation`, `Requirements_Load_BlankMappingId_ReportsErrorWithFileLocation` |
