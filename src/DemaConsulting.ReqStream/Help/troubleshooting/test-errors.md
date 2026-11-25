---
title: Test and enforcement errors
summary: Why a requirement is unsatisfied or a result file is rejected.
aliases:
  - test-failures
tags:
  - troubleshooting
see-also:
  - test-linking
  - test-linking/source-filters
  - reports/matrix
examples:
  - name: requirements
    kind: source
    language: yaml
    save-as: requirements.yaml
    description: One requirement whose test name is spelled differently from the test in the results.
    content: |
      root-tags:
        - system
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
              tags:
                - system
              tests:
                - Cart_total_SumsItems
  - name: unsatisfied
    kind: command
    language: shell
    description: results.xml is a result file of your test run that records the test as Cart_Total_SumsItems, with different capitals than the requirement lists. The name does not match, so the test counts as not executed. Exits 1.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --matrix matrix.md --enforce'
    exit: 1
    expect: 'Error: Only 0 of 1 requirements are satisfied with tests.'
  - name: no-result-files
    kind: command
    language: shell
    description: The glob matches nothing, so the matrix cannot be built. Exits 1.
    content: '{{tool}} --requirements requirements.yaml --tests "missing/*.trx" --matrix matrix.md'
    exit: 1
    expect: 'No test result files were provided or matched'
  - name: bad-result-file
    kind: command
    language: shell
    description: broken.trx is not a valid result file, for example an empty or truncated one, so it cannot be parsed. Exits 1.
    content: '{{tool}} --requirements requirements.yaml --tests broken.trx --matrix matrix.md'
    exit: 1
    expect: 'Failed to parse test result file'
---

- `Only N of M requirements are satisfied with tests.`: the next lines list the unsatisfied ids, a failing requirement
  and its ancestors ([[reports/matrix]]). Open the matrix and read the columns described below.
- `No test result files were provided or matched.`: `--matrix` found no file for `--tests` ([[#no-result-files]]).
  Run the tests first, and check the glob, its quotes, and the current folder.
- `Failed to parse test result file: path`: the file is not valid TRX or JUnit XML, often because it is empty or
  truncated ([[#bad-result-file]]). Rerun the tests.
- `Cannot enforce requirements without test results or root tags`: add `--tests`, or set root tags
  ([[enforcement]]).

**Reading the matrix** ([[reports/matrix]]). The columns count a row's **own** tests only. For an unsatisfied
requirement from the list, look at its row:

- *Tests Linked* is 0: the requirement names no test of its own. A leaf with no children needs `tests:`
  ([[test-linking]]). A parent with zeros can still be unsatisfied through a descendant: follow its `children`.
- *Not Executed* above 0: no result matches the name. Names are exact and case-sensitive, as [[#unsatisfied]] shows with
  [[#requirements]]. Copy the test method name, without namespace or class. A skipped
  test also counts here.
- *Failed* above 0: a test of this row failed. Fix the test, not the requirement.
- A `windows@` style entry is not executed: no result file name contains the filter
  ([[test-linking/source-filters]]). Name the result files after the job.

**Several result files.** A test that appears in several files must pass in all of them; a filtered test needs only the
files that match its filter.

**Parents.** A requirement with no tests of its own is satisfied when its children have tests and all pass. A parent
whose children have no tests at all is unsatisfied.
