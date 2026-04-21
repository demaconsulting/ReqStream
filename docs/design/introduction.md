# Introduction

This document provides the detailed design for the ReqStream tool, a .NET command-line application
for managing software requirements in YAML format.

## Purpose

The purpose of this document is to describe the internal design of each software unit that comprises
ReqStream. It captures data models, algorithms, key methods, and inter-unit interactions at a level
of detail sufficient for formal code review, compliance verification, and future maintenance. The
document does not restate requirements; it explains how they are realized.

## Scope

This document covers the detailed design of the following software units:

- **Program** — entry point and execution orchestrator (`Program.cs`)
- **Context** — command-line argument parser and I/O owner (`Cli/Context.cs`)
- **Validation** — self-validation test runner (`SelfTest/Validation.cs`)
- **Requirements, Section, and Requirement** — YAML parsing, section merging, validation, and export
  (`Modeling/Requirements.cs`, `Modeling/Section.cs`, `Modeling/Requirement.cs`)
- **TraceMatrix** — test result loader and requirement-coverage analyzer (`Tracing/TraceMatrix.cs`)

The following topics are out of scope:

- External library internals (YamlDotNet, DemaConsulting.TestResults)
- Build pipeline configuration
- Deployment and packaging

## Software Structure

The following tree shows how the ReqStream software items are organized across the system,
subsystem, and unit levels:

```text
ReqStream (System)
├── Program (Unit)
├── Cli (Subsystem)
│   └── Context (Unit)
├── Modeling (Subsystem)
│   ├── Requirements (Unit)
│   ├── Section (Unit)
│   └── Requirement (Unit)
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
    │   └── requirements.md                 — Requirements, Section, and Requirement units design
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
│   ├── Requirement.cs          — single requirement with ID, title, and test links
│   ├── Requirements.cs         — parsed requirements document with section tree
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

## References

- [ReqStream System Design][arch]
- [ReqStream Repository][repo]

[arch]: reqstream/reqstream.md
[repo]: https://github.com/demaconsulting/ReqStream
