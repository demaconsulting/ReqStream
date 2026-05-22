### RequirementsLoader

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
is considered verified when every requirement in the Requirements Coverage table is mapped to at
least one passing test method.

#### Test Scenarios

**File Loading**: Tests verify that file path errors are correctly reported, including invalid
paths, missing files, I/O read failures, non-mapping root nodes, malformed YAML, and circular
file includes. This scenario is tested by
`RequirementsLoader_Load_WithInvalidFilePath_ReportsError`,
`RequirementsLoader_Load_WithMissingFile_ReportsError`,
`RequirementsLoader_Load_WithIoReadFailure_ReportsError`,
`RequirementsLoader_Load_WithNonMappingRoot_ReportsError`,
`RequirementsLoader_Load_WithMalformedYaml_ReportsError`, and
`RequirementsLoader_Load_WithCircularFileInclude_ReportsError`.

**Document Structure**: Tests verify that unknown document fields, unknown section fields, missing
section titles, and blank section titles are reported as errors. This scenario is tested by
`RequirementsLoader_Load_WithUnknownDocumentField_ReportsError`,
`RequirementsLoader_Load_WithUnknownSectionField_ReportsError`,
`RequirementsLoader_Load_WithSectionMissingTitle_ReportsError`, and
`RequirementsLoader_Load_WithBlankSectionTitle_ReportsError`.

**Requirement Structure**: Tests verify that unknown requirement fields, nested section issues,
missing requirement IDs, blank requirement IDs, missing requirement titles, and blank requirement
titles are reported as errors. This scenario is tested by
`RequirementsLoader_Load_WithUnknownRequirementField_ReportsError`,
`RequirementsLoader_Load_WithNestedSectionIssues_ReportsError`,
`RequirementsLoader_Load_WithRequirementMissingId_ReportsError`,
`RequirementsLoader_Load_WithBlankRequirementId_ReportsError`,
`RequirementsLoader_Load_WithRequirementMissingTitle_ReportsError`, and
`RequirementsLoader_Load_WithBlankRequirementTitle_ReportsError`.

**Duplicate and Reference**: Tests verify that duplicate IDs (within and across files), multiple
cycles, and unknown child references are detected and reported. This scenario is tested by
`RequirementsLoader_Load_WithDuplicateIds_ReportsError`,
`RequirementsLoader_Load_WithDuplicateIdsAcrossFiles_ReportsError`,
`RequirementsLoader_Load_WithMultipleCycles_ReportsAllCycles`, and
`RequirementsLoader_Load_WithUnknownChildReference_ReportsError`.

**Validation and Reporting**: Tests verify that multiple issues are all reported at once, included
files are linted, valid and empty files produce no issues, and error messages include the file path
and location. This scenario is tested by
`RequirementsLoader_Load_WithMultipleIssues_ReportsAllIssues`,
`RequirementsLoader_Load_WithIncludes_LintsIncludedFiles`,
`RequirementsLoader_Load_WithValidFile_ReportsNoIssues`,
`RequirementsLoader_Load_WithEmptyFile_ReportsNoIssues`, and
`RequirementsLoader_Load_ErrorFormat_IncludesFileAndLocation`.

