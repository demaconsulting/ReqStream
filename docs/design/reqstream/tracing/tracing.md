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

| Interface                                    | Direction | Description                                               |
|----------------------------------------------|-----------|-----------------------------------------------------------|
| `TraceMatrix` constructor                    | Outbound  | Loads test results and maps them to requirements.         |
| `TraceMatrix.Export`                         | Outbound  | Exports the trace matrix to a Markdown report.            |
| `TraceMatrix.CalculateSatisfiedRequirements` | Outbound  | Returns satisfied and total requirement counts.           |
| `TraceMatrix.GetUnsatisfiedRequirements`     | Outbound  | Returns IDs of requirements not covered by passing tests. |
| `TraceMatrix.GetTestResult`                  | Outbound  | Returns pass/fail counts for a named test across results. |

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

| Exception | Trigger | Detail |
|-----------|---------|--------|
| `FileNotFoundException` | A path supplied in `testResultFiles` does not exist on disk. | The exception message includes the offending file path. |
| `InvalidOperationException` | A test result file exists but cannot be parsed (malformed TRX or JUnit XML). | The exception message includes the offending file path; the original parse exception is available as the inner exception. |

For the full error-handling design of `ProcessTestResultFile`, see [TraceMatrix Unit Design][tm].

## References

- [ReqStream System Design][arch]
- [TraceMatrix Unit Design][tm]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[tm]: trace-matrix.md
[repo]: https://github.com/demaconsulting/ReqStream
