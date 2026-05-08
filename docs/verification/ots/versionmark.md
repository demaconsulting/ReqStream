## VersionMark Verification

### Required Functionality

VersionMark (`ReqStream-OTS-VersionMark`) shall publish captured tool-version
information. The `DemaConsulting.VersionMark` tool reads version metadata for each
`dotnet tool` used in the pipeline and writes a versions Markdown document included in the
release artifacts.

### Verification Approach

VersionMark is verified by CI pipeline step evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file.
The TRX file is consumed by ReqStream to satisfy the OTS requirement.

Test evidence names (test methods written to the TRX file by `dotnet versionmark --validate`):

- `VersionMark_CapturesVersions` — validates that VersionMark can capture tool version metadata
- `VersionMark_GeneratesMarkdownReport` — validates that VersionMark can generate a Markdown report

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-VersionMark` | `VersionMark_CapturesVersions`, `VersionMark_GeneratesMarkdownReport` |
