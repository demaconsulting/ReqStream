# Linter Unit Design

## Overview

`Linter` performs structural validation of requirement YAML files and reports all issues found
without stopping at the first error. It is intended as a developer-aid tool that catches
authoring mistakes — unknown fields, missing required fields, blank values, and duplicate
requirement IDs — before the files are processed by the requirements loader.

Unlike the requirements loader, which uses `YamlDotNet`'s deserializer, `Linter` uses
`YamlDotNet`'s **representation model** (`YamlStream`, `YamlMappingNode`, `YamlSequenceNode`,
`YamlScalarNode`). This preserves `Mark.Line` and `Mark.Column` position information so that
every error message includes an accurate file location.

## Known Field Sets

The linter maintains four constant sets of known field names, compared using ordinal (case-sensitive)
equality:

| Set | Members |
| --- | ------- |
| `KnownDocumentFields` | `sections`, `mappings`, `includes` |
| `KnownSectionFields` | `title`, `requirements`, `sections` |
| `KnownRequirementFields` | `id`, `title`, `justification`, `tests`, `children`, `tags` |
| `KnownMappingFields` | `id`, `tests` |

Any key found in a YAML node that is not a member of the corresponding set is reported as an unknown
field error.

## Error Format

All errors emitted by the linter use the following format:

```text
{path}({line},{col}): error: {description}
```

`line` and `col` are taken from `YamlNode.Start.Line` and `YamlNode.Start.Column` respectively,
providing the exact position of the offending node in the source file.

Errors are written via `context.WriteError`, which sets `Context._hasErrors = true` and eventually
causes the process to exit with code `1`.

## Methods

### `Lint(context, files)`

`Lint` is the single public entry point. If `files` is empty it prints a notice and returns. For
each file in `files` it calls `LintFile`, accumulating the returned issue count across all files.
After all files are processed, it prints `"{files[0]}: No issues found"` via `context.WriteLine`
only if the total issue count is zero. Accumulating all issues before deciding on the success
message ensures that a clean run produces exactly one affirmative line of output and that a run
with issues lists every problem without any misleading success message.

### `LintFile(context, path, seenIds, visitedFiles)`

`LintFile` lints a single YAML file and follows its `includes:` entries. Three design points
govern its behavior:

- **Deduplication**: `path` is resolved to a full absolute path and checked against
  `visitedFiles`. If already visited, the method returns `0` immediately. This mirrors the
  deduplication in `ReadFile` and prevents the same file from being linted twice when it is
  included from multiple parents.
- **Error-at-position reporting**: the file text is parsed via `YamlDotNet`'s representation model
  rather than the deserializer, so every issue is emitted with the exact line and column of the
  offending node. I/O errors, YAML parse exceptions, and structural type mismatches are all caught
  and reported as positioned errors rather than unhandled exceptions.
- **Recursive includes**: after linting the document root via `LintDocumentRoot`, the method
  delegates to `LintIncludes` to follow and lint any files listed in the `includes:` sequence,
  accumulating their issue counts alongside the root document's counts.

### `LintIncludes(context, parentFullPath, includes, seenIds, visitedFiles)`

`LintIncludes` resolves each path in the `includes:` sequence relative to the parent file's
directory and recursively lints it via `LintFile`. Blank entries are skipped. It returns the
accumulated issue count from all included files.

### `LintDocumentRoot(context, path, root, seenIds)`

`LintDocumentRoot` validates the top-level structure of a single YAML document. It checks every
key in the root mapping against `KnownDocumentFields`, emitting an unknown-field error for any
unrecognized key, then delegates to `LintDocumentSections` and `LintDocumentMappings` to validate
the document's content.

### `LintDocumentSections(context, path, root, seenIds)`

`LintDocumentSections` validates that the `sections:` key, if present, is a sequence, and that
each element of that sequence is a mapping node. It delegates each valid element to `LintSection`.

### `LintDocumentMappings(context, path, root)`

`LintDocumentMappings` validates that the `mappings:` key, if present, is a sequence, and that
each element of that sequence is a mapping node. It delegates each valid element to `LintMapping`.

### `LintSection(context, path, section, seenIds)`

`LintSection` validates one section mapping node. It checks all keys against `KnownSectionFields`,
validates that `title` is present and non-blank, then delegates to `LintSectionRequirements` and
`LintSectionChildren` for the section's contents.

### `LintSectionRequirements(context, path, section, seenIds)`

`LintSectionRequirements` validates that the `requirements:` key, if present, is a sequence, and
that each element is a mapping node. It delegates each valid element to `LintRequirement`.

### `LintSectionChildren(context, path, section, seenIds)`

`LintSectionChildren` validates that the `sections:` key within a section, if present, is a
sequence, and that each element is a mapping node. It delegates each valid element back to
`LintSection` for recursive validation.

### `LintRequirement(context, path, requirement, seenIds)`

`LintRequirement` validates one requirement mapping node. It checks all keys against
`KnownRequirementFields`, then delegates ID validation to `LintRequirementId` and title validation
to `LintRequirementTitle`. It also checks `tests:` and `tags:` sequences for blank string entries,
emitting a positioned error for each one found.

### `LintRequirementId(context, path, requirement, seenIds, ref issueCount)`

`LintRequirementId` validates the `id` field and registers it in `seenIds` to detect
cross-file duplicates. If the `id` is absent or blank it emits an error and returns `null`. If the
ID is a duplicate, it emits an error referencing the first file but still returns the ID string so
that downstream validators (`LintRequirementTitle`) can include it in their error messages for
better context.

### `LintRequirementTitle(context, path, requirement, reqId)`

`LintRequirementTitle` validates that the `title` field is present and non-blank. When `reqId` is
non-null it includes the ID in the error description, making the error actionable even when
the offending requirement is one of many in a large file.

### `LintMapping(context, path, mapping)`

`LintMapping` validates one mapping entry. It checks all keys against `KnownMappingFields`,
validates that `id` is present and non-blank, and checks any `tests:` sequence for blank entries.

## Issue Accumulation and No-Issues Message

The linter accumulates all issues across all files before deciding whether to print the no-issues
message. This ensures that a clean run produces exactly one affirmative line of output and that a
run with issues lists every problem without any misleading success message.

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Program` | Calls `Linter.Lint(context, context.RequirementsFiles)` when `--lint` is present |
| `Context` | Provides `WriteError` for issue reporting and `WriteLine` for the no-issues message |
| `Validation` | `RunLintTest` exercises `Linter.Lint` with fixture YAML files |

## References

- [ReqStream Architecture][arch]
- [ReqStream Repository][repo]

[arch]: ../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
