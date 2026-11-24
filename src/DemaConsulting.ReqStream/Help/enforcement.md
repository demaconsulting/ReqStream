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
    file: requirements.yaml
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
  - name: results-pass
    kind: source
    language: xml
    file: results-pass.xml
    description: Both tests passed.
    content: |
      <testsuites name="all tests">
        <testsuite name="Shop.Tests" tests="2" failures="0">
          <testcase name="Cart_Total_SumsItems" classname="Shop.Tests.CartTests" time="0.010" />
          <testcase name="Payment_Charge_ChargesOnce" classname="Shop.Tests.PaymentTests" time="0.020" />
        </testsuite>
      </testsuites>
  - name: results-fail
    kind: source
    language: xml
    file: results-fail.xml
    description: The payment test failed.
    content: |
      <testsuites name="all tests">
        <testsuite name="Shop.Tests" tests="2" failures="1">
          <testcase name="Cart_Total_SumsItems" classname="Shop.Tests.CartTests" time="0.010" />
          <testcase name="Payment_Charge_ChargesOnce" classname="Shop.Tests.PaymentTests" time="0.020">
            <failure message="charged twice" />
          </testcase>
        </testsuite>
      </testsuites>
  - name: enforce-pass
    kind: command
    language: shell
    description: Every requirement is proven and none is orphaned. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests results-pass.xml --enforce'
  - name: enforce-fail
    kind: command
    language: shell
    description: A test failed, so the requirement is unsatisfied. Prints the ids and exits 1.
    content: '{{tool}} --requirements requirements.yaml --tests results-fail.xml --enforce'
  - name: enforce-orphans
    kind: command
    language: shell
    description: Without --tests, root tags alone make --enforce check for orphans only. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --enforce'
  - name: plain
    kind: source
    language: yaml
    file: plain.yaml
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

`{{tool}} help` has its own codes: 0 on success, 2 for a wrong help argument or an unknown topic.

In CI, run the report and the gate together, after the tests have written their result files:

```shell
{{tool}} --requirements requirements.yaml --tests "test-results/**/*.trx" --matrix matrix.md --enforce
```

Use [[#enforce-pass]] and [[#enforce-fail]] to see both outcomes; the failing run lists the unsatisfied ids.
