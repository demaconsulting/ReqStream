---
title: Justifications report
summary: Export why each requirement exists.
aliases:
  - justify
tags:
  - reference
see-also:
  - requirements-file/requirement
  - reports
examples:
  - name: requirements
    kind: source
    language: yaml
    save-as: requirements.yaml
    description: Requirements with a short and a multi-line justification.
    content: |
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
              justification: |
                Checkout is the purpose of the shop.
                Regulators ask for proof that it works.
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
  - name: justifications
    kind: command
    language: shell
    description: Write the justifications report.
    content: '{{tool}} --requirements requirements.yaml --justifications justifications.md'
  - name: justifications-deeper
    kind: command
    language: shell
    description: Start at heading level 3, for example to embed the report under two outer headings.
    content: '{{tool}} --requirements requirements.yaml --justifications justifications.md --justifications-depth 3'
---
`--justifications <file>` writes the `justification` text of every requirement ([[#requirements]]), so a reviewer
can read why each requirement exists without reading the YAML.

The file has a heading for each section. Under it each requirement has a sub-heading with its id, its title in bold,
and its justification. A requirement without a justification still appears, with the title only. Run
[[#justifications]] on the sample to see both cases.

Set the first heading level with `--justifications-depth <n>`, or with `--depth <n>` for all reports
([[#justifications-deeper]]). `--filter` limits the report to requirements with a matching tag ([[reports/filter]]).

Write a justification for every requirement: it is the record of the need behind the requirement.
