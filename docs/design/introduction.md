# Introduction

This document provides the detailed design for the ReqStream tool, a .NET command-line application
for managing software requirements in YAML format. It covers full architectural and detailed design
for local items (the ReqStream system, its subsystems, and units), and integration/usage design for
the OTS software items consumed by the system.

## Purpose

This document defines the design for each software item in ReqStream — full architectural and
detailed design for local items (systems, subsystems, and units), and integration/usage design for
OTS software items. A reviewer should be able to understand how each item satisfies its requirements
without reading source code.

## Scope

This document covers the detailed design of the following software items:

Local items:

- **ReqStream**: system, subsystem, and unit design.

OTS items:

- **YamlDotNet**: integration and usage design.
- **Microsoft.Extensions.FileSystemGlobbing**: integration and usage design.
- **DemaConsulting.TestResults**: integration and usage design.

The following topics are out of scope:

- Internal design of OTS items (YamlDotNet, Microsoft.Extensions.FileSystemGlobbing,
  DemaConsulting.TestResults)
- Build pipeline configuration
- Deployment and packaging
- Test projects, test classes, and test infrastructure

## Software Structure

```text
ReqStream (System)
├── Program (Unit)
├── Cli (Subsystem)
│   └── Context (Unit)
├── Utilities (Subsystem)
│   ├── GlobMatcher (Unit)
│   └── PathHelpers (Unit)
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

OTS Dependencies:
├── YamlDotNet (OTS)
├── Microsoft.Extensions.FileSystemGlobbing (OTS)
└── DemaConsulting.TestResults (OTS)
```

## Folder Layout

```text
src/DemaConsulting.ReqStream/
├── Program.cs                  — entry point and execution orchestrator
├── Cli/
│   └── Context.cs              — command-line argument parser and I/O owner
├── Utilities/
│   ├── GlobMatcher.cs          — glob-pattern file matching utility
│   └── PathHelpers.cs          — safe path combination with traversal protection
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

test/DemaConsulting.ReqStream.Tests/
├── CliTests.cs                 — CLI subsystem integration tests
├── Modeling/
│   └── ModelingTests.cs        — Modeling subsystem integration tests
├── Tracing/
│   └── TracingTests.cs         — Tracing subsystem integration tests
└── Utilities/
    └── UtilitiesTests.cs       — Utilities subsystem integration tests
```

## Companion Artifact Structure

Each local software item has corresponding artifacts in parallel directory trees:

- Requirements: `docs/reqstream/reqstream.yaml`, `docs/reqstream/reqstream/.../{item}.yaml`
- Design: `docs/design/reqstream.md`, `docs/design/reqstream/.../{item}.md`
- Verification: `docs/verification/reqstream.md`, `docs/verification/reqstream/.../{item}.md`
- Source: `src/DemaConsulting.ReqStream/.../{Item}.cs`
- Tests: `test/DemaConsulting.ReqStream.Tests/.../{Item}Tests.cs`

OTS items have integration/usage design documentation parallel to system folders:

- Requirements: `docs/reqstream/ots/{ots-name}.yaml`
- Design: `docs/design/ots/{ots-name}.md`
- Verification: `docs/verification/ots/{ots-name}.md`

Review-sets: defined in `.reviewmark.yaml`

## References

- [ReqStream releases](https://github.com/demaconsulting/ReqStream/releases)
