## FileAssert Verification

### Required Functionality

FileAssert (`ReqStream-OTS-FileAssert`) shall validate generated documents against
acceptance criteria. It validates HTML and PDF documents produced during the build, asserting
that each document exists, has a non-trivial size, is structurally valid, and contains
expected content. It also provides verification evidence for Pandoc and WeasyPrint.

### Verification Approach

FileAssert is verified by CI pipeline step evidence using two complementary evidence types:

1. **Self-validation**: The tool's built-in `--validate` command is executed in the CI
   pipeline and writes test method results to a TRX file. This confirms that FileAssert
   itself is operational on the target platform.

2. **Document validation acceptance test**: FileAssert is invoked by the CI pipeline to
   validate the generated design HTML document (`Pandoc_DesignHtml`). A passing result
   confirms that FileAssert's document-validation logic — file existence, structural
   validity, and content assertion — operates correctly end-to-end.

Test evidence names:

- `FileAssert_VersionDisplay` — validates that FileAssert responds correctly to `--version` (self-validation)
- `FileAssert_HelpDisplay` — validates that FileAssert responds correctly to `--help` (self-validation)
- `Pandoc_DesignHtml` — validates that FileAssert successfully validates a real HTML document (document validation acceptance)

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-FileAssert` | `FileAssert_VersionDisplay`, `FileAssert_HelpDisplay`, `Pandoc_DesignHtml` |
