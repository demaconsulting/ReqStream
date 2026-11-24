---
title: Requirement ids
summary: Choose readable System-Component-Feature ids.
aliases:
  - semantic-ids
tags:
  - reference
see-also:
  - requirements-file/requirement
  - troubleshooting/requirements-errors
examples:
  - name: ids
    kind: source
    language: yaml
    file: requirements.yaml
    description: Ids that name the system, the component, and the feature.
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
  - name: lint-ids
    kind: command
    language: shell
    description: Check that every id is present and unique.
    content: '{{tool}} --requirements requirements.yaml --lint'
---
{{tool}} prints each id exactly as written in every report and in the trace matrix, so the id is the name people
read. Use the pattern `System-Component-Feature`, shown in [[#ids]]:

- System level: `Shop-System-Checkout`
- Component or subsystem level: `Shop-Cart-Total`
- Unit level: `Shop-Cart-Pricing-RoundHalfUp`

Rules the tool enforces:

- An id is required and may not be blank.
- An id is unique across all loaded files. A repeat fails with `Duplicate requirement ID` and names the first place
  the id was seen.
- Ids are case-sensitive: `Shop-Cart-Total` and `shop-cart-total` are different requirements.
- Every `children` entry must be the id of a requirement that exists.

Avoid opaque ids such as `REQ-042`: the reports are then unreadable without a lookup table. Use hyphens, letters, and
digits so the id needs no YAML quoting.
