### TraceMatrix

#### Purpose

`TraceMatrix` maps test execution results to requirements and calculates requirement-coverage
metrics. It consumes an already-validated `Requirements` tree and a list of test-result file
paths, then provides lookup and satisfaction-analysis methods used by `Program` to generate
reports and enforce coverage.

#### Data Model

**`TestMetrics`**: Immutable record aggregating pass/fail counts for a single named test.

- `Passes` (`int`) — total passing executions.
- `Fails` (`int`) — total failing executions.
- `Executed` (`int`) — computed: `Passes + Fails`.
- `AllPassed` (`bool`) — computed: `Fails == 0 && Executed > 0`.

**`TestExecution`**: Immutable record holding results for one test name from one result file.

- `FileBaseName` (`string`) — base name (no extension) of the result file; used for
  source-specific matching.
- `Name` (`string`) — test name as it appears in the result file.
- `Metrics` (`TestMetrics`) — aggregated pass/fail counts for this test in this file.

**`_testExecutions`**: `Dictionary<string, List<TestExecution>>` — maps test names to lists of
`TestExecution` entries.

**`_requirements`**: `Requirements` — the validated requirement tree; held for iteration in
analysis methods.

#### Key Methods

**TraceMatrix(requirements, testResultFiles)**: Constructor that stores the `Requirements` tree
and calls `ProcessTestResultFile` for each path to populate `_testExecutions`.

- *Parameters*: `Requirements requirements`; `IEnumerable<string> testResultFiles`.
- *Preconditions*: `requirements` is a validated, acyclic tree.
- *Postconditions*: `_testExecutions` is fully populated and read-only after construction.

**GetTestResult(testName)**: Returns aggregated `TestMetrics` for a named test.

- *Parameters*: `string testName` — plain name or `filepart@testname` format.
- *Returns*: `TestMetrics` — aggregated metrics (returns `TestMetrics(0, 0)` if not found).
- *Preconditions*: None.
- *Postconditions*: None (read-only).

When `testName` contains `'@'` (not at position 0 or end), the part before `'@'` is matched
case-insensitively against each `TestExecution.FileBaseName` for source-specific filtering.

**CalculateSatisfiedRequirements(filterTags)**: Iterates every requirement and returns a
`(satisfied, total)` tuple.

- *Parameters*: `HashSet<string>? filterTags` — optional tag filter.
- *Returns*: `(int satisfied, int total)`.
- *Preconditions*: None.
- *Postconditions*: None (read-only).

**GetUnsatisfiedRequirements(filterTags)**: Returns a list of requirement IDs not satisfied.

- *Parameters*: `HashSet<string>? filterTags` — optional tag filter.
- *Returns*: `List<string>` — unsatisfied requirement IDs.
- *Preconditions*: None.
- *Postconditions*: None (read-only).

**Export(filePath, depth, filterTags)**: Writes the trace matrix to a Markdown file with three
sections: Summary, Requirements, and Testing.

- *Parameters*: `string filePath`; `int depth`; `HashSet<string>? filterTags`.
- *Returns*: `void`.
- *Preconditions*: `filePath` must not be null or empty.
- *Postconditions*: Markdown file written.

**CollectAllTests(requirement, rootSection, allTests)**: Returns the union of all test names for
a requirement and its entire descendant subtree. Recurses without a cycle guard because
`ValidateCycles` has already confirmed the graph is acyclic.

**IsRequirementSatisfied(requirement, rootSection)**: Returns `true` if the requirement has at
least one test mapped and every test has `AllPassed == true`.

#### Error Handling

- **`FileNotFoundException`** — thrown by `ProcessTestResultFile` when a path does not exist.
- **`InvalidOperationException`** — thrown by `ProcessTestResultFile` when a file cannot be
  parsed. The message includes the file path; the original parse exception is the inner
  exception.

All query methods are read-only and do not throw for missing test names or empty trees. `Export`
throws `ArgumentException` for null/empty path and propagates `IOException` from file-write
operations.

#### Dependencies

- **Requirements** — provides the validated requirement tree iterated during analysis.
- **DemaConsulting.TestResults** — provides `TestResults`, `TestResult`, and `TestOutcome` model
  types used for deserialization.
- **DemaConsulting.TestResults.IO.Serializer** — auto-detects and deserializes TRX and JUnit XML
  test result files.

#### Callers

- **Program** — constructs `TraceMatrix` and calls `CalculateSatisfiedRequirements`,
  `GetUnsatisfiedRequirements`, and `Export`.
- **Validation** — exercises `TraceMatrix` construction with fixture test-result files in
  self-validation tests.
