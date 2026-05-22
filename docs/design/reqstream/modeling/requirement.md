### Requirement Unit Design

#### Purpose

`Requirement` is the domain model for a single requirement entry. It is a simple mutable
data-transfer object with no business logic; its fields are populated by `RequirementsLoader`
during YAML DOM traversal and consumed by `Requirements`, `TraceMatrix`, and the export methods.

#### Data Model

##### Properties

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

#### Key Methods

N/A — `Requirement` is a data-transfer object. It has no methods; all population logic resides
in `RequirementsLoader` and all consumption logic resides in the caller units.

#### Error Handling

N/A — `Requirement` contains no executable logic and performs no validation. All constraint
checking (blank `Id`, blank `Title`, duplicate `Id`, non-scalar list entries) is performed by
`RequirementsLoader` during YAML DOM traversal. `Requirement` itself never throws.

#### Interactions

**Dependencies**: N/A — `Requirement` is a data-transfer object with no dependencies on other units, OTS items,
or shared packages. It contains only built-in .NET collection types.

**Callers**:

| Unit | Nature of interaction |
| ---- | --------------------- |
| `RequirementsLoader` | Creates `Requirement` objects and populates their fields during YAML DOM traversal |
| `Section` | Holds `Requirement` objects in its `Requirements` list |
| `TraceMatrix` | Reads `Tests`, `Children`, and `Tags` to compute coverage and apply tag filtering |
| `Requirements` | Exports `Requirement` fields to Markdown via `Export` and `ExportJustifications` |
