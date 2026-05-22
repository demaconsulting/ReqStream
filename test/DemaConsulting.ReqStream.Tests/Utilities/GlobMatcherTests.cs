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
/// Unit tests for the GlobMatcher class.
/// </summary>
public sealed class GlobMatcherTests : IDisposable
{
    /// <summary>Temporary directory providing isolated file-system workspace for this test class instance.</summary>
    private readonly TemporaryDirectory _testDirectory = new();

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    public GlobMatcherTests()
    {

    }

    /// <summary>
    /// Clean up test by deleting the temporary test directory.
    /// </summary>
    public void Dispose()
    {
        _testDirectory.Dispose();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Test that a relative glob pattern matches files in the current directory.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_RelativePattern_MatchesFiles()
    {
        // Arrange: create test files in the test directory and set it as current
        var file1 = _testDirectory.GetFilePath("file1.yaml");
        var file2 = _testDirectory.GetFilePath("file2.yaml");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: find files using a relative glob pattern
            var files = GlobMatcher.FindMatchingFiles(["*.yaml"]);

            // Assert: both files are found
            Assert.Equal(2, files.Count);
            Assert.Single(files, f => f.EndsWith("file1.yaml"));
            Assert.Single(files, f => f.EndsWith("file2.yaml"));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test that an absolute glob pattern with a wildcard matches files.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_AbsolutePatternWithWildcard_MatchesFiles()
    {
        // Arrange: create test files in the test directory
        var file1 = _testDirectory.GetFilePath("test1.trx");
        var file2 = _testDirectory.GetFilePath("test2.trx");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        // Act: find files using an absolute glob pattern
        var pattern = _testDirectory.GetFilePath("*.trx");
        var files = GlobMatcher.FindMatchingFiles([pattern]);

        // Assert: both files are found
        Assert.Equal(2, files.Count);
        Assert.Single(files, f => f.EndsWith("test1.trx"));
        Assert.Single(files, f => f.EndsWith("test2.trx"));
    }

    /// <summary>
    /// Test that an absolute glob pattern with ** wildcard matches files in subdirectories.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_AbsolutePatternWithDoubleWildcard_MatchesFilesInSubdirectories()
    {
        // Arrange: create test files in a subdirectory
        var subDir = _testDirectory.GetFilePath("sub");
        Directory.CreateDirectory(subDir);
        var file1 = PathHelpers.SafePathCombine(subDir, "test1.trx");
        var file2 = PathHelpers.SafePathCombine(subDir, "test2.trx");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        // Act: find files using an absolute glob pattern with **
        var pattern = PathHelpers.SafePathCombine(_testDirectory.GetFilePath("**"), "*.trx");
        var files = GlobMatcher.FindMatchingFiles([pattern]);

        // Assert: both files are found
        Assert.Equal(2, files.Count);
        Assert.Single(files, f => f.EndsWith("test1.trx"));
        Assert.Single(files, f => f.EndsWith("test2.trx"));
    }

    /// <summary>
    /// Test that an absolute literal file path matches exactly that file.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_AbsoluteLiteralPath_MatchesSingleFile()
    {
        // Arrange: create a test file
        var file = _testDirectory.GetFilePath("exact.yaml");
        File.WriteAllText(file, "test");

        // Act: find file using an absolute literal path (no wildcards)
        var files = GlobMatcher.FindMatchingFiles([file]);

        // Assert: exactly that file is found
        Assert.Single(files);
        Assert.Single(files, f => f.EndsWith("exact.yaml"));
    }

    /// <summary>
    /// Test that an absolute pattern for a non-existent root directory returns an empty list.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_AbsolutePatternNonExistentDirectory_ReturnsEmpty()
    {
        // Arrange: construct a pattern rooted in a directory that does not exist
        var nonExistentDir = _testDirectory.GetFilePath("does_not_exist");
        var pattern = PathHelpers.SafePathCombine(nonExistentDir, "*.yaml");

        // Act: find files using the pattern
        var files = GlobMatcher.FindMatchingFiles([pattern]);

        // Assert: no files are returned
        Assert.Empty(files);
    }

    /// <summary>
    /// Test that returned paths are absolute (rooted).
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_ReturnsAbsolutePaths()
    {
        // Arrange: create a test file and set the working directory
        var file = _testDirectory.GetFilePath("abs.yaml");
        File.WriteAllText(file, "test");

        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory.DirectoryPath);

            // Act: find file using a relative pattern
            var files = GlobMatcher.FindMatchingFiles(["abs.yaml"]);

            // Assert: the returned path is absolute
            Assert.Single(files);
            Assert.True(Path.IsPathRooted(files[0]));
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test SplitAbsolutePattern with a wildcard at top level.
    /// </summary>
    [Fact]
    public void GlobMatcher_SplitAbsolutePattern_WildcardAtTopLevel_SplitsAtRoot()
    {
        // Arrange
        var root = Path.GetPathRoot(_testDirectory.DirectoryPath)!;
        var pattern = PathHelpers.SafePathCombine(root, "*.yaml");

        // Act
        var (rootDir, relativePattern) = GlobMatcher.SplitAbsolutePattern(pattern);

        // Assert: root directory equals the path root and relative pattern is the wildcard part
        Assert.Equal(root, rootDir);
        Assert.Equal("*.yaml", relativePattern);
    }

    /// <summary>
    /// Test SplitAbsolutePattern with a literal file path (no wildcard).
    /// </summary>
    [Fact]
    public void GlobMatcher_SplitAbsolutePattern_LiteralPath_SplitsAtLastSeparator()
    {
        // Arrange
        var pattern = _testDirectory.GetFilePath("requirements.yaml");

        // Act
        var (rootDir, relativePattern) = GlobMatcher.SplitAbsolutePattern(pattern);

        // Assert
        Assert.Equal(_testDirectory.DirectoryPath, rootDir);
        Assert.Equal("requirements.yaml", relativePattern);
    }

    /// <summary>
    /// Test SplitAbsolutePattern with a double-star wildcard in a subdirectory.
    /// </summary>
    [Fact]
    public void GlobMatcher_SplitAbsolutePattern_DoubleStarWildcard_SplitsBeforeWildcard()
    {
        // Arrange
        var pattern = PathHelpers.SafePathCombine(_testDirectory.GetFilePath("**"), "*.trx");

        // Act
        var (rootDir, relativePattern) = GlobMatcher.SplitAbsolutePattern(pattern);

        // Assert: rootDir is the test directory; relativePattern contains the wildcard segments
        Assert.Equal(_testDirectory.DirectoryPath, rootDir);
        Assert.Contains("**", relativePattern);
        Assert.Contains("*.trx", relativePattern);
    }

    /// <summary>
    /// Test that multiple patterns are combined and duplicate files are deduplicated.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_MultiplePatterns_DeduplicatesResults()
    {
        // Arrange: create test files
        var file1 = _testDirectory.GetFilePath("shared1.yaml");
        var file2 = _testDirectory.GetFilePath("shared2.yaml");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        // Act: two patterns that both match the same files
        var absoluteWildcard = _testDirectory.GetFilePath("*.yaml");
        var absoluteLiteral1 = _testDirectory.GetFilePath("shared1.yaml");
        var files = GlobMatcher.FindMatchingFiles([absoluteWildcard, absoluteLiteral1]);

        // Assert: each file appears only once despite being matched by multiple patterns
        Assert.Equal(2, files.Count);
        Assert.Single(files, f => f.EndsWith("shared1.yaml"));
        Assert.Single(files, f => f.EndsWith("shared2.yaml"));
    }

    /// <summary>
    ///     Validates that results are returned in lexicographic ascending order.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_MultipleMatches_ReturnsSortedResults()
    {
        // Arrange: create files whose names would be unsorted if added in creation order
        var fileB = _testDirectory.GetFilePath("b_file.yaml");
        var fileA = _testDirectory.GetFilePath("a_file.yaml");
        var fileC = _testDirectory.GetFilePath("c_file.yaml");
        File.WriteAllText(fileB, "test");
        File.WriteAllText(fileA, "test");
        File.WriteAllText(fileC, "test");

        // Act: find files using an absolute glob pattern
        var pattern = _testDirectory.GetFilePath("*.yaml");
        var files = GlobMatcher.FindMatchingFiles([pattern]);

        // Assert: files are returned in lexicographic ascending order (a before b before c)
        Assert.Equal(3, files.Count);
        Assert.Equal(fileA, files[0]);
        Assert.Equal(fileB, files[1]);
        Assert.Equal(fileC, files[2]);
    }

    /// <summary>
    ///     Validates that an empty patterns collection returns an empty result without throwing.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_EmptyPatterns_ReturnsEmpty()
    {
        // Arrange: no patterns supplied

        // Act: find files with an empty patterns list
        var files = GlobMatcher.FindMatchingFiles([]);

        // Assert: result is empty
        Assert.Empty(files);
    }

    /// <summary>
    /// Test that multiple patterns from different directories are combined into one result.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_MultiplePatterns_CombinesFromDifferentSources()
    {
        // Arrange: create files in two separate subdirectories
        var dir1 = _testDirectory.GetFilePath("dir1");
        var dir2 = _testDirectory.GetFilePath("dir2");
        Directory.CreateDirectory(dir1);
        Directory.CreateDirectory(dir2);
        var file1 = PathHelpers.SafePathCombine(dir1, "req1.yaml");
        var file2 = PathHelpers.SafePathCombine(dir2, "req2.yaml");
        File.WriteAllText(file1, "test");
        File.WriteAllText(file2, "test");

        // Act: one pattern per directory
        var pattern1 = PathHelpers.SafePathCombine(dir1, "*.yaml");
        var pattern2 = PathHelpers.SafePathCombine(dir2, "*.yaml");
        var files = GlobMatcher.FindMatchingFiles([pattern1, pattern2]);

        // Assert: files from both directories are returned
        Assert.Equal(2, files.Count);
        Assert.Single(files, f => f.EndsWith("req1.yaml"));
        Assert.Single(files, f => f.EndsWith("req2.yaml"));
    }

    /// <summary>
    ///     Verifies that passing null as the patterns argument throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_NullPatterns_ThrowsArgumentNullException()
    {
        // Act and Assert: null patterns must throw ArgumentNullException immediately
        Assert.Throws<ArgumentNullException>(() => GlobMatcher.FindMatchingFiles(null!));
    }

    /// <summary>
    ///     Verifies that a null element within the patterns collection is silently skipped
    ///     and does not prevent the non-null patterns from being processed.
    /// </summary>
    [Fact]
    public void GlobMatcher_FindMatchingFiles_NullElementInPatterns_SkipsElement()
    {
        // Arrange: create a test file matched by the non-null pattern
        var file = _testDirectory.GetFilePath("skip_null.yaml");
        File.WriteAllText(file, "test");
        var pattern = _testDirectory.GetFilePath("*.yaml");

        // Act: collection contains a null element alongside a valid pattern
        var files = GlobMatcher.FindMatchingFiles([null!, pattern]);

        // Assert: the null element is skipped; the valid pattern still matches
        Assert.Single(files);
        Assert.Single(files, f => f.EndsWith("skip_null.yaml"));
    }
}
