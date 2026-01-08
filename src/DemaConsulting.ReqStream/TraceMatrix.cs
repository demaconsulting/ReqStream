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

using DemaConsulting.TestResults.IO;

namespace DemaConsulting.ReqStream;

/// <summary>
///     Represents a traceability matrix that maps test results to requirements.
///     Supports TRX and JUnit test result formats.
/// </summary>
public class TraceMatrix
{
    /// <summary>
    ///     Dictionary mapping test names to their execution results.
    /// </summary>
    private readonly Dictionary<string, TestResultEntry> _testResults = new();

    /// <summary>
    ///     Initializes a new instance of the TraceMatrix class.
    /// </summary>
    /// <param name="requirements">The requirements containing test mappings.</param>
    /// <param name="testResultFiles">Paths to test result files (TRX or JUnit format).</param>
    /// <exception cref="ArgumentNullException">Thrown when requirements is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when a test result file does not exist.</exception>
    public TraceMatrix(Requirements requirements, params string[] testResultFiles)
    {
        if (requirements == null)
        {
            throw new ArgumentNullException(nameof(requirements));
        }

        // Collect all test names from requirements
        var requiredTests = CollectTestNames(requirements);

        // Process each test result file
        foreach (var filePath in testResultFiles)
        {
            ProcessTestResultFile(filePath, requiredTests);
        }
    }

    /// <summary>
    ///     Gets the test result entry for a specific test name.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <returns>The TestResultEntry for the test, or null if the test was not found.</returns>
    public TestResultEntry? GetTestResult(string testName)
    {
        return _testResults.TryGetValue(testName, out var result) ? result : null;
    }

    /// <summary>
    ///     Gets all test result entries.
    /// </summary>
    /// <returns>A read-only dictionary of test names to their result entries.</returns>
    public IReadOnlyDictionary<string, TestResultEntry> GetAllTestResults()
    {
        return _testResults;
    }

    /// <summary>
    ///     Collects all test names from the requirements tree.
    /// </summary>
    /// <param name="section">The section to search for tests.</param>
    /// <returns>A hash set containing all unique test names.</returns>
    private static HashSet<string> CollectTestNames(Section section)
    {
        var testNames = new HashSet<string>();

        // Collect tests from requirements in this section
        foreach (var requirement in section.Requirements)
        {
            foreach (var test in requirement.Tests)
            {
                testNames.Add(test);
            }
        }

        // Recursively collect tests from child sections
        foreach (var childSection in section.Sections)
        {
            var childTests = CollectTestNames(childSection);
            testNames.UnionWith(childTests);
        }

        return testNames;
    }

    /// <summary>
    ///     Processes a test result file and updates test execution counts.
    /// </summary>
    /// <param name="filePath">Path to the test result file.</param>
    /// <param name="requiredTests">Set of test names that are referenced in requirements.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    private void ProcessTestResultFile(string filePath, HashSet<string> requiredTests)
    {
        // Verify file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test result file not found: {filePath}", filePath);
        }

        // Read the file content
        var content = File.ReadAllText(filePath);

        // Try to parse as TRX first, then JUnit
        DemaConsulting.TestResults.TestResults testResults;
        try
        {
            testResults = TrxSerializer.Deserialize(content);
        }
        catch
        {
            // If TRX parsing fails, try JUnit
            try
            {
                testResults = JUnitSerializer.Deserialize(content);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to parse test result file as TRX or JUnit format: {filePath}", ex);
            }
        }

        // Process each test result
        foreach (var result in testResults.Results)
        {
            // Only process tests that are referenced in requirements
            if (!requiredTests.Contains(result.Name))
            {
                continue;
            }

            // Get or create the test result entry
            if (!_testResults.TryGetValue(result.Name, out var entry))
            {
                entry = new TestResultEntry();
                _testResults[result.Name] = entry;
            }

            // Update execution counts
            entry.Executed++;
            
            if (result.Outcome == DemaConsulting.TestResults.TestOutcome.Passed)
            {
                entry.Passed++;
            }
        }
    }
}
