---
title: Includes and the root file
summary: Split files with includes; set root-tags in the root file.
aliases:
  - include
  - root-file
tags:
  - reference
order: 3
see-also:
  - includes/hierarchy
  - requirements-file/sections
  - orphans
  - troubleshooting/requirements-errors
examples:
  - name: root
    kind: source
    language: yaml
    save-as: requirements.yaml
    description: The root file holds only root-tags and includes.
    content: |
      root-tags:
        - system
      includes:
        - cart.yaml
        - payment.yaml
  - name: cart
    kind: source
    language: yaml
    save-as: cart.yaml
    description: An included file with the system requirement and a unit requirement.
    content: |
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
              tags:
                - system
              children:
                - Shop-Cart-Total
                - Shop-Payment-Charge
          sections:
            - title: Cart Requirements
              requirements:
                - id: Shop-Cart-Total
                  title: The cart shall total the prices of its items.
                  tests:
                    - Cart_Total_SumsItems
  - name: payment
    kind: source
    language: yaml
    save-as: payment.yaml
    description: A second included file. Its section path merges into the same Shop Requirements section.
    content: |
      sections:
        - title: Shop Requirements
          sections:
            - title: Payment Requirements
              requirements:
                - id: Shop-Payment-Charge
                  title: The payment service shall charge the total once.
                  tests:
                    - Payment_Charge_ChargesOnce
  - name: lint
    kind: command
    language: shell
    description: Load the root file and everything it includes. Prints nothing when all files are clean.
    content: '{{tool}} --requirements requirements.yaml --lint --enforce'
  - name: report
    kind: command
    language: shell
    description: Export one merged report for all three files.
    content: '{{tool}} --requirements requirements.yaml --report requirements-report.md'
---
Split a large requirement set into files and point `--requirements` at one **root file** that lists the others under
`includes` ([[#root]]). The tool follows every include, so you name only the root. Keep the root file to `root-tags`
and `includes`, and put requirements in the included files ([[#cart]], [[#payment]]).

Rules:

- Each path is relative to the file that lists it and must stay inside that file's folder: `../shared.yaml` is
  rejected as an invalid include path.
- Includes nest. A file reached twice loads once, so shared includes are fine.
- Two files that include each other fail with `Circular include detected`.
- Requirement ids are unique across all loaded files, and sections with the same title path merge.
- `root-tags` from every loaded file are combined. The `--root-tags` option adds more ([[orphans]]).

Do not point `--requirements` at a broad pattern such as `"**/*.yaml"` in a repository with other YAML files:
every matching file is read as a requirements file, and a workflow or settings file fails with
`Unknown field`. Name the root file, or a folder that holds only requirements.

A pattern that matches several files is also fine; each matching file is a root, and ids still must not repeat.
The console line `Reading N requirements file(s)` counts these roots, not the included files, so a root with five
includes reads `Reading 1`. For a tree of system, subsystem and unit requirements over several files, see
[[includes/hierarchy]].
