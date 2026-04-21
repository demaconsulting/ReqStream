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

| Interface                                    | Direction | Description                                       |
|----------------------------------------------|-----------|---------------------------------------------------|
| `TraceMatrix` constructor                    | Outbound  | Loads test results and maps them to requirements. |
| `TraceMatrix.Export`                         | Outbound  | Exports the trace matrix to a Markdown report.    |
| `TraceMatrix.CalculateSatisfiedRequirements` | Outbound  | Returns satisfied and total requirement counts.   |

## Interactions

| Dependency     | Direction | Purpose                                                                |
|----------------|-----------|------------------------------------------------------------------------|
| `Context`      | Uses      | Receives test file paths from `Context.TestFiles`.                     |
| `Requirements` | Uses      | Receives the requirement tree to map tests to requirements.            |
| `Program`      | Used by   | Constructs `TraceMatrix` and calls enforcement/export methods.         |

## References

- [ReqStream System Design][arch]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[repo]: https://github.com/demaconsulting/ReqStream
