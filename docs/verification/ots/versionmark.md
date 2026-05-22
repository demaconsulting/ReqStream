## VersionMark

### Required Functionality

VersionMark (`ReqStream-OTS-VersionMark`) shall publish captured tool-version
information. The `DemaConsulting.VersionMark` tool reads version metadata for each
`dotnet tool` used in the pipeline and writes a versions Markdown document included in the
release artifacts.

### Verification Approach

VersionMark is verified by CI pipeline step evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file.
The TRX file is consumed by ReqStream to satisfy the OTS requirement.

### Test Scenarios

**Version Capture**: Validates that VersionMark can capture tool version metadata for all
configured tools. This scenario is tested by `VersionMark_CapturesVersions`.

**Markdown Report Generation**: Validates that VersionMark can generate a Markdown report from
captured version metadata. This scenario is tested by `VersionMark_GeneratesMarkdownReport`.

### Requirements Coverage

| Requirement | Scenario | Test Method(s) |
| --- | --- | --- |
| ReqStream-OTS-VersionMark | Version Capture | `VersionMark_CapturesVersions` |
| ReqStream-OTS-VersionMark | Markdown Report Generation | `VersionMark_GeneratesMarkdownReport` |
