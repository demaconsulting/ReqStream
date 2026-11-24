---
title: Requirement fields
summary: The fields of one requirement and which are required.
aliases:
  - fields
tags:
  - reference
see-also:
  - requirements-file/ids
  - test-linking
  - reports/justifications
examples:
  - name: full
    kind: source
    language: yaml
    file: requirement.yaml
    description: One requirement that uses every field.
    content: |
      root-tags:
        - system
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
              justification: |
                Checkout is the purpose of the shop.
                Regulators ask for proof that it works.
              tags:
                - system
                - critical
              children:
                - Shop-Cart-Total
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
              tests:
                - Cart_Total_SumsItems
  - name: lint-fields
    kind: command
    language: shell
    description: Check the fields. An unknown field name fails with an unknown-field error.
    content: '{{tool}} --requirements requirement.yaml --lint'
---

Two fields are required and four are optional:

- `id` (required): unique across every loaded file and used as written in all reports.
- `title` (required): one sentence, usually "The X shall Y." Neither field may be blank.
- `justification`: why the requirement exists. Write several lines as a YAML literal block.
  See [[reports/justifications]].
- `tags`: free-form words for `--filter` ([[reports/filter]]) and for root tags ([[orphans]]).
- `children`: ids of the lower-level requirements that decompose this one. They must exist and must not loop.
- `tests`: names of the tests that prove it. See [[test-linking]].

Rules that trip people up:

- A field name that is not in the list is an error, for example `Unknown field 'priority' in requirement`.
- `children` point down, from the broad requirement to the narrow one, never back up.
- A requirement counts as proven when it, or any requirement below it through `children`, has tests and they all
  pass ([[enforcement]]). A parent needs no tests of its own when its children have them.
- Quote an `id` or `title` that contains a colon followed by a space, or that starts with a character such as `*`.

See [[#full]] for a file that uses every field.
