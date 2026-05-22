### TemporaryDirectory

#### Purpose

`TemporaryDirectory` is a disposable utility class that creates a uniquely named scratch
directory under `Environment.CurrentDirectory` on construction and deletes it recursively
on disposal. It provides isolated file-system workspaces for tests and self-validation runs,
ensuring that each caller operates in a clean directory without interfering with other callers
and without leaving artifacts on disk after the workspace is no longer needed.

`TemporaryDirectory` depends on `PathHelpers.SafePathCombine` to construct all paths, so path
traversal is rejected by the same mechanism used elsewhere in the assembly.

#### Data Model

| Field | Type | Description |
| --- | --- | --- |
| `DirectoryPath` | `string` (read-only property) | Full absolute path to the temporary directory created in the constructor. |

#### Key Methods

**Constructor**: Creates a uniquely named subdirectory under `Environment.CurrentDirectory`.

- *Parameters*: None.
- *Returns*: N/A.
- *Preconditions*: `Environment.CurrentDirectory` must be a valid, writable path.
- *Postconditions*: `DirectoryPath` is set; a directory at that path exists on disk.
- *Exceptions*: Throws `InvalidOperationException` (wrapping `IOException`,
  `UnauthorizedAccessException`, or `ArgumentException`) when the directory cannot be created.

The algorithm:

1. Read `Environment.CurrentDirectory` as the effective base.
2. Construct `DirectoryPath` by calling `PathHelpers.SafePathCombine(effectiveBase, $"tmp-{Guid.NewGuid():N}")`.
3. Call `Directory.CreateDirectory(DirectoryPath)`.

Using `Environment.CurrentDirectory` rather than `Path.GetTempPath()` avoids OS-level
symlink indirections such as `/tmp` resolving to `/private/tmp` on macOS, which can cause
path-comparison failures when the OS returns the resolved path.

**GetFilePath(relativePath)**: Returns the full path to a file within the temporary directory,
creating any required intermediate subdirectories.

- *Parameters*: `string relativePath` — a relative path within the temporary directory.
- *Returns*: `string` — the combined full path.
- *Preconditions*: `relativePath` must not be null and must not escape the temporary directory.
- *Postconditions*: All intermediate directories in the returned path exist on disk.
- *Exceptions*: Throws `ArgumentNullException` when `relativePath` is null; throws
  `ArgumentException` when the combined path escapes the temporary directory (path traversal).

The algorithm:

1. Call `PathHelpers.SafePathCombine(DirectoryPath, relativePath)` to validate and combine.
2. Derive the parent directory using `Path.GetDirectoryName`.
3. Call `Directory.CreateDirectory(parent)` to ensure intermediate directories exist.
4. Return the combined path.

**Dispose**: Deletes the temporary directory and all its contents.

- *Parameters*: None.
- *Returns*: N/A.
- *Preconditions*: None.
- *Postconditions*: `DirectoryPath` no longer exists on disk (if it existed at call time).
- *Exceptions*: `IOException` and `UnauthorizedAccessException` are suppressed; cleanup
  failures are non-fatal.

#### Error Handling

- **`InvalidOperationException`** — thrown by the constructor when the directory cannot be
  created. Wraps the underlying `IOException`, `UnauthorizedAccessException`, or
  `ArgumentException`.
- **`ArgumentNullException`** — thrown by `GetFilePath` when `relativePath` is `null`.
- **`ArgumentException`** — thrown by `GetFilePath` when the resolved combined path escapes
  the temporary directory (path-traversal attempt detected).
- **`IOException`** / **`UnauthorizedAccessException`** — suppressed during `Dispose`.
  Cleanup failures are non-fatal and do not propagate.

#### Interactions

##### Dependencies

- **PathHelpers** — `GetFilePath` calls `PathHelpers.SafePathCombine` to validate and
  construct paths within the temporary directory. The constructor also calls
  `PathHelpers.SafePathCombine` to build the unique directory path.

##### Callers

- **Validation** — uses `TemporaryDirectory` (previously a private nested class) to create
  isolated scratch directories for each self-validation test method.
- **Test project** — all test classes that require temporary file-system workspace use
  `TemporaryDirectory` via a `private readonly TemporaryDirectory _testDirectory = new()`
  field and call `GetFilePath` to obtain paths for fixture files.
