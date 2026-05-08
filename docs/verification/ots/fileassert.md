## FileAssert Verification

### Required Functionality

FileAssert (`ReqStream-OTS-FileAssert`) shall validate generated documents against
acceptance criteria. It validates HTML and PDF documents produced during the build, asserting
that each document exists, has a non-trivial size, is structurally valid, and contains
expected content. It also provides verification evidence for Pandoc and WeasyPrint.

### Verification Approach

FileAssert is verified by CI pipeline step evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file.
The TRX file is consumed by ReqStream to satisfy the OTS requirement.

Test evidence names (test methods written to the TRX file by `dotnet fileassert --validate`):

- `FileAssert_VersionDisplay` — validates that FileAssert responds correctly to `--version`
- `FileAssert_HelpDisplay` — validates that FileAssert responds correctly to `--help`

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-FileAssert` | `FileAssert_VersionDisplay`, `FileAssert_HelpDisplay` |
