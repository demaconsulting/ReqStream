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
ensuring callers always receive a valid object. See the Test Name Format Summary table below
for a quick reference of both formats.

### `GetAllTestResults()`

`GetAllTestResults` returns a read-only dictionary mapping each test name (referenced by any
requirement in the tree) to its aggregated `TestMetrics`. Only tests that have been executed at
least once (`Executed > 0`) are included; unexecuted tests are omitted. This method is not
called by `Export` or `ExportTesting`: the Testing section is built by calling
`BuildTestToRequirementsMap` and `GetTestResult` directly, so the Testing table includes
unexecuted tests showing `0 / 0` counts. `GetAllTestResults` is available for callers that
want an executed-only summary without generating a full report.

### `GetUnsatisfiedRequirements(filterTags)`

`GetUnsatisfiedRequirements` returns a list of requirement IDs that are not satisfied (subject to
`filterTags` filtering). A requirement is unsatisfied if it has no tests or if any of its tests
have not been executed or have failed. This is the inverse of `IsRequirementSatisfied` applied
across all requirements in the tree.

### `CalculateSatisfiedRequirements(filterTags)`

`CalculateSatisfiedRequirements` iterates every requirement in the tree (subject to `filterTags`
filtering) and returns a `(satisfied, total)` tuple. It calls `IsRequirementSatisfied` for each
requirement to determine whether all associated tests have passed. This provides `Program` with the
counts needed to report coverage status and determine whether `--enforce` should fail.

### `CollectAllTests(requirement, rootSection, allTests)`

`CollectAllTests` returns the union of all test names associated with a requirement and its
entire descendant subtree. Child requirements inherit their parent's coverage obligations, so a
requirement is only considered covered when all tests across its whole subtree pass. Because
`RequirementsLoader.ValidateCycles()` has already confirmed the child graph is acyclic, this method
recurses without a cycle guard.

### `IsRequirementSatisfied(requirement)`

`IsRequirementSatisfied` returns `true` if and only if the requirement has at least one test
mapped (directly or via descendants) and every one of those tests has `AllPassed == true`. A
requirement with no tests is never satisfied, enforcing the design expectation that every
requirement must be traced to at least one passing test.

### `Export(filePath, depth, filterTags)`

`Export` writes the trace matrix to a Markdown file at `filePath`. The output has three sections,
written in this order by three helper methods: `ExportSummary`, `ExportRequirements`, and
`ExportTesting`.

**Output structure**:

- **Summary** (`ExportSummary`) — Single sentence: "N of M requirements are satisfied with tests."
- **Requirements** (`ExportRequirements`) — One sub-section per requirements section;
  table with columns: ID, Tests Linked, Passed, Failed, Not Executed.
- **Testing** (`ExportTesting`) — Flat table of all requirement-referenced tests (including
  unexecuted ones showing 0/0);
  columns: Test, Requirement, Passed, Failed.

**Table format** (Requirements section): `| ID | Tests Linked | Passed | Failed | Not Executed |`

**Table format** (Testing section): `| Test | Requirement | Passed | Failed |`

The Requirements table rows show only the **direct** tests listed on each requirement (not child
tests). The Summary satisfied-count is calculated by `CalculateSatisfiedRequirements`, which in
turn calls `IsRequirementSatisfied`; that method uses `CollectAllTests` to recurse through the
entire descendant subtree. This creates a deliberate asymmetry: the table shows direct-test counts
while the Summary reflects full-subtree satisfaction.

**Parameter behavior**:

- `filePath`: required; an `ArgumentException` is thrown when `filePath` is null or empty.
  On file-system write failure (for example, permission denied or an invalid path), the
  underlying `IOException` or `UnauthorizedAccessException` is propagated to the caller without
  wrapping.
- `depth`: controls the starting Markdown heading level for the three top-level sections (Summary,
  Requirements, Testing). Each requirements sub-section heading uses `depth + 1`; individual
  requirements are rows in a table, not sub-headings. Defaults to `1`.
- `filterTags`: when non-`null`, only requirements whose `Tags` list contains at least one
  matching tag are included in the Requirements table and counted in the Summary. The Testing
  section is filtered by the same criteria: tests linked only from filtered-out requirements do
  not appear in the Testing table. Defaults to `null`.
- `rootSection`: `_requirements` is used internally as the root to iterate the requirement tree.

## Test Name Format Summary

| Format | Example | Matching rule |
| ------ | ------- | ------------- |
| Plain | `TestFeature_Valid_Passes` | Aggregates across all result files |
| Source-specific | `ubuntu@TestFeature_Valid_Passes` | Restricted to files whose base name contains `ubuntu` |

## Interactions with Other Units

- **`Program`** — Constructs `TraceMatrix`; calls `CalculateSatisfiedRequirements`, `GetUnsatisfiedRequirements`, and `Export`.
- **`Requirements`** — Provides the requirement tree; iterated during analysis.
- **`Validation`** — Exercises `TraceMatrix` with fixture test-result files in validation tests.
