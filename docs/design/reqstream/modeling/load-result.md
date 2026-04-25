# LoadResult Unit Design

## Overview

`LoadResult` encapsulates the combined outcome of a `Requirements.Load` call. It holds the
parsed `Requirements` tree (or `null` if error-level issues prevented successful loading) and
the complete list of `LintIssue` objects collected during the load. By combining both into a
single return value, `LoadResult` ensures that the requirements tree and the lint issues are
always consistent with each other and can be inspected by the caller in any order.

## Properties

| Member | Type | Notes |
| ------ | ---- | ----- |
| `Requirements` | `Requirements?` | Parsed tree; `null` when error-level issues are present |
| `Issues` | `IReadOnlyList<LintIssue>` | All lint issues collected during loading |
| `HasErrors` | `bool` | `true` when any issue has `LintSeverity.Error` |

## Methods

### `ReportIssues(context)`

`ReportIssues` routes each `LintIssue` in `Issues` to the appropriate output channel of the
supplied `Context`:

- Warning-level issues are sent to `context.WriteLine`.
- Error-level issues are sent to `context.WriteError`.

This method exists to decouple `LoadResult` from knowledge of how issues are displayed; it
delegates all formatting and routing decisions to the `Context` unit.

## Construction

`LoadResult` has an `internal` constructor called only by `RequirementsLoader.Load`. The
constructor accepts the `Requirements?` tree and the collected `IReadOnlyList<LintIssue>`.
`HasErrors` is computed lazily on first access by scanning `Issues` for any entry whose
`Severity` is `LintSeverity.Error`.

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `RequirementsLoader` | Constructs `LoadResult` and populates it with issues and the requirements tree |
| `Requirements` | Returns a `LoadResult` from its `Load` factory method |
| `Context` | Receives routed issues via `ReportIssues(context)` |
| `Program` | Calls `result.ReportIssues(context)` and checks `result.HasErrors` |

## References

- [ReqStream System Design][arch]
- [Modeling Subsystem Design][modeling]

[arch]: ../reqstream.md
[modeling]: modeling.md
