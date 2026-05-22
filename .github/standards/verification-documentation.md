---
name: Verification Documentation
description: Follow these standards when creating software verification design documentation.
globs: ["docs/verification/**/*.md"]
---

# Required Standards

- **`technical-documentation.md`** - General technical documentation standards
- **`software-items.md`** - Software categorization (System/Subsystem/Unit/OTS/Shared Package)

# Folder Structure

```text
docs/verification/
├── introduction.md              # heading depth #
├── {system-name}.md             # heading depth #
├── {system-name}/
│   ├── {subsystem-name}.md      # heading depth ##
│   ├── {subsystem-name}/
│   │   └── {unit-name}.md       # heading depth ###
│   └── {unit-name}.md           # heading depth ##
├── ots.md                       # heading depth # (if OTS items exist)
├── ots/
│   └── {ots-name}.md            # heading depth ##
├── shared.md                    # heading depth # (if Shared Packages exist)
└── shared/
    └── {package-name}.md        # heading depth ##
```

Subsystems may nest recursively. Each file's heading depth equals its folder depth under `docs/verification/`.

# introduction.md (MANDATORY)

Must include:

- **Purpose**: audience and compliance drivers
- **Scope**: items covered and explicitly excluded (no test projects)
- **Companion Artifact Structure**: parallel paths for requirements, design, verification, source, tests
- **References** _(if applicable)_: external standards or specifications - only in `introduction.md`

# System Verification Design (MANDATORY)

Create `{system-name}.md` (`#` heading) and `{system-name}/` folder. All sections mandatory;
write "N/A - {justification}" rather than removing any section:

- **Verification Strategy**: test types (unit, integration, end-to-end), framework, project structure
- **Test Environment**: OS, runtime, external services, files, or configuration required
- **Acceptance Criteria**: what constitutes a passing system test (IEC 62304 §5.7.2)
- **System-Level Test Scenarios**: named scenarios for each system requirement
- **Requirements Coverage**: requirement → scenario(s) → test method(s) mapping

# Subsystem Verification Design (MANDATORY)

Place `{subsystem-name}.md` in the **parent** folder; create `{subsystem-name}/` for children.
All sections mandatory; write "N/A - {justification}" rather than removing any section:

- **Verification Strategy**: integration test approach and mocking at subsystem boundary
- **Test Environment**: any environment setup beyond the standard test runner
- **Acceptance Criteria**: what constitutes a passing subsystem test (IEC 62304 §5.5.2)
- **Test Scenarios**: named scenarios including boundary conditions, error paths, and normal operation
- **Requirements Coverage**: requirement → scenario(s) → test method(s) mapping

# Unit Verification Design (MANDATORY)

Place `{unit-name}.md` in the **parent** folder. All sections mandatory;
write "N/A - {justification}" rather than removing any section:

- **Verification Approach**: what is mocked/stubbed and why; injected vs. real dependencies
- **Test Environment**: any environment setup beyond the standard test runner
- **Acceptance Criteria**: what constitutes passing unit tests (IEC 62304 §5.5.2)
- **Test Scenarios**: named scenarios including boundary values, error paths, and normal operation
- **Requirements Coverage**: requirement → scenario(s) → test method(s) mapping

# OTS Verification Evidence (when OTS items exist)

Create `docs/verification/ots.md` (`#` heading) covering the overall OTS verification strategy.

For each OTS item, create `docs/verification/ots/{ots-name}.md` (`##` heading) covering:
verification approach (self-validation, integration tests, vendor evidence) and requirements coverage.

# Shared Package Verification Evidence (when Shared Packages exist)

Create `docs/verification/shared.md` (`#` heading) covering the overall Shared Package verification strategy.

For each Shared Package, create `docs/verification/shared/{package-name}.md` (`##` heading) covering:
verification approach and requirements coverage.

# Writing Guidelines

- Name scenarios clearly ("Valid input returns parsed result", not "Test 1")
- Use verbal cross-references - not markdown hyperlinks (break in PDF)
- Use Mermaid diagrams to supplement (not replace) text

# Quality Checks

- [ ] `introduction.md` includes Companion Artifact Structure
- [ ] Each file's heading depth matches its folder depth
- [ ] All folders use kebab-case mirroring source structure
- [ ] System verification includes all mandatory sections (Verification Strategy, Test Environment,
  Acceptance Criteria, System-Level Test Scenarios, Requirements Coverage)
- [ ] Subsystem verification includes all mandatory sections (Verification Strategy, Test Environment,
  Acceptance Criteria, Test Scenarios, Requirements Coverage)
- [ ] Unit verification includes all mandatory sections (Verification Approach, Test Environment,
  Acceptance Criteria, Test Scenarios, Requirements Coverage)
- [ ] Non-applicable mandatory sections contain "N/A - {justification}"
- [ ] Every requirement is mapped to at least one named test scenario
- [ ] `docs/verification/ots.md` and `docs/verification/ots/{ots-name}.md` exist when OTS items are present
- [ ] `docs/verification/shared.md` and `docs/verification/shared/{package-name}.md` exist when Shared Packages are present
- [ ] Documents are integrated into ReviewMark review-sets
