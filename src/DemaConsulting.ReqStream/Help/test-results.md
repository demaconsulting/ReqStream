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
    save-as: requirements.yaml
    description: Requirements whose tests appear in the result files that the matrix command reads.
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
  - name: matrix
    kind: command
    language: shell
    description: Read every TRX and every XML file below test-results, write the matrix, and enforce coverage. The folder holds the TRX file of a Windows run and the JUnit file of a Linux run, in which both linked tests passed and a failing test is linked by no requirement. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests "test-results/**/*.trx" --tests "test-results/**/*.xml" --matrix matrix.md --enforce'
---
`--tests <pattern>` names the result files that your test runs write. ReqStream detects the format of each file from
its content, whatever the extension, so TRX and JUnit files can be mixed:

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
  code, and nothing lists them ([[test-linking]]). A failing test that no requirement lists is such a test, so
  [[#matrix]] still exits 0 when one result file holds one.
- Results with one method name from different classes in the same file are combined into one test.
- The name of the result file, without its extension, is what a source filter such as `windows@` matches
  ([[test-linking/source-filters]]).
- A file that is not a valid TRX or JUnit file stops the run with `Failed to parse test result file`.
- `--matrix` needs at least one matching file; with none it reports an error ([[troubleshooting/test-errors]]).
  `--enforce` checks coverage against the matching files and, when root tags are set, also checks orphans without any
  `--tests` ([[enforcement]]). It fails only when it has neither files nor root tags.

[[#matrix]] reads the result files and writes the trace matrix ([[reports/matrix]]).
