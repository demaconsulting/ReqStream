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
    description: One tested requirement, one requirement whose test failed, and one with no test.
    content: |
      sections:
        - title: Shop Requirements
          requirements:
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
  - name: results-matrix
    kind: source
    language: xml
    file: results.xml
    description: Results where the payment test failed.
    content: |
      <testsuites name="all tests">
        <testsuite name="Shop.Tests" tests="2" failures="1">
          <testcase name="Cart_Total_SumsItems" classname="Shop.Tests.CartTests" time="0.010" />
          <testcase name="Payment_Charge_ChargesOnce" classname="Shop.Tests.PaymentTests" time="0.020">
            <failure message="charged twice" />
          </testcase>
        </testsuite>
      </testsuites>
  - name: matrix
    kind: command
    language: shell
    description: Write the trace matrix. Without --enforce it exits 0 even though a requirement fails.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --matrix matrix.md'
  - name: matrix-titles
    kind: command
    language: shell
    description: Add a Title column to the Requirements table.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --matrix matrix.md --matrix-titles'
---
`--matrix <file>` writes a trace matrix from the requirements and the test results. It needs `--tests`; with no
matching result file the run reports `No test result files were provided or matched` and exits 1.

The file has three parts:

- **Summary**: `2 of 3 requirements are satisfied with tests.` A requirement is satisfied when its tests, and the
  tests of its children, all passed ([[test-linking]]).
- **Requirements**: for each requirement, the number of tests linked and how many passed, failed, or did not run. With
  `--matrix-titles` the table also shows each title ([[#matrix-titles]]). A requirement that is proven only through its
  children shows zero tests of its own.
- **Testing**: for each test, the requirement it covers and its passes and failures. A source filter appears in the
  name, such as `windows@Cart_Total_SumsItems`.

[[#requirements-matrix]] and [[#results-matrix]] give a matrix with a passing test, a failing test, and a requirement
with no test; run [[#matrix]] to see all three. To turn the numbers into a failing build, add `--enforce`
([[enforcement]]).

Ids and test names contain invisible zero-width spaces so long names can wrap in rendered documents. Remove them with
a search before comparing text. Set the first heading level with `--matrix-depth <n>` or `--depth <n>`, and narrow the
matrix with `--filter` ([[reports/filter]]).
