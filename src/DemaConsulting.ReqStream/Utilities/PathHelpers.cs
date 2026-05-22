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

namespace DemaConsulting.ReqStream.Utilities;

/// <summary>
///     Helper utilities for safe path operations.
///     Protects against path-traversal attacks by ensuring combined paths remain
///     within the intended base directory.
/// </summary>
internal static class PathHelpers
{
    /// <summary>
    ///     Safely combines two paths, ensuring the resolved combined path stays within the base directory.
    /// </summary>
    /// <remarks>
    ///     This method exists to prevent path-traversal attacks on user-supplied include paths: a
    ///     crafted relative component such as <c>../../etc/passwd</c> or an absolute override could
    ///     otherwise cause the tool to read files outside the intended directory tree.
    ///
    ///     The algorithm validates the combined path against the base directory using fully resolved
    ///     absolute forms, but deliberately returns the non-resolved <c>Path.Combine</c> result so
    ///     that the caller's relative or working-directory style is preserved for downstream display
    ///     and further path operations.
    ///
    ///     Stateless; safe for concurrent use.
    /// </remarks>
    /// <param name="basePath">The base directory path. Must not be <see langword="null"/>.</param>
    /// <param name="relativePath">The relative path segment to combine. Must not be <see langword="null"/>.</param>
    /// <returns>
    ///     The combined path in non-resolved form (equivalent to <c>Path.Combine(basePath, relativePath)</c>),
    ///     guaranteed to resolve within <paramref name="basePath"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="basePath"/> or <paramref name="relativePath"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown when the resolved combined path escapes the base directory, or when a supplied path is invalid.
    /// </exception>
    /// <exception cref="NotSupportedException">Thrown when a supplied path contains an unsupported format.</exception>
    /// <exception cref="PathTooLongException">Thrown when the combined or resolved path exceeds the system-defined maximum length.</exception>
    internal static string SafePathCombine(string basePath, string relativePath)
    {
        // Validate inputs
        ArgumentNullException.ThrowIfNull(basePath);
        ArgumentNullException.ThrowIfNull(relativePath);

        // Combine the paths (preserves the caller's relative/absolute style)
        var combinedPath = Path.Combine(basePath, relativePath);

        // Security check: resolve both paths to absolute form and verify the combined
        // path is still inside the base directory. Path.GetRelativePath handles root
        // paths, platform case-sensitivity, and directory-separator normalization natively.
        var absoluteBase = Path.GetFullPath(basePath);
        var absoluteCombined = Path.GetFullPath(combinedPath);
        var checkRelative = Path.GetRelativePath(absoluteBase, absoluteCombined);

        if (string.Equals(checkRelative, "..", StringComparison.Ordinal)
            || checkRelative.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            || checkRelative.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal)
            || Path.IsPathRooted(checkRelative))
        {
            throw new ArgumentException($"Invalid path component: {relativePath}", nameof(relativePath));
        }

        return combinedPath;
    }
}
