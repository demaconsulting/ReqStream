# Tracing Subsystem Design

The `Tracing` subsystem provides test result loading and requirement-to-test traceability
for ReqStream. It maps test execution evidence to requirements and supports enforcement of
full test coverage.

## Overview

The `Tracing` subsystem reads test result files in TRX or JUnit XML format, correlates
each test result with the requirements that reference it, and produces a trace matrix report
or coverage enforcement decision.

## Units

The `Tracing` subsystem contains the following software unit:

| Unit          | File                    | Responsibility                                                       |
|---------------|-------------------------|----------------------------------------------------------------------|
| `TraceMatrix` | `Tracing/TraceMatrix.cs`| Test result loading, requirement mapping, and coverage enforcement.  |

## Interfaces

The `Tracing` subsystem exposes the following interface to the rest of the tool:

| Interface | Description |
| --- | --- |
| `TraceMatrix` constructor | Loads test results and maps them to requirements. |
| `TraceMatrix.Export` | Exports the trace matrix to a Markdown report. |
| `TraceMatrix.CalculateSatisfiedRequirements` | Returns satisfied and total requirement counts. |
| `TraceMatrix.GetUnsatisfiedRequirements` | Returns IDs of requirements not covered by passing tests. |
| `TraceMatrix.GetTestResult` | Returns pass/fail counts for a named test across results. |
| `TraceMatrix.GetAllTestResults` | Returns pass/fail `TestMetrics` for all tests referenced in requirements. |

## Interactions

| Dependency     | Direction | Purpose                                                                |
|----------------|-----------|------------------------------------------------------------------------|
| `Context`      | Uses      | Receives test file paths from `Context.TestFiles`.                     |
| `Requirements` | Uses      | Receives the requirement tree to map tests to requirements.            |
| `Program`      | Used by   | Constructs `TraceMatrix` and calls enforcement/export methods.         |

## Error Handling

The `Tracing` subsystem raises the following exceptions at the subsystem boundary. Both
exceptions are thrown by the `TraceMatrix` constructor and propagate to `Program` for
display as fatal errors.

- **`FileNotFoundException`** — A path supplied in `testResultFiles` does not exist on disk.
  The exception message includes the offending file path.
- **`InvalidOperationException`** — A test result file exists but cannot be parsed
  (malformed TRX or JUnit XML). The exception message includes the offending file path;
  the original parse exception is available as the inner exception.

For the full error-handling design of `ProcessTestResultFile`, see the TraceMatrix unit design documentation.
