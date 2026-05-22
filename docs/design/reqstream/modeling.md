## Modeling

### Overview

The `Modeling` subsystem provides the data model and YAML parsing for ReqStream requirements
documents. It handles all YAML file parsing and requirement data structures, reading one or more
requirement YAML files (including those referenced via `includes`), merging them into a unified
requirement tree, and exposing that tree to the rest of the tool. Its boundaries extend from raw
YAML file I/O through to the populated in-memory requirement tree; it has no knowledge of test
results, tracing, or report file formats.

The `Modeling` subsystem contains the following software units:

- **LintIssue** (`Modeling/LintIssue.cs`) — Lint issue with severity, file location, and
  description.
- **LoadResult** (`Modeling/LoadResult.cs`) — Combined result of loading requirements and
  associated lint issues.
- **Requirement** (`Modeling/Requirement.cs`) — Single requirement with ID, title, tags, and
  test links.
- **Requirements** (`Modeling/Requirements.cs`) — YAML parsing, section merging, and requirements
  document root.
- **RequirementsLoader** (`Modeling/RequirementsLoader.cs`) — YAML deserializer and lint validator
  for individual requirements files.
- **Section** (`Modeling/Section.cs`) — Named group of requirements within a requirements
  document.

### Interfaces

**Requirements.Load**: Reads and merges YAML requirement files into a requirement tree.

- *Type*: In-process .NET public API (static factory).
- *Role*: Provider.
- *Contract*: Accepts file paths; returns `LoadResult` containing the tree (or `null` on error)
  and the lint issue list.
- *Constraints*: Throws `ArgumentException` when no paths are provided.

**Requirements.Export**: Exports the requirement tree to Markdown.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Accepts `filePath`, `depth`, and optional `filterTags`; writes a Markdown report.
- *Constraints*: Throws `ArgumentException` for null/empty path; propagates I/O exceptions.

**Requirements.ExportJustifications**: Exports justifications to Markdown.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Same as `Export` but includes justification text per requirement.
- *Constraints*: Same as `Export`.

**LoadResult.ReportIssues**: Reports lint issues via the context output channels.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Routes warnings to `context.WriteLine` and errors to `context.WriteError`.
- *Constraints*: None.

### Design

The `Modeling` subsystem units collaborate in a directed chain during a `Requirements.Load` call:

1. `Requirements.Load` delegates immediately to `RequirementsLoader.Load`, passing the file paths.
2. `RequirementsLoader` creates `Section`, `Requirement`, and `LintIssue` objects as it walks
   the YAML DOM. It writes into the shared `Requirements` tree directly (title-based section
   merging across files).
3. After all files are processed, `RequirementsLoader` runs `ValidateCycles` to detect circular
   child references. Any issues are appended to the `LintIssue` list.
4. `RequirementsLoader.Load` assembles and returns a `LoadResult` containing the (possibly null)
   `Requirements` tree and the complete `LintIssue` list.
5. `Requirements.Load` returns the `LoadResult` to the caller (`Program`).

For export paths, `Requirements.Export` and `Requirements.ExportJustifications` walk the
`Section` tree recursively, emitting Markdown at the caller-specified heading depth. Neither
method calls back into `RequirementsLoader`; they read only the already-populated tree.

All lint issues carry `LintSeverity.Error`. Any single error causes `LoadResult` to return
`null` for `Requirements`.
