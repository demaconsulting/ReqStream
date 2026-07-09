## Tracing

![Tracing Structure](TracingView.svg)

### Overview

The `Tracing` subsystem provides test result loading and requirement-to-test traceability
for ReqStream. It reads test result files in TRX or JUnit XML format, correlates each test
result with the requirements that reference it, and produces a trace matrix report or coverage
enforcement decision. Its boundaries begin where the `Modeling` subsystem ends: it receives an
already-validated `Requirements` tree and test-result file paths, and produces coverage analysis
and Markdown report output.

The `Tracing` subsystem contains the following software unit:

- **TraceMatrix** (`Tracing/TraceMatrix.cs`) — Test result loading, requirement mapping, and
  coverage enforcement.

### Interfaces

**TraceMatrix constructor**: Loads test results and maps them to requirements.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Accepts a `Requirements` tree and test result file paths; populates internal lookup
  structures. Throws `FileNotFoundException` for missing files; throws
  `InvalidOperationException` for unparseable files.
- *Constraints*: Construction is the only phase that performs I/O.

**TraceMatrix.Export**: Exports the trace matrix to a Markdown report.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Accepts `filePath`, `depth`, and optional `filterTags`; writes a Markdown report
  with Summary, Requirements, and Testing sections. The `depth` parameter controls the Markdown
  heading level (valid range 1–6, matching ATX heading levels `#` through `######`); when omitted
  the default is 1 (top-level `#` headings).
- *Constraints*: Throws `ArgumentException` for null/empty path.

**TraceMatrix.CalculateSatisfiedRequirements**: Returns satisfied and total requirement counts.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Returns `(satisfied, total)` tuple subject to `filterTags` filtering.
- *Constraints*: Read-only; does not throw.

**TraceMatrix.GetUnsatisfiedRequirements**: Returns IDs of unsatisfied requirements.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Returns a list of requirement IDs not covered by passing tests.
- *Constraints*: Read-only; does not throw.

**TraceMatrix.GetTestResult**: Returns aggregated pass/fail metrics for a named test.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Accepts `testName` (may include a source filter as `"source@testname"`); returns
  a `TestMetrics` value with aggregated pass and fail counts across all matching executions.
  Returns `TestMetrics(0, 0)` when the test name is not found — callers do not need to
  null-check the result.
- *Constraints*: Read-only; does not throw.

**TraceMatrix.GetAllTestResults**: Returns metrics for all tests referenced in requirements.

- *Type*: In-process .NET public API.
- *Role*: Provider.
- *Contract*: Returns an `IReadOnlyDictionary<string, TestMetrics>` mapping each test name
  (referenced by at least one requirement) to its aggregated `TestMetrics`. Only tests that
  have been executed (i.e., `Executed > 0`) are included in the returned dictionary.
- *Constraints*: Read-only; does not throw.

### Design

The `Tracing` subsystem contains a single unit, `TraceMatrix`. Its internal design follows a
two-phase construction-then-query pattern:

1. **Construction phase** — the `TraceMatrix` constructor calls `ProcessTestResultFile` for each
   path in `testResultFiles`. Each call deserializes the file via
   `DemaConsulting.TestResults.IO.Serializer` and accumulates `TestExecution` records into
   `_testExecutions`. After construction, the lookup structures are fully populated and read-only.
2. **Query phase** — `Program` calls `GetTestResult`, `GetAllTestResults`,
   `CalculateSatisfiedRequirements`, `GetUnsatisfiedRequirements`, and `Export` in any order.
   All query methods are read-only; they do not modify internal state.

The subsystem raises `FileNotFoundException` when a test result file does not exist and
`InvalidOperationException` when a file cannot be parsed. Both exceptions propagate to `Program`
for display as fatal errors.
