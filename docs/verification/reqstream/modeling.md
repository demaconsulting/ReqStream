## Modeling

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
every Modeling requirement is mapped to at least one passing test method in the ReqStream trace
matrix.

### Test Scenarios

**YAML Parsing**: Tests verify that valid YAML files load correctly, that duplicate IDs are
detected, and that providing no file paths raises an `ArgumentException`. This scenario is tested
by `Modeling_YamlParsing_ValidFile_LoadsRequirements`, which verifies a valid file loads
requirements; `Modeling_YamlParsing_ValidFile_ReturnsNoLintIssues`, which verifies a valid file
returns no lint issues; `Modeling_YamlParsing_DuplicateIds_DetectsLintError`, which verifies
duplicate IDs produce an error lint issue; and
`Modeling_YamlParsing_NoPaths_ThrowsArgumentException`, which verifies an empty paths array
throws `ArgumentException`.

**Export**: Tests verify that requirements and justifications are exported to Markdown files. This
scenario is tested by `Modeling_Export_Requirements_GeneratesMarkdownFile`, which verifies
requirements Markdown export, and `Modeling_Export_Justifications_GeneratesMarkdownFile`, which
verifies justifications Markdown export.

**Linting**: Tests verify that structural issues are detected and that valid files return no
issues. This scenario is tested by `Modeling_Linting_MalformedYaml_DetectsError`, which verifies
malformed YAML produces an error with null requirements;
`Modeling_Linting_ValidFile_ReturnsNoIssues`, which verifies a valid file returns no issues; and
`Modeling_LintingReporting_MultipleConditions_ReportsAllIssues`, which verifies all issues are
reported at once.

**Orphan Detection**: Tests verify that the subsystem identifies every requirement not reachable,
via child requirement references, from any requirement tagged with a configured root tag. This
scenario is tested by `Requirements_FindOrphans_EmptyRootTags_ReturnsNoOrphans`, which verifies
an empty root-tag set is a no-op, and
`Requirements_FindOrphans_IsolatedRequirement_NoTagsNoParentNoChildren_IsOrphaned`, which
verifies a fully isolated requirement is reported as orphaned.
