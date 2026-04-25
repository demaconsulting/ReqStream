# Requirements, Section, and Requirement Unit Design

## Overview

The classes `Requirements`, `Section`, `Requirement`, and `LoadResult` together form the domain
model for requirement data in ReqStream. They are responsible for loading YAML files, merging
hierarchical section trees, validating data integrity, preventing infinite include loops and circular
child references, applying test mappings, and exporting content to Markdown reports.

## Data Model

### `LintSeverity`

`LintSeverity` is an enum that classifies the severity of a lint issue.

| Member | Description |
| ------ | ----------- |
| `Warning` | A non-fatal issue; processing can continue. |
| `Error` | A fatal issue that prevents successful requirements loading. |

### `LintIssue`

`LintIssue` represents a single issue found during requirements linting or loading.

| Member | Type | Notes |
| ------ | ---- | ----- |
| `LintIssue(location, severity, description)` | Constructor | Initializes all three properties |
| `Location` | `string` | The source location (e.g. `"file.yaml"` or `"file.yaml(3,5)"`) |
| `Severity` | `LintSeverity` | The severity of the issue |
| `Description` | `string` | A human-readable description of the issue |
| `ToString()` | `string` | Returns the issue formatted as `"location: severity: description"` |

### `Requirement`

`Requirement` represents a single requirement node.

| Property | Type | YAML key | Notes |
| -------- | ---- | -------- | ----- |
| `Id` | `string` | `id` | Unique across all files; must not be blank |
| `Title` | `string` | `title` | Human-readable name; must not be blank |
| `Justification` | `string?` | `justification` | Optional rationale text |
| `Tests` | `List<string>` | `tests` | Test identifiers linked to this requirement |
| `Children` | `List<string>` | `children` | IDs of child requirements |
| `Tags` | `List<string>` | `tags` | Optional labels for filtering |

### `Section`

`Section` is a container node in the requirement hierarchy.

| Property | Type | YAML key | Notes |
| -------- | ---- | -------- | ----- |
| `Title` | `string` | `title` | Used to match and merge sections across files |
| `Requirements` | `List<Requirement>` | `requirements` | Requirements directly in this section |
| `Sections` | `List<Section>` | `sections` | Child sections |

### `Requirements`

`Requirements` extends `Section` and acts as the root of the tree.

### `LoadResult`

`LoadResult` encapsulates the outcome of a `Requirements.Load` call.

| Member | Type | Notes |
| ------ | ---- | ----- |
| `Requirements` | `Requirements?` | Parsed tree; `null` when error-level issues are present |
| `Issues` | `IReadOnlyList<LintIssue>` | All lint issues collected during loading |
| `HasErrors` | `bool` | `true` when any issue has `LintSeverity.Error` |
| `ReportIssues(context)` | `void` | Routes each issue to the context output |

`ReportIssues` accepts a `Context` argument. Warning-level issues are sent to `context.WriteLine`;
error-level issues are sent to `context.WriteError`.

## YAML DOM Traversal

YAML is parsed using `YamlDotNet`'s `RepresentationModel` (DOM) API. `RequirementsLoader` reads
the raw YAML text into a `YamlStream`, then walks the resulting node tree directly:

| DOM node type | Used for |
| ------------- | -------- |
| `YamlMappingNode` | Document root, section entries, requirement entries, mapping entries |
| `YamlSequenceNode` | `sections`, `requirements`, `mappings`, `tests`, `children`, `tags` arrays |
| `YamlScalarNode` | Individual field values (titles, IDs, test names, etc.) |

`RequirementsLoader` maintains static `HashSet<string>` sets of known field names for each
structural level (`KnownDocumentFields`, `KnownSectionFields`, `KnownRequirementFields`,
`KnownMappingFields`) to detect and report unknown fields. There are no intermediate C# model
classes; each DOM node is consumed directly and converted to the long-lived `Requirement`,
`Section`, and `Requirements` objects during the walk.

## Methods

### `Requirements.Load(paths)`

`Load` is the single static factory method on `Requirements`. It delegates to
`RequirementsLoader.Load` and returns a `LoadResult` containing:

- The parsed `Requirements` tree (or `null` if any error-level issues were found), and
- The complete list of `LintIssue` objects collected during the walk.

