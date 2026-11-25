---
title: Requirements file format
summary: Layout of a requirements YAML file.
aliases:
  - format
  - yaml
  - requirements
  - config
tags:
  - reference
order: 2
see-also:
  - includes
  - test-linking
  - orphans
examples:
  - name: shop
    kind: source
    language: yaml
    file: requirements-file/shop.yaml
    save-as: requirements-file/shop.yaml
    description: A complete file with nested sections, tags, children, and a separate mappings block.
  - name: lint
    kind: command
    language: shell
    description: Check the file. Silence and exit code 0 mean it is valid.
    content: '{{tool}} --requirements requirements-file/shop.yaml --lint'
  - name: report
    kind: command
    language: shell
    description: Export the sections and requirements, with headings starting at level 2.
    content: '{{tool}} --requirements requirements-file/shop.yaml --report shop-report.md --report-depth 2'
---
A requirements file is one YAML document. Every top-level key is optional, and any other key is an error.

| Key | Holds |
| --- | --- |
| `sections` | Titled sections that hold requirements and nested sections. |
| `includes` | More files to load, relative to this file. See [[includes]]. |
| `root-tags` | Tags that mark the requirements all others must trace to. See [[orphans]]. |
| `mappings` | Test names attached to requirements by id. See [[test-linking]]. |

A section needs a `title` and may hold `requirements` and nested `sections`. Nest as deeply as the structure of the
system needs; sections that repeat a title path across files merge ([[requirements-file/sections]]). Each requirement
needs an `id` and a `title` ([[requirements-file/requirement]]), and its id should follow the
`System-Component-Feature` pattern ([[requirements-file/ids]]). Give each requirement a `justification` that says why
it exists, for the report reviewers read ([[reports/justifications]]).

[[#shop]] shows every key. Save it as shown, then run [[#lint]]. A file that is empty is valid and loads nothing.
