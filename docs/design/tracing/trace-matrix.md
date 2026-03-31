# TraceMatrix Unit Design

## Overview

`TraceMatrix` maps test execution results to requirements and calculates requirement-coverage
metrics. It consumes an already-validated `Requirements` tree and a list of test-result file paths,
then provides lookup and satisfaction-analysis methods used by `Program` to generate reports and
enforce coverage.

## Supporting Value Types

### `TestMetrics`

`TestMetrics` is an immutable record that aggregates pass/fail counts for a single named test
across all loaded result files.

| Property | Type | Formula | Notes |
| -------- | ---- | ------- | ----- |
| `Passes` | `int` | — | Total passing executions |
| `Fails` | `int` | — | Total failing executions |
| `Executed` | `int` | `Passes + Fails` | Total executions recorded |
| `AllPassed` | `bool` | `Fails == 0 && Executed > 0` | True only when executed at least once with no failures |

`GetTestResult` returns `TestMetrics(0, 0)` when the test name has no recorded executions, so
callers always receive a valid object.

### `TestExecution`

`TestExecution` is an immutable record that holds the results for one test name from one
result file.

| Property | Type | Notes |
| -------- | ---- | ----- |
| `FileBaseName` | `string` | Base name (no extension) of the result file; used for source-specific matching |
| `Name` | `string` | Test name as it appears in the result file |
| `Metrics` | `TestMetrics` | Aggregated pass/fail counts for this test in this file |

## Private State

| Field | Type | Purpose |
| ----- | ---- | ------- |
| `_testExecutions` | `Dictionary<string, List<TestExecution>>` | Maps test names to lists of `TestExecution` entries |
| `_requirements` | `Requirements` | The validated requirement tree; held for iteration in analysis methods |

## Construction

### `TraceMatrix(requirements, testResultFiles)`

The constructor stores the `Requirements` tree for later iteration and calls
`ProcessTestResultFile` for each path in `testResultFiles` to populate `_testExecutions`. After
construction, `_testExecutions` contains every unique test name seen, each mapped to one
`TestExecution` record per result file that contained that test name.

### `ProcessTestResultFile(filePath)`

`ProcessTestResultFile` reads one test-result file, auto-detects its format (TRX or JUnit) via
`DemaConsulting.TestResults.IO.Serializer.Deserialize`, and adds a `TestExecution` record to
`_testExecutions` for each test case found. If parsing fails, the underlying exception is wrapped
in an `InvalidOperationException` that includes `filePath` — this ensures the caller can identify
the offending file without inspecting nested exception detail.

## Methods

### `GetTestResult(testName)`

`GetTestResult` returns aggregated `TestMetrics` for a named test. When `testName` contains a
`'@'` separator (not at position 0 or end), it applies source-specific filtering: the part before
`'@'` is matched case-insensitively against each `TestExecution.FileBaseName`, so only results
from files whose base name contains that prefix are summed. This lets a requirement reference a
test from a specific result file (e.g., `ubuntu@TestFeature_Valid_Passes`) without excluding that
test from plain-name lookups in other requirements.

When no `'@'` separator is present, all executions for the test name are summed across all result
files. If the test name is not found in `_testExecutions`, the method returns `TestMetrics(0, 0)`,
ensuring callers always receive a valid object. See the [Test Name Format Summary](#test-name-format-summary)
table for a quick reference of both formats.

### `CalculateSatisfiedRequirements(filterTags)`

`CalculateSatisfiedRequirements` iterates every requirement in the tree (subject to `filterTags`
filtering) and returns a `(satisfied, total)` tuple. It calls `IsRequirementSatisfied` for each
requirement to determine whether all associated tests have passed. This provides `Program` with the
counts needed to report coverage status and determine whether `--enforce` should fail.

### `CollectAllTests(requirement)`

`CollectAllTests` returns the union of all test names associated with a requirement and its
entire descendant subtree. Child requirements inherit their parent's coverage obligations, so a
requirement is only considered covered when all tests across its whole subtree pass. Because
`Requirements.ValidateCycles()` has already confirmed the child graph is acyclic, this method
recurses without a cycle guard.

### `IsRequirementSatisfied(requirement)`

`IsRequirementSatisfied` returns `true` if and only if the requirement has at least one test
mapped (directly or via descendants) and every one of those tests has `AllPassed == true`. A
requirement with no tests is never satisfied, enforcing the design expectation that every
requirement must be traced to at least one passing test.

### `Export(filePath, depth, filterTags)`

`Export` writes the trace matrix to a Markdown file at `filePath`. The output lists each
requirement (respecting `filterTags`), its associated tests, and the pass/fail status of each
test. The heading depth for requirement IDs is controlled by `depth`.

## Test Name Format Summary

| Format | Example | Matching rule |
| ------ | ------- | ------------- |
| Plain | `TestFeature_Valid_Passes` | Aggregates across all result files |
| Source-specific | `ubuntu@TestFeature_Valid_Passes` | Restricted to files whose base name contains `ubuntu` |

## Interactions with Other Units

| Unit | Nature of interaction |
| ---- | --------------------- |
| `Program` | Constructs `TraceMatrix`; calls `CalculateSatisfiedRequirements` and `Export` |
| `Requirements` | Provides the requirement tree; iterated during analysis |
| `Validation` | Exercises `TraceMatrix` with fixture test-result files in validation tests |

## References

- [ReqStream Architecture][arch]
- [ReqStream Repository][repo]

[arch]: ../../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
