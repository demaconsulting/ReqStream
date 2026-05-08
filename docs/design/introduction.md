# Introduction

This document provides the detailed design for the ReqStream tool, a .NET command-line application
for managing software requirements in YAML format.

## Purpose

The purpose of this document is to describe the internal design of the ReqStream system, its
subsystems, and each software unit. It captures data models, algorithms, key methods, and
inter-unit interactions at a level of detail sufficient for formal code review, compliance
verification, and future maintenance. The document does not restate requirements; it explains how
they are realized.

## Scope

This document covers the detailed design of the following software items, spanning system, subsystem, and unit levels:

- **Program** — entry point and execution orchestrator (`Program.cs`)
- **Context** — command-line argument parser and I/O owner (`Cli/Context.cs`)
- **Validation** — self-validation test runner (`SelfTest/Validation.cs`)
- **LintIssue and LoadResult** — lint severity classification, issue data model, and load-result
  encapsulation (`Modeling/LintIssue.cs`, `Modeling/LoadResult.cs`)
- **Requirement, Requirements, RequirementsLoader, and Section** — YAML parsing, section merging,
  validation, lint reporting, and export (`Modeling/Requirement.cs`, `Modeling/Requirements.cs`,
  `Modeling/RequirementsLoader.cs`, `Modeling/Section.cs`)
- **TraceMatrix** — test result loader and requirement-coverage analyzer (`Tracing/TraceMatrix.cs`)

The following topics are out of scope:

- External library internals (YamlDotNet, DemaConsulting.TestResults)
- Build pipeline configuration
- Deployment and packaging
- Test projects, test classes, and test infrastructure

## Software Structure

The following tree shows how the ReqStream software items are organized across the system,
subsystem, and unit levels:

```text
ReqStream (System)
├── Program (Unit)
├── Cli (Subsystem)
│   └── Context (Unit)
├── Modeling (Subsystem)
│   ├── LintIssue (Unit)
│   ├── LoadResult (Unit)
│   ├── Requirement (Unit)
│   ├── Requirements (Unit)
│   ├── RequirementsLoader (Unit)
│   └── Section (Unit)
├── Tracing (Subsystem)
│   └── TraceMatrix (Unit)
└── SelfTest (Subsystem)
    └── Validation (Unit)
```

Each unit is described in detail in its own chapter within this document.

## Folder Layout

The design documents are organized into subsystem subdirectories that mirror the top-level subsystem
breakdown above:

```text
docs/design/
├── introduction.md                         — document introduction and architecture overview
└── reqstream/
    ├── reqstream.md                        — system integration design
    ├── program.md                          — Program unit design
    ├── cli/
    │   ├── cli.md                          — Cli subsystem design
    │   └── context.md                      — Context unit design
    ├── modeling/
    │   ├── modeling.md                     — Modeling subsystem design
    │   ├── lint-issue.md                   — LintIssue unit design
    │   ├── load-result.md                  — LoadResult unit design
    │   ├── requirement.md                  — Requirement unit design
    │   ├── requirements-loader.md          — RequirementsLoader unit design
    │   ├── requirements.md                 — Requirements unit design
    │   └── section.md                      — Section unit design
    ├── tracing/
    │   ├── tracing.md                      — Tracing subsystem design
    │   └── trace-matrix.md                 — TraceMatrix unit design
    └── self-test/
        ├── self-test.md                    — SelfTest subsystem design
        └── validation.md                   — Validation unit design
```

The source code folder structure mirrors the top-level subsystem breakdown above, giving
reviewers an explicit navigation aid from design to code:

```text
src/DemaConsulting.ReqStream/
├── Program.cs                  — entry point and execution orchestrator
├── Cli/
│   └── Context.cs              — command-line argument parser and I/O owner
├── Modeling/
│   ├── LintIssue.cs            — lint issue severity and data model
│   ├── LoadResult.cs           — combined result of loading requirements and associated lint issues
│   ├── Requirement.cs          — single requirement with ID, title, and test links
│   ├── Requirements.cs         — parsed requirements document with section tree
│   ├── RequirementsLoader.cs   — YAML deserializer and lint validator for requirements files
│   └── Section.cs              — named group of requirements within a document
├── Tracing/
│   └── TraceMatrix.cs          — test result loader and requirement-coverage analyzer
└── SelfTest/
    └── Validation.cs           — self-validation test runner
```

The test project mirrors the same layout under `test/DemaConsulting.ReqStream.Tests/`.

## Document Conventions

Throughout this document:

- Class names, method names, property names, and file names appear in `monospace` font.
- The word **shall** denotes a design constraint that the implementation must satisfy.
- Section headings within each unit chapter follow a consistent structure: overview, data model,
  methods/algorithms, and interactions with other units.
- Text tables are used in preference to diagrams, which may not render in all PDF viewers.

## Companion Artifact Structure

Each software item in the structure above has corresponding artifacts in parallel directory trees,
enabling reviewers and auditors to navigate from any one artifact to all related files:

```text
Each software item has parallel artifacts organized as follows:
- Requirements: docs/reqstream/reqstream/.../{item}.yaml  (kebab-case)
- Design docs:  docs/design/reqstream/.../{item}.md        (kebab-case)
- Verification: docs/verification/reqstream/.../{item}.md  (kebab-case)
- Source code:  src/DemaConsulting.ReqStream/.../{Item}.cs (PascalCase)
- Tests:        test/DemaConsulting.ReqStream.Tests/.../{Item}Tests.cs (PascalCase)
- Review-sets:  defined in .reviewmark.yaml
```

For example, the `Requirements` unit maps to:

| Artifact | Path |
| -------- | ---- |
| Requirements | `docs/reqstream/reqstream/modeling/requirements.yaml` |
| Design | `docs/design/reqstream/modeling/requirements.md` |
| Verification | `docs/verification/reqstream/modeling/requirements.md` |
| Source | `src/DemaConsulting.ReqStream/Modeling/Requirements.cs` |
| Tests | `test/DemaConsulting.ReqStream.Tests/Modeling/ModelingTests.cs` |
