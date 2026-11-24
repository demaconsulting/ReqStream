---
title: Requirements file errors
summary: Lint messages and how to fix each one.
aliases:
  - yaml-errors
tags:
  - troubleshooting
see-also:
  - lint
  - requirements-file
examples:
  - name: broken
    kind: source
    language: yaml
    file: broken.yaml
    description: A file with an unknown field, a duplicate id, an unknown child, and a missing title.
    content: |
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
              children:
                - Shop-Cart-Missing
            - id: Shop-Cart-Total
              title: The cart shall round totals.
            - id: Shop-Cart-Owner
              priority: high
  - name: lint-broken
    kind: command
    language: shell
    description: Lint prints every problem with its line and column, and exits 1.
    content: '{{tool}} --requirements broken.yaml --lint'
  - name: loop-a
    kind: source
    language: yaml
    file: loop-a.yaml
    description: A file that includes loop-b.yaml.
    content: |
      includes:
        - loop-b.yaml
  - name: loop-b
    kind: source
    language: yaml
    file: loop-b.yaml
    description: A file that includes loop-a.yaml, which closes the loop.
    content: |
      includes:
        - loop-a.yaml
  - name: lint-loop
    kind: command
    language: shell
    description: The circular include is reported once, and the exit code is 1.
    content: '{{tool}} --requirements loop-a.yaml --lint'
---
Each line has the form `path(line,column): error: message`. Fix the first one first, then lint again: [[#broken]] and
[[#lint-broken]] show several at once.

- `Unknown field 'x' in requirement` (or `in section`, `in mapping`): a key the tool does not know, often a typo.
  Allowed requirement keys: `id`, `title`, `justification`, `tests`, `children`, `tags`.
- `Unknown field 'x'` at the top of a file: only `sections`, `mappings`, `includes`, and `root-tags` are allowed.
  A YAML file that is not a requirements file probably matched your glob.
- `requirement missing required field 'title'`: add a `title`. The same holds for a missing `id` and for a section
  `title`.
- `Duplicate requirement ID 'x' (first seen at ...)`: ids are unique across all files. Rename one or delete the copy.
- `Requirement 'a' references unknown child 'b'`: a `children` entry is not an existing id. Fix the typo.
- `Circular requirement reference detected: a -> b -> a`: `children` point back up. Remove the upward link.
- `Circular include detected`: two files include each other ([[#lint-loop]]).
- `File not found`: an `includes` path does not exist. Paths are relative to the including file.
- `Invalid 'includes' path`: the path leaves the folder of the including file, for example `../x.yaml`.
- `Mapping references unknown requirement id 'x'`: a `mappings` entry names an id that is not loaded.
- `Malformed YAML: ...`: a YAML syntax error at the given line and column. Check indentation, and quote text that
  contains a colon and a space.
- `Document root must be a mapping`: the file is a list or a single value. Start with `sections:`.
- `Field 'x' must be a sequence`: the value is not a list. Write each entry on its own line with a dash.

A requirement that has no `id` or no `title` is not loaded, so other errors that depend on it can appear after you fix
it.
