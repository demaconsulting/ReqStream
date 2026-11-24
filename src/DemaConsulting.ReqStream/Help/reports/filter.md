---
title: Filter by tag
summary: Limit reports and enforcement to requirements with certain tags.
aliases:
  - tags
  - tag-filter
tags:
  - reference
see-also:
  - requirements-file/requirement
  - enforcement
examples:
  - name: requirements-tagged
    kind: source
    language: yaml
    file: requirements.yaml
    description: Requirements with different tags. Only the payment requirement is tagged security.
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
              tags:
                - performance
              tests:
                - Cart_Total_SumsItems
            - id: Shop-Payment-Charge
              title: The payment service shall charge the total once.
              tags:
                - security
              tests:
                - Payment_Charge_ChargesOnce
  - name: results-partial
    kind: source
    language: xml
    file: results.xml
    description: Results that contain only the payment test.
    content: |
      <testsuites name="payment tests">
        <testsuite name="Shop.Tests" tests="1" failures="0">
          <testcase name="Payment_Charge_ChargesOnce" classname="Shop.Tests.PaymentTests" time="0.020" />
        </testsuite>
      </testsuites>
  - name: filter-report
    kind: command
    language: shell
    description: Report only the requirements tagged security.
    content: '{{tool}} --requirements requirements.yaml --filter security --report security-report.md'
  - name: filter-either
    kind: command
    language: shell
    description: A comma-separated list matches any of its tags.
    content: '{{tool}} --requirements requirements.yaml --filter security,performance --report report.md'
  - name: enforce-filtered
    kind: command
    language: shell
    description: Enforce only the security requirements. They are all proven, so this exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --filter security --enforce'
  - name: enforce-all
    kind: command
    language: shell
    description: Without the filter the cart requirement has no passing test, so this exits 1.
    content: '{{tool}} --requirements requirements.yaml --tests results.xml --enforce'
---
`--filter <tags>` takes a comma-separated list of tags and keeps the requirements that carry **any** of them
([[#filter-report]], [[#filter-either]]). Repeat the option to add more tags. Tags are matched exactly, and a
requirement with no tags is dropped whenever a filter is active.

The filter applies to all three reports (`--report`, `--justifications`, `--matrix`) and to the coverage count of
`--enforce`. A section whose requirements are all filtered out is left out of the report.

Two things the filter does not change:

- **Proof through children.** A kept requirement is still proven or not by its own tests and those of its children,
  filtered or not.
- **Orphan checks.** They always run on the whole graph, so `--enforce` can fail on an orphan that the filter hides
  ([[orphans]]).

Filtering is how a pipeline checks one concern at a time. [[#enforce-filtered]] enforces the security requirements
alone and exits 0; [[#enforce-all]] enforces everything and exits 1 because the cart requirement has no passing test in
[[#results-partial]]. A filter that matches nothing is not an error: the reports are empty and nothing is enforced.
