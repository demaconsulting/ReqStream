# Modeling Subsystem Design

The `Modeling` subsystem provides the data model and YAML parsing for ReqStream requirements
documents. It is responsible for reading, validating, and structuring requirement data for use
by the tracing, reporting, and enforcement subsystems.

## Overview

The `Modeling` subsystem handles all YAML file parsing and requirement data structures. It
reads one or more requirement YAML files (including those referenced via `includes`), merges
them into a unified requirement tree, and exposes that tree to the rest of the tool.

## Units

The `Modeling` subsystem contains the following software units:

| Unit                 | File                             | Responsibility                                                         |
|----------------------|----------------------------------|------------------------------------------------------------------------|
| `LintIssue`          | `Modeling/LintIssue.cs`          | Lint issue with severity, file location, and description.              |
| `LoadResult`         | `Modeling/LoadResult.cs`         | Combined result of loading requirements and associated lint issues.     |
| `Requirement`        | `Modeling/Requirement.cs`        | Single requirement with ID, title, tags, and test links.               |
| `Requirements`       | `Modeling/Requirements.cs`       | YAML parsing, section merging, and requirements document.              |
| `RequirementsLoader` | `Modeling/RequirementsLoader.cs` | YAML deserializer and lint validator for individual requirements files. |
| `Section`            | `Modeling/Section.cs`            | Named group of requirements within a requirements document.            |

## Interfaces

The `Modeling` subsystem exposes the following interface to the rest of the tool:

| Interface                          | Direction | Description                                                         |
|------------------------------------|-----------|---------------------------------------------------------------------|
| `Requirements.Load`                | Outbound  | Reads and merges YAML requirement files into a requirement tree.    |
| `Requirements.Export`              | Outbound  | Exports requirements to a Markdown report.                          |
| `Requirements.ExportJustifications`| Outbound  | Exports requirement justifications to a Markdown report.            |
| `LoadResult.ReportIssues`          | Outbound  | Reports lint issues discovered during loading via the context.      |
| `RequirementsLoader.Load`          | Outbound  | Deserializes a single requirements file and collects lint issues.   |

## Interactions

| Dependency                         | Direction | Purpose                                                             |
|------------------------------------|-----------|---------------------------------------------------------------------|
| `Context`                          | Uses      | Receives file paths from `Context.RequirementsFiles`.               |
| `TraceMatrix`                      | Used by   | Receives the requirement tree to map test results to requirements.  |
| `Program`                          | Used by   | Calls `Requirements.Load` to load requirements.                     |

## Operation

A call to `Requirements.Load(paths)` follows this sequence:

1. **Initialization** — `RequirementsLoader.Load` allocates shared state: an empty `Requirements`
   tree, a `seenIds` dictionary (requirement ID → first-seen location), an `allRequirements`
   dictionary (ID → `Requirement` object), and a `visitedFiles` set of fully-resolved paths.

2. **Per-file DOM walk** — for each path in `paths`, `LoadFile` is called:
   1. Resolve the path to its canonical full path with `Path.GetFullPath`.
   2. Check `visitedFiles`; skip the file if it has already been processed (include-loop guard).
   3. Verify the file exists; emit an Error lint issue and return if not.
   4. Read the file text; emit an Error lint issue and return on I/O failure.
   5. Parse the text into a YAML DOM tree using YamlDotNet; emit an Error lint issue (with
      line and column from `YamlException`) and return on malformed YAML.
   6. Treat an empty document or a null-value root scalar as an empty file (no issues).
   7. Require the root node to be a `YamlMappingNode`; emit an Error lint issue and return
      if it is not.
   8. Call `LoadDocument`, which:
      - Reports every key at document root that is not in `{sections, mappings, includes}` as an
        Error lint issue.
      - Calls `LoadDocumentSections` to walk the `sections` sequence and merge sections into the
        `Requirements` tree.
      - Calls `LoadDocumentMappings` to walk the `mappings` sequence and apply supplementary test
        references to already-loaded requirements.
   9. Resolve include paths relative to the directory of the current file, then call `LoadFile`
      recursively for each entry in the `includes` list.

3. **Section merging** — when `LoadSection` encounters a section whose title already exists under
   the same parent, it reuses the existing `Section` object instead of creating a new one. This
   allows multiple files to contribute requirements to the same section hierarchy.

4. **Post-load cycle check** — after all files have been processed, `ValidateCycles` performs a
   DFS over the `allRequirements` graph to detect circular `children` references and unknown child
   IDs. Issues are always reported even when other errors exist.

5. **Result assembly** — if any Error-level lint issue is present, `Requirements` is set to `null`
   in the returned `LoadResult`; otherwise it contains the populated `Requirements` tree.

## Lint Check Categories

`RequirementsLoader` performs structural validation during loading. For the complete list of
lint check categories and checked conditions, see the
[RequirementsLoader Unit Design](./requirements-loader.md#lint-check-categories).

No Warning-level issues are emitted by the current implementation; all detected conditions are
Error-level and cause `LoadResult.Requirements` to be `null`.

## Error Handling

### Severity Classification

All lint issues emitted by the Modeling subsystem carry `LintSeverity.Error`. There are no
Warning-level conditions in the current implementation. Any single Error causes `LoadResult`
to return `null` for `Requirements`.

### Include Loop Guard

`LoadFile` resolves each file path to its canonical full path before processing and records it
in `visitedFiles`. If the same canonical path is encountered again (via a direct or transitive
include), `LoadFile` silently skips it. This prevents infinite recursion from cyclic include
directives without emitting a lint issue.

### LoadResult Contract

`LoadResult` always contains the full `Issues` list regardless of severity. The `Requirements`
property is:

- **Non-null** when `Issues` contains no Error-level entries — the caller may use the requirements
  tree for tracing, reporting, and enforcement.
- **Null** when any Error-level entry is present — the caller must not attempt to use the
  requirements tree.

The `HasErrors` property provides a convenience shortcut for `Issues.Any(i => i.Severity == Error)`.

## References

- [ReqStream System Design][arch]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[repo]: https://github.com/demaconsulting/ReqStream
