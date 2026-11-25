---
title: Source filters
summary: Accept a test result from one platform, such as windows, or runtime only.
aliases:
  - source
  - platform
tags:
  - reference
see-also:
  - test-linking
  - troubleshooting/test-errors
examples:
  - name: requirements
    kind: source
    language: yaml
    save-as: requirements.yaml
    description: Two requirements that need evidence from a specific platform and runtime.
    content: |
      root-tags:
        - system
      sections:
        - title: Shop Requirements
          requirements:
            - id: Shop-System-Portability
              title: The shop shall run on every supported platform.
              tags:
                - system
              children:
                - Shop-Platform-Paths
                - Shop-Platform-Runtime
            - id: Shop-Platform-Paths
              title: The shop shall read data files on Windows and Linux.
              tests:
                - windows@Paths_Read_HandlesBackslashes
                - ubuntu@Paths_Read_HandlesSlashes
            - id: Shop-Platform-Runtime
              title: The shop shall start on .NET 10.
              tests:
                - net10.0@Runtime_Starts_Succeeds
  - name: matrix
    kind: command
    language: shell
    description: Both result files are read, windows-net10.0-results.xml from the Windows job and ubuntu-net10.0-results.xml from the Linux job; each filtered test counts only from a matching file. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests "*-results.xml" --matrix matrix.md --enforce'
---
Without a filter a test passes when any result file records it. That is wrong for platform requirements: a
Windows-only requirement would pass on a Linux run. Prefix the test name with a filter and the name of the result
file decides what counts, as in [[#requirements]]:

- `windows@Paths_Read_HandlesBackslashes` counts only results from a file whose name contains `windows`.
- `net10.0@Runtime_Starts_Succeeds` counts only results from a file whose name contains `net10.0`.

**How the match works.** The text before the first `@` is a case-insensitive substring of the result file name
without its extension, such as `windows-net10.0-results`. It is not a match against anything inside the file. The `@`
must be neither first nor last in the entry; otherwise the whole entry is read as a plain test name.

In [[#matrix]] the Windows job wrote `windows-net10.0-results.xml`, which records
`Paths_Read_HandlesBackslashes` and `Runtime_Starts_Succeeds`, and the Linux job wrote `ubuntu-net10.0-results.xml`,
which records `Paths_Read_HandlesSlashes`. Each job's test run writes its own file, for example with
`dotnet test --logger trx`.

Name the result files after the job that produced them, for example `test-results-windows-latest.trx`, so that every
filter finds exactly the files it means. If no file name contains the filter, the test counts as not executed and the
requirement is unsatisfied ([[troubleshooting/test-errors]]).

Run [[#matrix]] with both files to see the filtered tests satisfied. The glob is in double quotes so the tool,
not the shell, expands it ([[test-results]]).
