// Copyright (c) 2026 DEMA Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.ReqStream.Utilities;

namespace DemaConsulting.ReqStream.Tests.Utilities;

/// <summary>
///     Unit tests for the <see cref="TemporaryDirectory"/> class.
/// </summary>
/// <remarks>
///     These tests are placed in the Sequential collection to avoid races on the process-wide
///     current working directory, which <see cref="TemporaryDirectory"/> reads in its constructor.
/// </remarks>
[Collection("Sequential")]
public sealed class TemporaryDirectoryTests
{
    /// <summary>
    ///     Verifies that the constructor creates a directory on disk that exists for the
    ///     lifetime of the instance.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Constructor_Default_CreatesDirectory()
    {
        // Arrange / Act
        using var dir = new TemporaryDirectory();

        // Assert: the directory was created under the current working directory
        Assert.True(Directory.Exists(dir.DirectoryPath));
    }

    /// <summary>
    ///     Verifies that two independent instances receive distinct directory paths so that
    ///     concurrent or sequential tests do not collide.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Constructor_TwoInstances_CreateUniqueDirectories()
    {
        // Arrange / Act: create two instances
        using var dir1 = new TemporaryDirectory();
        using var dir2 = new TemporaryDirectory();

        // Assert: each instance has a different directory path
        Assert.NotEqual(dir1.DirectoryPath, dir2.DirectoryPath);
    }

    /// <summary>
    ///     Verifies that <see cref="TemporaryDirectory.GetFilePath"/> returns a path that is
    ///     rooted inside the temporary directory and ends with the supplied file name.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_SimpleFile_ReturnsPathUnderDirectory()
    {
        // Arrange
        using var dir = new TemporaryDirectory();

        // Act
        var filePath = dir.GetFilePath("output.md");

        // Assert: path is inside the temporary directory and has the expected file name
        Assert.StartsWith(dir.DirectoryPath, filePath, StringComparison.Ordinal);
        Assert.EndsWith("output.md", filePath, StringComparison.Ordinal);
    }

    /// <summary>
    ///     Verifies that <see cref="TemporaryDirectory.GetFilePath"/> creates any required
    ///     intermediate subdirectories so that the caller can write the file immediately.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_NestedPath_CreatesIntermediateDirectories()
    {
        // Arrange
        using var dir = new TemporaryDirectory();

        // Act: request a path nested two levels deep
        var filePath = dir.GetFilePath(Path.Combine("sub", "nested", "output.md"));

        // Assert: the parent directory exists even though no file has been written
        var parent = Path.GetDirectoryName(filePath);
        Assert.NotNull(parent);
        Assert.True(Directory.Exists(parent));
    }

    /// <summary>
    ///     Verifies that <see cref="TemporaryDirectory.GetFilePath"/> rejects a path containing
    ///     a traversal sequence (<c>../</c>) that would escape the temporary directory.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_GetFilePath_TraversalAttempt_ThrowsArgumentException()
    {
        // Arrange
        using var dir = new TemporaryDirectory();

        // Act + Assert: path traversal must be rejected
        Assert.Throws<ArgumentException>(() => dir.GetFilePath("../escaped.txt"));
    }

    /// <summary>
    ///     Verifies that disposing a <see cref="TemporaryDirectory"/> deletes the directory and
    ///     all its contents from disk.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Dispose_PopulatedDirectory_DeletesDirectory()
    {
        // Arrange: create a directory and write a file inside it
        string capturedPath;
        using (var dir = new TemporaryDirectory())
        {
            capturedPath = dir.DirectoryPath;
            File.WriteAllText(dir.GetFilePath("probe.txt"), "probe");

            // Assert: directory exists while still in scope
            Assert.True(Directory.Exists(capturedPath));
        }

        // Assert: directory no longer exists after disposal
        Assert.False(Directory.Exists(capturedPath));
    }

    /// <summary>
    ///     Verifies that calling <see cref="TemporaryDirectory.Dispose"/> on an instance whose
    ///     directory has already been deleted externally does not throw.
    /// </summary>
    [Fact]
    public void TemporaryDirectory_Dispose_AlreadyDeleted_DoesNotThrow()
    {
        // Arrange: create the directory and then delete it manually
        var dir = new TemporaryDirectory();
        Directory.Delete(dir.DirectoryPath, recursive: true);

        // Act + Assert: second disposal must not throw
        var exception = Record.Exception(() => dir.Dispose());
        Assert.Null(exception);
    }
}
