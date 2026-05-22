## WeasyPrint Verification

### Required Functionality

WeasyPrint (`ReqStream-OTS-WeasyPrint`) shall convert HTML documents to valid PDF. The
`DemaConsulting.WeasyPrintTool` wrapper converts HTML documents to PDF as part of the
documentation build pipeline.

### Verification Approach

WeasyPrint is verified by CI pipeline step evidence combined with FileAssert document
validation. Each HTML document (build notes, code quality report, review plan, review report,
design document, user guide, requirements document, requirements report, and verification
document) is converted to PDF by WeasyPrint in the CI pipeline. FileAssert then asserts that each generated
PDF file exists, has a non-trivial size, contains at least one page, and includes expected
document content in the rendered text. Passing FileAssert assertions confirm WeasyPrint
executed correctly and produced meaningful output.

Note: `WeasyPrint_RequirementsPdf` and `WeasyPrint_TraceMatrixPdf` are excluded from OTS
evidence because they depend on ReqStream output (the requirements PDF and trace matrix PDF
are generated from ReqStream output). These tests cannot serve as pre-ReqStream qualification
evidence; they exercise ReqStream functionality rather than WeasyPrint independently.

Test evidence names:

- `WeasyPrint_BuildNotesPdf` — build-notes PDF document validated
- `WeasyPrint_CodeQualityPdf` — code quality PDF document validated
- `WeasyPrint_ReviewPlanPdf` — review plan PDF document validated
- `WeasyPrint_ReviewReportPdf` — review report PDF document validated
- `WeasyPrint_DesignPdf` — design document PDF validated
- `WeasyPrint_UserGuidePdf` — user guide PDF document validated
- `WeasyPrint_VerificationPdf` — verification document PDF validated

### Coverage Summary

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-OTS-WeasyPrint` | `WeasyPrint_BuildNotesPdf` |
| `ReqStream-OTS-WeasyPrint` | `WeasyPrint_CodeQualityPdf` |
| `ReqStream-OTS-WeasyPrint` | `WeasyPrint_ReviewPlanPdf` |
| `ReqStream-OTS-WeasyPrint` | `WeasyPrint_ReviewReportPdf` |
| `ReqStream-OTS-WeasyPrint` | `WeasyPrint_DesignPdf` |
| `ReqStream-OTS-WeasyPrint` | `WeasyPrint_UserGuidePdf` |
| `ReqStream-OTS-WeasyPrint` | `WeasyPrint_VerificationPdf` |
