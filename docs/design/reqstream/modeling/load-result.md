### LoadResult Unit Design

#### Purpose

`LoadResult` encapsulates the combined outcome of a `Requirements.Load` call. It holds the
parsed `Requirements` tree (or `null` if error-level issues prevented successful loading) and
the complete list of `LintIssue` objects collected during the load. By combining both into a
single return value, `LoadResult` ensures that the requirements tree and the lint issues are
always consistent with each other and can be inspected by the caller in any order.

#### Data Model

##### Properties

| Member | Type | Notes |
| ------ | ---- | ----- |
| `Requirements` | `Requirements?` | Parsed tree; `null` when error-level issues are present |
| `Issues` | `IReadOnlyList<LintIssue>` | All lint issues collected during loading |
| `HasErrors` | `bool` | `true` when any issue has `LintSeverity.Error` |

#### Key Methods

##### `ReportIssues(context)`

`ReportIssues` routes each `LintIssue` in `Issues` to the appropriate output channel of the
supplied `Context`:

- Warning-level issues are sent to `context.WriteLine`.
- Error-level issues are sent to `context.WriteError`.

This method exists to decouple `LoadResult` from knowledge of how issues are displayed; it
delegates all formatting and routing decisions to the `Context` unit.

#### Error Handling

`LoadResult` contains no executable logic and does not throw. `ReportIssues(context)` iterates
`Issues` and routes each item to `context.WriteLine` (warnings) or `context.WriteError` (errors)
without any branching on failure; the method always runs to completion. The caller is responsible
for checking `HasErrors` or inspecting `Requirements == null` after the call.

#### Construction

`LoadResult` has an `internal` constructor called only by `RequirementsLoader.Load`. The
constructor accepts the `Requirements?` tree and the collected `IReadOnlyList<LintIssue>`.
`HasErrors` is a computed property that evaluates `Issues.Any(i => i.Severity == LintSeverity.Error)`
on each access.

#### Interactions

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Context` | Receives routed lint issues via `ReportIssues(context)`; `context.WriteError` is called for errors and `context.WriteLine` for warnings |
| `RequirementsLoader` | Constructs `LoadResult` and populates it with the requirements tree and issues list |
| `Requirements` | Returns a `LoadResult` from its `Load` factory method to the caller |
| `Program` | Calls `result.ReportIssues(context)` to display issues; checks `result.HasErrors` |
