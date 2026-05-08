## FileAssert Verification

### Required Functionality

FileAssert (`ReqStream-OTS-FileAssert`) shall validate generated documents against
acceptance criteria. It validates HTML and PDF documents produced during the build, asserting
that each document exists, has a non-trivial size, is structurally valid, and contains
expected content. It also provides verification evidence for Pandoc and WeasyPrint.

### Verification Approach

FileAssert is verified using self-validation evidence. The tool is invoked with `--version`
and `--help` flags in the CI pipeline. A zero exit code and expected output confirm the tool
is operational before it validates the generated documents.

Test evidence names:

- `FileAssert_VersionDisplay` — confirms FileAssert responds correctly to `--version`
- `FileAssert_HelpDisplay` — confirms FileAssert responds correctly to `--help`

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-FileAssert` | `FileAssert_VersionDisplay`, `FileAssert_HelpDisplay` |
