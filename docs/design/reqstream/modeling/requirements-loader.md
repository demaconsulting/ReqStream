### RequirementsLoader

#### Purpose

`RequirementsLoader` is the YAML deserializer and structural lint validator for requirements
files. It walks the YAML DOM, merges sections into the shared `Requirements` tree, validates
all required fields, and collects `LintIssue` objects for every problem found. It is the only
unit that reads from the file system for requirements data and the only unit with knowledge of
the YAML DOM representation. `RequirementsLoader` is declared `internal static`; it has no
instances and is inaccessible outside the assembly.

#### Data Model

**`KnownDocumentFields`**: `HashSet<string>` — static set of known document-level field names.

**`KnownSectionFields`**: `HashSet<string>` — static set of known section-level field names.

**`KnownRequirementFields`**: `HashSet<string>` — static set of known requirement-level field
names.

**`KnownMappingFields`**: `HashSet<string>` — static set of known mapping-level field names.

Shared state allocated per `Load` call:

**`requirements`**: `Requirements` — root of the section tree being built.

**`seenIds`**: `Dictionary<string, string>` — maps requirement ID to first-seen location for
duplicate detection.

**`allRequirements`**: `Dictionary<string, Requirement>` — maps ID to `Requirement` object for
cycle detection.

**`visitedFiles`**: `HashSet<string>` — fully-resolved paths of already-processed files
(include-loop guard).

**`activeFiles`**: `HashSet<string>` — tracks the current include call stack for circular file
include detection.

**`issues`**: `List<LintIssue>` — all issues collected during the load.

#### Key Methods

**Load(paths)**: Initializes shared state, calls `LoadFile` for each path, runs
`ValidateCycles`, and assembles the `LoadResult`.

- *Parameters*: `IEnumerable<string> paths` — file paths to load.
- *Returns*: `LoadResult` — contains the tree (or `null` on error) and issues.
- *Preconditions*: None.
- *Postconditions*: If any error-level issue was collected, `LoadResult.Requirements` is `null`.

**LoadFile(path)**: Loads a single YAML file and merges its content into the shared tree.

- *Parameters*: `string path` — file path to load.
- *Returns*: `void`.
- *Preconditions*: None.
- *Postconditions*: File content is merged into `requirements`; includes are followed
  recursively.

Four design points govern its behavior: deduplication (via `visitedFiles`), circular include
detection (via `activeFiles`), YAML parsing (via `YamlDotNet` DOM API), and validation/merging
(title-based section merge strategy). A `YamlScalarNode` at the document root with null/empty
value is silently accepted as an empty file.

**ValidateCycles()**: Performs a depth-first search over all requirements to detect circular
child references.

- *Parameters*: None.
- *Returns*: `void`.
- *Preconditions*: All files are loaded; `allRequirements` is populated.
- *Postconditions*: Any cycles are reported as `LintIssue` objects.

Uses three tracking structures: `visiting` (IDs on the current DFS stack), `currentPath`
(ordered IDs for error messages), and `visited` (IDs confirmed cycle-free). For each child ID:
if not in `allRequirements`, reports unknown child reference; if in `visiting`, reports cycle
with path formatted as `REQ-A -> REQ-B -> ... -> REQ-A`.

#### Error Handling

`RequirementsLoader` reports all detected structural issues as `LintIssue` objects with
`LintSeverity.Error`. It does not throw for domain errors; instead it records each problem and
continues processing to collect as many issues as possible in a single run.

Both `IOException` and `YamlException` are caught within `LoadFile`, converted to `LintIssue`
objects, and processing continues. No exception escapes from `Load` for domain-level problems.

Lint check categories: file access, YAML syntax, document structure, field types, section rules,
requirement rules, list entry rules, mapping rules, and graph rules.

#### Dependencies

- **Section** — creates and merges `Section` objects into the shared requirements tree.
- **Requirement** — creates `Requirement` objects and populates their fields from YAML nodes.
- **LintIssue** — creates `LintIssue` objects for every structural problem detected.
- **LoadResult** — assembled by `Load` from the populated tree and the collected issues list.
- **PathHelpers** — called to combine include-file directory paths with relative `includes`
  entries safely.
- **YamlDotNet** — provides the `RepresentationModel` DOM API used to parse YAML text.

#### Callers

- **Requirements** — delegates YAML parsing and validation to `RequirementsLoader.Load` from
  its own `Load` factory.
