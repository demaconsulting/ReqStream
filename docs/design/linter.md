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

1. If `files` is empty, print `"No requirements files specified."` and return.
2. Initialize a `Dictionary<string, string> seenIds` (maps requirement ID to source file path) and
   a `HashSet<string> visitedFiles` shared across all files in `files`.
3. For each path in `files`, accumulate the return value of `LintFile(context, path, seenIds, visitedFiles)`
   into a local `issueCount`.
4. If the total issue count is zero, print `"{files[0]}: No issues found"` via `context.WriteLine`.

### `LintFile(context, path, seenIds, visitedFiles)`

`LintFile` lints a single YAML file and follows its `includes:` entries.

1. Resolve `path` to its full absolute path (`fullPath`). On failure, emit an error and return `1`.
2. If `fullPath` is already in `visitedFiles`, return `0` immediately (deduplication).
3. Add `fullPath` to `visitedFiles`.
4. Verify the file exists; emit an error and return `1` if not.
5. Read the file text. On I/O failure, emit an error and return `1`.
6. Parse the text via `ParseYaml`. If a `YamlException` or `InvalidOperationException` is thrown
   (malformed YAML), emit an error at the reported source position and return `1`.
7. If the parsed root is `null` (empty document), return `0` — empty files are valid.
8. If the root node is not a `YamlMappingNode`, emit a type-mismatch error and return `1`.
9. Delegate to `LintDocumentRoot(context, path, root, seenIds)`.
10. Delegate to `LintIncludes(context, fullPath, GetStringList(root, "includes"), seenIds, visitedFiles)`
    to follow included files.
11. Return the accumulated issue count from steps 9–10.

### `LintIncludes(context, parentFullPath, includes, seenIds, visitedFiles)`

`LintIncludes` resolves and recursively lints all files listed in an `includes:` sequence.

1. If `includes` is `null`, return `0`.
2. Derive `baseDirectory` from `parentFullPath`.
3. Filter out any blank entries (using `!string.IsNullOrWhiteSpace`).
4. For each non-blank include path, call `LintFile(context, Path.Combine(baseDirectory, include),
   seenIds, visitedFiles)` and accumulate the returned issue count.
5. Return the total issue count.

### `LintDocumentRoot(context, path, root, seenIds)`

`LintDocumentRoot` validates the top-level structure of a single YAML document.

1. For each key in the root mapping, check that it is a member of `KnownDocumentFields`; if not,
   emit an unknown-field error at the key's position.
2. Delegate to `LintDocumentSections(context, path, root, seenIds)`.
3. Delegate to `LintDocumentMappings(context, path, root)`.

### `LintDocumentSections(context, path, root, seenIds)`

`LintDocumentSections` retrieves the `sections:` sequence from the document root and lints each
child.

1. Call `GetSequenceChecked` for the `"sections"` key on `root`. If the key is absent, return `0`.
   If it is present but not a sequence, emit a type-mismatch error and return `1`.
2. Iterate `sections.Children`. For each child:

   - If it is a `YamlMappingNode`, call `LintSection(context, path, child, seenIds)`.
   - Otherwise emit a `"Section must be a mapping"` error.

### `LintDocumentMappings(context, path, root)`

`LintDocumentMappings` retrieves the `mappings:` sequence from the document root and lints each
child.

1. Call `GetSequenceChecked` for the `"mappings"` key on `root`. If the key is absent, return `0`.
   If it is present but not a sequence, emit a type-mismatch error and return `1`.
2. Iterate `mappings.Children`. For each child:

   - If it is a `YamlMappingNode`, call `LintMapping(context, path, child)`.
   - Otherwise emit a `"Mapping must be a mapping node"` error.

### `LintSection(context, path, section, seenIds)`

`LintSection` validates one section mapping node. The caller (`LintDocumentSections` or
`LintSectionChildren`) has already asserted that the node is a `YamlMappingNode`.

1. For each key in `section`, check against `KnownSectionFields`; emit an unknown-field error for
   any unknown key.
2. Check that `title` is present and non-blank via `GetScalar`; emit an error at the section start
   if missing, or at the scalar start if blank.
