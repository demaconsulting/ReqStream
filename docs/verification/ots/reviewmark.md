## ReviewMark Verification

### Required Functionality

ReviewMark (`ReqStream-OTS-ReviewMark`) shall generate a review plan and review report
from the review configuration. The `DemaConsulting.ReviewMark` tool reads `.reviewmark.yaml`
and the review evidence store to produce a review plan and review report documenting file
review coverage and currency.

### Verification Approach

ReviewMark is verified by CI pipeline step evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file.
The TRX file is consumed by ReqStream to satisfy the OTS requirement.

Test evidence names (test methods written to the TRX file by `dotnet reviewmark --validate`):

- `ReviewMark_ReviewPlanGeneration` — validates that ReviewMark can generate a review plan
- `ReviewMark_ReviewReportGeneration` — validates that ReviewMark can generate a review report

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-ReviewMark` | `ReviewMark_ReviewPlanGeneration` |
| `ReqStream-OTS-ReviewMark` | `ReviewMark_ReviewReportGeneration` |
