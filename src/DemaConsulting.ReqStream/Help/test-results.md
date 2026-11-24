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
    description: A JUnit XML file. Two tests passed, one was skipped, and one failed. No requirement lists the last two.
  - name: matrix
    kind: command
    language: shell
    description: Read every TRX and every XML file below test-results, write the matrix, and enforce coverage. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests "test-results/**/*.trx" --tests "test-results/**/*.xml" --matrix matrix.md --enforce'
---
`--tests <pattern>` names the result files. ReqStream detects the format of each file, so TRX and JUnit files can
be mixed ([[#trx]], [[#junit]]):

- **TRX** is what `dotnet test --logger trx` or `--report-trx` writes. A test is a `UnitTestResult`, and its test
  name is the `name` of its `TestMethod`: the method name only. The `testName` and display name are not used, so a
  requirement lists `Cart_Total_SumsItems`, not `Shop.Tests.CartTests.Cart_Total_SumsItems`.
- **JUnit XML** is written by many test runners. A test is a `testcase`, its `name` is the test name, and `failure` or
  `error` marks it failed.

Pass the option once per pattern, or use one pattern that matches several files. `--tests` can be repeated and mixes
TRX and JUnit, as [[#matrix]] does. Always put the pattern in double quotes, so the tool expands it and the same
command works in every shell: `--tests "test-results/**/*.trx"`. `**` matches any folders, `*` matches any characters
in a file name, and a pattern can be absolute. A pattern is relative to the current folder.

**Quoting.** PowerShell passes an unquoted `test-results/*.xml` to the tool, which expands it, so that works there.
bash, zsh, and sh expand it into several arguments first, and the run fails with `Unsupported argument`. Quote globs in
scripts and CI files. A pattern such as `"results/**/*.*"` also matches files that are not results, and one such file
stops the run with `Failed to parse test result file`.

Facts to know:

- Only tests that ran count. A skipped or not-run test is ignored, so a requirement that depends on it is unsatisfied.
- **Tests that no requirement lists are ignored**, passed, failed or skipped: they never change the matrix or the exit
  code, and nothing lists them ([[test-linking]]). In [[#junit]] the failing `Cart_PrintReceipt_Fails` is such a test,
  so [[#matrix]] still exits 0.
- Results with one method name from different classes in the same file are combined into one test.
- The name of the result file, without its extension, is what a source filter such as `windows@` matches
  ([[test-linking/source-filters]]).
- A file that is not a valid TRX or JUnit file stops the run with `Failed to parse test result file`.
- `--matrix` and `--enforce` need at least one matching file; with none, `--matrix` reports an error
  ([[troubleshooting/test-errors]]).

[[#matrix]] reads both files and writes the trace matrix ([[reports/matrix]]).
