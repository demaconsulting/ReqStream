### Requirement Unit Design

#### Overview

`Requirement` is the domain model for a single requirement entry. It is a simple mutable
data-transfer object with no business logic; its fields are populated by `RequirementsLoader`
during YAML DOM traversal and consumed by `Requirements`, `TraceMatrix`, and the export methods.

#### Properties

| Property | Type | YAML key | Notes |
| -------- | ---- | -------- | ----- |
| `Id` | `string` | `id` | Unique across all loaded files; must not be blank |
| `Title` | `string` | `title` | Human-readable name; must not be blank |
| `Justification` | `string?` | `justification` | Optional rationale text |
| `Tests` | `List<string>` | `tests` | Test identifiers linked to this requirement |
| `Children` | `List<string>` | `children` | IDs of child requirements |
| `Tags` | `List<string>` | `tags` | Optional labels for filtering and export |
| `Location` | `string?` | — | Source path and line/column where the requirement is defined |

#### Constraints

- `Id` must be unique across all files loaded in a single `Requirements.Load` call.
  Duplicates are detected and reported by `RequirementsLoader`.
- `Title` must not be blank.
- Entries in `Tests`, `Children`, and `Tags` must be non-blank scalar strings.
  Non-scalar or blank entries are reported as errors by `RequirementsLoader`.

**Default property values**: All list properties (`Tests`, `Children`, `Tags`) are initialized
to empty `List<string>` instances. `Justification` defaults to `null`. `Location` defaults to
`null`. No property is left uninitialized; callers can safely iterate lists without null checks.

#### Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `RequirementsLoader` | Creates and populates `Requirement` objects during YAML DOM traversal |
| `Section` | Holds `Requirement` objects in its `Requirements` list |
| `TraceMatrix` | Reads `Tests` and `Children` to compute coverage |
| `Requirements` | Exports `Requirement` fields to Markdown via `Export` and `ExportJustifications` |
