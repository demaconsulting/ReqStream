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

`Lint` is the single public entry point.

1. Initialize a `Dictionary<string, string> seenIds` (maps requirement ID to source file path) and
   a `HashSet<string> visitedFiles` shared across all files in `files`.
2. For each path in `files`, call `LintFile(context, path, seenIds, visitedFiles)`.
3. Count the total number of issues written (tracked via `context`'s error state or a local
   counter).
4. If the total issue count is zero, print `"{files[0]}: No issues found"` via `context.WriteLine`.

### `LintFile(context, path, seenIds, visitedFiles)`

`LintFile` lints a single YAML file.

1. Resolve `path` to its full absolute path.
2. If `path` is already in `visitedFiles`, return immediately.
3. Add `path` to `visitedFiles`.
4. Read the file text.
5. Attempt to parse the text with `YamlStream.Load()`. If a `YamlException` is thrown (malformed
   YAML), call `context.WriteError` with the exception message and return; do not attempt to lint
   further.
6. If the stream has no documents (empty file), return immediately — empty files are valid.
7. If the root node is present but is not a `YamlMappingNode` (e.g. a top-level sequence or scalar),
   emit an error at the node's position and return.
8. Call `LintDocumentRoot(context, path, root, seenIds)` with the mapping root.
9. After all documents are linted, locate the `includes:` sequence in the root mapping (if present)
   and for each scalar entry call `LintFile` recursively, resolving the include path relative to
   the directory of the current file.

### `LintDocumentRoot(context, path, root, seenIds)`

`LintDocumentRoot` validates the top-level structure of a single YAML document.

1. For each key in the root mapping, check that it is a member of `KnownDocumentFields`; if not,
   emit an unknown-field error at the key's position.
2. Locate the `sections:` node. If the key exists but its value is not a `YamlSequenceNode`, emit a
   type-mismatch error. Otherwise delegate to `LintSections`.
3. Locate the `mappings:` node. If the key exists but its value is not a `YamlSequenceNode`, emit a
   type-mismatch error. Otherwise delegate to `LintMappings`.

### `LintSections(context, path, sectionsNode, seenIds)`

`LintSections` iterates a `YamlSequenceNode` of section entries and calls `LintSection` for each.

### `LintSection(context, path, sectionNode, seenIds)`

`LintSection` validates one section mapping node.

1. Assert `sectionNode` is a `YamlMappingNode`; emit an error and return if not.
2. For each key, check against `KnownSectionFields`; emit an unknown-field error for any unknown key.
3. Check that `title` is present and non-blank; emit an error if missing or blank.
4. If `sections:` key is present but its value is not a sequence, emit a type-mismatch error;
   otherwise call `LintSections` recursively.
5. If `requirements:` key is present but its value is not a sequence, emit a type-mismatch error;
   otherwise call `LintRequirements`.

### `LintRequirements(context, path, requirementsNode, seenIds)`

`LintRequirements` iterates a `YamlSequenceNode` of requirement entries and calls
`LintRequirement` for each.

### `LintRequirement(context, path, requirementNode, seenIds)`

`LintRequirement` validates one requirement mapping node.

1. Assert `requirementNode` is a `YamlMappingNode`; emit an error and return if not.
2. For each key, check against `KnownRequirementFields`; emit an unknown-field error for any
   unknown key.
3. Check that `id` is present and non-blank; emit an error if missing or blank.
4. If `id` is valid, check `seenIds` for a duplicate; if found, emit a duplicate-ID error
   referencing both the current file position and the previously seen file. Add the ID to
   `seenIds` if not already present.
5. Check that `title` is present and non-blank; emit an error if missing or blank.
6. If `tests:` is present, iterate each entry and emit an error for any blank scalar.
7. If `tags:` is present, iterate each entry and emit an error for any blank scalar.

### `LintMappings(context, path, mappingsNode, seenIds)`

`LintMappings` iterates a `YamlSequenceNode` of mapping entries and calls `LintMapping` for each.

### `LintMapping(context, path, mappingNode, seenIds)`

`LintMapping` validates one mapping entry.

1. Assert `mappingNode` is a `YamlMappingNode`; emit an error and return if not.
2. For each key, check against `KnownMappingFields`; emit an unknown-field error for any unknown key.
3. Check that `id` is present and non-blank; emit an error if missing or blank.
4. If `tests:` is present, iterate each entry and emit an error for any blank scalar.

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
