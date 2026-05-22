### RequirementsLoader Unit Verification

#### Verification Approach

The RequirementsLoader unit is verified using xUnit unit tests in `RequirementsLoaderTests.cs`.
Tests create YAML requirements files with specific structural conditions (unknown fields, missing
fields, duplicate IDs, circular references, etc.) and assert on the lint issues reported.

#### Test Environment

The RequirementsLoader unit tests require no setup beyond the standard xUnit test runner and .NET
runtime. Temporary YAML requirements files are created on disk and deleted on test completion.

#### Acceptance Criteria

The RequirementsLoader unit verification is complete when all xUnit tests in
`RequirementsLoaderTests.cs` pass without uncaught exceptions and all assertions succeed. The unit
is considered verified when every requirement in the Requirements Coverage table is mapped to at least one
passing test method.

#### Test Scenarios

##### File Loading Scenario

Tests verify that file path errors are correctly reported.

Test methods:

- `RequirementsLoader_Load_WithInvalidFilePath_ReportsError` — invalid path → error
- `RequirementsLoader_Load_WithMissingFile_ReportsError` — missing file → error
- `RequirementsLoader_Load_WithIoReadFailure_ReportsError` — I/O failure → error
- `RequirementsLoader_Load_WithNonMappingRoot_ReportsError` — non-mapping root → error
- `RequirementsLoader_Load_WithMalformedYaml_ReportsError` — malformed YAML → error
- `RequirementsLoader_Load_WithCircularFileInclude_ReportsError` — circular file include → error

##### Document Structure Scenario

Test methods:

- `RequirementsLoader_Load_WithUnknownDocumentField_ReportsError` — unknown document field
- `RequirementsLoader_Load_WithUnknownSectionField_ReportsError` — unknown section field
- `RequirementsLoader_Load_WithSectionMissingTitle_ReportsError` — missing section title
- `RequirementsLoader_Load_WithBlankSectionTitle_ReportsError` — blank section title

##### Requirement Structure Scenario

Test methods:

- `RequirementsLoader_Load_WithUnknownRequirementField_ReportsError` — unknown requirement field
- `RequirementsLoader_Load_WithNestedSectionIssues_ReportsError` — nested section issues reported
- `RequirementsLoader_Load_WithRequirementMissingId_ReportsError` — missing requirement ID
- `RequirementsLoader_Load_WithBlankRequirementId_ReportsError` — blank requirement ID
- `RequirementsLoader_Load_WithRequirementMissingTitle_ReportsError` — missing requirement title
- `RequirementsLoader_Load_WithBlankRequirementTitle_ReportsError` — blank requirement title

##### Duplicate and Reference Scenario

Test methods:

- `RequirementsLoader_Load_WithDuplicateIds_ReportsError` — duplicate IDs
- `RequirementsLoader_Load_WithDuplicateIdsAcrossFiles_ReportsError` — cross-file duplicates
- `RequirementsLoader_Load_WithMultipleCycles_ReportsAllCycles` — all circular refs reported
- `RequirementsLoader_Load_WithUnknownChildReference_ReportsError` — unknown child reference

##### Validation and Reporting Scenario

Test methods:

- `RequirementsLoader_Load_WithMultipleIssues_ReportsAllIssues` — all issues reported at once
- `RequirementsLoader_Load_WithIncludes_LintsIncludedFiles` — includes are linted
- `RequirementsLoader_Load_WithValidFile_ReportsNoIssues` — valid file → no issues
- `RequirementsLoader_Load_WithEmptyFile_ReportsNoIssues` — empty file → no issues
- `RequirementsLoader_Load_ErrorFormat_IncludesFileAndLocation` — error format includes file path

##### Mapping and List Scenario

Test methods:

- `RequirementsLoader_Load_WithUnknownMappingField_ReportsError` — unknown mapping field
- `RequirementsLoader_Load_WithMappingMissingId_ReportsError` — mapping missing ID
- `RequirementsLoader_Load_WithBlankMappingId_ReportsError` — blank mapping ID
- `RequirementsLoader_Load_WithBlankTestName_ReportsError` — blank test name
- `RequirementsLoader_Load_WithBlankMappingTestName_ReportsError` — blank mapping test name
- `RequirementsLoader_Load_WithBlankTagName_ReportsError` — blank tag name
- `RequirementsLoader_Load_WithNonScalarTestEntry_ReportsError` — non-scalar test entry
- `RequirementsLoader_Load_WithNonScalarChildEntry_ReportsError` — non-scalar child entry
- `RequirementsLoader_Load_WithNonScalarTagEntry_ReportsError` — non-scalar tag entry
- `RequirementsLoader_Load_WithNonScalarMappingTestEntry_ReportsError` — non-scalar mapping test
- `RequirementsLoader_Load_WithNonScalarIncludeEntry_ReportsError` — non-scalar include entry

#### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Lint-InvalidFilePath` | File Loading Scenario | `RequirementsLoader_Load_WithInvalidFilePath_ReportsError` |
| `ReqStream-Lint-FileNotFound` | File Loading Scenario | `RequirementsLoader_Load_WithMissingFile_ReportsError` |
| `ReqStream-Lint-IoReadFailure` | File Loading Scenario | `RequirementsLoader_Load_WithIoReadFailure_ReportsError` |
| `ReqStream-Lint-NonMappingRoot` | File Loading Scenario | `RequirementsLoader_Load_WithNonMappingRoot_ReportsError` |
| `ReqStream-Lint-MalformedYaml` | File Loading Scenario | `RequirementsLoader_Load_WithMalformedYaml_ReportsError` |
| `ReqStream-Lint-UnknownDocumentField` | Document Structure Scenario | `RequirementsLoader_Load_WithUnknownDocumentField_ReportsError` |
| `ReqStream-Lint-UnknownSectionField` | Document Structure Scenario | `RequirementsLoader_Load_WithUnknownSectionField_ReportsError` |
| `ReqStream-Lint-MissingSectionTitle` | Document Structure Scenario | `RequirementsLoader_Load_WithSectionMissingTitle_ReportsError`, `RequirementsLoader_Load_WithBlankSectionTitle_ReportsError` |
| `ReqStream-Lint-UnknownRequirementField` | Requirement Structure Scenario | `RequirementsLoader_Load_WithUnknownRequirementField_ReportsError`, `RequirementsLoader_Load_WithNestedSectionIssues_ReportsError` |
| `ReqStream-Lint-MissingRequirementId` | Requirement Structure Scenario | `RequirementsLoader_Load_WithRequirementMissingId_ReportsError`, `RequirementsLoader_Load_WithBlankRequirementId_ReportsError` |
| `ReqStream-Lint-MissingRequirementTitle` | Requirement Structure Scenario | `RequirementsLoader_Load_WithRequirementMissingTitle_ReportsError`, `RequirementsLoader_Load_WithBlankRequirementTitle_ReportsError` |
| `ReqStream-Lint-DuplicateIds` | Duplicate and Reference Scenario | `RequirementsLoader_Load_WithDuplicateIds_ReportsError`, `RequirementsLoader_Load_WithDuplicateIdsAcrossFiles_ReportsError` |
| `ReqStream-Lint-MultipleIssues` | Validation and Reporting Scenario | `RequirementsLoader_Load_WithMultipleIssues_ReportsAllIssues` |
| `ReqStream-Lint-FollowsIncludes` | Validation and Reporting Scenario | `RequirementsLoader_Load_WithIncludes_LintsIncludedFiles` |
| `ReqStream-Lint-NoIssuesMessage` | Validation and Reporting Scenario | `RequirementsLoader_Load_WithValidFile_ReportsNoIssues`, `RequirementsLoader_Load_WithEmptyFile_ReportsNoIssues` |
| `ReqStream-Lint-ErrorFormat` | Validation and Reporting Scenario | `RequirementsLoader_Load_ErrorFormat_IncludesFileAndLocation` |
| `ReqStream-Lint-UnknownMappingField` | Mapping and List Scenario | `RequirementsLoader_Load_WithUnknownMappingField_ReportsError` |
| `ReqStream-Lint-MissingMappingId` | Mapping and List Scenario | `RequirementsLoader_Load_WithMappingMissingId_ReportsError`, `RequirementsLoader_Load_WithBlankMappingId_ReportsError` |
| `ReqStream-Lint-BlankTestName` | Mapping and List Scenario | `RequirementsLoader_Load_WithBlankTestName_ReportsError`, `RequirementsLoader_Load_WithBlankMappingTestName_ReportsError` |
| `ReqStream-Lint-BlankTagName` | Mapping and List Scenario | `RequirementsLoader_Load_WithBlankTagName_ReportsError` |
| `ReqStream-Lint-NonScalarListEntries` | Mapping and List Scenario | `RequirementsLoader_Load_WithNonScalarTestEntry_ReportsError`, `RequirementsLoader_Load_WithNonScalarChildEntry_ReportsError`, `RequirementsLoader_Load_WithNonScalarTagEntry_ReportsError`, `RequirementsLoader_Load_WithNonScalarMappingTestEntry_ReportsError`, `RequirementsLoader_Load_WithNonScalarIncludeEntry_ReportsError` |
| `ReqStream-Lint-CircularReferences` | Duplicate and Reference Scenario | `RequirementsLoader_Load_WithMultipleCycles_ReportsAllCycles` |
| `ReqStream-Lint-UnknownChildReference` | Duplicate and Reference Scenario | `RequirementsLoader_Load_WithUnknownChildReference_ReportsError` |
| `ReqStream-Lint-CircularFileIncludes` | File Loading Scenario | `RequirementsLoader_Load_WithCircularFileInclude_ReportsError` |
