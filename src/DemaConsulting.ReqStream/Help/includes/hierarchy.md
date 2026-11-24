---
title: Three-level requirement tree
summary: Split system, subsystem and unit requirements across files.
aliases:
  - three-level
tags:
  - reference
see-also:
  - orphans
  - test-linking/source-filters
  - reports/matrix
  - enforcement
examples:
  - name: tree-root
    kind: source
    language: yaml
    file: hierarchy/requirements.yaml
    description: The root file. It sets two root tags and includes three files, one of them in a sub-folder.
  - name: tree-system
    kind: source
    language: yaml
    file: hierarchy/system.yaml
    description: The system requirement. It carries the root tag and names the subsystem requirement as its child.
  - name: tree-quality
    kind: source
    language: yaml
    file: hierarchy/quality.yaml
    description: A second root, tagged quality, for a requirement about the project itself. It has its own test.
  - name: tree-subsystem
    kind: source
    language: yaml
    file: hierarchy/cart/subsystem.yaml
    description: The subsystem requirement, with no tag. It names two unit requirements and includes their file.
  - name: tree-units
    kind: source
    language: yaml
    file: hierarchy/cart/units.yaml
    description: The unit requirements with their tests. One test accepts a result from a Windows run only.
  - name: tree-windows
    kind: source
    language: xml
    file: hierarchy/test-results/windows-results.xml
    description: The Windows run. Every linked test passed; the last test is linked by no requirement.
  - name: tree-ubuntu
    kind: source
    language: xml
    file: hierarchy/test-results/ubuntu-results.xml
    description: The Linux run. The Windows-only test fails here, and the filter makes the matrix ignore it.
  - name: tree-lint
    kind: command
    language: shell
    description: Load all five requirements files from the root and check the tree. Prints nothing and exits 0.
    content: '{{tool}} --requirements hierarchy/requirements.yaml --lint --enforce'
  - name: tree-matrix
    kind: command
    language: shell
    description: Write the matrix from both runs and enforce coverage. Exits 0.
    content: '{{tool}} --requirements hierarchy/requirements.yaml --tests "hierarchy/test-results/*.xml" --matrix matrix.md --enforce'
---
A system of any size follows one chain: a **system** requirement lists a **subsystem** requirement under `children`,
which lists **unit** requirements. Only the system requirement is tagged and reachable from a root. Subsystem and unit
requirements are never root-tagged: they are proven through the chain ([[orphans]]).

[[#tree-root]] sets `root-tags` to `system` and `quality`. It includes [[#tree-system]], [[#tree-quality]] and
[[#tree-subsystem]]. The subsystem file includes [[#tree-units]] itself, so includes nest, and each path is relative to
the file that lists it ([[includes]]). A `children` entry may name a requirement in any other file. The files all use
the section `Shop Requirements`, so they merge into one tree: `Cart Subsystem` and `Cart Units` nest below it.

The quality requirement is a second root with its own tag. It is not a child of the system requirement, and the orphan
check accepts it because its tag is a root tag.

If [[#tree-system]] dropped its `children` entry, the subsystem and both units would be orphans: `--lint` warns, and
`--lint --enforce` fails ([[#tree-lint]]).

[[#tree-units]] links `windows@Cart_Save_WritesFile`, so only a result file whose name contains `windows` counts
([[test-linking/source-filters]]). The failure in [[#tree-ubuntu]] is ignored, so [[#tree-matrix]] exits 0. Without the
prefix the unit would fail on the Linux failure. `Cart_Unlinked_Test` in [[#tree-windows]] is linked by no requirement,
so it is ignored ([[test-linking]]).

The matrix shows `Shop-System-Checkout` and `Shop-Cart-Pricing` with zero tests of their own: they are satisfied
through the units ([[reports/matrix]]). The run prints `Reading 1 requirements file(s)`: only the root counts.
