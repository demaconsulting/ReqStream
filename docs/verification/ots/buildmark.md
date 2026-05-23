## BuildMark

### Required Functionality

BuildMark (`ReqStream-OTS-BuildMark`) shall generate build-notes documentation from
GitHub Actions metadata. It queries the GitHub API to capture workflow run details and renders
them as a Markdown build-notes document included in the release artifacts.

### Verification Approach

BuildMark is verified by a combination of self-validation and CI integration evidence.

BuildMark supports a `--validate` self-test command. The CI pipeline runs this step — "Run
BuildMark self-validation" — using `dotnet buildmark --validate`, producing the TRX result file
`artifacts/buildmark-self-validation.trx`. The `BuildMark_MarkdownReportGeneration` test result
is produced by that self-validation run and constitutes the primary qualification evidence.

The "Generate Build Notes" step provides complementary integration evidence: it invokes BuildMark
to query the GitHub API and write the Markdown build-notes document, confirming end-to-end
operation on the target platform. Structural correctness of the generated document is further
validated by the subsequent FileAssert step.

### Test Scenarios

**Markdown Report Generation**: Verifies that BuildMark self-validates successfully and its internal
report-generation test passes. This scenario is tested by `BuildMark_MarkdownReportGeneration`,
produced by the `dotnet buildmark --validate` self-validation run in the CI pipeline.

### Requirements Coverage

| Requirement | Scenario | Test Method(s) |
| --- | --- | --- |
| ReqStream-OTS-BuildMark | Markdown Report Generation | `BuildMark_MarkdownReportGeneration` |
