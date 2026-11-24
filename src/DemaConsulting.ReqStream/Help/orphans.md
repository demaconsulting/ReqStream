---
title: Orphans and root tags
summary: Find requirements that trace to no root requirement.
aliases:
  - root-tags
  - orphan
tags:
  - reference
order: 8
see-also:
  - includes
  - enforcement
  - requirements-file/requirement
examples:
  - name: requirements
    kind: source
    language: yaml
    file: requirements.yaml
    description: The checkout requirement is a root. The owner requirement is linked from no parent.
    content: |
      root-tags:
        - system
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
              tags:
                - system
              children:
                - Shop-Cart-Total
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
            - id: Shop-Cart-Owner
              title: The cart shall remember its owner.
  - name: lint-warning
    kind: command
    language: shell
    description: Lint reports the orphan as a warning and exits 0.
    content: '{{tool}} --requirements requirements.yaml --lint'
  - name: lint-enforce
    kind: command
    language: shell
    description: With --enforce the orphan is an error and the exit code is 1.
    content: '{{tool}} --requirements requirements.yaml --lint --enforce'
  - name: fixed
    kind: source
    language: yaml
    file: fixed.yaml
    description: The same requirements with the missing link added to the children of the root.
    content: |
      root-tags:
        - system
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
              tags:
                - system
              children:
                - Shop-Cart-Total
                - Shop-Cart-Owner
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
            - id: Shop-Cart-Owner
              title: The cart shall remember its owner.
  - name: lint-fixed
    kind: command
    language: shell
    description: The fixed file has no orphans, so this prints nothing and exits 0.
    content: '{{tool}} --requirements fixed.yaml --lint --enforce'
  - name: lint-option
    kind: command
    language: shell
    description: Add root tags on the command line. They combine with the root-tags of the files.
    content: '{{tool}} --requirements fixed.yaml --root-tags system,quality --lint --enforce'
---
A requirement that no real need leads to is scope creep or a missing link. ReqStream finds such **orphans** by
following `children` links down from the requirements that carry a **root tag**. A requirement is orphaned when no
root-tagged requirement reaches it. Root-tagged requirements themselves are never orphans.

Declare the root tags in the root file ([[includes]]), as in [[#requirements]], or on the command line:

- `root-tags:` in any loaded file. The lists of all files are combined.
- `--root-tags a,b` on the command line, which adds to the file lists ([[#lint-option]]).

With no root tags nothing is checked. Convention: tag every system-level requirement `system`, and every repository
quality requirement `quality`, and list both as root tags. Never root-tag a subsystem or unit requirement; link it as a
child of the requirement it serves.

An orphan prints a summary and one line per id:

```text
Warning: 1 of 3 requirements orphaned (not reachable from any requirement tagged: system).
  - Shop-Cart-Owner
```

It is a warning in a normal run and in [[#lint-warning]]. With `--enforce` it is an error with exit code 1
([[#lint-enforce]]). **Fix an orphan by adding the missing `children` link** from the requirement that needs it, as in
[[#fixed]]. If no parent truly needs it, the requirement has no justification: remove it rather than invent a parent.
`--filter` does not hide orphans: the check always covers every requirement.