3. Delegate to `LintSectionRequirements(context, path, section, seenIds)`.
4. Delegate to `LintSectionChildren(context, path, section, seenIds)`.

### `LintSectionRequirements(context, path, section, seenIds)`

`LintSectionRequirements` retrieves the `requirements:` sequence from a section and lints each
child.

1. Call `GetSequenceChecked` for the `"requirements"` key on `section`. If the key is absent,
   return `0`. If it is present but not a sequence, emit a type-mismatch error and return `1`.
2. Iterate `requirements.Children`. For each child:

   - If it is a `YamlMappingNode`, call `LintRequirement(context, path, child, seenIds)`.
   - Otherwise emit a `"Requirement must be a mapping"` error.

### `LintSectionChildren(context, path, section, seenIds)`

`LintSectionChildren` retrieves the `sections:` sequence from a section and lints each child
section recursively.

1. Call `GetSequenceChecked` for the `"sections"` key on `section`. If the key is absent, return
   `0`. If it is present but not a sequence, emit a type-mismatch error and return `1`.
2. Iterate `sections.Children`. For each child:

   - If it is a `YamlMappingNode`, call `LintSection(context, path, child, seenIds)` recursively.
   - Otherwise emit a `"Section must be a mapping"` error.

### `LintRequirement(context, path, requirement, seenIds)`

`LintRequirement` validates one requirement mapping node. The caller (`LintSectionRequirements`)
has already asserted that the node is a `YamlMappingNode`.

1. For each key in `requirement`, check against `KnownRequirementFields`; emit an unknown-field
   error for any unknown key.
2. Call `LintRequirementId(context, path, requirement, seenIds, ref issueCount)` to validate and
   register the `id` field; capture the returned ID string (or `null` on error).
3. Call `LintRequirementTitle(context, path, requirement, reqId)` to validate the `title` field.
4. If `tests:` is present, find blank entries using
   `.OfType<YamlScalarNode>().Where(s => string.IsNullOrWhiteSpace(s.Value)).Select(s => s.Start)`
   and emit a `"Test name cannot be blank"` error for each.
5. If `tags:` is present, apply the same method chain and emit a `"Tag name cannot be blank"` error
   for each blank entry.

### `LintRequirementId(context, path, requirement, seenIds, ref issueCount)`

`LintRequirementId` validates the `id` field of a requirement, checks for duplicates, and
registers the ID.

1. Look up the `id` scalar via `GetScalar`. If absent, emit a `"Requirement missing required field
   'id'"` error (at the mapping start), increment `issueCount`, and return `null`.
2. If the scalar value is blank, emit a `"Requirement 'id' cannot be blank"` error (at the scalar
   start), increment `issueCount`, and return `null`.
3. Check `seenIds` for the ID. If already present, emit a duplicate-ID error referencing the first
   file, increment `issueCount`, and return `reqId` (the ID is still returned so downstream
   validation can include it in error messages).
4. Register `seenIds[reqId] = path` and return the ID string.

### `LintRequirementTitle(context, path, requirement, reqId)`

`LintRequirementTitle` validates the `title` field of a requirement.

1. Look up the `title` scalar via `GetScalar`. If absent, emit an error whose description uses
   `"requirement '{reqId}'"` when `reqId` is non-null, or `"requirement"` otherwise (at the
   mapping start), and return `1`.
2. If the scalar value is blank, emit a `"Requirement 'title' cannot be blank"` error (at the
   scalar start) and return `1`.
3. Return `0`.

### `LintMapping(context, path, mapping)`

`LintMapping` validates one mapping entry. The caller (`LintDocumentMappings`) has already
asserted that the node is a `YamlMappingNode`.

1. For each key in `mapping`, check against `KnownMappingFields`; emit an unknown-field error for
   any unknown key.
2. Check that `id` is present and non-blank; emit an error at the mapping start if missing, or at
   the scalar start if blank.
3. If `tests:` is present, apply the same blank-entry method chain used in `LintRequirement` and
   emit a `"Test name cannot be blank"` error for each blank entry.

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
