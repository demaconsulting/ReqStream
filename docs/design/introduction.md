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
- Build pipeline configuration and the build/pipeline OTS tools that support it (for example
  SysML2Tools, Pandoc, WeasyPrint, BuildMark, VersionMark, ReviewMark, SarifMark, SonarMark,
  XUnit, and FileAssert). These pipeline tools are not modeled as SysML2 structural parts; each
  still has its own requirements, design, and verification companion artifacts under
  `docs/design/ots/`, `docs/reqstream/ots/`, and `docs/verification/ots/`, and is covered by its
  own ReviewMark OTS review-set.
- Deployment and packaging
- Test projects, test classes, and test infrastructure

## Software Structure

The software structure is modeled in SysML2 under `docs/sysml2/` and rendered to the
diagram below by SysML2Tools as part of the build pipeline. AI agents should query the
SysML2 model directly (see the `sysml2tools-query` skill) rather than parsing this
diagram or the prose below.

![Software Structure](SoftwareStructureView.svg)

## Folder Layout

```text
src/DemaConsulting.ReqStream/
├── Program.cs                  — entry point and execution orchestrator
├── Cli/
│   └── Context.cs              — command-line argument parser and I/O owner
├── Utilities/
│   ├── GlobMatcher.cs          — glob-pattern file matching utility
│   ├── PathHelpers.cs          — safe path combination with traversal protection
│   └── TemporaryDirectory.cs   — disposable temporary directory for isolated file-system workspaces
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
├── AssemblyInfo.cs             — test assembly configuration
├── IntegrationTests.cs         — system-level integration tests
├── Runner.cs                   — test runner infrastructure
├── CliTests.cs                 — CLI subsystem integration tests
├── Modeling/
│   └── ModelingTests.cs        — Modeling subsystem integration tests
├── Tracing/
│   └── TracingTests.cs         — Tracing subsystem integration tests
└── Utilities/
    ├── GlobMatcherTests.cs              — GlobMatcher unit tests
    ├── PathHelpersTests.cs              — PathHelpers unit tests
    └── TemporaryDirectoryTests.cs       — TemporaryDirectory unit tests
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

- [REF-1] ReqStream Releases, DEMA Consulting, <https://github.com/demaconsulting/ReqStream/releases>
