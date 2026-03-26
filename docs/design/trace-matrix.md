# Trace Matrix Design

## Overview

The `TraceMatrix` class maps test execution results to requirements, computing which requirements are
satisfied by passing tests. It supports TRX and JUnit test result formats via the
`DemaConsulting.TestResults` library.

## Structure

### Supporting Records

#### TestMetrics

```csharp
public record TestMetrics(int Passes, int Fails)
```

Aggregates pass and fail counts for a test name:

| Member | Description |
| :--- | :--- |
| `Passes` | Number of passing executions |
| `Fails` | Number of failing executions |
| `Executed` | Computed: `Passes + Fails` |
| `AllPassed` | `true` when `Executed > 0` and `Fails == 0` |

#### TestExecution

```csharp
public record TestExecution(string FileBaseName, string Name, TestMetrics Metrics)
```

Represents a single test execution from a specific result file. `FileBaseName` is the base name of the
result file (file name without extension, e.g., `windows-latest-results` from `windows-latest-results.trx`).
It is used for source filter prefix matching: a prefix like `windows` matches any `FileBaseName` that
contains the substring `"windows"`.

### TraceMatrix Class

`TraceMatrix` is a `public` class. Its constructor accepts a `Requirements` object and one or more test
result file paths.

#### Constructor

```csharp
public TraceMatrix(Requirements requirements, params string[] testResultFiles)
```

For each test result file:

1. Deserializes the file using `DemaConsulting.TestResults.IO.Serializer.Deserialize` (auto-detects
   TRX or JUnit from the file content)
2. For each test result, creates a `TestExecution` keyed by test name and stores it in the private
   `_testExecutions` dictionary
3. Wraps parse failures in `InvalidOperationException` with the file path included in the message

#### GetTestResult

```csharp
public TestMetrics GetTestResult(string testName)
```

Resolves and aggregates metrics for a given test name. If the name contains a source filter prefix
(`sourceFilter@testName`), only executions from files whose base name contains the filter substring
are included. Returns a zero metric (`0` passes, `0` fails) if no matching executions are found.

#### CalculateSatisfiedRequirements

```csharp
public (int satisfied, int total) CalculateSatisfiedRequirements(HashSet<string>? filterTags)
```

Returns the count of satisfied requirements versus total requirements. A requirement is satisfied when:

- It has at least one test name that resolves to `AllPassed == true`, **or**
- All of its child requirements (referenced by `Children`) are satisfied (recursive)

Tag filtering is applied: if `filterTags` is non-null, only requirements with at least one matching tag
are counted.

#### GetUnsatisfiedRequirements

```csharp
public List<string> GetUnsatisfiedRequirements(HashSet<string>? filterTags)
```

Returns the IDs of requirements that are not satisfied. Used by `Program.EnforceRequirementsCoverage`
to report which requirements are missing coverage.

#### Export

```csharp
public void Export(string filePath, int depth, HashSet<string>? filterTags)
```

Exports a Markdown trace matrix with three sections:

1. **Summary** — overall pass/fail statistics
2. **Requirements** — table of requirements with their test links and satisfaction status
3. **Testing** — table of tests with their pass/fail counts

## Key Design Decisions

- **Source filter prefix**: The `sourceFilter@testName` convention allows requirements to constrain
  which CI platform's test results must pass, enabling cross-platform traceability without requiring
  separate requirements per platform.
- **Aggregation across files**: Multiple test result files are aggregated into a single dictionary by
  test name, so the same test run on multiple machines counts as multiple executions.
- **Child requirement inheritance**: A parent requirement is satisfied if all its children are
  satisfied, enabling hierarchical requirements without requiring each parent to have its own direct
  test links.
- **Zero metric for missing tests**: Returning `TestMetrics(0, 0)` for unresolved tests keeps
  downstream logic uniform — `AllPassed` is `false`, so the requirement is correctly marked unsatisfied.

## Relationships

- **Created by**: `Program.ProcessRequirements` when test files are specified
- **Uses**: `Requirements` (for requirement and test name enumeration), `DemaConsulting.TestResults`
  (for deserializing TRX and JUnit files)
- **Used by**: `Program.ProcessRequirements` (for matrix export and enforcement)
