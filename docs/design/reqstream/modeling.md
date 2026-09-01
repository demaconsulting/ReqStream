## Modeling

![Modeling Structure](ModelingView.svg)

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
- **Requirements** (`Modeling/Requirements.cs`) — YAML parsing, section merging, requirements
  document root, root-tag storage, and orphan detection.
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

**Requirements.FindOrphans**: Identifies requirements unreachable from any root-tagged requirement.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Accepts the merged root-tag set (`IReadOnlySet<string>`); flattens the tree, seeds a
  visited-set with every requirement whose `Tags` intersects the root-tag set, then performs a
  breadth-first flood-fill via each requirement's `Children` list. Returns an `OrphanResult`
  containing the ordered list of unreached requirement IDs and the total requirement count.
- *Constraints*: Returns an empty `OrphanResult` (no-op) when `rootTags` is empty. Operates on
  the full tree, independent of any `--filter` narrowing applied elsewhere. Runs in `O(V+E)` via
  a visited-set, correctly handling DAG requirement graphs with multiple parents per child.

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

For orphan detection, `Program` computes the merged root-tag set (YAML-declared `Requirements
.RootTags` combined with any CLI `--root-tags` values) and, when non-empty, calls
`Requirements.FindOrphans` on the fully-loaded, unfiltered tree. `FindOrphans` flattens the tree
once, seeds the BFS frontier with every requirement whose `Tags` intersects the root-tag set, and
flood-fills outward via `Children`, so a requirement that is itself root-tagged is never orphaned
regardless of its children or test coverage. This check has no dependency on the trace matrix or
`--tests`, and is unaffected by `--filter`.

For export paths, `Requirements.Export` and `Requirements.ExportJustifications` walk the
`Section` tree recursively, emitting Markdown at the caller-specified heading depth. Neither
method calls back into `RequirementsLoader`; they read only the already-populated tree.

> **Test dependency note**: The test fixture for the Modeling subsystem
> (`ModelingTests.cs`) uses `Utilities.PathHelpers.SafePathCombine` to construct temporary
> file paths in a safe, cross-platform manner. Access is enabled by the
> `[assembly: InternalsVisibleTo("DemaConsulting.ReqStream.Tests")]` attribute declared in the
> main assembly. This dependency on `Utilities.PathHelpers` is confined to tests and does not
> affect the production `Modeling` subsystem boundary.

Lint issues may carry either `LintSeverity.Warning` (non-fatal; reported but loading continues)
or `LintSeverity.Error` (fatal; `LoadResult.Requirements` is `null` when at least one error-level
issue is present).

The table below maps each specific structural defect to its assigned severity. All defects
currently detected by `RequirementsLoader` use `Error` severity; no conditions currently produce
`Warning` severity (the severity level exists to accommodate future non-fatal checks).

| Lint Condition | Severity |
| --- | --- |
| Invalid file path (path resolution failure) | `Error` |
| Circular include detected | `Error` |
| File not found | `Error` |
| Failed to read file (I/O error) | `Error` |
| Malformed YAML (parse exception) | `Error` |
| Document root is not a mapping | `Error` |
| Unknown field at document root | `Error` |
| Section is not a mapping node | `Error` |
| Section missing required field `title` | `Error` |
| Section `title` is blank | `Error` |
| Unknown field in section | `Error` |
| Requirement is not a mapping node | `Error` |
| Requirement missing required field `id` | `Error` |
| Requirement `id` is blank | `Error` |
| Duplicate requirement ID | `Error` |
| Requirement missing required field `title` | `Error` |
| Requirement `title` is blank | `Error` |
| Unknown field in requirement | `Error` |
| Invalid `includes` path | `Error` |
| Non-scalar entry in `includes`, `tests`, `children`, or `tags` sequence | `Error` |
| Blank entry in `includes`, `tests`, `children`, or `tags` sequence | `Error` |
| Non-scalar entry in `root-tags` sequence | `Error` |
| Blank entry in `root-tags` sequence | `Error` |
| Field expected to be a sequence but is not | `Error` |
| Mapping missing required field `id` | `Error` |
| Mapping `id` is blank | `Error` |
| Unknown field in mapping | `Error` |
| Non-scalar test entry in mapping | `Error` |
| Requirement references unknown child ID | `Error` |
| Circular requirement child reference | `Error` |
