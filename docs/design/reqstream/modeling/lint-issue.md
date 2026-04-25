# LintIssue Unit Design

## Overview

`LintIssue` and its companion enum `LintSeverity` represent a single structural issue discovered
during requirements loading or linting. They are simple value types with no dependencies on other
units; they carry the three pieces of information needed to display and route a lint diagnostic:
where the issue occurred, how severe it is, and what the problem is.

## Data Model

### `LintSeverity`

`LintSeverity` is an enum that classifies the severity of a lint issue.

| Member | Description |
| ------ | ----------- |
| `Warning` | A non-fatal issue; processing can continue. |
| `Error` | A fatal issue that prevents successful requirements loading. |

### `LintIssue`

`LintIssue` represents a single issue found during requirements linting or loading.

| Member | Type | Notes |
| ------ | ---- | ----- |
| `LintIssue(location, severity, description)` | Constructor | Initializes all three properties |
| `Location` | `string` | The source location (e.g. `"file.yaml"` or `"file.yaml(3,5)"`) |
| `Severity` | `LintSeverity` | The severity of the issue |
| `Description` | `string` | A human-readable description of the issue |
| `ToString()` | `string` | Returns the issue formatted as `"location: severity: description"` |

## Formatting

`ToString()` returns the issue in the standard diagnostic format:

```text
file.yaml(3,5): error: Unknown field 'unknown_field'
```

This format is recognized by editors and CI tools that can parse file locations and navigate to
the line containing the issue.

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `LoadResult` | Holds a list of `LintIssue` objects; routes them to context output |
| `RequirementsLoader` | Creates `LintIssue` objects during YAML validation |

## References

- [ReqStream System Design][arch]
- [Modeling Subsystem Design][modeling]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[modeling]: modeling.md
[repo]: https://github.com/demaconsulting/ReqStream
