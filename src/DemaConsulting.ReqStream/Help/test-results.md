---
title: Test result files
summary: Read TRX and JUnit result files with --tests globs.
aliases:
  - trx
  - junit
  - results
tags:
  - reference
order: 5
see-also:
  - test-linking
  - reports/matrix
  - troubleshooting/test-errors
examples:
  - name: requirements
    kind: source
    language: yaml
    file: requirements.yaml
    description: Requirements whose tests appear in the two result files below.
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
  - name: trx
    kind: source
    language: xml
    file: test-results/windows-results.trx
    description: A TRX file, the format of `dotnet test --logger trx`. Two tests passed.
  - name: junit
    kind: source
    language: xml
    file: test-results/ubuntu-results.xml
    description: A JUnit XML file. Two tests passed and one was skipped, which counts as not executed.
  - name: matrix
    kind: command
    language: shell
    description: Read every TRX and every XML file below test-results, write the matrix, and enforce coverage. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests "test-results/**/*.trx" --tests "test-results/**/*.xml" --matrix matrix.md --enforce'
---
`--tests <pattern>` names the result files. ReqStream detects the format of each file, so TRX and JUnit files can
be mixed ([[#trx]], [[#junit]]):

- **TRX** is what `dotnet test --logger trx` or `--report-trx` writes. A test is a `UnitTestResult`, and its test
  method name is the test name.
- **JUnit XML** is written by many test runners. A test is a `testcase`, and `failure` or `error` marks it failed.

Pass the option once per pattern, or use one pattern that matches several files. Always put the pattern in double
quotes, so the tool expands it and the same command works in every shell:
`--tests "test-results/**/*.trx"`. `**` matches any folders, `*` matches any characters in a file name, and a pattern
can be absolute. A pattern is relative to the current folder.

Facts to know:

- Only tests that ran count. A skipped or not-run test is ignored, so a requirement that depends on it is unsatisfied.
- The name of the result file, without its extension, is what a source filter such as `windows@` matches
  ([[test-linking/source-filters]]).
- A file that is not a valid TRX or JUnit file stops the run with `Failed to parse test result file`.
- `--matrix` and `--enforce` need at least one matching file; with none, `--matrix` reports an error
  ([[troubleshooting/test-errors]]).

[[#matrix]] reads both files and writes the trace matrix ([[reports/matrix]]).