Callers that need to abort on errors check `result.HasErrors` or `result.Requirements == null`.
Callers that need to surface issues to the user call `result.ReportIssues(context)`.

### `LoadFile(path)`

`LoadFile` loads a single YAML file and merges its content into the `Requirements` tree. Four
design points govern its behavior:

- **Deduplication**: `path` is normalized to an absolute path and checked against `visitedFiles`
  before any work is done. If already present, the method returns immediately. This prevents
  infinite loops when files include each other directly or transitively.
- **YAML parsing**: the file text is parsed into a `YamlStream` using `YamlDotNet`'s
  `RepresentationModel` DOM API. An empty or `null` root node is silently accepted.
- **Validation and merging**: each section is validated (title must not be blank) and each
  requirement is validated (ID and title must not be blank; ID must not duplicate an entry already
  seen). Validated sections are merged into the tree inline: if `parent.Sections` already contains
  a section whose `Title` matches the incoming section title, the incoming requirements are appended
  to that existing section and child sections are recursively merged; if no match is found, a new
  `Section` is created and appended to `parent.Sections`. This same-title merge strategy is the key
  design decision that enables modular requirements management: multiple YAML files can contribute
  requirements to the same logical section without requiring a single monolithic file. Mapping
  entries append additional test IDs to already-registered requirements.
- **Recursive includes**: each path in the document's `includes` block is resolved relative to the
  current file's directory and passed to `LoadFile` recursively, enabling modular file organization.

### `ValidateCycles()`

`ValidateCycles` performs a depth-first search (DFS) over all requirements to detect circular child
references. It is called once after all files are loaded.

**Tracking structures**:

| Structure | Type | Purpose |
| --------- | ---- | ------- |
| `visiting` | `HashSet<string>` | IDs on the current DFS stack; a hit here indicates a cycle |
| `currentPath` | `List<string>` | Ordered IDs on the current stack; used to build the error message |
| `visited` | `HashSet<string>` | IDs whose entire sub-tree is confirmed cycle-free; skipped on future encounters |

**Algorithm** (applied via `ValidateCyclesFrom` for each unvisited requirement):

1. Add the current ID to `visiting` and `currentPath`.
2. For each child ID of the current requirement:
   a. If the child ID is not in `allRequirements`, report an error: unknown child reference.
   b. If the child ID is in `visiting`, a cycle is detected; add an error `LintIssue` with the
      cycle path formatted as `REQ-A -> REQ-B -> ... -> REQ-A`.
   c. If the child ID is not in `visited`, recurse into it.
3. Remove the current ID from `visiting` and `currentPath`; add it to `visited`.

Because `ValidateCycles` runs before any downstream analysis, `TraceMatrix.CollectAllTests` can
recurse through child requirements without its own cycle guard.

### Export Methods

| Method | Output | Notes |
| ------ | ------ | ----- |
| `Export(filePath, depth, filterTags)` | Requirements Markdown report | Recursive; applies `filterTags` |
| `ExportJustifications(filePath, depth, filterTags)` | Justifications Markdown report | Recursive with tag filtering |

When `filterTags` is non-`null`, only requirements whose `Tags` list contains at least one
matching tag are included in the output.

## Validation Error Table

| Check | Condition | Error text |
| ----- | --------- | ---------- |
| Section title | Blank | `Section 'title' cannot be blank` |
| Requirement ID | Blank | `Requirement 'id' cannot be blank` |
| Requirement ID | Duplicate | `Duplicate requirement ID '{id}' (first seen at {location})` |
| Requirement title | Blank | `Requirement 'title' cannot be blank` |
| Test name | Blank entry in `tests` list | `Test name cannot be blank` |
| Mapping ID | Blank | `Mapping 'id' cannot be blank` |

All validation errors are reported as `LintSeverity.Error` `LintIssue` objects and include the
source file path for actionable debugging. When any error-level issue is present, `LoadResult.Requirements`
is `null` and `LoadResult.HasErrors` returns `true`.

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Program` | Calls `Requirements.Load`; passes file paths from `Context.RequirementsFiles`; |
| | calls `result.ReportIssues(context)` |
| `TraceMatrix` | Receives the populated `Requirements` root and iterates the tree |
| `Validation` | Exercises `Requirements.Load` with fixture YAML files in tests |

## References

- [ReqStream System Design][arch]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[repo]: https://github.com/demaconsulting/ReqStream
