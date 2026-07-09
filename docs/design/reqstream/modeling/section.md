### Section

![Modeling Structure](ModelingView.svg)

#### Purpose

`Section` is the container node in the requirements tree. It groups a set of `Requirement`
objects under a common title and optionally nests child `Section` objects to represent
hierarchical document structure. `Section` is a simple mutable data object; all merging and
validation logic resides in `RequirementsLoader`. `Requirements` extends `Section` to serve as
the root of the tree.

#### Data Model

**`Title`**: `string` — used to identify and merge sections across files. YAML key: `title`.
Default: `""`. YamlDotNet deserializes this property via its public setter (`{ get; set; }`).

**`Requirements`**: `List<Requirement>` — requirements directly in this section. YAML key:
`requirements`. Default: `[]` (pre-initialized empty list). YamlDotNet populates this
collection by calling `.Add()` on the pre-initialized list; no setter is required.

**`Sections`**: `List<Section>` — child sections. YAML key: `sections`. Default: `[]`
(pre-initialized empty list). YamlDotNet populates this collection by calling `.Add()` on
the pre-initialized list; no setter is required.

When `RequirementsLoader` encounters a section whose `Title` matches an existing section under
the same parent, it reuses the existing `Section` object rather than creating a new one. This
same-title merge strategy enables modular requirements management: multiple YAML files can
contribute requirements to the same logical section without requiring a single monolithic file.

#### Key Methods

N/A — `Section` is a data container with no methods. All merging logic resides in
`RequirementsLoader`; all traversal and export logic resides in `Requirements.Export` and
`TraceMatrix`.

#### Error Handling

N/A — `Section` contains no executable logic; all validation errors are produced by
`RequirementsLoader`.

#### Interactions

##### Dependencies

N/A — `Section` is a data container with no dependencies on other units, OTS items, or shared
packages beyond the `Requirement` objects it holds in its list.

##### Callers

- **RequirementsLoader** — creates `Section` objects and merges them into the shared
  requirements tree.
- **Requirements** — extends `Section` to serve as the tree root; inherits container properties.
- **TraceMatrix** — recursively visits `Sections` children to collect all requirements for
  analysis.
- **Requirements.Export** — recursively visits `Sections` children to generate Markdown headings
  and tables.
