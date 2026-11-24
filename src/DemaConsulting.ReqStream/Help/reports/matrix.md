---
title: Trace matrix
summary: Show which tests cover each requirement.
aliases:
  - trace-matrix
tags:
  - reference
see-also:
  - test-linking
  - test-results
  - enforcement
examples:
  - name: requirements-matrix
    kind: source
    language: yaml
    file: requirements.yaml
    description: A parent of a tested and a failing requirement, plus one requirement with no test.
    content: |
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
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
            - id: Shop-Cart-Owner
              title: The cart shall remember its owner.
  - name: matrix
    kind: command
    language: shell
    description: Write the trace matrix. results.xml is a result file of your test run in which Cart_Total_SumsItems passed and Payment_Charge_ChargesOnce failed. Without --enforce it exits 0 even though a requirement fails.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --matrix matrix.md'
  - name: matrix-titles
    kind: command
    language: shell
    description: Add a Title column to the Requirements table.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --matrix matrix.md --matrix-titles'
  - name: matrix-enforce
    kind: command
    language: shell
    description: Write the matrix, then enforce. The parent is unsatisfied and listed, though its own row is all zeros. Exits 1.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --matrix matrix.md --enforce'
---
`--matrix <file>` writes a trace matrix from the requirements and the test results. It needs `--tests`; with no
matching result file the run reports `No test result files were provided or matched` and exits 1.

The file has three parts:

- **Summary**: `1 of 4 requirements are satisfied with tests.` A requirement is satisfied only when it and **all its
  descendants** have tests and every one passed ([[test-linking]]).
- **Requirements**: for each requirement, the number of tests linked and how many passed, failed, or did not run. These
  counts cover the row's **own** tests only. With `--matrix-titles` the table also shows each title
  ([[#matrix-titles]]).
- **Testing**: for each test, the requirement it covers and its passes and failures. A source filter appears in the
  name, such as `windows@Cart_Total_SumsItems`. Tests that no requirement lists are not shown ([[test-linking]]).

**A green-looking row can be unsatisfied.** A parent is unsatisfied when a test of any descendant fails or did not run,
even if its own row looks fine. A parent proven only through children shows `0 | 0 | 0 | 0`, the same as an
untested requirement. In [[#requirements-matrix]], `Shop-System-Checkout` is such a parent: its child
`Shop-Payment-Charge` failed.

**Where the unsatisfied ids are.** Not in the matrix file, and not in a run without `--enforce`, which prints only the
count and exits 0. With `--enforce` the run prints `Error: Only N of M requirements are satisfied with tests.` and one
`  - <id>` line each, to the error stream and `--log` (hidden by `--silent`), and exits 1 ([[#matrix-enforce]]). The
matrix is written first, so read it after the failure. To find the culprit: of the listed ids, the rows with *Failed*
or *Not Executed* above 0 are the failing requirements; a listed row with all zeros is an ancestor, or a leaf with no
test. Follow its `children` down to the failing rows.

[[#requirements-matrix]] and a result file in which the payment test failed give a matrix with a parent, a failing test
and a requirement with no test; run [[#matrix]] to see them. The result file is what your test run writes, for example
`dotnet test --logger trx`; the file name may end in `.trx` or `.xml`, because the format is detected from the content.
To turn the numbers into a failing build, add `--enforce` ([[enforcement]]).
`--tests` can be repeated to mix TRX and JUnit files ([[test-results]]).

Ids and test names contain invisible zero-width spaces so long names can wrap in rendered documents. Remove them with
a search before comparing text. Set the first heading level with `--matrix-depth <n>` or `--depth <n>`, and narrow the
matrix with `--filter` ([[reports/filter]]).
