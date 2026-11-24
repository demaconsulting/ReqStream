---
title: Getting started
summary: Write a first requirements file, lint it, and report.
aliases:
  - start
  - quickstart
tags:
  - tutorial
order: 1
see-also:
  - requirements-file
  - test-linking
  - enforcement
examples:
  - name: requirements
    kind: source
    language: yaml
    file: requirements.yaml
    description: A small requirements file with one system requirement and one tested requirement.
    content: |
      root-tags:
        - system
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
              justification: Checkout is the purpose of the shop.
              tags:
                - system
              children:
                - Shop-Cart-Total
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
              justification: A customer must see what they will pay.
              tests:
                - Cart_Total_SumsItems
  - name: lint
    kind: command
    language: shell
    description: Check the file. Prints nothing and exits 0 when it is clean.
    content: '{{tool}} --requirements requirements.yaml --lint'
  - name: report
    kind: command
    language: shell
    description: Write the requirements as a Markdown table per section.
    content: '{{tool}} --requirements requirements.yaml --report requirements-report.md'
  - name: justifications
    kind: command
    language: shell
    description: Write the reason each requirement exists.
    content: '{{tool}} --requirements requirements.yaml --justifications justifications.md'
---
{{tool}} {{version}} reads requirements from YAML files, links them to the tests that prove them, and writes Markdown
reports. It only reads your files and writes the report files you name.

1. **Write a requirements file.** Save the file printed by [[#requirements]] as `requirements.yaml`. A requirement
   needs an `id` and a `title`; [[requirements-file]] lists every field.
2. **Lint it.** [[#lint]] prints nothing and exits 0 when the file is clean. A problem prints as
   `path(line,column): error: message` and the exit code is 1. See [[lint]].
3. **Write reports.** [[#report]] lists the requirements and [[#justifications]] lists why each exists.
   See [[reports]].
4. **Prove the requirements.** Name the tests in each requirement ([[test-linking]]), point `--tests` at the result
   files ([[test-results]]), and add `--enforce` in CI ([[enforcement]]).

Every run needs `--requirements`. A pattern that matches no file is not an error: the tool prints
`No requirements files specified.` and exits 0, so check the path when a run does nothing.
