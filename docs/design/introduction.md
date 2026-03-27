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
- **Context** — command-line argument parser and I/O owner (`Context.cs`)
- **Validation** — self-validation test runner (`Validation.cs`)
- **Requirements, Section, and Requirement** — YAML parsing, section merging, validation, and export
  (`Requirements.cs`, `Section.cs`, `Requirement.cs`)
- **TraceMatrix** — test result loader and requirement-coverage analyzer (`TraceMatrix.cs`)
- **Linter** — structural linter for requirement YAML files (`Linter.cs`)

The following topics are out of scope:

- External library internals (YamlDotNet, DemaConsulting.TestResults)
- Build pipeline configuration
- Deployment and packaging

## Software Structure

The following tree shows how the ReqStream software items are organized across the system,
subsystem, and unit levels:

```text
ReqStream (System)
├── Program Orchestration (Subsystem)
│   ├── Command-Line Interface (Subsystem)
│   │   └── Context (Unit)
│   └── Program (Unit)
├── Requirements (Subsystem)
│   ├── Requirements File Processing (Subsystem)
│   │   ├── Requirements (Unit)
│   │   ├── Section (Unit)
│   │   └── Requirement (Unit)
│   ├── Test Integration (Subsystem)
│   │   └── TraceMatrix (Unit)
│   └── Linting (Subsystem)
│       └── Linter (Unit)
└── Validation (Subsystem)
    └── Validation (Unit)
```

Each unit is described in detail in its own chapter within this document.

## Document Conventions

Throughout this document:

- Class names, method names, property names, and file names appear in `monospace` font.
- The word **shall** denotes a design constraint that the implementation must satisfy.
- Section headings within each unit chapter follow a consistent structure: overview, data model,
  methods/algorithms, and interactions with other units.
- Text tables are used in preference to diagrams, which may not render in all PDF viewers.

## References

- [ReqStream Architecture][arch]
- [ReqStream User Guide][guide]
- [ReqStream Repository][repo]

[arch]: ../../ARCHITECTURE.md
[guide]: ../../README.md
[repo]: https://github.com/demaconsulting/ReqStream
