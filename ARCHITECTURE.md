# ReqStream Architecture

This document provides a comprehensive guide to the architecture and internal workings of ReqStream, a .NET
command-line tool for managing requirements written in YAML files.

**Note**: This is the authoritative source for understanding ReqStream's behavior and internal design.

## Table of Contents

- [Overview](#overview)
- [Core Data Model](#core-data-model)
- [Requirements Processing Flow](#requirements-processing-flow)
- [Trace Matrix Construction and Analysis](#trace-matrix-construction-and-analysis)
- [Test Coverage Enforcement](#test-coverage-enforcement)
- [Program Execution Flow](#program-execution-flow)
- [Implementation Notes](#implementation-notes)

## Overview

ReqStream is a .NET command-line tool designed to manage requirements written in YAML files. It provides three core
capabilities:

1. **Requirements Management**: Read, parse, and merge requirements from multiple YAML files into a hierarchical
   structure
2. **Trace Matrix Construction**: Map test results (TRX and JUnit formats) to requirements for traceability
3. **Test Coverage Enforcement**: Ensure all requirements have adequate test coverage as part of CI/CD quality gates

The tool is built with .NET 8.0+, uses YamlDotNet for YAML parsing, and follows a clear separation of concerns with
distinct classes for each major responsibility.

## Core Data Model

The data model consists of several key classes that represent requirements, sections, test results, and program state:

### Requirement

**Location**: `Requirement.cs`

Represents a single requirement with its metadata.

```csharp
public class Requirement
{
    public string Id { get; set; }          // Unique identifier (e.g., "SYS-SEC-001")
    public string Title { get; set; }       // Human-readable description
    public string? Justification { get; set; } // Optional rationale for the requirement
    public List<string> Tests { get; }      // Test identifiers linked to this requirement
    public List<string> Children { get; }   // Child requirement IDs for hierarchical requirements
}
```

**Key Characteristics**:

- `Id` must be unique across all requirements files
- `Title` must not be blank
- `Justification` is optional and explains why the requirement exists
- `Tests` can be added inline in YAML or through separate mappings
- `Children` enables hierarchical requirements where high-level requirements are satisfied by child requirements

### Section

**Location**: `Section.cs`

Container for requirements and child sections, enabling hierarchical organization.

```csharp
public class Section
{
    public string Title { get; set; }           // Section title
    public List<Requirement> Requirements { get; }  // Requirements in this section
    public List<Section> Sections { get; }      // Child sections
}
```

**Key Characteristics**:

- Sections form a tree structure with arbitrary depth
- Section titles are used to match and merge sections across files
- Sections with identical titles at the same hierarchy level are merged

### Requirements

**Location**: `Requirements.cs`

Root class that extends Section and manages YAML file parsing.

```csharp
public class Requirements : Section
{
    private readonly HashSet<string> _includedFiles;        // Prevents infinite include loops
    private readonly Dictionary<string, Requirement> _allRequirements;  // Duplicate detection
    
    public static Requirements Read(params string[] paths);
    public void Export(string filePath, int depth = 1);
    public void ExportJustifications(string filePath, int depth = 1);
}
```

**Key Responsibilities**:

- Parse YAML files using YamlDotNet
- Merge sections with identical hierarchy paths
- Validate requirement IDs are unique
- Process file includes recursively with loop prevention
- Apply test mappings to requirements
- Export requirements to Markdown reports
- Export justifications to Markdown documents

### TraceMatrix

**Location**: `TraceMatrix.cs`

Maps test results to requirements and analyzes test coverage.

```csharp
public class TraceMatrix
{
    private readonly Dictionary<string, List<TestExecution>> _testExecutions;
    private readonly Requirements _requirements;
    
    public TraceMatrix(Requirements requirements, params string[] testResultFiles);
    public TestMetrics GetTestResult(string testName);
    public IReadOnlyDictionary<string, TestMetrics> GetAllTestResults();
    public (int satisfied, int total) CalculateSatisfiedRequirements();
    public List<string> GetUnsatisfiedRequirements();
    public void Export(string filePath, int depth = 1);
}
```

**Key Responsibilities**:

- Parse test result files (TRX and JUnit formats)
- Aggregate test executions from multiple result files by test name
- Match test names to requirements (plain names vs source-specific `filepart@testname`)
- Provide fast lookup of test executions with optional source filtering
- Return `TestMetrics` for queried tests (returns 0/0 if not found)
- Calculate requirement satisfaction (must have tests, all must pass)
- Consider child requirement tests transitively
- Export trace matrix reports to Markdown

**Internal Structure**:

The TraceMatrix uses a two-tier structure for efficient test result management:

1. **TestExecution Records**: Each test result file produces TestExecution records that capture:
   - `FileBaseName`: The base name of the test result file (for source matching)
   - `Name`: The actual test name (without source filter)
   - `Passes`: Number of passing executions for this test in this file
   - `Fails`: Number of failing executions for this test in this file

2. **Dictionary by Test Name**: Test executions are organized in a `Dictionary<string, List<TestExecution>>` where:
   - The key is the actual test name (e.g., "Test_Platform")
   - The value is a list of all executions of that test across different files
   - This enables efficient lookup and filtering by source

3. **FindTestExecutions Method**: Given a test name (with or without source filter):
   - Parses the test name to extract optional source filter (e.g., "windows@Test_Platform")
   - Looks up test executions by actual test name
   - Returns all executions if no source filter is specified
   - Filters executions by matching file base name if source filter is specified

This design optimizes for the common case where the number of unique test names is much larger than the
number of test result files, making test name lookup O(1) with optional O(n) filtering where n is the
number of files containing that test.

### TestMetrics

**Location**: `TraceMatrix.cs`

Represents test metrics for test executions.

```csharp
public record TestMetrics(int Passes, int Fails)
{
    public int Executed => Passes + Fails;
    public bool AllPassed => Fails == 0 && Executed > 0;
};
```

**Key Characteristics**:

- Immutable record type capturing passes and fails
- Can be aggregated across multiple executions
- `Executed` property returns total number of executions (Passes + Fails)
- `AllPassed` property indicates if all executions passed (no failures and at least one execution)
- Used as return type for `GetTestResult` (returns `TestMetrics(0, 0)` if test not found)

### TestExecution

**Location**: `TraceMatrix.cs`

Represents a single test execution from a specific test result file.

```csharp
public record TestExecution(string FileBaseName, string Name, TestMetrics Metrics);
```

**Key Characteristics**:

- Immutable record type capturing test results from one file
- `FileBaseName` is used for source-specific test matching
- `Name` is the actual test name without any source filter prefix
- `Metrics` contains passes and fails aggregated from potentially multiple test results with the same name in one file
  (e.g., test results from different test classes)

### Context

**Location**: `Context.cs`

Handles CLI arguments and program state.

```csharp
public sealed class Context : IDisposable
{
    public bool Version { get; }
    public bool Help { get; }
    public bool Silent { get; }
    public bool Validate { get; }
    public bool Enforce { get; }
    public List<string> RequirementsFiles { get; }
    public List<string> TestFiles { get; }
    public string? RequirementsReport { get; }
    public int ReportDepth { get; }
    public string? Matrix { get; }
    public int MatrixDepth { get; }
    public string? JustificationsFile { get; }
    public int JustificationsDepth { get; }
    public int ExitCode { get; }
    
    public static Context Create(string[] args);
    public void WriteLine(string message);
    public void WriteError(string message);
}
```

**Key Responsibilities**:

- Parse command-line arguments
- Expand glob patterns for file matching
- Manage console and log file output
- Track error state for exit code determination

## Requirements Processing Flow

The requirements processing flow handles YAML parsing, section merging, validation, test mappings, and file includes:

### 1. YAML File Parsing

ReqStream uses **YamlDotNet** with the `HyphenatedNamingConvention` to parse YAML files. The YAML structure is
deserialized into internal classes (`YamlDocument`, `YamlSection`, `YamlRequirement`, `YamlMapping`):

```yaml
---
sections:
  - title: "System Security"
    requirements:
      - id: "SYS-SEC-001"
        title: "The system shall support credentials authentication."
        justification: |
          Authentication is critical to ensure only authorized users can access the system.
          This requirement establishes the foundation for our security posture.
        tests:
          - "AuthTest_ValidCredentials_Allowed"
        children:
          - "AUTH-001"

mappings:
  - id: "SYS-SEC-001"
    tests:
      - "AuthTest_MultipleUsers_Allowed"

includes:
  - additional_requirements.yaml
```

**Key Points**:

- Files are read as text and deserialized using YamlDotNet
- Empty or null documents are silently skipped
- File paths are resolved relative to the current file's directory

### 2. Section Merging

Sections with **identical titles at the same hierarchy level** are merged. This enables modular requirements where
multiple files can contribute to the same section:

**Example**:

File 1:

```yaml
sections:
  - title: "Authentication"
    requirements:
      - id: "AUTH-001"
        title: "User login required"
```

File 2:

```yaml
sections:
  - title: "Authentication"
    requirements:
      - id: "AUTH-002"
        title: "Password complexity required"
```

**Result**: Both requirements are in the same "Authentication" section.

**Algorithm**:

- When processing a section, search for an existing section with the same title
- If found, merge requirements and recursively merge child sections
- If not found, create a new section

### 3. Validation

Requirements are validated during parsing to ensure data integrity:

| Validation | Condition | Error |
| ---------- | --------- | ----- |
| Section Title | Must not be blank | `Section title cannot be blank` |
| Requirement ID | Must not be blank | `Requirement ID cannot be blank` |
| Requirement ID | Must be unique | `Duplicate requirement ID found: '{id}'` |
| Requirement Title | Must not be blank | `Requirement title cannot be blank` |
| Test Names | Must not be blank | `Test name cannot be blank` |
| Mapping ID | Must not be blank | `Mapping requirement ID cannot be blank` |

**Error Handling**:

- Validation errors throw `InvalidOperationException` with file path context
- Errors are caught in `Program.Main` and reported to the user

### 4. Test Mappings

Tests can be mapped to requirements in two ways:

**Inline Tests** (defined with the requirement):

```yaml
requirements:
  - id: "REQ-001"
    title: "Feature X shall work"
    tests:
      - "TestFeatureX_Valid_Passes"
      - "TestFeatureX_Invalid_Fails"
```

**Separate Mappings** (defined in the `mappings` section):

```yaml
mappings:
  - id: "REQ-001"
    tests:
      - "TestFeatureX_EdgeCase_Passes"
```

**Key Points**:

- Both methods add tests to the same `Requirement.Tests` list
- Mappings can be in the same file or included files
- Mappings are applied after all sections are processed
- If a mapping references a non-existent requirement ID, it is silently ignored

### 5. File Includes

The `includes` section enables splitting requirements across multiple files:

```yaml
includes:
  - auth_requirements.yaml
  - data_requirements.yaml
  - test_mappings.yaml
```

**Include Processing**:

- Include paths are resolved relative to the current file's directory
- Files are processed recursively (includes can have includes)
- Loop prevention: Each file's full path is tracked in `_includedFiles`
- If a file is included multiple times, subsequent includes are skipped
- If an included file doesn't exist, a `FileNotFoundException` is thrown

**Loop Prevention Example**:

```text
File A includes File B
File B includes File C
File C includes File A  ← Detected and skipped
```

### 6. Child Requirements

Requirements can reference other requirements as children using the `children` field:

```yaml
requirements:
  - id: "HIGH-LEVEL-001"
    title: "System shall be secure"
    children:
      - "AUTH-001"
      - "ENCRYPT-001"
      - "AUDIT-001"
  
  - id: "AUTH-001"
    title: "Authentication required"
    tests:
      - "TestAuth_Valid_Passes"
```

**Key Points**:

- Child requirement IDs are stored in `Requirement.Children`
- When evaluating satisfaction, tests from all child requirements are included transitively
- Child requirements can themselves have children (recursive traversal)
- If a child requirement ID doesn't exist, it is ignored during satisfaction calculation

## Trace Matrix Construction and Analysis

The trace matrix maps test results to requirements and determines coverage satisfaction:

### 1. Test Result File Parsing

ReqStream supports two test result formats:

- **TRX** (Visual Studio Test Results format)
- **JUnit** (Java/XML test results format)

**Parsing Algorithm**:

```text
For each test result file:
  1. Read file content as text
  2. Try to parse as TRX format
  3. If TRX fails, try to parse as JUnit format
  4. If both fail, throw InvalidOperationException
  5. Extract test name and outcome from each result
  6. Update test result entries
```

**Implementation**:

- Uses `DemaConsulting.TestResults.IO` library for parsing
- Parsing errors include the file path for debugging
- Test outcomes are mapped to `Passed` or `Failed` states

### 2. Test Name Matching

ReqStream supports two test name formats:

**Plain Test Names** (aggregate results from all files):

```yaml
tests:
  - "TestFeature_Valid_Passes"
```

**Source-Specific Test Names** (match results from specific files):

```yaml
tests:
  - "windows-latest@TestPlatform_Windows_Passes"
  - "ubuntu-latest@TestPlatform_Linux_Passes"
```

**Matching Algorithm**:

1. Extract file base name (without extension) from test result file
2. For each test result, parse test name to extract optional file part and actual test name
3. Match source-specific tests first:
   - If requirement test name contains `@`, split into `filepart@testname`
   - Check if file base name contains the file part (case-insensitive)
   - Check if actual test name matches
4. If no source-specific match, match plain test names:
   - Requirement test name has no `@`
   - Actual test name matches

**Example**:

```text
Test result file: test-results-windows-latest.trx
Contains test: "TestAuth_Valid_Passes"

Requirement tests:
  - "windows-latest@TestAuth_Valid_Passes"  ✓ Matches (source-specific)
  - "ubuntu-latest@TestAuth_Valid_Passes"   ✗ No match (wrong source)
  - "TestAuth_Valid_Passes"                 ✓ Matches (plain name)
```

### 3. Test Execution Aggregation and Storage

The TraceMatrix uses a simplified two-tier structure for efficient test result management:

**Step 1: Parse and Aggregate by File**

For each test result file:
1. Extract the file base name (without extension) for source matching
2. Parse the file as TRX or JUnit format
3. Skip non-executed tests (using `IsExecuted()` extension method)
4. **Capture ALL executed tests** (no filtering by requirements)
5. Aggregate test results by test name within the file (collapsing duplicate results from different test classes)
6. Create a `TestExecution` record for each unique test name containing:
   - FileBaseName: The base name of the test result file
   - Name: The actual test name
   - Metrics: A `TestMetrics` record with passes and fails counts

**Step 2: Store in Dictionary Structure**

Store test executions in a `Dictionary<string, List<TestExecution>>` where:
- Key: The actual test name (e.g., "Test_Platform")
- Value: List of all TestExecution records for that test from different files

**Example Internal Structure**:

```text
_testExecutions = {
  "Test_Platform": [
    TestExecution("test-results-windows-latest", "Test_Platform", new TestMetrics(1, 0)),
    TestExecution("test-results-ubuntu-latest", "Test_Platform", new TestMetrics(1, 0))
  ],
  "Test_Auth": [
    TestExecution("test-results-windows-latest", "Test_Auth", new TestMetrics(2, 1))
  ]
}
```

**Step 3: Query with FindTestExecutions**

When querying for test results (via `GetTestResult`):
1. Parse the requested test name to extract optional source filter (e.g., "windows@Test_Platform")
2. Look up test executions by the actual test name in the dictionary
3. If no source filter: Return all executions for that test name
4. If source filter present: Filter to executions where FileBaseName contains the source filter (case-insensitive)
5. Aggregate the filtered executions into a single TestResultEntry

`GetAllTestResults()` returns only tests referenced in requirements by:
1. Collecting all test names from the requirements tree
2. Calling `GetTestResult()` for each referenced test
3. Returning only those that have execution data

**Key Benefits**:

- **Efficient Lookup**: O(1) dictionary lookup by test name
- **Complete Data Capture**: All test executions are captured, not just those in requirements
- **Flexible Querying**: Can query any test, but GetAllTestResults filters to requirements
- **Source-Specific Filtering**: Filters at query time, not storage time
- **Clear Structure**: TestExecution records explicitly capture what was tested in each file
- **Optimized for Scale**: Works best when number of unique test names >> number of test files (typical case)

**Example Query Behavior**:

```text
Query: "windows@Test_Platform"
  → Parse: sourceFilter="windows", actualTestName="Test_Platform"
  → Lookup: Find executions for "Test_Platform"
  → Filter: Keep only executions where FileBaseName contains "windows"
  → Result: TestExecution("test-results-windows-latest", "Test_Platform", 1, 0)
  → Aggregate: TestResultEntry(Executed=1, Passed=1)

Query: "Test_Platform" (no source filter)
  → Parse: sourceFilter=null, actualTestName="Test_Platform"
  → Lookup: Find executions for "Test_Platform"
  → Filter: None (return all)
  → Result: Both windows and ubuntu executions
  → Aggregate: TestResultEntry(Executed=2, Passed=2)
```

### 4. Requirement Satisfaction Calculation

A requirement is considered **satisfied** if:

1. It has at least one test (including tests from child requirements)
2. All tests have been executed (`Executed > 0`)
3. All tests have passed (`Passed == Executed`)

**Algorithm**:

```text
For each requirement:
  1. Collect all tests from the requirement
  2. Recursively collect tests from child requirements
  3. If no tests found, requirement is unsatisfied
  4. For each test:
     - If test not found in results, requirement is unsatisfied
     - If test not executed (Executed == 0), requirement is unsatisfied
     - If test failed (Passed != Executed), requirement is unsatisfied
  5. If all tests pass, requirement is satisfied
```

### 5. Child Requirement Test Consideration

When calculating satisfaction, child requirement tests are considered transitively:

**Example**:

```yaml
requirements:
  - id: "PARENT-001"
    title: "System shall be secure"
    children:
      - "CHILD-001"
      - "CHILD-002"
  
  - id: "CHILD-001"
    title: "Authentication required"
    tests:
      - "TestAuth_Passes"
    children:
      - "GRANDCHILD-001"
  
  - id: "CHILD-002"
    title: "Encryption required"
    tests:
      - "TestEncrypt_Passes"
  
  - id: "GRANDCHILD-001"
    title: "Password complexity required"
    tests:
      - "TestPasswordComplexity_Passes"
```

**Result**: `PARENT-001` is satisfied if all three tests pass:

- `TestAuth_Passes`
- `TestEncrypt_Passes`
- `TestPasswordComplexity_Passes`

## Test Coverage Enforcement

Test coverage enforcement ensures that all requirements have adequate test coverage, making it ideal for CI/CD quality
gates:

### 1. How the `--enforce` Flag Works

When `--enforce` is specified:

1. Requirements and test results are processed normally
2. Reports are generated (if requested)
3. Requirement satisfaction is calculated
4. If any requirements are unsatisfied:
   - An error message is written to stderr
   - Unsatisfied requirement IDs are listed
   - Exit code is set to 1
5. If all requirements are satisfied:
   - Exit code remains 0

**Example**:

```bash
reqstream --requirements "**/*.yaml" --tests "**/*.trx" --matrix matrix.md --enforce
```

**Output** (if unsatisfied):

```text
Error: Only 45 of 50 requirements are satisfied with tests.
Unsatisfied requirements:
  - REQ-001
  - REQ-015
  - REQ-023
  - REQ-034
  - REQ-042
```

### 2. Requirement Satisfaction Criteria

A requirement is **satisfied** if:

| Criteria | Description |
| -------- | ----------- |
| Has tests | At least one test is mapped (directly or through children) |
| Tests found | All mapped tests exist in test result files |
| Tests executed | All mapped tests have `Executed > 0` |
| Tests passed | All mapped tests have `Passed == Executed` |

A requirement is **unsatisfied** if:

| Condition | Reason |
| --------- | ------ |
| No tests | Requirement has no tests and no children with tests |
| Test not found | A mapped test doesn't exist in any test result file |
| Test not executed | A mapped test has `Executed == 0` |
| Test failed | A mapped test has `Passed != Executed` |

### 3. Unsatisfied Requirements Identification

Unsatisfied requirements are collected by recursively traversing the section tree:

```csharp
public List<string> GetUnsatisfiedRequirements()
{
    var unsatisfied = new List<string>();
    CollectUnsatisfiedRequirements(_requirements, unsatisfied);
    return unsatisfied;
}

private void CollectUnsatisfiedRequirements(Section section, List<string> unsatisfied)
{
    foreach (var requirement in section.Requirements)
    {
        if (!IsRequirementSatisfied(requirement, _requirements))
        {
            unsatisfied.Add(requirement.Id);
        }
    }
    foreach (var childSection in section.Sections)
    {
        CollectUnsatisfiedRequirements(childSection, unsatisfied);
    }
}
```

### 4. Exit Code Behavior

| Condition | Exit Code | Behavior |
| --------- | --------- | -------- |
| No errors | 0 | Success |
| Argument error | 1 | `ArgumentException` caught in `Main` |
| Enforcement failed | 1 | `Context.WriteError` sets `_hasErrors = true` |
| Unexpected error | Exception | Re-thrown for event logging |

**Key Points**:

- Enforcement errors are written **after** all reports are generated
- This allows users to review reports for failure analysis
- The `Context` class tracks error state and returns appropriate exit code

## Program Execution Flow

The program follows a priority-based execution flow:

### Priority Order

```text
1. Version query (--version)
   └─> Print version and exit

2. Banner
   └─> Print application banner (skipped if version was queried)

3. Help (--help)
   └─> Print usage information and exit

4. Self-Validation (--validate)
   └─> Run self-validation tests and exit

5. Requirements Processing
   ├─> Read and merge requirements files
   ├─> Export requirements report (if --report specified)
   ├─> Parse test result files (if --tests specified)
   ├─> Export trace matrix (if --matrix specified)
   └─> Enforce coverage (if --enforce specified)
```

### Error Handling Patterns

ReqStream uses different exception types for different error scenarios:

| Exception Type | Usage | Handling |
| -------------- | ----- | -------- |
| `ArgumentException` | Invalid command-line arguments | Caught in `Main`, error printed, exit code 1 |
| `InvalidOperationException` | Runtime errors during execution | Caught in `Main`, error printed, exit code 1 |
| Other exceptions | Unexpected errors | Printed and re-thrown for event logging |

**Design Rationale**:

- `ArgumentException` is thrown during `Context.Create` for argument parsing errors
- `InvalidOperationException` is thrown during execution for validation and processing errors
- Write methods (`WriteLine`, `WriteError`) are only used after successful argument parsing

### Key Execution Steps

**1. Context Creation** (`Context.Create`):

- Parse command-line arguments
- Expand glob patterns to file lists
- Validate argument values (depths, file paths)
- Open log file if specified
- Throw `ArgumentException` on errors

**2. Version Query** (`--version`):

- Print version string only (no banner)
- Exit immediately

**3. Banner Printing**:

- Print application name and version
- Print copyright notice
- Always printed except when `--version` is specified

**4. Help Query** (`--help`):

- Print usage information
- List all command-line options
- Exit after printing

**5. Self-Validation** (`--validate`):

- Run internal validation tests
- Write results to file if `--results` specified
- Exit after validation

**6. Requirements Processing**:

- Read and merge requirements files
- Validate requirements structure
- Export requirements report if requested
- Parse test result files if specified
- Build trace matrix
- Export trace matrix if requested
- Enforce coverage if requested

### Report Generation

Reports are generated in order:

1. **Requirements Report** (`--report`): Markdown table of requirements by section
2. **Justifications Report** (`--justifications`): Markdown document showing requirement IDs, titles, and
   justifications organized by section
3. **Trace Matrix Report** (`--matrix`): Three sections:
   - Summary: Satisfied vs total requirements
   - Requirements: Test statistics per requirement
   - Testing: Test-to-requirement mappings

**Example Justifications Report Structure**:

```markdown
# Security

## SEC-001: The system shall encrypt all data at rest.

Data encryption at rest protects sensitive information from unauthorized access
in case of physical storage theft or unauthorized access to storage media.

## SEC-002: The system shall use TLS 1.3 for network communications.

TLS 1.3 provides modern cryptographic security and eliminates known vulnerabilities
present in earlier TLS versions.
```

**Example Trace Matrix Structure**:

```markdown
# Summary

45 of 50 requirements are satisfied with tests.

## Requirements

### Authentication

| ID | Tests Linked | Passed | Failed | Not Executed |
|----|--------------|--------|--------|--------------|
| AUTH-001 | 3 | 3 | 0 | 0 |
| AUTH-002 | 2 | 1 | 1 | 0 |

## Testing

| Test | Requirement | Passed | Failed |
|------|-------------|--------|--------|
| TestAuth_Valid_Passes | AUTH-001 | 10 | 0 |
| TestAuth_Invalid_Fails | AUTH-001 | 10 | 0 |
```

## Implementation Notes

### Why Sections Are Merged by Matching Titles

Sections are merged by matching titles at the same hierarchy level to enable **modular requirements management**:

**Benefits**:

- Multiple files can contribute to the same section
- Requirements can be organized by feature, component, or responsibility
- Test mappings can be in separate files from requirements
- Teams can work on different requirement files without conflicts

**Example Use Case**:

```text
requirements/
  ├─ system_requirements.yaml       # Defines top-level sections
  ├─ auth_requirements.yaml         # Adds to "Authentication" section
  ├─ data_requirements.yaml         # Adds to "Data Management" section
  └─ test_mappings.yaml             # Adds tests to all sections
```

### How Infinite Include Loops Are Prevented

The `Requirements` class maintains a `HashSet<string>` of included file paths (`_includedFiles`):

```csharp
private void ReadFile(string path)
{
    var fullPath = Path.GetFullPath(path);
    
    if (_includedFiles.Contains(fullPath))
    {
        return;  // Skip if already included
    }
    
    _includedFiles.Add(fullPath);
    // ... process file and includes ...
}
```

**Key Points**:

- File paths are converted to full paths for consistent comparison
- If a file is encountered again, it is silently skipped
- This prevents infinite recursion and stack overflow
- The same file can be safely referenced by multiple parent files

### Design Decisions for Maintainability

**1. Separation of Concerns**:

- `Context`: CLI argument handling and output
- `Requirements`: YAML parsing and merging
- `TraceMatrix`: Test result analysis
- `Program`: Execution flow orchestration

**2. Immutable Data Structures**:

- Properties use `{ get; private init; }` or `{ get; }` to prevent modification after construction
- Lists use `{ get; } = []` to allow adding items but not replacing the list

**3. Error Context**:

- Validation errors include file paths for debugging
- Test result parsing errors include test file paths
- Error messages are clear and actionable

**4. Testability**:

- Static factory methods (`Context.Create`, `Requirements.Read`) enable testing
- Public methods for satisfaction calculation enable verification
- Clear separation between parsing and analysis

### Key Architectural Patterns

**1. Factory Pattern**:

- `Context.Create(args)` - Creates context from arguments
- `Requirements.Read(paths)` - Creates requirements from files

**2. Visitor Pattern** (implicit):

- Section tree traversal for validation
- Section tree traversal for export
- Section tree traversal for satisfaction calculation

**3. Builder Pattern** (implicit):

- Requirements are built incrementally through file reading and merging
- Trace matrix is built incrementally through test result processing

**4. Strategy Pattern**:

- Test result parsing tries TRX first, then JUnit
- Test name matching tries source-specific first, then plain names

**5. Disposable Pattern**:

- `Context` implements `IDisposable` for log file cleanup
- Ensures resources are released properly

---

**Document Version**: 1.0

**Last Updated**: 2026-01-11

For questions or suggestions about this architecture document, please open an issue or submit a pull request.
