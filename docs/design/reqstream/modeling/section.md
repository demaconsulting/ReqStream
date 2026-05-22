### Section Unit Design

#### Purpose

`Section` is the container node in the requirements tree. It groups a set of `Requirement`
objects under a common title and optionally nests child `Section` objects to represent
hierarchical document structure. `Section` is a simple mutable data object; all merging and
validation logic resides in `RequirementsLoader`.

`Requirements` extends `Section` to serve as the root of the tree, inheriting its container
properties without adding additional state.

#### Data Model

##### Properties

| Property | Type | YAML key | Default | Notes |
| -------- | ---- | -------- | ------- | ----- |
| `Title` | `string` | `title` | `""` | Used to identify and merge sections across files |
| `Requirements` | `List<Requirement>` | `requirements` | `[]` | Requirements directly in this section |
| `Sections` | `List<Section>` | `sections` | `[]` | Child sections |

#### Section Merging

When `RequirementsLoader` encounters a section whose `Title` matches an existing section under
the same parent, it reuses the existing `Section` object rather than creating a new one. This
same-title merge strategy is the design decision that enables modular requirements management:
multiple YAML files can contribute requirements to the same logical section without requiring a
single monolithic file.

#### Key Methods

N/A — `Section` is a data container with no methods. All merging logic resides in
`RequirementsLoader`; all traversal and export logic resides in `Requirements.Export` and
`TraceMatrix`.

#### Error Handling

Section contains no executable logic; all validation errors are produced by `RequirementsLoader`.

#### Interactions

**Dependencies**: N/A — `Section` is a data container with no dependencies on other units, OTS items, or shared
packages beyond the `Requirement` objects it holds in its list.

**Callers**:

| Unit | Nature of interaction |
| ---- | --------------------- |
| `RequirementsLoader` | Creates `Section` objects and merges them into the shared requirements tree |
| `Requirements` | Extends `Section` to serve as the tree root; inherits container properties |
| `TraceMatrix` | Recursively visits `Sections` children to collect all requirements for analysis |
| `Requirements.Export` | Recursively visits `Sections` children to generate Markdown headings and tables |
