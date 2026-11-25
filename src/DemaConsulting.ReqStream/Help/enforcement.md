---
title: Enforcement and exit codes
summary: Fail the build on untested or orphaned requirements.
aliases:
  - enforce
  - exit-codes
tags:
  - reference
  - ci
order: 9
see-also:
  - test-linking
  - orphans
  - troubleshooting/test-errors
examples:
  - name: requirements
    kind: source
    language: yaml
    save-as: requirements.yaml
    description: A system requirement proven through two children.
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
                - Shop-Payment-Charge
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
              tests:
                - Cart_Total_SumsItems
            - id: Shop-Payment-Charge
              title: The payment service shall charge the total once.
              tests:
                - Payment_Charge_ChargesOnce
  - name: enforce-pass
    kind: command
    language: shell
    description: results-pass.xml is a result file in which both tests passed. Every requirement is proven and none is orphaned. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests results-pass.xml --enforce'
  - name: enforce-fail
    kind: command
    language: shell
    description: results-fail.xml is a result file in which Payment_Charge_ChargesOnce failed, so the requirement is unsatisfied. Prints the ids and exits 1.
    content: '{{tool}} --requirements requirements.yaml --tests results-fail.xml --enforce'
    exit: 1
    expect: 'Error: Only 1 of 3 requirements are satisfied with tests.'
  - name: enforce-orphans
    kind: command
    language: shell
    description: Without --tests, root tags alone make --enforce check for orphans only. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --enforce'
  - name: plain
    kind: source
    language: yaml
    save-as: plain.yaml
    description: A file with no root tags.
    content: |
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
  - name: enforce-nothing
    kind: command
    language: shell
    description: No tests and no root tags give --enforce nothing to check. This is an error and exits 1.
    content: '{{tool}} --requirements plain.yaml --enforce'
    exit: 1
    expect: 'Cannot enforce requirements without test results or root tags'
  - name: ci-lint
    kind: command
    language: shell
    description: CI step 1, before the tests run. Lints the files and checks orphans. Prints nothing and exits 0.
    content: '{{tool}} --requirements requirements.yaml --lint --enforce'
  - name: ci-report
    kind: command
    language: shell
    description: CI step 2, after the tests ran. Writes all three reports, then enforces coverage. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests results-pass.xml --report requirements-report.md --justifications justifications.md --matrix matrix.md --enforce'
---
`--enforce` turns a report run into a gate. It checks two things, each only when it has something to check:

- **Test coverage**, when `--tests` matches files: every requirement, after `--filter`, must be satisfied. A
  requirement is satisfied when it and its children have tests and all of them ran and passed ([[test-linking]]).
- **Orphan freedom**, when root tags are set: no requirement may be unreachable from a root ([[orphans]]).

With neither, the run fails with `Cannot enforce requirements without test results or root tags` ([[#enforce-nothing]]).
Reports are written first, so a failed gate still leaves the matrix to read ([[reports/matrix]]).

| Exit code | Meaning |
| --- | --- |
| 0 | The run succeeded: lint warnings and a clean enforcement are both 0. |
| 1 | Any error: a lint error, a failed enforcement, a bad option, an unreadable result file, or a missing file. |

An `Unexpected error` message, such as a report folder that does not exist, ends with the runtime's own non-zero crash
code instead of 1, so treat any non-zero exit as a failure. `{{tool}} help` has its own codes: 0 on success, 2 for a
wrong help argument or an unknown topic. For `--validate` see [[command-line/self-validation]].

**Where the unsatisfied ids go.** A failed gate prints `Error: Only N of M requirements are satisfied with tests.`
and one `  - <id>` line per unsatisfied requirement, in file order. They go to the error stream, to `--log`, and not to
the console with `--silent`. The matrix file does not list them ([[reports/matrix]]). Use [[#enforce-pass]] and
[[#enforce-fail]] to see both outcomes.

**What `--enforce` does not check.** Tests that are in a result file but that no requirement lists are ignored: no
warning, no failure ([[test-linking]]). Missing justifications are not checked either; see [[lint]] for what lint
checks.

**The result files.** The commands read result files that your test run writes: `dotnet test --logger trx` writes a
`.trx` file, and any JUnit XML works too ([[test-results]]). `results-pass.xml` must record both linked tests as
passed, and `results-fail.xml` must record `Payment_Charge_ChargesOnce` as failed.

**CI recipe.** Two commands, because `--lint` ignores the report options. Create the output folders first.

1. Before the tests, as a fast check that needs no results ([[#ci-lint]]):
   `{{tool}} --requirements requirements.yaml --lint --enforce`
2. After the tests have written their result files ([[#ci-report]]):
   `{{tool}} --requirements requirements.yaml --tests "test-results/**/*.trx" --report r.md --matrix m.md --enforce`

Step 2 writes the reports first and then enforces, so a failed gate still leaves the reports to publish.
