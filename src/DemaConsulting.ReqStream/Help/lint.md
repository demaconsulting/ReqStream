---
title: Lint requirements files
summary: Check requirements files for structural problems.
aliases:
  - check
tags:
  - tutorial
order: 7
see-also:
  - orphans
  - troubleshooting/requirements-errors
examples:
  - name: requirements
    kind: source
    language: yaml
    save-as: requirements.yaml
    description: A valid file with no root tags.
    content: |
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
              tests:
                - Cart_Total_SumsItems
  - name: lint
    kind: command
    language: shell
    description: Lint the file. A clean file prints nothing and exits 0.
    content: '{{tool}} --requirements requirements.yaml --lint'
  - name: broken
    kind: source
    language: yaml
    save-as: broken.yaml
    description: A file with an unknown field and a missing title.
    content: |
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-Cart-Total
              priority: high
  - name: lint-broken
    kind: command
    language: shell
    description: Lint the broken file. It prints one line per problem and exits 1.
    content: '{{tool}} --requirements broken.yaml --lint'
    exit: 1
    expect: "Unknown field 'priority' in requirement"
---
`--lint` checks requirements files without writing reports. Run it before every commit and in CI. Unlike a normal run
it reports **all** problems in one pass, and it does not print the version banner, so silence means success
([[#lint]]).

Each problem is one line, `path(line,column): error: message`, which editors and CI systems can parse
([[#lint-broken]]). The exit code is 1 when any error is reported and 0 otherwise.

It checks:

- YAML syntax, unknown keys at any level, and missing or blank `id` and `title` fields.
- Duplicate requirement ids across all files, and `children` ids that do not exist or form a loop.
- Include files that are missing or that include each other, and mappings that name unknown requirements.
- Blank entries in `tests`, `tags`, `children`, `includes`, and `root-tags`.
- **Orphans**: when root tags are set, requirements that no root requirement reaches through `children`. These are
  warnings, and errors with `--enforce`: `--lint --enforce` is the usual CI check ([[orphans]]).

It does not read test results, so it cannot see a missing, misspelled, or failing test: that is `--tests` with
`--enforce` ([[enforcement]]). The CI recipe in [[enforcement]] runs lint first and the report run second.

**What lint does not check.** All of these are clean, with exit 0 and no output:

- A requirement with no `tests`, no `justification`, or no `tags`.
- Id naming, title wording (no "shall" check), and tag names.
- A test name that no result file contains, or one shared by two requirements.
- Orphans, unless root tags are set (from a file or `--root-tags`). Then they are warnings, or errors with `--enforce`.

`--lint` also ignores `--tests`, `--report`, `--justifications`, and `--matrix`. For the meaning of each message and its
fix, see [[troubleshooting/requirements-errors]].
