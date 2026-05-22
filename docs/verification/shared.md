# Shared Package Verification

## Verification Strategy

Shared packages used by ReqStream are verified using integration test evidence. Integration tests
in the main test project (`DemaConsulting.ReqStream.Tests`) exercise each shared package through
the specific features consumed by ReqStream. Passing tests confirm that the package behaves as
expected in the local execution environment.

## Overview

The ReqStream tool uses one shared package developed by DEMA Consulting. Shared packages are
maintained as separate NuGet packages and have design documentation in `docs/design/shared/`.
Verification evidence is collected from integration test execution.

## Verification Approach

Each shared package is verified using integration test evidence. The shared package is exercised
indirectly by test methods that depend on its correct operation. Passing tests confirm the package
delivered the expected results.

Requirements for each shared package are defined in the corresponding
`docs/reqstream/shared/{name}.yaml` file. For DemaConsulting.TestResults, the requirements
are at `docs/reqstream/shared/dema-consulting-test-results.yaml`. Test evidence is recorded in the ReqStream requirements
traceability matrix.

## Shared Package Summary

| Shared Package | Primary Evidence Type |
| --- | --- |
| DemaConsulting.TestResults | Integration test evidence |

## Qualification Evidence

Qualification evidence for each shared package is integration test results. The shared package is
exercised indirectly through unit and integration tests that depend on its correct operation. The
TRX and JUnit XML result files published by the CI pipeline are the evidence artifacts.

The specific evidence for each shared package is documented in its individual verification file
under `docs/verification/shared/`.

## Regression Approach

When a shared package version is updated:

1. Review the release notes and changelog for breaking changes or known issues.
2. Re-run the full test suite via `build.ps1` to confirm all integration tests still pass.
3. Update the version entry in `.versionmark.yaml` and the shared package requirements file.
