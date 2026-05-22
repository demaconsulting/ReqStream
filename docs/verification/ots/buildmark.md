## BuildMark Verification

### Required Functionality

BuildMark (`ReqStream-OTS-BuildMark`) shall generate build-notes documentation from
GitHub Actions metadata. It queries the GitHub API to capture workflow run details and renders
them as a Markdown build-notes document included in the release artifacts.

### Verification Approach

BuildMark is verified by CI pipeline step evidence. The tool runs as a named step in the
GitHub Actions pipeline that produces the release artifacts. A successful pipeline run
demonstrates that BuildMark executed without error and produced the required output.

BuildMark does not provide a `--validate` self-test command. CI pipeline step success is
therefore the primary and sufficient integration evidence: the GitHub Actions workflow logs
confirm that BuildMark ran, queried the GitHub API, and wrote the Markdown build-notes document
without error. Structural correctness of the output document is validated by the subsequent
FileAssert step.

The test evidence name `BuildMark_MarkdownReportGeneration` is linked to the pipeline step
that invokes BuildMark. A passing result confirms that the generated Markdown build-notes
document was produced.

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-BuildMark` | `BuildMark_MarkdownReportGeneration` |
