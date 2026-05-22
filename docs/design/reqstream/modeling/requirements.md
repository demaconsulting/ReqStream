### Requirements Unit Design

#### Purpose

`Requirements` is the root of the requirements section tree and the public API entry point for
the Modeling subsystem. It extends `Section` to inherit the container properties (title,
requirements list, child sections list) and adds the `Load` static factory method and the
`Export`/`ExportJustifications` report-generation methods.

`Requirements` has no knowledge of YAML parsing or lint validation; those responsibilities belong
entirely to `RequirementsLoader`. Its role is to provide the public surface through which callers
load and export requirements data.

#### Data Model

`Requirements` extends `Section` and inherits its container properties (`Title`, `Requirements`,
`Sections`). It adds no additional instance state; its role is to provide the public API surface
(static `Load` factory and the two export methods) on top of the `Section` tree root.

#### Key Methods

##### `Load(paths)` — Factory Method

`Load` is the single static factory method. It accepts one or more file paths, delegates to
`RequirementsLoader.Load`, and returns the resulting `LoadResult` containing the populated
`Requirements` tree (or `null` on error) and the complete list of `LintIssue` objects.
Throws `ArgumentException` when no paths are provided.

Callers that need to abort on errors check `result.HasErrors` or `result.Requirements == null`.
Callers that need to surface issues to the user call `result.ReportIssues(context)`.

##### Export Methods

| Method | Output | Notes |
| ------ | ------ | ----- |
| `Export(filePath, depth, filterTags)` | Requirements Markdown report | Recursive; applies `filterTags` |
| `ExportJustifications(filePath, depth, filterTags)` | Justifications Markdown report | Recursive with tag filtering |

Both methods walk the section tree recursively, emitting Markdown headings at the configured
`depth` and a requirements table for each section. When `filterTags` is non-`null`, only
requirements whose `Tags` list contains at least one matching tag are included in the output.

**Error handling**: Both `Export` and `ExportJustifications` throw `ArgumentException` when
`filePath` is `null` or empty. Both methods propagate any `IOException` or
`UnauthorizedAccessException` thrown by the underlying file-write operations to the caller
without wrapping. Callers are responsible for handling file-write failures; the methods do not
catch or suppress I/O exceptions.

**Export output format**:

- Each `Section` produces a Markdown heading (`#` through `######` depending on `depth`) with
  the section title.
- Each `Section` produces a Markdown table with columns `ID` and `Title` for its requirements.

**ExportJustifications output format**:

- Each `Section` produces a Markdown heading at the configured depth.
- Each requirement produces a sub-heading with its ID and bold title; justification text is
  included only when `Justification` is non-null and non-empty.

#### Error Handling

- `Load` throws `ArgumentException` when no paths are provided.
- `Export` and `ExportJustifications` throw `ArgumentException` when `filePath` is null or
  empty. Both methods propagate `IOException` and `UnauthorizedAccessException` from file-write
  operations without wrapping; callers are responsible for handling file-write failures.
- Neither export method catches or suppresses exceptions; all failure signalling propagates to
  the caller (`Program`).

#### Interactions

**Dependencies**:

| Unit | Purpose |
| ---- | ------- |
| `Section` | `Requirements` extends `Section` and inherits its container properties (`Title`, `Requirements`, `Sections`) |
| `RequirementsLoader` | Delegated to by `Requirements.Load` to perform YAML parsing and validation |
| `LoadResult` | Returned by `Requirements.Load`; holds the populated tree and the lint issue list |

**Callers**:

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Program` | Calls `Requirements.Load` to build the requirement tree; calls `Export` and `ExportJustifications` |
| `TraceMatrix` | Receives the populated `Requirements` root from `Program` and iterates the section tree |
| `Validation` | Exercises `Requirements.Load` with fixture YAML files during self-validation tests |
