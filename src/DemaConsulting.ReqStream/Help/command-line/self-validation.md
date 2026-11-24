---
title: Self-validation
summary: Prove that the installed tool works, and record the proof.
aliases:
  - validate
tags:
  - reference
see-also:
  - command-line
examples:
  - name: validate
    kind: command
    language: shell
    description: Run the built-in tests. Prints one line per test and exits 0 when all pass.
    content: '{{tool}} --validate'
  - name: validate-trx
    kind: command
    language: shell
    description: Also write the results as a TRX file.
    content: '{{tool}} --validate --results validation.trx'
  - name: validate-junit
    kind: command
    language: shell
    description: Write the results as JUnit XML instead.
    content: '{{tool}} --validate --results validation.xml'
  - name: validate-bad-extension
    kind: command
    language: shell
    description: Any other extension is rejected with an error and exit code 1.
    content: '{{tool}} --validate --results validation.txt'
---
`--validate` runs a short set of tests of the tool itself against temporary files, so a regulated project can show that
the installed version works on its build machine. Each test prints `Passed` or `Failed`, and the exit code is 0 only
when all pass.

`--results <file>` also records the outcome. The extension chooses the format: `.trx` writes TRX and `.xml` writes
JUnit; any other extension is an error ([[#validate-bad-extension]]). The result files can feed a requirements
file of your own: the test names are ReqStream-prefixed names such as `ReqStream_Lint`, so a requirement can list them
as evidence ([[test-linking]]).

Self-validation ignores the other options except `--silent` and `--log`.
