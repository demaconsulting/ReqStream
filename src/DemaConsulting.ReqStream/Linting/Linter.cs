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

using DemaConsulting.ReqStream.Cli;
using DemaConsulting.ReqStream.Modeling;

namespace DemaConsulting.ReqStream.Linting;

/// <summary>
///     Provides linting functionality for ReqStream requirement YAML files.
/// </summary>
public static class Linter
{
    /// <summary>
    ///     Lints a list of requirement files and returns all issues found.
    ///     Delegates to <see cref="Requirements.Load"/> so that linting and loading share a
    ///     single YAML DOM tree walk.
    /// </summary>
    /// <param name="files">The list of requirement files to lint.</param>
    /// <returns>A read-only list of lint issues found across all files and their includes.</returns>
    public static IReadOnlyList<LintIssue> Lint(IReadOnlyList<string> files)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(files);

        // No files to lint
        if (files.Count == 0)
        {
            return [];
        }

        // Delegate to Requirements.Load which performs the unified single-pass DOM walk
        var (_, issues) = Requirements.Load(files.ToArray());
        return issues;
    }

    /// <summary>
    ///     Lints a list of requirement files and reports all issues found.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="files">The list of requirement files to lint.</param>
    public static void Lint(Context context, IReadOnlyList<string> files)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(files);

        // No files to lint
        if (files.Count == 0)
        {
            context.WriteLine("No requirements files specified.");
            return;
        }

        // Collect and report issues
        var issues = Lint(files);
        foreach (var issue in issues)
        {
            context.WriteError(issue.ToString());
        }

        // If no issues found, print success message using first file as root
        if (issues.Count == 0)
        {
            context.WriteLine($"{files[0]}: No issues found");
        }
    }
}
