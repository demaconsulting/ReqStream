### LoadResult

#### Purpose

`LoadResult` encapsulates the combined outcome of a `Requirements.Load` call. It holds the
parsed `Requirements` tree (or `null` if error-level issues prevented successful loading) and
the complete list of `LintIssue` objects collected during the load. By combining both into a
single return value, `LoadResult` ensures that the requirements tree and the lint issues are
always consistent with each other.

#### Data Model

**`Requirements`**: `Requirements?` — parsed tree; `null` when error-level issues are present.

**`Issues`**: `IReadOnlyList<LintIssue>` — all lint issues collected during loading.

**`HasErrors`**: `bool` — `true` when any issue has `LintSeverity.Error`. Computed property
evaluating `Issues.Any(i => i.Severity == LintSeverity.Error)` on each access.

#### Key Methods

**ReportIssues(context)**: Routes each `LintIssue` in `Issues` to the appropriate output channel.

- *Parameters*: `Context context` — the output channel owner.
- *Returns*: `void`.
- *Preconditions*: None.
- *Postconditions*: Warning-level issues sent to `context.WriteLine`; error-level issues sent to
  `context.WriteError`.

This method decouples `LoadResult` from knowledge of how issues are displayed; it delegates all
formatting and routing decisions to the `Context` unit.

`LoadResult` has an `internal` constructor called only by `RequirementsLoader.Load`. The
constructor accepts the `Requirements?` tree and the collected `IReadOnlyList<LintIssue>`.

#### Error Handling

`LoadResult` contains no executable logic that can fail. `ReportIssues(context)` iterates
`Issues` and routes each item without branching on failure; the method always runs to completion.
The caller is responsible for checking `HasErrors` or inspecting `Requirements == null` after the
call.

#### Dependencies

N/A — `LoadResult` is a data container. It holds references to `Requirements` and `LintIssue`
objects but does not call into other units except `Context` (via `ReportIssues`).

#### Callers

- **RequirementsLoader** — constructs `LoadResult` and populates it with the requirements tree
  and issues list.
- **Requirements** — returns a `LoadResult` from its `Load` factory method.
- **Program** — calls `result.ReportIssues(context)` to display issues; checks
  `result.HasErrors`.
