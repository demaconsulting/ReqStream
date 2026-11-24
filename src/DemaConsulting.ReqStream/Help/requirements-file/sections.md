---
title: Sections and merging
summary: Nest sections and merge sections that repeat across files.
aliases:
  - nesting
tags:
  - reference
see-also:
  - includes
  - reports
examples:
  - name: root
    kind: source
    language: yaml
    file: requirements.yaml
    description: The root file. It adds a requirement to the Shop Requirements section and includes a second file.
    content: |
      includes:
        - cart.yaml
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
  - name: cart
    kind: source
    language: yaml
    file: cart.yaml
    description: A second file whose section has the same title path, so its requirement joins the same section.
    content: |
      sections:
        - title: Shop Requirements
          sections:
            - title: Cart Requirements
              requirements:
                - id: Shop-Cart-Total
                  title: The cart shall total the prices of its items.
  - name: report-merged
    kind: command
    language: shell
    description: Export the merged tree. The report has one Shop Requirements heading with Cart Requirements below it.
    content: '{{tool}} --requirements requirements.yaml --report requirements-report.md'
---
Sections form a tree. A section may hold `requirements`, nested `sections`, or both, and its `title` is required.
In a report each nesting level is one heading level deeper; `--depth` and `--report-depth` set the first level
([[reports]]).

When two files define a section with the same title at the same place in the tree, the tool merges them into one
section instead of creating a duplicate. This lets an included file add requirements to a section that another file
began, as [[#root]] and [[#cart]] show. Titles are compared exactly, so `Cart Requirements` and `Cart requirements`
stay separate.

A common layout mirrors the product: one file per level, each spelling out its full title path, such as
`Shop Requirements` then `Cart Requirements`. Then [[includes]] collect the files under one root.
