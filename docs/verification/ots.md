# OTS Software Verification

## Verification Strategy

OTS items used by ReqStream are verified using one or more of the following evidence types:
self-validation (invoking the tool with `--validate` where supported), CI pipeline step
evidence (successful execution as a named pipeline step), and integration test evidence
(xUnit tests that depend on the item's correct operation). The evidence type for each item is
selected based on the capabilities the tool exposes and the depth of qualification required.

Full details of the verification approach, OTS item summary, and qualification evidence are
provided in the sections below. Per-item verification files are in `docs/verification/ots/`.

## Overview

The ReqStream tool uses twelve OTS (Off-The-Shelf) software items to provide build, test,
documentation, and quality-reporting functionality. OTS items are not developed in-house and
have no design documentation. Verification evidence is collected from CI pipeline run results,
self-validation output, and integration test execution rather than from unit tests of internal
implementation.

## Verification Approach

Each OTS item is verified using one or more of the following evidence types:

- **Self-validation**: The OTS tool is invoked with a `--validate` flag (where supported) on
  the target platform. A zero exit code and expected console output confirm the tool is
  operational.
- **CI pipeline step evidence**: The OTS tool runs as a named step in the GitHub Actions
  pipeline. A successful pipeline run is proof the tool executed without error.
- **Integration test evidence**: The OTS tool is exercised indirectly by test methods that
  depend on its correct operation. Passing tests confirm the tool delivered the expected results.

Requirements for each OTS item are defined in the corresponding `docs/reqstream/ots/{name}.yaml`
file. Test evidence is recorded in the ReqStream requirements traceability matrix.

## OTS Item Summary

The following table lists all OTS items and their primary evidence type. Full verification
details for each item are provided in the individual OTS item verification documents under
`docs/verification/ots/`.

| OTS Item | Primary Evidence Type |
| --- | --- |
| BuildMark | CI pipeline step evidence |
| FileAssert | Self-validation |
| xUnit | Integration test evidence |
| Pandoc | CI pipeline step evidence combined with FileAssert document validation |
| ReviewMark | CI pipeline step evidence |
| SarifMark | CI pipeline step evidence |
| SonarMark | CI pipeline step evidence |
| VersionMark | CI pipeline step evidence |
| WeasyPrint | CI pipeline step evidence combined with FileAssert document validation |
| YamlDotNet | Integration test evidence |
| Microsoft.Extensions.FileSystemGlobbing | Integration test evidence |
| DemaConsulting.TestResults | Integration test evidence |

## OTS Coverage Table Convention

OTS verification documents use a two-column requirements coverage table
(`Requirement ID | Test Method(s)`) rather than the three-column table used for ReqStream units
(`Requirement ID | Scenario(s) | Test Method(s)`). This is a deliberate convention because OTS
requirements are verified through pipeline or acceptance evidence rather than named unit-test
scenarios. The acceptance testing approach — CI pipeline invocation, `--validate` command output,
or integration test execution — serves as the scenario context and is described in the
Verification Approach section of each OTS item's verification document.

When a requirement is linked to multiple test methods, each method occupies its own row in
the coverage table (one row per method). This makes the table more scannable and avoids
long comma-separated lists in a single cell. For example:

```markdown
| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-XUnit` | `Test_Method_One` |
| `ReqStream-OTS-XUnit` | `Test_Method_Two` |
```

## OTS Section Heading Convention

OTS verification documents open with a `### Required Functionality` section that summarises
what the OTS item must do for this project. This heading is a project convention at OTS level;
the standard section name `Purpose` used for ReqStream units is not applied to OTS items because
OTS items are not decomposed to the same level of detail.

## Qualification Evidence

Qualification evidence for each OTS item is one or more of the following artifact types:

- **Self-validation report**: The OTS tool is invoked with a `--validate` flag where supported. A
  zero exit code and expected console output confirm the tool is operational on the target platform.
- **CI pipeline step evidence**: The OTS tool runs as a named step in the GitHub Actions workflow. A
  successful run on the release commit is the evidence artifact.
- **Integration test results**: The OTS tool is exercised indirectly through unit and integration
  tests that depend on its correct operation. The TRX and JUnit XML result files published by the CI
  pipeline are the evidence artifacts.

The specific evidence type for each OTS item is documented in its individual verification file under
`docs/verification/ots/`.

## Regression Approach

When an OTS item version is updated:

1. Review the vendor release notes and changelog for breaking changes or known issues.
2. Re-run the full test suite via `build.ps1` to confirm all integration tests still pass.
3. Re-run any self-validation commands for tools that support `--validate`.
4. Confirm that CI pipeline steps that depend on the updated tool still succeed.
5. Update the version entry in `.versionmark.yaml` and the OTS requirements file.

Patch-level updates with no breaking changes and no relevant bug fixes do not require
re-qualification beyond confirming the CI pipeline passes. Minor and major version updates
require the full regression procedure above.
