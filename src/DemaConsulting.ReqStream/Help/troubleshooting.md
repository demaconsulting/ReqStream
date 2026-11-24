---
title: Troubleshooting
summary: Find the fix for a failing or silent run.
aliases:
  - errors
  - faq
  - fix
tags:
  - troubleshooting
order: 11
see-also:
  - troubleshooting/requirements-errors
  - troubleshooting/test-errors
  - enforcement
examples:
  - name: no-files
    kind: command
    language: shell
    description: A pattern that matches nothing is not an error. The run prints one line and exits 0.
    content: '{{tool}} --requirements "nothing/*.yaml" --report report.md'
  - name: unknown-option
    kind: command
    language: shell
    description: An unknown option stops the run with an error and exit code 1.
    content: '{{tool}} --lint-all'
---
Find the symptom, then follow the link or the fix.

- `No requirements files specified.` with exit code 0: `--requirements` matched no file ([[#no-files]]). Check the
  path, run from the right folder, and quote the glob.
- `Unsupported argument 'x'`: an unknown or misspelled option, a stray value, or an unquoted glob that the shell
  expanded into several arguments ([[#unknown-option]]). Quote the glob.
- `--report requires a filename argument`: an option that takes a value was last, or its value is missing.
- `Unexpected error: Could not find a part of the path`: the folder of a report file does not exist. Create it
  first; reports never create folders.
- `path(line,col): error: ...`: a requirements file problem. See [[troubleshooting/requirements-errors]].
- `Only N of M requirements are satisfied with tests.`: a test is missing, failed, or does not match. See
  [[troubleshooting/test-errors]].
- `Cannot enforce requirements without test results or root tags`: `--enforce` had nothing to check. Add `--tests`
  or root tags ([[enforcement]]).
- `N of M requirements orphaned`: requirements that no root reaches. See [[orphans]].

**A run does nothing visible.** `--lint` is silent when it succeeds, and `--silent` hides all output; read the exit
code. Exit code 0 means success and 1 means any error.

**A report is empty.** A `--filter` that matches no tag, or requirements without tags, give an empty report
([[reports/filter]]).

When a message is not in this list, run the failing command with `--log run.log` to keep a copy, and fix the first
error first: later errors are often caused by it.
