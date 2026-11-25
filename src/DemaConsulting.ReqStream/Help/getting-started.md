---
title: Getting started
summary: Install the tool, write a first requirements file, lint it, and report.
aliases:
  - start
  - quickstart
  - examples
tags:
  - tutorial
order: 1
see-also:
  - requirements-file
  - test-linking
  - enforcement
examples:
  - name: install-local
    kind: snippet
    language: shell
    save-as: install-local.sh
    description: Add the tool to the tool manifest of a repository.
    content: |
      dotnet new tool-manifest
      dotnet tool install --local DemaConsulting.ReqStream
  - name: run-in-ci
    kind: snippet
    language: shell
    save-as: run-in-ci.sh
    description: Restore the manifest tools in CI, then run the tool.
    content: |
      dotnet tool restore
      dotnet {{tool}} --version
  - name: requirements
    kind: source
    language: yaml
    save-as: requirements.yaml
    description: A small requirements file with one system requirement and one tested requirement.
    content: |
      root-tags:
        - system
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Checkout
              title: The shop shall let a customer check out a basket.
              justification: Checkout is the purpose of the shop.
              tags:
                - system
              children:
                - Shop-Cart-Total
            - id: Shop-Cart-Total
              title: The cart shall total the prices of its items.
              justification: A customer must see what they will pay.
              tests:
                - Cart_Total_SumsItems
  - name: lint
    kind: command
    language: shell
    description: Check the file. Prints nothing and exits 0 when it is clean.
    content: '{{tool}} --requirements requirements.yaml --lint'
  - name: report
    kind: command
    language: shell
    description: Write the requirements as a Markdown table per section.
    content: '{{tool}} --requirements requirements.yaml --report requirements-report.md'
  - name: justifications
    kind: command
    language: shell
    description: Write the reason each requirement exists.
    content: '{{tool}} --requirements requirements.yaml --justifications justifications.md'
---
{{tool}} {{version}} reads requirements from YAML files, links them to the tests that prove them, and writes Markdown
reports. It only reads your files and writes the report files you name.

1. **Install it.** The tool needs the .NET 10 SDK or later. In CI, set it
   up first when the runner has an older .NET, for example with
   `actions/setup-dotnet` and `dotnet-version: 10.x`.
   For a repository or CI, install it as a local tool. Run the commands
   that [[#install-local]] prints once, without
   `dotnet new tool-manifest` when `.config/dotnet-tools.json` exists, and
   commit that file. In CI, run `dotnet tool restore` first, then run the
   tool as `dotnet {{tool}}`, as in the commands that
   [[#run-in-ci]] prints. For one machine, install it globally and
   run it as `{{tool}}`:

   ```bash
   dotnet tool install -g DemaConsulting.ReqStream
   ```

2. **Write a requirements file.** Save the file printed by [[#requirements]] as `requirements.yaml`. A requirement
   needs an `id` and a `title`, and should have a `justification` that says why it exists; [[requirements-file]]
   lists every field.
3. **Lint it.** [[#lint]] prints nothing and exits 0 when the file is clean. A problem prints as
   `path(line,column): error: message` and the exit code is 1. See [[lint]].
4. **Write reports.** [[#report]] lists the requirements and [[#justifications]] lists why each exists.
   See [[reports]].
5. **Prove the requirements.** Name the tests in each requirement ([[test-linking]]), point `--tests` at the result
   files ([[test-results]]), and add `--enforce` in CI ([[enforcement]]).

Every run needs `--requirements`. A pattern that matches no file is not an error: the tool prints
`No requirements files specified.` and exits 0, so check the path when a run does nothing.
