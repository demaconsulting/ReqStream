# Requirements, Section, and Requirement Unit Design

## Overview

The three classes `Requirements`, `Section`, and `Requirement` together form the domain model for
requirement data in ReqStream. They are responsible for reading YAML files, merging hierarchical
section trees, validating data integrity, preventing infinite include loops and circular child
references, applying test mappings, and exporting content to Markdown reports.

## Data Model

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

`Requirements` extends `Section` and acts as the root of the tree. In addition to the properties
inherited from `Section`, it maintains two private fields that span the lifetime of a load
operation:

| Field | Type | Purpose |
| ----- | ---- | ------- |
| `_includedFiles` | `HashSet<string>` | Absolute paths of files already processed; prevents infinite include loops |
| `_allRequirements` | `Dictionary<string, string>` | Maps requirement ID to source file path; detects duplicate IDs |

## YAML Intermediate Types

YAML is deserialized into a set of intermediate types using `YamlDotNet` with the
`HyphenatedNamingConvention`:

| Intermediate type | Maps to | Notes |
| ----------------- | ------- | ----- |
| `YamlDocument` | Top-level document | Contains `sections`, `mappings`, `includes` |
| `YamlSection` | `sections[]` entries | Contains `title`, `requirements`, `sections` |
| `YamlRequirement` | `requirements[]` entries | Contains `id`, `title`, `justification`, `tests`, `children`, `tags` |
| `YamlMapping` | `mappings[]` entries | Contains `id`, `tests` |

These intermediate types are discarded after `ReadFile` completes; the resulting `Requirement`,
`Section`, and `Requirements` objects are the only long-lived representations.

## Methods

### `Requirements.Read(paths)`

`Read` is the static factory method that constructs and returns a fully loaded `Requirements`
instance.

1. Create a new `Requirements` with empty collections.
2. For each path in `paths`, call `ReadFile(path)`.
3. Call `ValidateCycles()` to detect circular child-requirement references.
4. Return the populated `Requirements` instance.

### `ReadFile(path)`

`ReadFile` loads a single YAML file and merges its content into the `Requirements` tree.

1. Normalize `path` to an absolute path.
2. If the path is already in `_includedFiles`, return immediately (loop prevention).
3. Add the path to `_includedFiles`.
4. Read the file text and deserialize it into a `YamlDocument` using `YamlDotNet`. If the document
   is empty or `null`, return silently.
5. Validate each section title (must not be blank) and each requirement ID and title (must not be
   blank). Duplicate IDs are detected against `_allRequirements`; a duplicate causes an
   `InvalidOperationException` with the file path and conflicting ID.
6. Call `MergeSection` for each top-level section in the document.
7. Apply each entry in the document's `mappings` block: find the matching `Requirement` by ID in
   `_allRequirements` (skipping unknown IDs silently) and append the mapping's tests to
   `Requirement.Tests`.
8. For each path in the document's `includes` block, resolve it relative to the current file's
   directory and call `ReadFile` recursively.

### `MergeSection(parent, yamlSection)`

`MergeSection` integrates a newly parsed section into an existing section tree.

1. Search `parent.Sections` for an existing `Section` whose `Title` equals `yamlSection.Title`.
2. If a match is found:
   - Append all requirements from `yamlSection` to the existing section's `Requirements` list.
   - Recursively call `MergeSection` for each child section in `yamlSection`.
3. If no match is found:
   - Create a new `Section` from `yamlSection` and append it to `parent.Sections`.

This algorithm ensures that sections with the same title at the same hierarchy level are merged
across multiple files, enabling modular requirements management.

### `ValidateCycles()`

`ValidateCycles` performs a depth-first search (DFS) over all requirements to detect circular child
references. It is called once after all files are loaded.

**Tracking structures**:

| Structure | Type | Purpose |
| --------- | ---- | ------- |
| `visiting` | `HashSet<string>` | IDs on the current DFS stack; a hit here indicates a cycle |
| `path` | `List<string>` | Ordered IDs on the current stack; used to build the error message |
| `visited` | `HashSet<string>` | IDs whose entire sub-tree is confirmed cycle-free; skipped on future encounters |

**Algorithm** (per requirement):

1. If the ID is in `visited`, return immediately.
2. If the ID is in `visiting`, a cycle is detected; throw `InvalidOperationException` with the
   cycle path formatted as `REQ-A -> REQ-B -> ... -> REQ-A`.
3. Add the ID to `visiting` and `path`.
4. Recurse into each child ID present in `_allRequirements`.
5. Remove the ID from `visiting` and `path`; add it to `visited`.

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
| Section title | Blank | `Section title cannot be blank` |
| Requirement ID | Blank | `Requirement ID cannot be blank` |
| Requirement ID | Duplicate | `Duplicate requirement ID found: '{id}'` |
| Requirement title | Blank | `Requirement title cannot be blank` |
| Test name | Blank entry in `tests` list | `Test name cannot be blank` |
| Mapping ID | Blank | `Mapping requirement ID cannot be blank` |

All validation errors throw `InvalidOperationException` and include the source file path for
actionable debugging.

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Program` | Calls `Requirements.Read`; passes file paths from `Context.RequirementsFiles` |
| `TraceMatrix` | Receives the populated `Requirements` root and iterates the tree |
| `Validation` | Exercises `Requirements.Read` with fixture YAML files in tests |

## References

- [ReqStream Architecture][arch]
- [ReqStream Repository][repo]

[arch]: ../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
