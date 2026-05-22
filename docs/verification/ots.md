# OTS Software Verification

## Verification Strategy

OTS items used by ReqStream are verified using one or more of the following evidence types:
self-validation (invoking the tool with `--validate` where supported), CI pipeline step
evidence (successful execution as a named pipeline step), and integration test evidence
(xUnit tests that depend on the item's correct operation). The evidence type for each item is
selected based on the capabilities the tool exposes and the depth of qualification required.

Per-item verification files are in `docs/verification/ots/`.

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

The following table lists all OTS items and their primary evidence type.

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
