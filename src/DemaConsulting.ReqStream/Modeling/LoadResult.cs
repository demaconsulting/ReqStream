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

namespace DemaConsulting.ReqStream.Modeling;

/// <summary>
///     Encapsulates the result of loading one or more requirements YAML files, including the
///     parsed requirements tree and any lint issues found during loading.
/// </summary>
public sealed class LoadResult
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LoadResult"/> class.
    /// </summary>
    /// <param name="requirements">
    ///     The parsed requirements tree, or <c>null</c> when error-level issues prevent successful loading.
    /// </param>
    /// <param name="issues">The read-only list of lint issues found during loading.</param>
    internal LoadResult(Requirements? requirements, IReadOnlyList<LintIssue> issues)
    {
        Requirements = requirements;
        Issues = issues;
    }

    /// <summary>
    ///     Gets the parsed requirements tree, or <c>null</c> when error-level issues are present.
    /// </summary>
    public Requirements? Requirements { get; }

    /// <summary>
    ///     Gets the read-only list of lint issues found during loading.
    /// </summary>
    public IReadOnlyList<LintIssue> Issues { get; }

    /// <summary>
    ///     Gets a value indicating whether any error-level lint issues were found during loading.
    /// </summary>
    public bool HasErrors => Issues.Any(i => i.Severity == LintSeverity.Error);

    /// <summary>
    ///     Reports all lint issues to the supplied output delegates.
    ///     Warning-level issues are sent to <paramref name="writeMessage"/>;
    ///     error-level issues are sent to <paramref name="writeError"/>.
    /// </summary>
    /// <param name="writeMessage">Delegate for non-fatal (warning) messages.</param>
    /// <param name="writeError">Delegate for fatal (error) messages.</param>
    public void ReportIssues(Action<string> writeMessage, Action<string> writeError)
    {
        foreach (var issue in Issues)
        {
            if (issue.Severity == LintSeverity.Error)
            {
                writeError(issue.ToString());
            }
            else
            {
                writeMessage(issue.ToString());
            }
        }
    }
}
