---
title: Command-line options
summary: Every option, and how a run is dispatched.
aliases:
  - options
  - flags
  - cli
tags:
  - reference
order: 10
see-also:
  - command-line/self-validation
  - enforcement
examples:
  - name: version
    kind: command
    language: shell
    description: Print the version and exit.
    content: '{{tool}} --version'
  - name: usage
    kind: command
    language: shell
    description: Print the short usage text. The topics are listed by the help command.
    content: '{{tool}} --help'
---
Options start with `--` (or a single letter after `-`). Every operation is an option; the only subcommand is
`{{tool}} help`, which takes topic names, and there are no other positional arguments: any other unknown argument
stops the run with `Unsupported argument`, exit code 1.

| Option | Meaning |
| --- | --- |
| `-v`, `--version` | Print the version. |
| `-?`, `-h`, `--help` | Print the usage text. |
| `--silent` | Hide console output. Errors still set exit code 1. |
| `--log <file>` | Also write the output to a file. |
| `--validate`, `--results <file>` | Run the self-test; see [[command-line/self-validation]]. |
| `--requirements <glob>` | Requirements files to load. Repeatable. |
| `--lint` | Check the files only ([[lint]]). |
| `--tests <glob>` | Test result files, TRX or JUnit. Repeatable ([[test-results]]). |
| `--report`, `--justifications`, `--matrix <file>` | Reports ([[reports]]). |
| `--depth <n>` | First heading level of all reports; default 1. |
| `--report-depth`, `--justifications-depth`, `--matrix-depth <n>` | Heading level of one report. |
| `--matrix-titles` | Add a title column to the matrix. |
| `--filter <tags>` | Keep requirements with any listed tag ([[reports/filter]]). |
| `--root-tags <tags>` | Add root tags for the orphan check ([[orphans]]). |
| `--enforce` | Fail on unproven or orphaned requirements ([[enforcement]]). |

**Dispatch order.** The first matching mode wins: `--version`, `--help`, `--validate`, `--lint`, then normal processing.
`--lint` therefore ignores `--report`, `--tests`, and `--matrix`.

**Globs.** Put every pattern in double quotes so the tool expands it: `**` crosses folders, `*` matches within a
name. Relative patterns start at the current folder. A pattern that matches no file is silent: a run with no
requirements prints `No requirements files specified.` and exits 0. `Reading N requirements file(s)` and
`Processing N test result file(s)` count matched files, so included requirements files are not counted.

**Repeating options.** `--requirements`, `--tests`, `--filter`, and `--root-tags` add up when given more than once.
Tag lists are comma-separated without spaces.
