## WeasyPrint

### Required Functionality

WeasyPrint (`ReqStream-OTS-WeasyPrint`) shall convert HTML documents to valid PDF. The
`DemaConsulting.WeasyPrintTool` wrapper converts HTML documents to PDF as part of the
documentation build pipeline.

### Verification Approach

WeasyPrint is verified by CI pipeline step evidence combined with FileAssert document
validation. Each HTML document (build notes, code quality report, review plan, review report,
design document, user guide, and verification document) is converted to PDF by WeasyPrint in the
CI pipeline. FileAssert then asserts that each generated PDF file exists, has a non-trivial size,
contains at least one page, and includes expected document content in the rendered text. Passing
FileAssert assertions confirm WeasyPrint executed correctly and produced meaningful output.

Note: `WeasyPrint_RequirementsPdf` and `WeasyPrint_TraceMatrixPdf` are excluded from OTS
evidence because they depend on ReqStream output (the requirements PDF and trace matrix PDF
are generated from ReqStream output). These tests cannot serve as pre-ReqStream qualification
evidence; they exercise ReqStream functionality rather than WeasyPrint independently.

### Test Scenarios

**Build Notes PDF**: Verifies that WeasyPrint converts the build-notes HTML document to a valid
PDF. This scenario is tested by `WeasyPrint_BuildNotesPdf`.

**Code Quality PDF**: Verifies that WeasyPrint converts the code quality HTML document to a valid
PDF. This scenario is tested by `WeasyPrint_CodeQualityPdf`.

**Review Plan PDF**: Verifies that WeasyPrint converts the review plan HTML document to a valid
PDF. This scenario is tested by `WeasyPrint_ReviewPlanPdf`.

**Review Report PDF**: Verifies that WeasyPrint converts the review report HTML document to a
valid PDF. This scenario is tested by `WeasyPrint_ReviewReportPdf`.

**Design Document PDF**: Verifies that WeasyPrint converts the design document HTML to a valid
PDF. This scenario is tested by `WeasyPrint_DesignPdf`.

**User Guide PDF**: Verifies that WeasyPrint converts the user guide HTML document to a valid PDF.
This scenario is tested by `WeasyPrint_UserGuidePdf`.

**Verification Document PDF**: Verifies that WeasyPrint converts the verification document HTML to
a valid PDF. This scenario is tested by `WeasyPrint_VerificationPdf`.

### Requirements Coverage

| Requirement | Scenario | Test Method(s) |
| --- | --- | --- |
| ReqStream-OTS-WeasyPrint | Build Notes PDF | `WeasyPrint_BuildNotesPdf` |
| ReqStream-OTS-WeasyPrint | Code Quality PDF | `WeasyPrint_CodeQualityPdf` |
| ReqStream-OTS-WeasyPrint | Review Plan PDF | `WeasyPrint_ReviewPlanPdf` |
| ReqStream-OTS-WeasyPrint | Review Report PDF | `WeasyPrint_ReviewReportPdf` |
| ReqStream-OTS-WeasyPrint | Design Document PDF | `WeasyPrint_DesignPdf` |
| ReqStream-OTS-WeasyPrint | User Guide PDF | `WeasyPrint_UserGuidePdf` |
| ReqStream-OTS-WeasyPrint | Verification Document PDF | `WeasyPrint_VerificationPdf` |
