# Introduction

This document describes the verification design for ReqStream, a .NET command-line
application that processes YAML requirements files, traces test results to requirements,
generates Markdown reports, and enforces coverage in CI/CD pipelines. It establishes the
test approach for each software item and proves that every requirement is covered by at
least one named test scenario.

## Purpose

The purpose of this document is to prove that all requirements for the ReqStream system
are covered by named test scenarios. Each requirement at every level — system, subsystem,
and unit — is mapped to at least one test method so that reviewers can confirm completeness
without reading implementation code.

## Scope

This document covers verification of the following software units:

- **Program** — entry point and execution orchestrator (`Program.cs`)
- **Cli** subsystem:
  - **Context** unit — command-line argument parser and I/O owner (`Cli/Context.cs`)
- **Modeling** subsystem:
  - **LintIssue** unit — lint severity classification and issue data model (`Modeling/LintIssue.cs`)
  - **LoadResult** unit — combined result of loading requirements and associated lint issues (`Modeling/LoadResult.cs`)
  - **Requirement** unit — single requirement with ID, title, and test links (`Modeling/Requirement.cs`)
  - **Requirements** unit — parsed requirements document with section tree (`Modeling/Requirements.cs`)
  - **RequirementsLoader** unit — YAML deserializer and lint validator (`Modeling/RequirementsLoader.cs`)
  - **Section** unit — named group of requirements within a document (`Modeling/Section.cs`)
- **Tracing** subsystem:
  - **TraceMatrix** unit — test result loader and requirement-coverage analyzer (`Tracing/TraceMatrix.cs`)
- **SelfTest** subsystem:
  - **Validation** unit — self-validation test runner (`SelfTest/Validation.cs`)

The following eleven OTS items are also verified:

- **BuildMark** — build-notes documentation generator
- **FileAssert** — document assertion tool
- **xUnit** — unit testing framework
- **Pandoc** — Markdown to HTML converter
- **ReviewMark** — file review tracking tool
- **SarifMark** — SARIF report processor
- **SonarMark** — SonarCloud quality reporter
- **VersionMark** — version tracking tool
- **WeasyPrint** — HTML to PDF converter
- **YamlDotNet** — YAML parsing library
- **DemaConsulting.TestResults** — test result file reader

The following topics are out of scope:

- External library internals
- Build pipeline configuration
- Deployment and packaging

## Companion Artifacts

In-house software items have parallel artifacts organized as follows:

- **Requirements**: `docs/reqstream/reqstream/.../{item}.yaml` (kebab-case)
- **Design**: `docs/design/reqstream/.../{item}.md` (kebab-case)
- **Verification**: `docs/verification/reqstream/.../{item}.md` (kebab-case, this document)
- **Source**: `src/DemaConsulting.ReqStream/.../{Item}.cs` (PascalCase)
- **Tests**: `test/DemaConsulting.ReqStream.Tests/.../{Item}Tests.cs` (PascalCase)

OTS software items have no design documentation. Their artifacts are:

- **Requirements**: `docs/reqstream/ots/{ots-name}.yaml`
- **Verification**: `docs/verification/ots/{ots-name}.md`

Review-sets for all items are defined in `.reviewmark.yaml` at the repository root.

## References

- ReqStream System Requirements
- ReqStream Software Design Document
- ReqStream User Guide
