### Requirement

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

**Properties**: Tests verify that requirement properties are parsed correctly from YAML, including
tests list, tags, justification, and children. This scenario is tested by
`Requirement_Properties_NewInstance_HasDefaultValues`,
`Requirements_Load_RequirementWithTests_ParsesTestsCorrectly`,
`Requirements_Load_RequirementWithTags_ParsesTagsCorrectly`,
`Requirements_Load_RequirementWithJustification_ParsesJustificationCorrectly`, and
`Requirements_Load_RequirementWithChildren_ParsesChildrenCorrectly`.

**Validation**: Tests verify that missing or invalid fields are reported as lint errors, including
blank IDs, blank titles, duplicate IDs, blank tag names, blank child IDs, blank test names, blank
mapping test names, and blank mapping IDs. This scenario is tested by
`Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation`,
`Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation`,
`Requirements_Load_DuplicateRequirementId_ReportsError`,
`Requirements_Load_DuplicateRequirementId_ErrorIncludesFileLocation`,
`Requirements_Load_MultipleFilesWithDuplicateIds_ReportsError`,
`Requirements_Load_BlankTagName_ReportsErrorWithFileLocation`,
`Requirements_Load_BlankChildIdInRequirement_ReportsErrorWithFileLocation`,
`Requirements_Load_BlankTestNameInRequirement_ReportsErrorWithFileLocation`,
`Requirements_Load_BlankTestNameInMapping_ReportsErrorWithFileLocation`,
`Requirements_Load_BlankMappingId_ReportsErrorWithFileLocation`, and
`Requirements_Load_TestMappings_AppliesMappingsCorrectly`.

**Circular Reference Detection**: Tests verify that child-reference cycles are detected and
reported as errors, covering both mutual cycles (A → B → A) and self-references (A → A).
The loader must halt hierarchy resolution and surface a descriptive error including the cycle
path so that authors can identify and correct the circular dependency. This scenario is tested by
`Requirements_Load_CircularRequirements_ReportsCircularReferenceError` and
`Requirements_Load_SelfReferencingRequirement_ReportsCircularReferenceError`.

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Requirements-UniqueIds` | Validation Scenario | `Requirements_Load_DuplicateRequirementId_ReportsError` |
| `ReqStream-Requirements-UniqueIds` | Validation Scenario | `Requirements_Load_DuplicateRequirementId_ErrorIncludesFileLocation` |
| `ReqStream-Requirements-UniqueIds` | Validation Scenario | `Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-UniqueIds` | Validation Scenario | `Requirements_Load_MultipleFilesWithDuplicateIds_ReportsError` |
| `ReqStream-Requirements-RequiredTitle` | Validation Scenario | `Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-ParentChild` | Properties Scenario | `Requirements_Load_RequirementWithChildren_ParsesChildrenCorrectly` |
| `ReqStream-Requirements-BlankChildId` | Validation Scenario | `Requirements_Load_BlankChildIdInRequirement_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Tags` | Properties Scenario | `Requirements_Load_RequirementWithTags_ParsesTagsCorrectly` |
| `ReqStream-Requirements-BlankTagName` | Validation Scenario | `Requirements_Load_BlankTagName_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-Justification` | Properties Scenario | `Requirements_Load_RequirementWithJustification_ParsesJustificationCorrectly` |
| `ReqStream-Requirements-TestMappings` | Properties Scenario | `Requirements_Load_RequirementWithTests_ParsesTestsCorrectly` |
| `ReqStream-Requirements-TestMappings` | Properties Scenario | `Requirements_Load_BlankTestNameInRequirement_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-TestMappings` | Properties Scenario | `Requirements_Load_TestMappings_AppliesMappingsCorrectly` |
| `ReqStream-Requirements-TestMappings` | Properties Scenario | `Requirements_Load_BlankTestNameInMapping_ReportsErrorWithFileLocation` |
| `ReqStream-Requirements-TestMappings` | Properties Scenario | `Requirements_Load_BlankMappingId_ReportsErrorWithFileLocation` |
| `ReqStream-Requirement-Location` | Validation Scenario | `Requirements_Load_BlankRequirementId_ReportsErrorWithFileLocation` |
| `ReqStream-Requirement-Location` | Validation Scenario | `Requirements_Load_BlankRequirementTitle_ReportsErrorWithFileLocation` |
| `ReqStream-Requirement-Location` | Validation Scenario | `Requirements_Load_BlankTestNameInRequirement_ReportsErrorWithFileLocation` |
| `ReqStream-Requirement-Location` | Validation Scenario | `Requirements_Load_BlankTestNameInMapping_ReportsErrorWithFileLocation` |
| `ReqStream-Requirement-Location` | Validation Scenario | `Requirements_Load_BlankMappingId_ReportsErrorWithFileLocation` |
| `ReqStream-Requirement-Location` | Validation Scenario | `Requirements_Load_BlankTagName_ReportsErrorWithFileLocation` |
| `ReqStream-Requirement-Location` | Validation Scenario | `Requirements_Load_BlankChildIdInRequirement_ReportsErrorWithFileLocation` |
| `ReqStream-Lint-CircularReferences` | Circular Reference Detection Scenario | `Requirements_Load_CircularRequirements_ReportsCircularReferenceError` |
| `ReqStream-Lint-CircularReferences` | Circular Reference Detection Scenario | `Requirements_Load_SelfReferencingRequirement_ReportsCircularReferenceError` |
