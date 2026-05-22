## Modeling Subsystem Verification

### Verification Strategy

The Modeling subsystem is verified using xUnit integration tests in `ModelingTests.cs`. Tests
create temporary YAML requirements files, invoke `Requirements.Load`, and assert on the
resulting data model, lint issues, and generated Markdown reports. The subsystem boundary
is the `Requirements` class which acts as the public API entry point.

### Test Environment

The Modeling subsystem tests require no setup beyond the standard xUnit test runner and .NET
runtime. Temporary YAML requirements files are created on disk by each test and deleted on test
completion.

### Acceptance Criteria

The Modeling subsystem verification is complete when all xUnit tests in `ModelingTests.cs` pass
without uncaught exceptions and all assertions succeed. The subsystem is considered verified when
every requirement in the Coverage Summary is mapped to at least one passing test method.

### Test Scenarios

#### YAML Parsing Scenario

Tests verify that valid YAML files load correctly and that duplicate IDs are detected.

Test methods:

- `Modeling_YamlParsing_ValidFile_LoadsRequirements` — valid file → loaded requirements
- `Modeling_YamlParsing_ValidFile_ReturnsNoLintIssues` — valid file → no lint issues
- `Modeling_YamlParsing_DuplicateIds_DetectsLintError` — duplicate IDs → error lint issue

#### Export Scenario

Tests verify that requirements and justifications are exported to Markdown files.

Test methods:

- `Modeling_Export_Requirements_GeneratesMarkdownFile` — requirements Markdown export
- `Modeling_Export_Justifications_GeneratesMarkdownFile` — justifications Markdown export

#### Linting Scenario

Tests verify that structural issues are detected and that valid files return no issues.

Test methods:

- `Modeling_Linting_MalformedYaml_DetectsError` — malformed YAML → error with null requirements
- `Modeling_Linting_ValidFile_ReturnsNoIssues` — valid file → no issues
- `Modeling_LintingReporting_MultipleConditions_ReportsAllIssues` — all issues reported at once

### Requirements Coverage

| Requirement ID | Scenario(s) | Test Method(s) |
| --- | --- | --- |
| `ReqStream-Modeling-YamlParsing` | YAML Parsing Scenario | `Modeling_YamlParsing_ValidFile_LoadsRequirements`, `Modeling_YamlParsing_ValidFile_ReturnsNoLintIssues`, `Modeling_YamlParsing_DuplicateIds_DetectsLintError` |
| `ReqStream-Modeling-Export` | Export Scenario | `Modeling_Export_Requirements_GeneratesMarkdownFile`, `Modeling_Export_Justifications_GeneratesMarkdownFile` |
| `ReqStream-Modeling-MultiFileLoading` | YAML Parsing Scenario | `Modeling_YamlParsing_ValidFile_LoadsRequirements`, `Modeling_MultiFileLoading_WithIncludes_LoadsRequirementsFromAllFiles` |
| `ReqStream-Modeling-Linting` | Linting Scenario | `Modeling_YamlParsing_DuplicateIds_DetectsLintError`, `Modeling_Linting_MalformedYaml_DetectsError`, `Modeling_Linting_ValidFile_ReturnsNoIssues` |
| `ReqStream-Modeling-LintingValidation` | Linting Scenario | `Modeling_Linting_MalformedYaml_DetectsError`, `Modeling_Linting_ValidFile_ReturnsNoIssues` |
| `ReqStream-Modeling-LintingReporting` | Linting Scenario | `Modeling_YamlParsing_DuplicateIds_DetectsLintError`, `Modeling_LintingReporting_MultipleConditions_ReportsAllIssues` |
