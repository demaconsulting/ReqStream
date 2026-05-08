## Modeling Subsystem Design

The `Modeling` subsystem provides the data model and YAML parsing for ReqStream requirements
documents. It is responsible for reading, validating, and structuring requirement data for use
by the tracing, reporting, and enforcement subsystems.

### Overview

The `Modeling` subsystem handles all YAML file parsing and requirement data structures. It
reads one or more requirement YAML files (including those referenced via `includes`), merges
them into a unified requirement tree, and exposes that tree to the rest of the tool.

### Units

The `Modeling` subsystem contains the following software units:

- **`LintIssue`** (`Modeling/LintIssue.cs`) — Lint issue with severity, file location, and description.
- **`LoadResult`** (`Modeling/LoadResult.cs`) — Combined result of loading requirements and associated lint issues.
- **`Requirement`** (`Modeling/Requirement.cs`) — Single requirement with ID, title, tags, and test links.
- **`Requirements`** (`Modeling/Requirements.cs`) — YAML parsing, section merging, and requirements document.
- **`RequirementsLoader`** (`Modeling/RequirementsLoader.cs`) — YAML deserializer and lint validator
  for individual requirements files.
- **`Section`** (`Modeling/Section.cs`) — Named group of requirements within a requirements document.

### Interfaces

The `Modeling` subsystem exposes the following interface to the rest of the tool:

| Interface | Description |
| --- | --- |
| `Requirements.Load` | Reads and merges YAML requirement files into a requirement tree. |
| `Requirements.Export` | Exports to Markdown; `depth` sets header level (default 1); `filterTags` restricts by tag. |
| `Requirements.ExportJustifications` | Exports justifications to Markdown. Supports `depth` and `filterTags`. |
| `LoadResult.ReportIssues` | Reports lint issues discovered during loading via the context. |

### Interactions

| Dependency                         | Direction | Purpose                                                             |
|------------------------------------|-----------|---------------------------------------------------------------------|
| `Cli (Context)`                    | Uses      | `LoadResult.ReportIssues` accepts a `Context` to route issues.      |
| `TraceMatrix`                      | Used by   | Receives the requirement tree to map test results to requirements.  |
| `Program`                          | Used by   | Calls `Requirements.Load` to load requirements.                     |

### Operation

A call to `Requirements.Load(paths)` follows this sequence:

1. **Initialization** — `RequirementsLoader.Load` allocates shared state for the loading pass.
2. **Per-file processing** — `RequirementsLoader` resolves each path, parses the YAML, validates
   structure, merges sections into the shared `Requirements` tree, and follows `includes`
   directives recursively. See the RequirementsLoader unit design documentation for the full algorithm.
3. **Post-load cycle check** — after all files are processed, a depth-first search over the
   requirements graph detects circular `children` references and unknown child IDs.
4. **Result assembly** — if any Error-level lint issue is present, `Requirements` is set to `null`
   in the returned `LoadResult`; otherwise it contains the populated `Requirements` tree.

### Lint Check Categories

`RequirementsLoader` performs structural validation during loading. For the complete list of
lint check categories and checked conditions, see the RequirementsLoader unit design documentation.

No Warning-level issues are emitted by the current implementation; all detected conditions are
Error-level and cause `LoadResult.Requirements` to be `null`.

### Error Handling

#### Severity Classification

All lint issues emitted by the Modeling subsystem carry `LintSeverity.Error`. There are no
Warning-level conditions in the current implementation. Any single Error causes `LoadResult`
to return `null` for `Requirements`.

#### Include Loop Guard and LoadResult Contract

For the include-loop deduplication strategy, see the RequirementsLoader unit design documentation.
For the `LoadResult` null-on-error contract, see the LoadResult unit design documentation.
