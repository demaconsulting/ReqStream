### LintIssue

![Modeling Structure](ModelingView.svg)

#### Purpose

`LintIssue` and its companion enum `LintSeverity` represent a single structural issue discovered
during requirements loading or linting. They are simple value types that carry where the issue
occurred, how severe it is, and what the problem is.

#### Data Model

**`LintSeverity`**: Enum classifying the severity of a lint issue.

- `Warning` — a non-fatal issue; processing can continue.
- `Error` — a fatal issue that prevents successful requirements loading.

**`LintIssue(location, severity, description)`**: Constructor initializing all three properties.

**`Location`**: `string` — the source location (e.g., `"file.yaml"` or `"file.yaml(3,5)"`).

**`Severity`**: `LintSeverity` — the severity of the issue.

**`Description`**: `string` — a human-readable description of the issue.

#### Key Methods

**ToString()**: Returns the issue formatted as `"location: severity: description"`.

- *Parameters*: None.
- *Returns*: `string` — formatted diagnostic string.
- *Preconditions*: None.
- *Postconditions*: Format matches `file.yaml(3,5): error: Unknown field 'unknown_field'`.

The `LintSeverity` enum values map to lowercase strings: `Error` → `"error"`, `Warning` →
`"warning"`. This format is recognized by editors and CI tools that can parse file locations.

#### Error Handling

N/A — `LintIssue` and `LintSeverity` are simple value types with no executable logic. `LintIssue`
objects are created only by `RequirementsLoader`; no validation or error detection occurs within
these types themselves.

#### Interactions

##### Dependencies

N/A — `LintIssue` and `LintSeverity` are simple value types with no outbound dependencies on
other units, OTS items, or shared packages.

##### Callers

- **RequirementsLoader** — creates `LintIssue` objects for every structural problem found during
  YAML validation.
- **LoadResult** — holds a list of `LintIssue` objects and routes them to the `Context` output
  channels.
