## DemaConsulting.TestResults Integration Design

### Purpose

`DemaConsulting.TestResults` provides test result deserialization for ReqStream's Tracing
and SelfTest subsystems. It reads TRX (MSTest) and JUnit XML result files and exposes the
results as .NET objects that `TraceMatrix` and `Validation` use for coverage analysis and
self-validation output.

### Integration

The `DemaConsulting.TestResults.IO.Serializer` class is used by `TraceMatrix` to load each
test result file. The auto-detection logic in `Serializer` tries TRX format first, then JUnit,
based on the file content rather than the file extension, ensuring format-agnostic loading.

`Validation` uses `DemaConsulting.TestResults.IO.TrxSerializer.Serialize` to write self-test
results to a TRX file when `--results` is provided.

### Usage in TraceMatrix

- `DemaConsulting.TestResults.IO.Serializer.Deserialize(content, path)` is called for each
  test result file.
- The returned `DemaConsulting.TestResults.TestResults` object's `Results` list is iterated
  to populate `_testExecutions`.
- Each `TestResult` contributes a `TestExecution` record with `Name`, `SourceFile`, and
  `Outcome`.

### Usage in Validation

- `DemaConsulting.TestResults.TestResults` and `DemaConsulting.TestResults.TestResult` objects
  are constructed to represent self-validation test outcomes.
- `DemaConsulting.TestResults.IO.TrxSerializer.Serialize(results)` serializes them to a TRX
  string written to the results file.

### Error Handling

`TraceMatrix` wraps `Serializer.Deserialize` calls: if the file does not exist,
`FileNotFoundException` is thrown with the file path. If parsing fails,
`InvalidOperationException` is thrown with the file path and original exception as the inner
exception. These are caught in `Program.Main` and converted to exit code `1`.

### Dependencies

`TraceMatrix` and `Validation` are the only units that use this package directly.
