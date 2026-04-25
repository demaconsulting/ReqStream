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
[TestClass]
public class LintIssueTests
{
    /// <summary>
    /// Test that LintIssue.ToString() formats as "location: severity: description".
    /// </summary>
    [TestMethod]
    public void LintIssue_ToString_FormatsCorrectly()
    {
        // Arrange: create LintIssue instances with error and warning severity
        var errorIssue = new LintIssue("file.yaml(3,5)", LintSeverity.Error, "Some error");
        var warningIssue = new LintIssue("file.yaml", LintSeverity.Warning, "Some warning");

        // Act / Assert: verify ToString formats as "location: severity: description"
        Assert.AreEqual("file.yaml(3,5): error: Some error", errorIssue.ToString());
        Assert.AreEqual("file.yaml: warning: Some warning", warningIssue.ToString());
    }
}
