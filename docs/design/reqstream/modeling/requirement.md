### Requirement

#### Purpose

`Requirement` is the domain model for a single requirement entry. It is a simple mutable
data-transfer object with no business logic; its fields are populated by `RequirementsLoader`
during YAML DOM traversal and consumed by `Requirements`, `TraceMatrix`, and the export methods.

#### Data Model

**`Id`**: `string` — unique across all loaded files; must not be blank. YAML key: `id`.

**`Title`**: `string` — human-readable name; must not be blank. YAML key: `title`.

**`Justification`**: `string?` — optional rationale text. YAML key: `justification`.

**`Tests`**: `List<string>` — test identifiers linked to this requirement. YAML key: `tests`.
Initialized to empty list.

**`Children`**: `List<string>` — IDs of child requirements. YAML key: `children`. Initialized to
empty list.

**`Tags`**: `List<string>` — optional labels for filtering and export. YAML key: `tags`.
Initialized to empty list.

**`Location`**: `string?` — source path and line/column where the requirement is defined. Not
from YAML; set by `RequirementsLoader`. Defaults to `null`.

All list properties are initialized to empty instances. `Justification` and `Location` default to
`null`. No property is left uninitialized; callers can safely iterate lists without null checks.

#### Key Methods

N/A — `Requirement` is a data-transfer object with no methods. All population logic resides
in `RequirementsLoader` and all consumption logic resides in the caller units.

#### Error Handling

N/A — `Requirement` contains no executable logic and performs no validation. All constraint
checking (blank `Id`, blank `Title`, duplicate `Id`, non-scalar list entries) is performed by
`RequirementsLoader` during YAML DOM traversal. `Requirement` itself never throws.

#### Interactions

##### Dependencies

N/A — `Requirement` is a data-transfer object with no dependencies on other units, OTS items,
or shared packages. It contains only built-in .NET collection types.

##### Callers

- **RequirementsLoader** — creates `Requirement` objects and populates their fields during YAML
  DOM traversal.
- **Section** — holds `Requirement` objects in its `Requirements` list.
- **TraceMatrix** — reads `Tests`, `Children`, and `Tags` to compute coverage and apply tag
  filtering.
- **Requirements** — exports `Requirement` fields to Markdown via `Export` and
  `ExportJustifications`.
