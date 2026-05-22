## SonarMark

### Required Functionality

SonarMark (`ReqStream-OTS-SonarMark`) shall generate a SonarCloud quality report. The
`DemaConsulting.SonarMark` tool retrieves quality-gate and metrics data from SonarCloud and
renders it as a Markdown document included in the release artifacts.

### Verification Approach

SonarMark is verified by CI pipeline step evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file.
The TRX file is consumed by ReqStream to satisfy the OTS requirement.

### Test Scenarios

**Quality Gate Retrieval**: Validates that SonarMark can retrieve quality gate data from
SonarCloud. This scenario is tested by `SonarMark_QualityGateRetrieval`.

**Issues Retrieval**: Validates that SonarMark can retrieve issue counts from SonarCloud. This
scenario is tested by `SonarMark_IssuesRetrieval`.

**Hotspots Retrieval**: Validates that SonarMark can retrieve hotspot data from SonarCloud. This
scenario is tested by `SonarMark_HotSpotsRetrieval`.

**Markdown Report Generation**: Validates that SonarMark can generate a Markdown report from
SonarCloud quality data. This scenario is tested by `SonarMark_MarkdownReportGeneration`.
