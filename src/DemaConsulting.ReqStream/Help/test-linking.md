---
title: Link tests to requirements
summary: Name the tests that prove each requirement.
aliases:
  - link
  - tests-field
tags:
  - reference
order: 4
see-also:
  - test-results
  - enforcement
  - troubleshooting/test-errors
examples:
  - name: requirements
    kind: source
    language: yaml
    file: requirements.yaml
    description: One requirement lists its tests inline; another gets its tests from a mappings entry.
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
      mappings:
        - id: Shop-Payment-Charge
          tests:
            - Payment_Charge_ChargesOnce
  - name: results
    kind: source
    language: xml
    file: results.xml
    description: A JUnit result file. The testcase names are the names that requirements list.
    content: |
      <testsuites name="all tests">
        <testsuite name="Shop.Tests" tests="2" failures="0">
          <testcase name="Cart_Total_SumsItems" classname="Shop.Tests.CartTests" time="0.010" />
          <testcase name="Payment_Charge_ChargesOnce" classname="Shop.Tests.PaymentTests" time="0.020" />
        </testsuite>
      </testsuites>
  - name: matrix
    kind: command
    language: shell
    description: Link the tests, write the trace matrix, and fail if any requirement is not proven. Exits 0 here.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --matrix matrix.md --enforce'
---
A requirement is proven by tests. List their names in one of two places ([[#requirements]]):

- `tests:` inside the requirement.
- `mappings:` at the top of any loaded file, with an `id` and `tests`. Use it when the tests are maintained apart from
  the requirements. A mapping whose `id` is not a loaded requirement fails with `Mapping references unknown
  requirement id`.

**What a test name is.** It is the name the test run records, without namespace or class: the test method name, such as
`Cart_Total_SumsItems`. In a TRX file that is the test method name, and in a JUnit file it is the `name` of the
`testcase` ([[test-results]]). Matching is exact and case-sensitive. To accept a result from one platform only, prefix
the name with a source filter such as `windows@` ([[test-linking/source-filters]]).

**How a requirement is proven.** A requirement counts as satisfied when it has at least one test, counting the tests of
every requirement below it through `children`, and every one of those tests ran and passed. A test that is listed but
missing from the results, or skipped, counts as not executed and fails the requirement. A test name that appears in
several result files is aggregated, and all of its runs must pass.

[[#matrix]] shows the whole flow. To see why a requirement is unsatisfied, read the Testing table of the matrix
([[reports/matrix]]).
