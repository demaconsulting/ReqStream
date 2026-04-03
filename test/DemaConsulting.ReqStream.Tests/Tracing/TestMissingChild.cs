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
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DemaConsulting.ReqStream.Tests.Tracing;

/// <summary>
///     Tests verifying that requirements with missing child references are reported as errors.
/// </summary>
[TestClass]
public class TestMissingChildRequirement
{
    /// <summary>
    ///     Verifies that loading a requirements file where a requirement references a
    ///     non-existent child ID produces an error-level lint issue.
    /// </summary>
    [TestMethod]
    public void Requirements_Load_WithMissingChild_ReportsError()
    {
        var yaml = @"---
sections:
  - title: ""Test""
    requirements:
      - id: ""PARENT""
        title: ""Parent requirement""
        children:
          - ""NONEXISTENT""
";
        var path = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.yaml");
        File.WriteAllText(path, yaml);
        try
        {
            var result = Requirements.Load(path);

            Assert.IsTrue(result.HasErrors, "Expected errors for missing child reference");
            Assert.IsNull(result.Requirements, "Requirements should be null when errors exist");
            Assert.IsTrue(
                result.Issues.Any(i => i.Severity == LintSeverity.Error && i.Description.Contains("NONEXISTENT")),
                "Expected an error mentioning the unknown child 'NONEXISTENT'");
        }
        finally
        {
            File.Delete(path);
        }
    }
}
