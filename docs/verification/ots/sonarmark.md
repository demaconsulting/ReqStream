## SonarMark Verification

### Required Functionality

SonarMark (`ReqStream-OTS-SonarMark`) shall generate a SonarCloud quality report. The
`DemaConsulting.SonarMark` tool retrieves quality-gate and metrics data from SonarCloud and
renders it as a Markdown document included in the release artifacts.

### Verification Approach

SonarMark is verified by CI pipeline step evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file.
The TRX file is consumed by ReqStream to satisfy the OTS requirement.

Test evidence names (test methods written to the TRX file by `dotnet sonarmark --validate`):

- `SonarMark_QualityGateRetrieval` — validates that SonarMark can retrieve quality gate data
- `SonarMark_IssuesRetrieval` — validates that SonarMark can retrieve issue counts
- `SonarMark_HotSpotsRetrieval` — validates that SonarMark can retrieve hotspot data
- `SonarMark_MarkdownReportGeneration` — validates that SonarMark can generate a Markdown report

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-SonarMark` | `SonarMark_QualityGateRetrieval` |
| `ReqStream-OTS-SonarMark` | `SonarMark_IssuesRetrieval` |
| `ReqStream-OTS-SonarMark` | `SonarMark_HotSpotsRetrieval` |
| `ReqStream-OTS-SonarMark` | `SonarMark_MarkdownReportGeneration` |
