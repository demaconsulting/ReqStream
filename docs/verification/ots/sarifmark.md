## SarifMark Verification

### Required Functionality

SarifMark (`ReqStream-OTS-SarifMark`) shall convert CodeQL SARIF results into a
Markdown report. The `DemaConsulting.SarifMark` tool reads the SARIF output produced by
CodeQL code scanning and renders it as a human-readable Markdown document included in the
release artifacts.

### Verification Approach

SarifMark is verified by CI pipeline step evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file.
The TRX file is consumed by ReqStream to satisfy the OTS requirement.

Test evidence names (test methods written to the TRX file by `dotnet sarifmark --validate`):

- `SarifMark_SarifReading` — validates that SarifMark can read SARIF input
- `SarifMark_MarkdownReportGeneration` — validates that SarifMark can generate a Markdown report

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-SarifMark` | `SarifMark_SarifReading` |
| `ReqStream-OTS-SarifMark` | `SarifMark_MarkdownReportGeneration` |
