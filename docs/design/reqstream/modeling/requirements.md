# Requirements Unit Design

## Overview

`Requirements` is the root of the requirements section tree and the public API entry point for
the Modeling subsystem. It extends `Section` to inherit the container properties (title,
requirements list, child sections list) and adds the `Load` static factory method and the
`Export`/`ExportJustifications` report-generation methods.

`Requirements` has no knowledge of YAML parsing or lint validation; those responsibilities belong
entirely to `RequirementsLoader`. Its role is to provide the public surface through which callers
load and export requirements data.

## Factory Method

### `Load(paths)`

`Load` is the single static factory method. It accepts one or more file paths, delegates to
`RequirementsLoader.Load`, and returns the resulting `LoadResult` containing the populated
`Requirements` tree (or `null` on error) and the complete list of `LintIssue` objects.

Callers that need to abort on errors check `result.HasErrors` or `result.Requirements == null`.
Callers that need to surface issues to the user call `result.ReportIssues(context)`.

## Export Methods

| Method | Output | Notes |
| ------ | ------ | ----- |
| `Export(filePath, depth, filterTags)` | Requirements Markdown report | Recursive; applies `filterTags` |
| `ExportJustifications(filePath, depth, filterTags)` | Justifications Markdown report | Recursive with tag filtering |

Both methods walk the section tree recursively, emitting Markdown headings at the configured
`depth` and a requirements table for each section. When `filterTags` is non-`null`, only
requirements whose `Tags` list contains at least one matching tag are included in the output.

**Error handling**: Both `Export` and `ExportJustifications` throw `ArgumentException` when
`filePath` is `null` or empty. Both methods propagate any `IOException` or
`UnauthorizedAccessException` thrown by the underlying file-write operations to the caller
without wrapping. Callers are responsible for handling file-write failures; the methods do not
catch or suppress I/O exceptions.

**Export output format**:

- Each `Section` produces a Markdown heading (`#` through `######` depending on `depth`) with
  the section title.
- Each `Section` produces a Markdown table with columns `ID` and `Title` for its requirements.

**ExportJustifications output format**:

- Each `Section` produces a Markdown heading at the configured depth.
- Each requirement with a non-null `Justification` produces a sub-heading and the justification
  text.

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `RequirementsLoader` | Called by `Requirements.Load`; provides the shared `Requirements` tree |
| `LoadResult` | Returned by `Requirements.Load`; holds the tree and lint issues |
| `Section` | `Requirements` extends `Section` and inherits its container properties |
| `Program` | Calls `Requirements.Load` to load requirements and calls `Export` / `ExportJustifications` |
| `TraceMatrix` | Receives the populated `Requirements` root and iterates the section tree |
