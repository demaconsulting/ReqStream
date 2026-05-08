# OTS Software Verification

## Overview

The ReqStream tool uses nine OTS (Off-The-Shelf) software items to provide build, test,
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

| OTS Item    | Primary Evidence Type                                                  |
| ----------- | ---------------------------------------------------------------------- |
| BuildMark   | CI pipeline step evidence                                              |
| FileAssert  | Self-validation                                                        |
| xUnit       | Integration test evidence                                              |
| Pandoc      | CI pipeline step evidence combined with FileAssert document validation |
| ReviewMark  | CI pipeline step evidence                                              |
| SarifMark   | CI pipeline step evidence                                              |
| SonarMark   | CI pipeline step evidence                                              |
| VersionMark | CI pipeline step evidence                                              |
| WeasyPrint  | CI pipeline step evidence combined with FileAssert document validation |
