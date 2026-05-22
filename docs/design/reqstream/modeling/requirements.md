### Requirements

#### Purpose

`Requirements` is the root of the requirements section tree and the public API entry point for
the Modeling subsystem. It extends `Section` to inherit the container properties (title,
requirements list, child sections list) and adds the `Load` static factory method and the
`Export`/`ExportJustifications` report-generation methods. `Requirements` has no knowledge of
YAML parsing or lint validation; those responsibilities belong entirely to `RequirementsLoader`.

#### Data Model

`Requirements` extends `Section` and inherits its container properties (`Title`, `Requirements`,
`Sections`). It adds no additional instance state; its role is to provide the public API surface
(static `Load` factory and the two export methods) on top of the `Section` tree root.

#### Key Methods

**Load(paths)**: Static factory method that loads and merges YAML requirement files.

- *Parameters*: `IEnumerable<string> paths` — file paths to load.
- *Returns*: `LoadResult` — contains the populated tree (or `null` on error) and lint issues.
- *Preconditions*: At least one path must be provided (throws `ArgumentException` otherwise).
- *Postconditions*: All files are processed; the returned `LoadResult` is consistent.

Delegates to `RequirementsLoader.Load` internally.

**Export(filePath, depth, filterTags)**: Exports the requirement tree to a Markdown report.

- *Parameters*: `string filePath` — output path; `int depth` — starting heading level;
  `HashSet<string>? filterTags` — optional tag filter.
- *Returns*: `void`.
- *Preconditions*: `filePath` must not be null or empty.
- *Postconditions*: Markdown file written with one heading per section and a table per section's
  requirements.

Walks the section tree recursively, emitting headings at the configured depth. When `filterTags`
is non-null, only requirements whose `Tags` list contains at least one matching tag are included.

**ExportJustifications(filePath, depth, filterTags)**: Exports justifications to Markdown.

- *Parameters*: Same as `Export`.
- *Returns*: `void`.
- *Preconditions*: Same as `Export`.
- *Postconditions*: Each requirement produces a sub-heading with its ID and bold title;
  justification text is included only when non-null and non-empty.

#### Error Handling

- `Load` throws `ArgumentException` when no paths are provided.
- `Export` and `ExportJustifications` throw `ArgumentException` when `filePath` is null or empty.
- Both export methods propagate `IOException` and `UnauthorizedAccessException` from file-write
  operations without wrapping; callers are responsible for handling file-write failures.

#### Dependencies

- **Section** — `Requirements` extends `Section` and inherits its container properties.
- **RequirementsLoader** — delegated to by `Requirements.Load` to perform YAML parsing and
  validation.
- **LoadResult** — returned by `Requirements.Load`; holds the populated tree and the lint issue
  list.

#### Callers

- **Program** — calls `Requirements.Load` to build the requirement tree; calls `Export` and
  `ExportJustifications`.
- **TraceMatrix** — receives the populated `Requirements` root from `Program` and iterates the
  section tree.
- **Validation** — exercises `Requirements.Load` with fixture YAML files during self-validation
  tests.
