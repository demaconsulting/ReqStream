## DemaConsulting.TestResults

`DemaConsulting.TestResults` provides test result deserialization and serialization for the
ReqStream Tracing and SelfTest subsystems. It reads TRX (MSTest) and JUnit XML result files and
exposes the results as .NET objects used for coverage analysis and self-validation output.

### Purpose

`DemaConsulting.TestResults` was chosen because it provides a unified API for reading both TRX
and JUnit XML test result formats with auto-detection, carries a compatible license, and is
maintained within the same program (DEMA Consulting). It enables ReqStream to consume test
evidence from diverse CI environments without format-specific parsing logic.

### Features Used

- **`DemaConsulting.TestResults.IO.Serializer.Deserialize(content, path)`** — auto-detects the
  format (TRX or JUnit) based on file content and returns a `TestResults` object.
- **`DemaConsulting.TestResults.TestResults`** — container holding a list of `TestResult` objects
  representing individual test executions.
- **`DemaConsulting.TestResults.TestResult`** — represents a single test execution with `Name`
  and `Outcome` properties.
- **`DemaConsulting.TestResults.TestOutcome`** — enum indicating test pass/fail status.
- **`DemaConsulting.TestResults.IO.TrxSerializer.Serialize(results)`** — serializes test results
  to TRX format for self-validation output.
- **`DemaConsulting.TestResults.IO.JUnitSerializer.Serialize(results)`** — serializes test
  results to JUnit XML format for self-validation output.

### Integration Pattern

`TraceMatrix` uses the deserialization API to load test results:

1. `File.ReadAllText(filePath)` reads the test result file content.
2. `Serializer.Deserialize(content, filePath)` auto-detects the format and returns a
   `TestResults` object.
3. The `Results` list is iterated to populate `_testExecutions` with `TestExecution` records.

`Validation` uses the serialization API to write self-test results:

1. `TestResults` and `TestResult` objects are constructed to represent self-validation outcomes.
2. `TrxSerializer.Serialize(results)` or `JUnitSerializer.Serialize(results)` produces the output
   string based on the requested file extension.
3. The serialized string is written to the results file via `File.WriteAllText`.

`TraceMatrix` wraps `Serializer.Deserialize` calls: if the file does not exist,
`FileNotFoundException` is thrown with the file path. If parsing fails,
`InvalidOperationException` is thrown with the file path and original exception as the inner
exception. `TraceMatrix` and `Validation` are the only units that use this package directly.

### Version Constraints

The TRX schema version consumed is the standard MSTest V2 TRX format; the JUnit XML schema is
the de-facto format used by most CI systems. Both are auto-detected by `Serializer.Deserialize`
based on root element names. Version numbers are managed centrally in the project file and SBOM
per the OTS Dependencies Design policy.
