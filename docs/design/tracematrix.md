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

The constructor builds the internal test-execution index:

1. Store `requirements` for later iteration.
2. For each path in `testResultFiles`, call `LoadTestResultFile(path)`.
3. After all files are loaded, `_testExecutions` contains every unique test name seen, each mapped
   to a list of `TestExecution` records (one per file that contained that test name).

### `LoadTestResultFile(path)`

`LoadTestResultFile` reads and parses one test-result file.

1. Read the file text.
2. Call `DemaConsulting.TestResults.IO.Serializer.Deserialize(content)` to auto-detect the format
   (TRX or JUnit) and parse the results.
3. If parsing fails, wrap the underlying exception in an `InvalidOperationException` that includes
   `path` so the caller can identify the offending file.
4. For each test case in the deserialized result set, create a `TestExecution` with:
   - `FileBaseName` = `Path.GetFileNameWithoutExtension(path)`
   - `Name` = test case name
   - `Metrics` = `TestMetrics(passes, fails)` derived from the test case outcome
5. Append the `TestExecution` to `_testExecutions[name]`, creating the list entry if absent.

## Methods

### `GetTestResult(testName, sourceFilter)`

`GetTestResult` returns aggregated `TestMetrics` for a named test, with optional source filtering.

**Source-specific format** (`testName` contains `'@'`):

1. Split `testName` on the first `'@'` to obtain `sourcePart` and `namePart`.
2. Look up `_testExecutions[namePart]`.
3. Filter the list to entries where `FileBaseName.Contains(sourcePart, OrdinalIgnoreCase)`.
4. Sum the `Metrics.Passes` and `Metrics.Fails` of the filtered entries.
5. Return `TestMetrics(totalPasses, totalFails)`.

**Plain format** (`testName` does not contain `'@'`):

1. Look up `_testExecutions[testName]`.
2. Sum all `Metrics.Passes` and `Metrics.Fails` without source filtering.
3. Return `TestMetrics(totalPasses, totalFails)`.

If the test name is not found in `_testExecutions`, return `TestMetrics(0, 0)`.

### `CalculateSatisfiedRequirements(filterTags)`

`CalculateSatisfiedRequirements` iterates every requirement in the tree and returns a two-element
tuple `(satisfied, total)`.

For each requirement (subject to `filterTags` filtering):

1. Increment `total`.
2. Call `IsRequirementSatisfied(requirement)`.
3. If satisfied, increment `satisfied`.

Returns `(satisfied, total)`.

### `CollectAllTests(requirement)`

`CollectAllTests` recursively collects every test name associated with a requirement and its
descendants.

1. Add all entries from `requirement.Tests` to the result set.
2. For each ID in `requirement.Children`:
   - Look up the child `Requirement` by ID.
   - If found, recurse into `CollectAllTests(child)` and union the results.
3. Return the union set.

Because `Requirements.ValidateCycles()` has already confirmed the child graph is acyclic, this
method recurses without a cycle guard.

### `IsRequirementSatisfied(requirement)`

`IsRequirementSatisfied` returns `true` if and only if the requirement has passing test coverage.

1. Call `CollectAllTests(requirement)` to obtain the complete set of test names.
2. If the set is empty, return `false` (no tests mapped — requirement is unsatisfied).
3. For each test name, call `GetTestResult(testName)`.
4. If any result has `AllPassed == false`, return `false`.
5. Return `true`.

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

[arch]: ../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
