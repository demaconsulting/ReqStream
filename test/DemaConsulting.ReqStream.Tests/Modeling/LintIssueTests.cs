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

using DemaConsulting.ReqStream.Modeling;

namespace DemaConsulting.ReqStream.Tests.Modeling;

/// <summary>
/// Unit tests for the LintIssue class, proving it correctly represents and formats lint issues.
/// </summary>
public class LintIssueTests
{
    /// <summary>
    /// Test that LintIssue.ToString() formats error severity as "error".
    /// </summary>
    [Fact]
    public void LintIssue_ToString_ErrorSeverity_FormatsAsError()
    {
        // Arrange: create a LintIssue with error severity
        var issue = new LintIssue("file.yaml(3,5)", LintSeverity.Error, "Some error");

        // Act / Assert: verify ToString uses "error" for LintSeverity.Error
        Assert.Equal("file.yaml(3,5): error: Some error", issue.ToString());
    }

    /// <summary>
    /// Test that LintIssue.ToString() formats warning severity as "warning".
    /// </summary>
    [Fact]
    public void LintIssue_ToString_WarningSeverity_FormatsAsWarning()
    {
        // Arrange: create a LintIssue with warning severity
        var issue = new LintIssue("file.yaml", LintSeverity.Warning, "Some warning");

        // Act / Assert: verify ToString uses "warning" for LintSeverity.Warning
        Assert.Equal("file.yaml: warning: Some warning", issue.ToString());
    }

    /// <summary>
    /// Test that LintIssue.ToString() handles an empty location correctly.
    /// </summary>
    [Fact]
    public void LintIssue_ToString_EmptyLocation_FormatsCorrectly()
    {
        // Arrange: create a LintIssue with an empty location string
        var issue = new LintIssue(string.Empty, LintSeverity.Error, "Some error");

        // Act / Assert: verify ToString still formats as "location: severity: description"
        Assert.Equal(": error: Some error", issue.ToString());
    }

    /// <summary>
    /// Test that LintIssue.ToString() handles an empty description correctly.
    /// </summary>
    [Fact]
    public void LintIssue_ToString_EmptyDescription_FormatsCorrectly()
    {
        // Arrange: create a LintIssue with an empty description string
        var issue = new LintIssue("file.yaml", LintSeverity.Warning, string.Empty);

        // Act / Assert: verify ToString still formats as "location: severity: description"
        Assert.Equal("file.yaml: warning: ", issue.ToString());
    }
}