**Mapping and List**: Tests verify that unknown mapping fields, missing and blank mapping IDs,
blank test names, blank tag names, and non-scalar list entries are reported as errors. This
scenario is tested by `RequirementsLoader_Load_WithUnknownMappingField_ReportsError`,
`RequirementsLoader_Load_WithMappingMissingId_ReportsError`,
`RequirementsLoader_Load_WithBlankMappingId_ReportsError`,
`RequirementsLoader_Load_WithBlankTestName_ReportsError`,
`RequirementsLoader_Load_WithBlankMappingTestName_ReportsError`,
`RequirementsLoader_Load_WithBlankTagName_ReportsError`,
`RequirementsLoader_Load_WithNonScalarTestEntry_ReportsError`,
`RequirementsLoader_Load_WithNonScalarChildEntry_ReportsError`,
`RequirementsLoader_Load_WithNonScalarTagEntry_ReportsError`,
`RequirementsLoader_Load_WithNonScalarMappingTestEntry_ReportsError`, and
`RequirementsLoader_Load_WithNonScalarIncludeEntry_ReportsError`.

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
| `ReqStream-Lint-MissingSectionTitle` | Document Structure Scenario | `RequirementsLoader_Load_WithSectionMissingTitle_ReportsError` |
| `ReqStream-Lint-MissingSectionTitle` | Document Structure Scenario | `RequirementsLoader_Load_WithBlankSectionTitle_ReportsError` |
| `ReqStream-Lint-UnknownRequirementField` | Requirement Structure Scenario | `RequirementsLoader_Load_WithUnknownRequirementField_ReportsError` |
| `ReqStream-Lint-UnknownRequirementField` | Requirement Structure Scenario | `RequirementsLoader_Load_WithNestedSectionIssues_ReportsError` |
| `ReqStream-Lint-MissingRequirementId` | Requirement Structure Scenario | `RequirementsLoader_Load_WithRequirementMissingId_ReportsError` |
| `ReqStream-Lint-MissingRequirementId` | Requirement Structure Scenario | `RequirementsLoader_Load_WithBlankRequirementId_ReportsError` |
| `ReqStream-Lint-MissingRequirementTitle` | Requirement Structure Scenario | `RequirementsLoader_Load_WithRequirementMissingTitle_ReportsError` |
| `ReqStream-Lint-MissingRequirementTitle` | Requirement Structure Scenario | `RequirementsLoader_Load_WithBlankRequirementTitle_ReportsError` |
| `ReqStream-Lint-DuplicateIds` | Duplicate and Reference Scenario | `RequirementsLoader_Load_WithDuplicateIds_ReportsError` |
| `ReqStream-Lint-DuplicateIds` | Duplicate and Reference Scenario | `RequirementsLoader_Load_WithDuplicateIdsAcrossFiles_ReportsError` |
| `ReqStream-Lint-MultipleIssues` | Validation and Reporting Scenario | `RequirementsLoader_Load_WithMultipleIssues_ReportsAllIssues` |
| `ReqStream-Lint-FollowsIncludes` | Validation and Reporting Scenario | `RequirementsLoader_Load_WithIncludes_LintsIncludedFiles` |
| `ReqStream-Lint-NoIssuesMessage` | Validation and Reporting Scenario | `RequirementsLoader_Load_WithValidFile_ReportsNoIssues` |
| `ReqStream-Lint-NoIssuesMessage` | Validation and Reporting Scenario | `RequirementsLoader_Load_WithEmptyFile_ReportsNoIssues` |
| `ReqStream-Lint-ErrorFormat` | Validation and Reporting Scenario | `RequirementsLoader_Load_ErrorFormat_IncludesFileAndLocation` |
| `ReqStream-Lint-UnknownMappingField` | Mapping and List Scenario | `RequirementsLoader_Load_WithUnknownMappingField_ReportsError` |
| `ReqStream-Lint-MissingMappingId` | Mapping and List Scenario | `RequirementsLoader_Load_WithMappingMissingId_ReportsError` |
| `ReqStream-Lint-MissingMappingId` | Mapping and List Scenario | `RequirementsLoader_Load_WithBlankMappingId_ReportsError` |
| `ReqStream-Lint-BlankTestName` | Mapping and List Scenario | `RequirementsLoader_Load_WithBlankTestName_ReportsError` |
| `ReqStream-Lint-BlankTestName` | Mapping and List Scenario | `RequirementsLoader_Load_WithBlankMappingTestName_ReportsError` |
| `ReqStream-Lint-BlankTagName` | Mapping and List Scenario | `RequirementsLoader_Load_WithBlankTagName_ReportsError` |
| `ReqStream-Lint-NonScalarListEntries` | Mapping and List Scenario | `RequirementsLoader_Load_WithNonScalarTestEntry_ReportsError` |
| `ReqStream-Lint-NonScalarListEntries` | Mapping and List Scenario | `RequirementsLoader_Load_WithNonScalarChildEntry_ReportsError` |
| `ReqStream-Lint-NonScalarListEntries` | Mapping and List Scenario | `RequirementsLoader_Load_WithNonScalarTagEntry_ReportsError` |
| `ReqStream-Lint-NonScalarListEntries` | Mapping and List Scenario | `RequirementsLoader_Load_WithNonScalarMappingTestEntry_ReportsError` |
| `ReqStream-Lint-NonScalarListEntries` | Mapping and List Scenario | `RequirementsLoader_Load_WithNonScalarIncludeEntry_ReportsError` |
| `ReqStream-Lint-CircularReferences` | Duplicate and Reference Scenario | `RequirementsLoader_Load_WithMultipleCycles_ReportsAllCycles` |
| `ReqStream-Lint-UnknownChildReference` | Duplicate and Reference Scenario | `RequirementsLoader_Load_WithUnknownChildReference_ReportsError` |
| `ReqStream-Lint-CircularFileIncludes` | File Loading Scenario | `RequirementsLoader_Load_WithCircularFileInclude_ReportsError` |
