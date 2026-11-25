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
  - name: root
    kind: source
    language: yaml
    file: hierarchy/requirements.yaml
    save-as: hierarchy/requirements.yaml
    description: The root file. It sets two root tags and includes three files, one of them in a sub-folder.
  - name: system
    kind: source
    language: yaml
    file: hierarchy/system.yaml
    save-as: hierarchy/system.yaml
    description: The system requirement. It carries the root tag and names the subsystem requirement as its child.
  - name: quality
    kind: source
    language: yaml
    file: hierarchy/quality.yaml
    save-as: hierarchy/quality.yaml
    description: A second root, tagged quality, for a requirement about the project itself. It has its own test.
  - name: subsystem
    kind: source
    language: yaml
    file: hierarchy/cart/subsystem.yaml
    save-as: hierarchy/cart/subsystem.yaml
    description: The subsystem requirement, with no tag. It names two unit requirements and includes their file.
  - name: units
    kind: source
    language: yaml
    file: hierarchy/cart/units.yaml
    save-as: hierarchy/cart/units.yaml
    description: The unit requirements with their tests. One test accepts a result from a Windows run only.
  - name: lint
    kind: command
    language: shell
    description: Load all five requirements files from the root and check the tree. Prints nothing and exits 0.
    content: '{{tool}} --requirements hierarchy/requirements.yaml --lint --enforce'
  - name: matrix
    kind: command
    language: shell
    description: Write the matrix from the result files of both runs in the hierarchy/test-results folder and enforce coverage. Exits 0.
    content: '{{tool}} --requirements hierarchy/requirements.yaml --tests "hierarchy/test-results/*.xml" --matrix matrix.md --enforce'
---
A system of any size follows one chain: a **system** requirement lists a **subsystem** requirement under `children`,
which lists **unit** requirements. Only the system requirement is tagged and reachable from a root. Subsystem and unit
requirements are never root-tagged: they are proven through the chain ([[orphans]]).

[[#root]] sets `root-tags` to `system` and `quality`. It includes [[#system]], [[#quality]] and
[[#subsystem]]. The subsystem file includes [[#units]] itself, so includes nest, and each path is relative to
the file that lists it ([[includes]]). A `children` entry may name a requirement in any other file. The files all use
the section `Shop Requirements`, so they merge into one tree: `Cart Subsystem` and `Cart Units` nest below it.

The quality requirement is a second root with its own tag. It is not a child of the system requirement, and the orphan
check accepts it because its tag is a root tag.

If [[#system]] dropped its `children` entry, the subsystem and both units would be orphans: `--lint` warns, and
`--lint --enforce` fails ([[#lint]]).

[[#units]] links `windows@Cart_Save_WritesFile`, so only a result file whose name contains `windows` counts
([[test-linking/source-filters]]). [[#matrix]] reads two result files that your test runs write, in the folder
`hierarchy/test-results`: `windows-results.xml`, in which all four tests passed, and `ubuntu-results.xml`, in which
`Cart_Save_WritesFile` failed. The failure in the Linux file is ignored, so [[#matrix]] exits 0. Without the
prefix the unit would fail on the Linux failure. `Cart_Unlinked_Test` in the Windows file is linked by no requirement,
so it is ignored ([[test-linking]]).

The matrix shows `Shop-System-Checkout` and `Shop-Cart-Pricing` with zero tests of their own: they are satisfied
through the units ([[reports/matrix]]). The run prints `Reading 1 requirements file(s)`: only the root counts.
