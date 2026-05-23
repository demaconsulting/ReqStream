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

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace DemaConsulting.ReqStream.Utilities;

/// <summary>
///     Provides glob-pattern file matching utilities.
/// </summary>
/// <remarks>
///     All methods are stateless and thread-safe; no shared mutable state exists at the class
///     level. Multiple threads may call <see cref="FindMatchingFiles"/> and
///     <see cref="SplitAbsolutePattern"/> concurrently without synchronization.
/// </remarks>
internal static class GlobMatcher
{
    /// <summary>
    ///     Finds all files matching the specified glob patterns.
    /// </summary>
    /// <remarks>
    ///     Case sensitivity is determined at runtime by checking
    ///     <see cref="OperatingSystem.IsWindows"/>: ordinal ignore-case is used on Windows and
    ///     ordinal (case-sensitive) on all other platforms. This ensures deduplication correctly
    ///     handles file-system semantics on each OS.
    ///
    ///     Relative patterns are batched into a single <c>Matcher</c> call against the current
    ///     working directory. Each absolute pattern is processed individually after decomposing it
    ///     into a root directory and relative sub-pattern via <see cref="SplitAbsolutePattern"/>.
    ///     Individual null elements within <paramref name="patterns"/> are skipped silently.
    /// </remarks>
    /// <param name="patterns">
    ///     Glob patterns to match. Must not be null. Patterns may be relative (matched against
    ///     the current working directory) or absolute (matched from the rooted prefix of the
    ///     pattern). Individual null elements are skipped.
    /// </param>
    /// <returns>
    ///     Sorted list of full file paths matching any of the supplied patterns. Duplicate paths
    ///     are removed using the file-system-appropriate comparer (ordinal ignore-case on Windows,
    ///     ordinal on case-sensitive systems).
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///     Thrown when <paramref name="patterns"/> is null.
    /// </exception>
    internal static List<string> FindMatchingFiles(IEnumerable<string> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);

        // Use a comparer that matches the underlying file-system's case-sensitivity so that
        // deduplication is correct: case-insensitive on Windows, case-sensitive elsewhere.
        var fsComparer = OperatingSystem.IsWindows()
            ? StringComparer.OrdinalIgnoreCase
            : StringComparer.Ordinal;
        var files = new HashSet<string>(fsComparer);
        var relativePatterns = new List<string>();

        foreach (var pattern in patterns)
        {
            // Skip null elements so callers do not need to filter their collections
            if (pattern is null)
            {
                continue;
            }

            if (Path.IsPathRooted(pattern))
            {
                // Handle absolute path by extracting the root directory and relative pattern
                var (rootDir, relativePattern) = SplitAbsolutePattern(pattern);
                if (!Directory.Exists(rootDir))
                {
                    continue;
                }

                var matcher = new Matcher();
                matcher.AddInclude(relativePattern);
                var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(rootDir)));
                foreach (var file in result.Files)
                {
                    files.Add(Path.GetFullPath(PathHelpers.SafePathCombine(rootDir, file.Path)));
                }
            }
            else
            {
                relativePatterns.Add(pattern);
            }
        }

        // Handle all relative patterns together against the current working directory
        if (relativePatterns.Count > 0)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var matcher = new Matcher();
            foreach (var pattern in relativePatterns)
            {
                matcher.AddInclude(pattern);
            }

            var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(currentDirectory)));
            foreach (var file in result.Files)
            {
                files.Add(Path.GetFullPath(PathHelpers.SafePathCombine(currentDirectory, file.Path)));
            }
        }

        return files.OrderBy(f => f, fsComparer).ToList();
    }

    /// <summary>
    ///     Splits an absolute glob pattern into a root directory and a relative pattern.
    /// </summary>
    /// <param name="absolutePattern">The absolute glob pattern to split.</param>
    /// <returns>
    ///     A tuple of (<c>rootDir</c>, <c>relativePattern</c>) where <c>rootDir</c> is the
    ///     deepest directory segment before the first wildcard character and
    ///     <c>relativePattern</c> is the remainder of the pattern relative to that directory.
    /// </returns>
    /// <remarks>
    ///     The method locates the last directory separator that precedes the first wildcard
    ///     character (<c>*</c>, <c>?</c>, or <c>[</c>) and uses that position as the
    ///     split point. If no wildcard is present the pattern is treated as a literal file
    ///     path and split at the final separator.
    /// </remarks>
    internal static (string rootDir, string relativePattern) SplitAbsolutePattern(string absolutePattern)
    {
        var pathRoot = Path.GetPathRoot(absolutePattern) ?? string.Empty;

        // Find the index of the first wildcard character in the pattern
        // Microsoft.Extensions.FileSystemGlobbing supports *, **, ?, and [abc] ranges
        var wildcardIndex = absolutePattern.IndexOfAny(['*', '?', '[']);

        if (wildcardIndex < 0)
        {
            // No wildcard - treat as a specific file path
            return (
                Path.GetDirectoryName(absolutePattern) ?? pathRoot,
                Path.GetFileName(absolutePattern));
        }

        // Find the last directory separator before the first wildcard.
        // LastIndexOfAny with a start index searches backwards from that position toward the start,
        // so this finds the rightmost separator that precedes the wildcard character.
        var lastSepIndex = absolutePattern.LastIndexOfAny(
            [Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar],
            wildcardIndex);

        if (lastSepIndex < 0)
        {
            // No separator before wildcard - use the path root
            return (pathRoot, absolutePattern[pathRoot.Length..]);
        }

        var rootDir = absolutePattern[..lastSepIndex];
        var relativePattern = absolutePattern[(lastSepIndex + 1)..];

        // Handle empty root (Unix paths like /file.json where separator is the very first char)
        if (string.IsNullOrEmpty(rootDir))
        {
            return (pathRoot, relativePattern);
        }

        // Ensure drive/volume root includes trailing separator.
        // On Windows, splitting "C:\*.json" at the backslash yields rootDir = "C:" (no trailing
        // backslash), but DirectoryInfo requires "C:\" to refer to the drive root.
        if (rootDir == pathRoot.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))
        {
            return (pathRoot, relativePattern);
        }

        return (rootDir, relativePattern);
    }
}
