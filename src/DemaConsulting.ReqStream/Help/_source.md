---
name: reqstream
title: ReqStream
summary: Requirements traceability from YAML files, test results, and reports.
---
{{tool}} reads requirements from YAML files, links each requirement to the tests that prove it, and writes Markdown
reports. In a build pipeline it fails the build when a requirement has no passing test or is not traced to a root
requirement.

Every command is a set of options; there are no sub-commands and no positional arguments. A run reads requirements with
`--requirements`, may add test results with `--tests`, and writes reports with `--report`, `--justifications`, and
`--matrix`.
