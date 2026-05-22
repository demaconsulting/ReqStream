## Tracing Subsystem Design

### Overview

The `Tracing` subsystem provides test result loading and requirement-to-test traceability
for ReqStream. It reads test result files in TRX or JUnit XML format, correlates each test
result with the requirements that reference it, and produces a trace matrix report or coverage
enforcement decision. Its boundaries begin where the `Modeling` subsystem ends: it receives an
already-validated `Requirements` tree and test-result file paths, and produces coverage analysis
and Markdown report output.

The `Tracing` subsystem contains the following software unit:

| Unit | File | Responsibility |
| ---- | ---- | -------------- |
| `TraceMatrix` | `Tracing/TraceMatrix.cs` | Test result loading, requirement mapping, and coverage enforcement. |

### Interfaces

The `Tracing` subsystem exposes the following interface to the rest of the tool:

| Interface | Description |
| --- | --- |
| `TraceMatrix` constructor | Loads test results and maps them to requirements. |
| `TraceMatrix.Export` | Exports the trace matrix to a Markdown report. |
| `TraceMatrix.CalculateSatisfiedRequirements` | Returns satisfied and total requirement counts. |
| `TraceMatrix.GetUnsatisfiedRequirements` | Returns IDs of requirements not covered by passing tests. |
| `TraceMatrix.GetTestResult` | Returns pass/fail counts for a named test across results. |
| `TraceMatrix.GetAllTestResults` | Returns pass/fail `TestMetrics` for all tests referenced in requirements. |

### Design

The `Tracing` subsystem contains a single unit, `TraceMatrix`. Its internal design is a
two-phase construction-then-query pattern:

1. **Construction phase** — the `TraceMatrix` constructor calls `ProcessTestResultFile` for each
   path in `testResultFiles`. Each call deserializes the file and accumulates `TestExecution`
   records into `_testExecutions`. After construction, the lookup structures are fully populated
   and read-only.
2. **Query phase** — `Program` calls `GetTestResult`, `GetAllTestResults`,
   `CalculateSatisfiedRequirements`, `GetUnsatisfiedRequirements`, and `Export` in any order.
   All query methods are read-only; they do not modify `_testExecutions` or `_requirements`.

The subsystem has no internal unit-to-unit collaboration beyond this single unit.

### Interactions

| Unit | Relationship | Description |
| ---- | ------------ | ----------- |
| `Context` | Uses | Receives test file paths from `Context.TestFiles`. |
| `Requirements` | Uses | Receives the requirement tree to map tests to requirements. |
| `Program` | Used by | Constructs `TraceMatrix` and calls enforcement/export methods. |

### Error Handling

The `Tracing` subsystem raises the following exceptions at the subsystem boundary. Both
exceptions are thrown by the `TraceMatrix` constructor and propagate to `Program` for
display as fatal errors.

- **`FileNotFoundException`** — A path supplied in `testResultFiles` does not exist on disk.
  The exception message includes the offending file path.
- **`InvalidOperationException`** — A test result file exists but cannot be parsed
  (malformed TRX or JUnit XML). The exception message includes the offending file path;
  the original parse exception is available as the inner exception.

For the full error-handling design of `ProcessTestResultFile`, see the TraceMatrix unit design documentation.
