### LintIssue Unit Design

#### Purpose

`LintIssue` and its companion enum `LintSeverity` represent a single structural issue discovered
during requirements loading or linting. They are simple value types with no dependencies on other
units; they carry the three pieces of information needed to display and route a lint diagnostic:
where the issue occurred, how severe it is, and what the problem is.

#### Data Model

##### `LintSeverity`

`LintSeverity` is an enum that classifies the severity of a lint issue.

| Member | Description |
| ------ | ----------- |
| `Warning` | A non-fatal issue; processing can continue. |
| `Error` | A fatal issue that prevents successful requirements loading. |

##### `LintIssue`

`LintIssue` represents a single issue found during requirements linting or loading.

| Member | Type | Notes |
| ------ | ---- | ----- |
| `LintIssue(location, severity, description)` | Constructor | Initializes all three properties |
| `Location` | `string` | The source location (e.g. `"file.yaml"` or `"file.yaml(3,5)"`) |
| `Severity` | `LintSeverity` | The severity of the issue |
| `Description` | `string` | A human-readable description of the issue |
| `ToString()` | `string` | Returns the issue formatted as `"location: severity: description"` |

#### Key Methods

##### `ToString()`

`ToString()` returns the issue in the standard diagnostic format:

```text
file.yaml(3,5): error: Unknown field 'unknown_field'
```

The `LintSeverity` enum values map to the following lowercase strings in `ToString()` output:

| `LintSeverity` value | String in output |
| -------------------- | ---------------- |
| `Error`              | `"error"`        |
| `Warning`            | `"warning"`      |

This format is recognized by editors and CI tools that can parse file locations and navigate to
the line containing the issue.

#### Error Handling

N/A — `LintIssue` and `LintSeverity` are simple value types with no executable logic.
`LintIssue` objects are created only by `RequirementsLoader`; no validation or error detection
occurs within these types themselves.

#### Interactions

N/A — `LintIssue` and `LintSeverity` are simple value types with no outbound dependencies on
other units, OTS items, or shared packages.

| Unit | Nature of interaction |
| ---- | --------------------- |
| `RequirementsLoader` | Creates `LintIssue` objects for every structural problem found during YAML validation |
| `LoadResult` | Holds a list of `LintIssue` objects and routes them to the `Context` output channels |
