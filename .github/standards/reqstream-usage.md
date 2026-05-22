---
name: ReqStream Usage
description: Follow these standards when managing requirements with ReqStream.
globs: ["requirements.yaml", "docs/reqstream/**/*.yaml"]
---

# Required Standards

Read these standards first before applying this standard:

- **`requirements-principles.md`** - Requirements principles and unidirectionality
- **`software-items.md`** - Software categorization (System/Subsystem/Unit/OTS/Shared Package)

# Requirements Organization

Organize requirements under `docs/reqstream/` mirroring the source code structure
because ReqStream discovers files via the includes chain in `requirements.yaml`
and organizes report output by this hierarchy:

```text
requirements.yaml                    # Root file (includes only)
docs/reqstream/
├── {system-name}.yaml               # System-level requirements
├── {system-name}/                   # System folder (one per system)
│   ├── platform-requirements.yaml  # Platform support requirements
│   ├── {subsystem-name}.yaml        # Subsystem requirements
│   ├── {subsystem-name}/            # Subsystem folder (kebab-case); may nest recursively
│   │   ├── {child-subsystem}.yaml   # Child subsystem requirements
│   │   ├── {child-subsystem}/       # Child subsystem folder
│   │   └── {unit-name}.yaml         # Unit requirements
│   └── {unit-name}.yaml             # System-level unit requirements
├── ots/                             # OTS items appear as a distinct section in reports
│   └── {ots-name}.yaml              # Requirements for OTS components
└── shared/                          # Shared Packages appear as a distinct section in reports
    └── {package-name}.yaml          # Requirements for Shared Package dependencies
```

Local items have matching relative paths across `docs/reqstream/`, `docs/design/`, and
`docs/verification/`. OTS items appear in `docs/reqstream/ots/`, `docs/design/ots/`, and
`docs/verification/ots/`. Shared Packages appear in `docs/reqstream/shared/`,
`docs/design/shared/`, and `docs/verification/shared/`.

# Requirements File Format

```yaml
sections:
  - title: Functional Requirements
    requirements:
      - id: System-Component-Feature      # Used as-is in all reports - make it readable
        title: The system shall perform the required function.
        justification: |
          Business rationale and any regulatory references.
          # ReqStream extracts this field into the justifications report (--justifications)
        children:                         # ReqStream validates this decomposition chain
          - ChildSystem-Feature-Behavior  # Downward links only (see requirements-principles.md)
        tests:                            # ReqStream matches these by method name in test results
          - TestMethodName
          - windows@PlatformSpecificTest  # Only test runs on Windows count as evidence
```

# Local System/Subsystem/Unit Requirements

Use nested sections to mirror the system/subsystem/unit hierarchy because ReqStream
does not infer nesting from folder structure — the section hierarchy in the YAML is
the document hierarchy. Identical section title paths across included files are
automatically merged by ReqStream.

**Subsystem file** (`docs/reqstream/{system-name}/{subsystem-name}.yaml`):

```yaml
sections:
  - title: '{SystemName} Requirements'
    sections:
      - title: '{SubsystemName} Requirements'
        requirements:
          - id: SystemName-SubsystemName-Feature
            title: The {SubsystemName} shall perform the required function.
            children:
              - SystemName-SubsystemName-UnitName-Feature
            tests:
              - SubsystemName_Functionality_Scenario_ExpectedBehavior
```

**Unit file** (`docs/reqstream/{system-name}/{subsystem-name}/{unit-name}.yaml`):

```yaml
sections:
  - title: '{SystemName} Requirements'
    sections:
      - title: '{SubsystemName} Requirements'
        sections:
          - title: '{UnitName} Requirements'
            requirements:
              - id: SystemName-SubsystemName-UnitName-Feature
                title: '{UnitName} shall perform the required function.'
                tests:
                  - UnitName_MethodUnderTest_Scenario_ExpectedBehavior
```

# OTS Software Requirements

Use nested sections in `docs/reqstream/ots/` because ReqStream renders the `ots/`
subtree as a distinct section in generated reports, separate from local
system requirements:

```yaml
sections:
  - title: OTS Software Requirements
    sections:
      - title: System.Text.Json
        requirements:
          - id: SystemTextJson-Core-ReadJson
            title: System.Text.Json shall be able to read JSON files.
            tests:
              - JsonReaderTests.TestReadValidJson
```

# Shared Package Requirements

Use nested sections in `docs/reqstream/shared/` - ReqStream renders the `shared/`
subtree as a distinct section in reports, separate from local and OTS requirements:

```yaml
sections:
  - title: Shared Package Requirements
    sections:
      - title: MyOrg.SharedLibrary
        requirements:
          - id: SharedLibrary-Core-ParseConfig
            title: MyOrg.SharedLibrary shall parse configuration files.
            tests:
              - SharedLibraryIntegrationTests.TestParseValidConfig
```

# Semantic IDs (MANDATORY)

Use the `System-Component-Feature` pattern because ReqStream uses IDs as-is in
all generated reports and the trace matrix - opaque IDs make those outputs
unreadable without a separate lookup:

- **System-level**: `TemplateTool-Core-DisplayHelp`
- **Subsystem-level**: `TemplateTool-Parser-ParseYaml`
- **Unit-level**: `TemplateTool-Validator-CheckFormat`
- **Bad**: `REQ-042` (meaningless in report output)

# Source Filter Requirements (CRITICAL)

Platform-specific requirements MUST use source filters because without them
ReqStream accepts test results from any platform as evidence - a Windows-only
requirement would incorrectly pass on Linux:

```yaml
tests:
  - "windows@TestMethodName"    # Only Windows test runs count as evidence
  - "ubuntu@TestMethodName"     # Only Linux test runs count as evidence
  - "net8.0@TestMethodName"     # Only .NET 8 runs count as evidence
  - "TestMethodName"            # Any platform acceptable
```

**WARNING**: Removing source filters invalidates platform-specific compliance
evidence.

# ReqStream Commands

```bash
# Validate YAML syntax and requirement IDs before generating any reports
dotnet reqstream --requirements requirements.yaml --lint

# Generate requirements document for compliance record
dotnet reqstream --requirements requirements.yaml \
  --report docs/requirements_doc/generated/requirements.md

# Generate justifications document for compliance record
dotnet reqstream --requirements requirements.yaml \
  --justifications docs/requirements_doc/generated/justifications.md

# Generate trace matrix proving each requirement is covered by passing tests
dotnet reqstream --requirements requirements.yaml \
  --tests "artifacts/**/*.trx" \
  --matrix docs/requirements_report/generated/trace_matrix.md
```

# Quality Checks

Before submitting requirements, verify:

- [ ] All requirements have semantic IDs (`System-Section-Feature` pattern)
- [ ] Every requirement links to at least one passing test
- [ ] Platform-specific requirements use source filters (`platform@TestName`)
- [ ] Comprehensive justification explains business/regulatory need
- [ ] Files organized under `docs/reqstream/` following the folder structure pattern above
- [ ] All documentation folders use kebab-case names matching source code structure
- [ ] OTS requirements placed in `ots/` subfolder
- [ ] Shared Package requirements placed in `shared/` subfolder
- [ ] Valid YAML syntax passes yamllint validation
- [ ] Test result formats compatible (TRX, JUnit XML)
