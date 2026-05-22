### TemporaryDirectory

#### Verification Approach

The TemporaryDirectory unit is verified using xUnit unit tests in `TemporaryDirectoryTests.cs`.
Tests assert correct lifecycle behavior (directory creation and deletion), correct path
construction including intermediate-directory creation, and rejection of path-traversal attempts.

#### Test Environment

The TemporaryDirectory unit tests require no setup beyond the standard xUnit test runner and
.NET runtime. Tests create temporary directories under the current working directory, which are
deleted by the class under test. The test class carries no `IDisposable` state; each test
manages its own `TemporaryDirectory` instance inline.

#### Acceptance Criteria

The TemporaryDirectory unit verification is complete when all xUnit tests in
`TemporaryDirectoryTests.cs` pass without uncaught exceptions and all assertions succeed.
The unit is considered verified when every requirement in the Requirements Coverage table is
mapped to at least one passing test method.

#### Test Scenarios

**Lifecycle**: Tests verify that a `TemporaryDirectory` instance creates a directory on
construction, that two instances receive distinct paths, and that `Dispose` deletes the
directory including any files written inside it. Calling `Dispose` when the directory has
already been deleted externally must not throw. This scenario is tested by
`TemporaryDirectory_Constructor_Default_CreatesDirectory`,
`TemporaryDirectory_Constructor_TwoInstances_CreateUniqueDirectories`,
`TemporaryDirectory_Dispose_PopulatedDirectory_DeletesDirectory`, and
`TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow`.

**Safe Path Construction**: Tests verify that `GetFilePath` returns a path rooted inside the
temporary directory and that path-traversal attempts using `../` are rejected with
`ArgumentException`. This scenario is tested by
`TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory` and
`TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException`.

**Intermediate Directories**: Tests verify that `GetFilePath` automatically creates any
intermediate subdirectories needed for a nested path. This scenario is tested by
`TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories`.

#### Requirements Coverage

| Requirement ID | Test Method(s) |
| --- | --- |
| `ReqStream-Utilities-TemporaryDirectory-Lifecycle` | `TemporaryDirectory_Constructor_Default_CreatesDirectory`, `TemporaryDirectory_Constructor_TwoInstances_CreateUniqueDirectories`, `TemporaryDirectory_Dispose_PopulatedDirectory_DeletesDirectory`, `TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow` |
| `ReqStream-Utilities-TemporaryDirectory-SafePathCombine` | `TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory`, `TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException` |
| `ReqStream-Utilities-TemporaryDirectory-IntermediateDirectories` | `TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories` |
