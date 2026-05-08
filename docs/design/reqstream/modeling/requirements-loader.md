### RequirementsLoader Unit Design

#### Overview

`RequirementsLoader` is the YAML deserializer and structural lint validator for requirements
files. It walks the YAML DOM, merges sections into the shared `Requirements` tree, validates
all required fields, and collects `LintIssue` objects for every problem found. It is the only
unit that reads from the file system for requirements data and the only unit with knowledge of
the YAML DOM representation. `RequirementsLoader` is declared `internal static`; it has no
instances and is inaccessible outside the assembly.

#### YAML DOM Traversal

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

#### Shared State

`RequirementsLoader.Load` allocates and shares the following state across all files loaded in
one call:

| Field | Type | Purpose |
| ----- | ---- | ------- |
| `requirements` | `Requirements` | Root of the section tree being built |
| `seenIds` | `Dictionary<string, string>` | Maps requirement ID to first-seen location for duplicate detection |
| `allRequirements` | `Dictionary<string, Requirement>` | Maps ID to `Requirement` object for cycle detection |
| `visitedFiles` | `HashSet<string>` | Fully-resolved paths of already-processed files (include-loop guard) |
| `issues` | `List<LintIssue>` | All issues collected during the load |

#### Methods

##### `Load(paths)`

`Load` initializes shared state, calls `LoadFile` for each path in `paths`, then calls
`ValidateCycles` and assembles the `LoadResult`. If any error-level issue was collected,
`LoadResult.Requirements` is `null`; otherwise it contains the populated tree.

`ValidateCycles` is only invoked when `allRequirements.Count > 0`; when no requirements were
loaded (e.g. all files were empty or contained only `---`), cycle detection is skipped entirely
because there are no nodes to traverse.

##### `LoadFile(path)`

`LoadFile` loads a single YAML file and merges its content into the shared `Requirements` tree.
Four design points govern its behavior:

- **Deduplication**: `path` is normalized to an absolute path and checked against `visitedFiles`
  before any work is done. If already present, the method returns immediately. This prevents
  infinite loops when files include each other directly or transitively.
- **YAML parsing**: the file text is parsed into a `YamlStream` using `YamlDotNet`'s
  `RepresentationModel` DOM API. A `YamlScalarNode` at the document root whose `Value` is
  `null` or empty (produced by a `---`-only YAML file or a blank document) is silently accepted
  and treated as an empty file with no sections. Any other non-mapping root node is reported as
  an error.
- **Validation and merging**: each section is validated (title must not be blank) and each
  requirement is validated (ID and title must not be blank; ID must not duplicate an entry already
  seen). Validated sections are merged into the tree inline using the same-title merge strategy
  described in the `Section` unit design.
- **Recursive includes**: each path in the document's `includes` block is resolved relative to
  the current file's directory and passed to `LoadFile` recursively.

##### `ValidateCycles()`

`ValidateCycles` performs a depth-first search (DFS) over all requirements to detect circular
child references. It is called once after all files are loaded.

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

#### Lint Check Categories

`RequirementsLoader` checks the following categories of structural issues, all reported as
Error-level:

- **File access** — Invalid path, file not found, I/O read failure.
- **YAML syntax** — Malformed YAML (line and column reported).
- **Document structure** — Root is not a mapping; unknown document-level fields.
- **Field types** — `sections`, `mappings`, or `includes` value is not a sequence;
  a `sections` or `requirements` entry is not a mapping node.
- **Section rules** — Missing or blank `title`; unknown field in section.
- **Requirement rules** — Missing or blank `id`; duplicate `id`; missing or blank `title`; unknown field in requirement.
- **List entry rules** — Non-scalar entry in `tests`, `children`, `tags`, or `includes`;
  blank entry in `tests`, `children`, `tags`, or `includes`.
- **Mapping rules** — Mapping entry is not a mapping node; missing or blank mapping `id`; unknown field in mapping.
- **Graph rules** — Unknown child requirement reference; circular `children` reference (DFS cycle detection).

#### Validation Error Table

| Check | Condition | Error text |
| ----- | --------- | ---------- |
| Section title | Blank | `Section 'title' cannot be blank` |
| Requirement ID | Blank | `Requirement 'id' cannot be blank` |
| Requirement ID | Duplicate | `Duplicate requirement ID '{id}' (first seen at {location})` |
| Requirement title | Blank | `Requirement 'title' cannot be blank` |
| Test name | Blank entry in `tests` list | `Test name cannot be blank` |
| Mapping ID | Blank | `Mapping 'id' cannot be blank` |

#### Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Requirements` | Called via `Requirements.Load`; provides the shared `Requirements` tree |
| `Section` | Creates and merges `Section` objects during DOM traversal |
| `Requirement` | Creates `Requirement` objects and populates their fields |
| `LintIssue` | Creates `LintIssue` objects for every structural problem found |
| `LoadResult` | Assembled by `Load` from the requirements tree and collected issues |
