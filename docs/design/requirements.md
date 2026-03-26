# Requirements Design

## Overview

The Requirements unit models the requirements tree loaded from one or more YAML files. It is composed of
three classes: `Requirement` (a leaf data model), `Section` (a tree node), and `Requirements` (the root
that handles loading, validation, and export).

## Structure

### Requirement Class (`Requirement.cs`)

`Requirement` is a simple data model representing a single requirement:

| Property | Type | Description |
| :--- | :--- | :--- |
| `Id` | `string` | Unique requirement identifier (e.g., `CLI-001`) |
| `Title` | `string` | Human-readable requirement title |
| `Justification` | `string?` | Optional justification text for non-tested requirements |
| `Tests` | `List<string>` | Test names that satisfy this requirement |
| `Children` | `List<string>` | IDs of child requirements (hierarchical composition) |
| `Tags` | `List<string>` | Tags used for filtering during export |

Test names may include a source filter prefix in the form `sourceFilter@testName`, which restricts matching
to test result files whose base name contains the filter substring.

### Section Class (`Section.cs`)

`Section` is a tree node grouping related requirements:

| Property | Type | Description |
| :--- | :--- | :--- |
| `Title` | `string` | Section heading text |
| `Requirements` | `List<Requirement>` | Requirements directly within this section |
| `Sections` | `List<Section>` | Child sections (recursive nesting) |

### Requirements Class (`Requirements.cs`)

`Requirements` extends `Section` and is the root of the requirements tree. It owns loading, merging,
validation, and export.

#### Reading YAML Files

```csharp
public static Requirements Read(params string[] paths)
```

- Creates a single `Requirements` instance and calls `ReadFile` for each path
- `ReadFile` deserializes the YAML using `YamlDotNet` with `CamelCaseNamingConvention`
- Supports an `includes:` field in YAML documents to recursively include other files
- Cycle detection for included files is maintained in the private `_includedFiles` `HashSet`
- Duplicate requirement IDs are detected using the private `_allRequirements` dictionary; a duplicate
  raises `InvalidOperationException`
- After all files are read, `ValidateCycles()` checks for cyclic parent/child references among
  requirements

#### YAML Deserialization

Internal private classes (`YamlDocument`, `YamlSection`, `YamlRequirement`, `YamlMapping`) mirror the YAML
structure and are mapped to the public model after deserialization. A `mappings:` field in the YAML
document allows test links to be added to requirements separately from their definition.

#### Export

```csharp
public void Export(string filePath, int depth = 1, HashSet<string>? filterTags = null)
```

Exports requirements to a Markdown file. If `filterTags` is non-null, only requirements with at least one
matching tag are included in the output. The `depth` parameter controls the starting Markdown heading level
(e.g., `depth: 2` produces `##` headings for top-level sections).

```csharp
public void ExportJustifications(string filePath, int depth = 1, HashSet<string>? filterTags = null)
```

Exports justification text for requirements that have a `Justification` property set. Applies the same
tag filter and depth logic as `Export`.

## Key Design Decisions

- **YAML private inner model**: Using private `Yaml*` classes as deserialization targets isolates the
  public API from YAML naming conventions and allows flexible mapping.
- **Single-pass cycle detection for includes**: Tracking included file paths in a `HashSet` before
  recursing prevents infinite loops from circular `includes:` references.
- **Duplicate ID detection at load time**: Checking for duplicate IDs during file reading gives an early,
  clear error before any processing occurs.
- **Tag filtering at export time**: Filtering is applied during export rather than at load time, allowing
  the same `Requirements` tree to be exported multiple times with different filters.

## Relationships

- **Created by**: `Program.ProcessRequirements` via `Requirements.Read`
- **Used by**: `Program.ProcessRequirements` (for export), `TraceMatrix` (to enumerate test names and
  compute coverage)
- **Depends on**: `YamlDotNet` (deserialization), standard file I/O
