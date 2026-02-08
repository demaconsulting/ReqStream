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

namespace DemaConsulting.ReqStream;

/// <summary>
///     Represents test execution results for a specific test, tracking total executions and passed executions.
/// </summary>
public class TestResultEntry
{
    /// <summary>
    ///     Gets or sets the total number of times this test was executed.
    /// </summary>
    public int Executed { get; set; }

    /// <summary>
    ///     Gets or sets the number of times this test passed.
    /// </summary>
    public int Passed { get; set; }
}

/// <summary>
///     Represents a single test execution from a specific test result file.
/// </summary>
/// <param name="FileBaseName">The base name of the test file (without extension).</param>
/// <param name="Name">The test name.</param>
/// <param name="Passes">Number of passes in the file matching the test name.</param>
/// <param name="Fails">Number of fails in the file matching the test name.</param>
public record TestExecution(string FileBaseName, string Name, int Passes, int Fails);
