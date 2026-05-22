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
///     Severity level for a lint issue.
/// </summary>
public enum LintSeverity
{
    /// <summary>
    ///     Warning: a non-fatal issue; processing can continue.
    /// </summary>
    Warning,

    /// <summary>
    ///     Error: a fatal issue that prevents successful requirements loading.
    /// </summary>
    Error
}

/// <summary>
///     Represents a single issue found during requirements linting or loading.
/// </summary>
public sealed class LintIssue
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="LintIssue"/> class.
    /// </summary>
    /// <param name="location">The location string (e.g. "file.yaml" or "file.yaml(3,5)").</param>
    /// <param name="severity">The severity of the issue.</param>
    /// <param name="description">A human-readable description of the issue.</param>
    public LintIssue(string location, LintSeverity severity, string description)
    {
        Location = location;
        Severity = severity;
        Description = description;
    }

    /// <summary>
    ///     Gets the location string (e.g. "file.yaml" or "file.yaml(3,5)").
    /// </summary>
    public string Location { get; }

    /// <summary>
    ///     Gets the severity of the issue.
    /// </summary>
    public LintSeverity Severity { get; }

    /// <summary>
    ///     Gets a human-readable description of the issue.
    /// </summary>
    public string Description { get; }

    /// <summary>
    ///     Returns the issue formatted as "location: severity: description".
    /// </summary>
    /// <returns>A formatted string representing the issue.</returns>
    /// <remarks>
    ///     The <c>"location: severity: description"</c> format was chosen for compatibility with
    ///     editors and CI tools that parse diagnostic location and severity tokens from tool output,
    ///     allowing them to navigate directly to the source of the issue.
    /// </remarks>
    public override string ToString()
    {
        var severityText = Severity == LintSeverity.Error ? "error" : "warning";
        return $"{Location}: {severityText}: {Description}";
    }
}
