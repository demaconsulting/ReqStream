## SarifMark

### Required Functionality

SarifMark (`ReqStream-OTS-SarifMark`) shall convert CodeQL SARIF results into a
Markdown report. The `DemaConsulting.SarifMark` tool reads the SARIF output produced by
CodeQL code scanning and renders it as a human-readable Markdown document included in the
release artifacts.

### Verification Approach

SarifMark is verified by CI pipeline step evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file.
The TRX file is consumed by ReqStream to satisfy the OTS requirement.

### Test Scenarios

**SARIF Reading**: Validates that SarifMark can read and parse SARIF input produced by CodeQL.
This scenario is tested by `SarifMark_SarifReading`.

**Markdown Report Generation**: Validates that SarifMark can generate a Markdown report from
parsed SARIF data. This scenario is tested by `SarifMark_MarkdownReportGeneration`.
