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
/// Unit tests for the PathHelpers class.
/// </summary>
public sealed class PathHelpersTests
{
    /// <summary>
    /// Test that a simple relative file name is combined correctly.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_ValidRelativePath_ReturnsCombinedPath()
    {
        // Arrange
        var basePath = Path.GetTempPath();
        const string relativePath = "output.log";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert: result starts with the base path and ends with the relative component
        Assert.StartsWith(basePath, result, StringComparison.Ordinal);
        Assert.EndsWith(relativePath, result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Test that a subdirectory component is combined correctly.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_ValidSubdirectory_ReturnsCombinedPath()
    {
        // Arrange
        var basePath = Path.GetTempPath();
        const string relativePath = "subdir/output.log";

        // Act
        var result = PathHelpers.SafePathCombine(basePath, relativePath);

        // Assert: path contains both the base and the relative component
        Assert.StartsWith(basePath, result, StringComparison.Ordinal);
        Assert.Contains("output.log", result, StringComparison.Ordinal);
    }

    /// <summary>
    /// Test that a single path-traversal component is rejected.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_DotDotPath_ThrowsArgumentException()
    {
        // Arrange
        var basePath = Path.GetTempPath();
        const string relativePath = "..";

        // Act + Assert
        Assert.Throws<ArgumentException>(() => PathHelpers.SafePathCombine(basePath, relativePath));
    }

    /// <summary>
    /// Test that nested path traversal is rejected.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_DeepDotDotPath_ThrowsArgumentException()
    {
        // Arrange
        var basePath = Path.GetTempPath();
        var relativePath = ".." + Path.DirectorySeparatorChar + "etc" + Path.DirectorySeparatorChar + "passwd";

        // Act + Assert
        Assert.Throws<ArgumentException>(() => PathHelpers.SafePathCombine(basePath, relativePath));
    }

    /// <summary>
    /// Test that an absolute path override is rejected.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_AbsoluteOverridePath_ThrowsArgumentException()
    {
        // Arrange
        var basePath = Path.GetTempPath();
        var relativePath = Path.GetPathRoot(basePath) + "override";

        // Act + Assert
        Assert.Throws<ArgumentException>(() => PathHelpers.SafePathCombine(basePath, relativePath));
    }

    /// <summary>
    /// Test that a null base path throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_NullBasePath_ThrowsArgumentNullException()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => PathHelpers.SafePathCombine(null!, "file.txt"));
    }

    /// <summary>
    /// Test that a null relative path throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void PathHelpers_SafePathCombine_NullRelativePath_ThrowsArgumentNullException()
    {
        // Act + Assert
        Assert.Throws<ArgumentNullException>(() => PathHelpers.SafePathCombine(Path.GetTempPath(), null!));
    }
}
