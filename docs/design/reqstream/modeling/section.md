### Section Unit Design

#### Overview

`Section` is the container node in the requirements tree. It groups a set of `Requirement`
objects under a common title and optionally nests child `Section` objects to represent
hierarchical document structure. `Section` is a simple mutable data object; all merging and
validation logic resides in `RequirementsLoader`.

`Requirements` extends `Section` to serve as the root of the tree, inheriting its container
properties without adding additional state.

#### Properties

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

#### Error Handling

Section contains no executable logic; all validation errors are produced by `RequirementsLoader`.

#### Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `RequirementsLoader` | Creates and merges `Section` objects during YAML DOM traversal |
| `Requirement` | Held in the `Requirements` list |
| `Requirements` | Extends `Section`; acts as the tree root |
| `TraceMatrix` | Recursively visits sections to collect requirements |
| `Requirements.Export` | Recursively visits sections to generate Markdown headings and tables |
