---
title: Reports
summary: Export requirements, justifications, and a trace matrix.
aliases:
  - report
  - export
tags:
  - reference
order: 6
see-also:
  - reports/justifications
  - reports/matrix
  - reports/filter
examples:
  - name: requirements
    kind: source
    language: yaml
    save-as: requirements.yaml
    description: Two sections of requirements to export.
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
          sections:
            - title: Cart Requirements
              requirements:
                - id: Shop-Cart-Total
                  title: The cart shall total the prices of its items.
                  justification: A customer must see what they will pay.
                  tests:
                    - Cart_Total_SumsItems
  - name: report
    kind: command
    language: shell
    description: Write the requirements report. Each section becomes a heading and a table of ids and titles.
    content: '{{tool}} --requirements requirements.yaml --report requirements-report.md'
  - name: report-deeper
    kind: command
    language: shell
    description: Start every report at heading level 2, so it can be pasted under a level-1 heading.
    content: '{{tool}} --requirements requirements.yaml --report requirements-report.md --depth 2'
  - name: report-quiet
    kind: command
    language: shell
    description: Write the report without console output. The exit code still reports failures.
    content: '{{tool}} --silent --requirements requirements.yaml --report requirements-report.md'
---
Three reports can be written in one run, each to a file you name. Files are overwritten, and the folder must already
exist: a missing folder stops the run with an unexpected-error message.

| Option | Writes |
| --- | --- |
| `--report <file>` | A heading per section and a table of requirement ids and titles. |
| `--justifications <file>` | The reason for each requirement. See [[reports/justifications]]. |
| `--matrix <file>` | Which tests cover each requirement. Needs `--tests`. See [[reports/matrix]]. |

[[#report]] writes the first one. Every report starts at heading level 1 and goes one level deeper per nested section.
`--depth <n>` sets the starting level of all reports ([[#report-deeper]]), and `--report-depth`,
`--justifications-depth`, and `--matrix-depth` override it for one report. A depth must be a positive integer.

`--filter <tags>` limits all three reports to the requirements that carry one of the tags ([[reports/filter]]).
`--silent` ([[#report-quiet]]) hides console output, and `--log <file>` copies it to a file.

Reports are written before `--enforce` checks coverage, so a failing enforcement run still leaves its reports for
inspection ([[enforcement]]).

**Reports are Markdown fragments.** There is no title or front matter, and headings start at `--depth`, so a report can
be one input file of Pandoc or pasted into a larger document under a heading. `--matrix` needs `--tests`, which can be
repeated for TRX and XML ([[test-results]]).
