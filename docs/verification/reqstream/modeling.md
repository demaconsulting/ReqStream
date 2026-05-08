## Modeling Subsystem Verification

### Verification Strategy

The Modeling subsystem is verified using xUnit integration tests in `ModelingTests.cs`. Tests
create temporary YAML requirements files, invoke `Requirements.Load`, and assert on the
resulting data model, lint issues, and generated Markdown reports. The subsystem boundary
is the `Requirements` class which acts as the public API entry point.

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

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Modeling-YamlParsing` | `Modeling_YamlParsing_ValidFile_LoadsRequirements`, `Modeling_YamlParsing_ValidFile_ReturnsNoLintIssues`, `Modeling_YamlParsing_DuplicateIds_DetectsLintError` |
| `ReqStream-Modeling-Export` | `Modeling_Export_Requirements_GeneratesMarkdownFile`, `Modeling_Export_Justifications_GeneratesMarkdownFile` |
| `ReqStream-Modeling-MultiFileLoading` | `Modeling_YamlParsing_ValidFile_LoadsRequirements`, `Modeling_MultiFileLoading_WithIncludes_LoadsRequirementsFromAllFiles` |
| `ReqStream-Modeling-Linting` | `Modeling_YamlParsing_DuplicateIds_DetectsLintError`, `Modeling_Linting_MalformedYaml_DetectsError`, `Modeling_Linting_ValidFile_ReturnsNoIssues` |
| `ReqStream-Modeling-LintingValidation` | `Modeling_Linting_MalformedYaml_DetectsError`, `Modeling_Linting_ValidFile_ReturnsNoIssues` |
| `ReqStream-Modeling-LintingReporting` | `Modeling_YamlParsing_DuplicateIds_DetectsLintError`, `Modeling_LintingReporting_MultipleConditions_ReportsAllIssues` |
