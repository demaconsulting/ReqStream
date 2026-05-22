## ReviewMark

### Required Functionality

ReviewMark (`ReqStream-OTS-ReviewMark-Plan`, `ReqStream-OTS-ReviewMark-Report`) shall
generate review plans and review reports from the review configuration. The
`DemaConsulting.ReviewMark` tool reads `.reviewmark.yaml` and the review evidence store to
produce a review plan and review report documenting file review coverage and currency.

### Verification Approach

ReviewMark is verified by self-validation report evidence. The tool's built-in `--validate`
command is executed in the CI pipeline and writes test method results to a TRX file
(`artifacts/reviewmark-self-validation.trx`). ReqStream consumes that TRX file to map the
self-validation test methods to the OTS requirements.

### Test Scenarios

**Review Plan Generation**: Validates that ReviewMark can generate a review plan document from
the review configuration. This scenario is tested by `ReviewMark_ReviewPlanGeneration`.

**Review Report Generation**: Validates that ReviewMark can generate a review report document
from the review evidence store. This scenario is tested by `ReviewMark_ReviewReportGeneration`.

### Requirements Coverage

| Requirement | Scenario | Test Method(s) |
| --- | --- | --- |
| ReqStream-OTS-ReviewMark-Plan | Review Plan Generation | `ReviewMark_ReviewPlanGeneration` |
| ReqStream-OTS-ReviewMark-Report | Review Report Generation | `ReviewMark_ReviewReportGeneration` |
