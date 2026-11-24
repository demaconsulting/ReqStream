---
title: Source filters
summary: Accept a test result from one platform or runtime only.
aliases:
  - source
  - platform
tags:
  - reference
see-also:
  - test-linking
  - troubleshooting/test-errors
examples:
  - name: platform-requirements
    kind: source
    language: yaml
    file: requirements.yaml
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
  - name: windows-results
    kind: source
    language: xml
    file: windows-net10.0-results.xml
    description: Results from the Windows job. The file name contains both windows and net10.0.
    content: |
      <testsuites name="windows">
        <testsuite name="Shop.Tests" tests="2" failures="0">
          <testcase name="Paths_Read_HandlesBackslashes" classname="Shop.Tests.PathTests" time="0.010" />
          <testcase name="Runtime_Starts_Succeeds" classname="Shop.Tests.RuntimeTests" time="0.010" />
        </testsuite>
      </testsuites>
  - name: ubuntu-results
    kind: source
    language: xml
    file: ubuntu-net10.0-results.xml
    description: Results from the Linux job. The file name contains ubuntu.
    content: |
      <testsuites name="ubuntu">
        <testsuite name="Shop.Tests" tests="1" failures="0">
          <testcase name="Paths_Read_HandlesSlashes" classname="Shop.Tests.PathTests" time="0.010" />
        </testsuite>
      </testsuites>
  - name: matrix-filtered
    kind: command
    language: shell
    description: Both results files are read; each filtered test counts only from a matching file. Exits 0.
    content: '{{tool}} --requirements requirements.yaml --tests "*-results.xml" --matrix matrix.md --enforce'
---
Without a filter a test passes when any result file records it. That is wrong for platform requirements: a
Windows-only requirement would pass on a Linux run. Prefix the test name with a filter and the name of the result
file decides what counts, as in [[#platform-requirements]]:

- `windows@Paths_Read_HandlesBackslashes` counts only results from a file whose name contains `windows`.
- `net10.0@Runtime_Starts_Succeeds` counts only results from a file whose name contains `net10.0`.

**How the match works.** The text before the first `@` is a case-insensitive substring of the result file name
without its extension, such as `windows-net10.0-results` ([[#windows-results]], [[#ubuntu-results]]). It is not a
match against anything inside the file. The `@` must be neither first nor last in the entry; otherwise the whole entry
is read as a plain test name.

Name the result files after the job that produced them, for example `test-results-windows-latest.trx`, so that every
filter finds exactly the files it means. If no file name contains the filter, the test counts as not executed and the
requirement is unsatisfied ([[troubleshooting/test-errors]]).

Run [[#matrix-filtered]] with both files to see the filtered tests satisfied. The glob is in double quotes so the tool,
not the shell, expands it ([[test-results]]).
