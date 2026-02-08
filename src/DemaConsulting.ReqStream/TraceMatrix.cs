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

using DemaConsulting.TestResults;
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.ReqStream;

/// <summary>
///     Represents test metrics for a single test execution.
/// </summary>
/// <param name="Passes">Number of passes in the file matching the test name.</param>
/// <param name="Fails">Number of fails in the file matching the test name.</param>
public record TestMetrics(int Passes, int Fails)
{
    /// <summary>
    ///     Gets the total number of executions (passes + fails).
    /// </summary>
    public int Executed => Passes + Fails;

    /// <summary>
    ///     Gets a value indicating whether all executions passed (no failures).
    /// </summary>
    public bool AllPassed => Fails == 0 && Executed > 0;
}

/// <summary>
///     Represents a single test execution from a specific test result file.
/// </summary>
/// <param name="FileBaseName">The base name of the test file (without extension).</param>
/// <param name="Name">The test name.</param>
/// <param name="Metrics">The test metrics (passes and fails).</param>
public record TestExecution(string FileBaseName, string Name, TestMetrics Metrics);

/// <summary>
///     Represents a traceability matrix that maps test results to requirements.
///     Supports TRX and JUnit test result formats.
/// </summary>
public class TraceMatrix
{
    /// <summary>
    ///     Dictionary mapping test names to their list of executions from different files.
    /// </summary>
    private readonly Dictionary<string, List<TestExecution>> _testExecutions = [];

    /// <summary>
    ///     The requirements object used to build this trace matrix.
    /// </summary>
    private readonly Requirements _requirements;

    /// <summary>
    ///     Initializes a new instance of the TraceMatrix class.
    /// </summary>
    /// <param name="requirements">The requirements containing test mappings.</param>
    /// <param name="testResultFiles">Paths to test result files (TRX or JUnit format).</param>
    /// <exception cref="ArgumentNullException">Thrown when requirements is null.</exception>
    /// <exception cref="FileNotFoundException">Thrown when a test result file does not exist.</exception>
    public TraceMatrix(Requirements requirements, params string[] testResultFiles)
    {
        ArgumentNullException.ThrowIfNull(requirements);

        _requirements = requirements;

        // Process each test result file
        foreach (var filePath in testResultFiles)
        {
            ProcessTestResultFile(filePath);
        }
    }

    /// <summary>
    ///     Gets the test metrics for a specific test name.
    /// </summary>
    /// <param name="testName">The name of the test (may include source filter as "source@testname").</param>
    /// <returns>The TestMetrics for the test (returns 0/0 if the test was not found).</returns>
    public TestMetrics GetTestResult(string testName)
    {
        var executions = FindTestExecutions(testName);
        if (executions.Count == 0)
        {
            return new TestMetrics(0, 0);
        }

        // Aggregate executions into a single metrics
        var totalPasses = executions.Sum(e => e.Metrics.Passes);
        var totalFails = executions.Sum(e => e.Metrics.Fails);
        return new TestMetrics(totalPasses, totalFails);
    }

    /// <summary>
    ///     Gets all test metrics for tests referenced in requirements.
    /// </summary>
    /// <returns>A read-only dictionary of test names to their metrics.</returns>
    public IReadOnlyDictionary<string, TestMetrics> GetAllTestResults()
    {
        // Build dictionary of all test results from required tests in the requirements
        var results = new Dictionary<string, TestMetrics>();
        var requiredTests = new HashSet<string>();
        CollectRequiredTestNames(_requirements, requiredTests);
        
        foreach (var testName in requiredTests)
        {
            var result = GetTestResult(testName);
            // Only include tests that have been executed
            if (result.Executed > 0)
            {
                results[testName] = result;
            }
        }
        
        return results;
    }

    /// <summary>
    ///     Collects all test names from the requirements tree.
    /// </summary>
    /// <param name="section">The section to search for tests.</param>
    /// <param name="testNames">The set to add test names to.</param>
    private static void CollectRequiredTestNames(Section section, HashSet<string> testNames)
    {
        // Collect tests from requirements in this section
        foreach (var test in section.Requirements.SelectMany(requirement => requirement.Tests))
        {
            testNames.Add(test);
        }

        // Recursively collect tests from child sections
        foreach (var childSection in section.Sections)
        {
            CollectRequiredTestNames(childSection, testNames);
        }
    }

    /// <summary>
    ///     Finds test executions for the given test name, optionally filtered by test source.
    /// </summary>
    /// <param name="testName">The test name (may include source filter as "source@testname").</param>
    /// <returns>A list of matching test executions.</returns>
    private List<TestExecution> FindTestExecutions(string testName)
    {
        // Parse test name to extract optional source filter
        var (sourceFilter, actualTestName) = ParseTestName(testName);

        // Look up test executions by actual test name
        if (!_testExecutions.TryGetValue(actualTestName, out var executions))
        {
            return [];
        }

        // If no source filter, return all executions
        if (sourceFilter == null)
        {
            return executions;
        }

        // Filter executions by source
        return executions
            .Where(e => e.FileBaseName.Contains(sourceFilter, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    ///     Exports the trace matrix to a Markdown file.
    /// </summary>
    /// <param name="filePath">The path to the output Markdown file.</param>
    /// <param name="depth">The starting depth for Markdown headers (default: 1).</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    public void Export(string filePath, int depth = 1)
    {
        // Validate file path
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        // Create a string builder to build the markdown content
        using var writer = new StringWriter();

        // Export Summary section
        ExportSummary(writer, depth);

        // Export Requirements section
        ExportRequirements(writer, depth);

        // Export Testing section
        ExportTesting(writer, depth);

        // Write the content to the file
        File.WriteAllText(filePath, writer.ToString());
    }

    /// <summary>
    ///     Exports the summary section showing satisfied requirements count.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="depth">The current depth for Markdown headers.</param>
    private void ExportSummary(TextWriter writer, int depth)
    {
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} Summary");
        writer.WriteLine();

        // Calculate satisfied requirements
        var (satisfied, total) = CalculateSatisfiedRequirements(_requirements);

        writer.WriteLine($"{satisfied} of {total} requirements are satisfied with tests.");
        writer.WriteLine();
    }

    /// <summary>
    ///     Calculates how many requirements are satisfied.
    ///     A requirement is satisfied if it has at least one test and all tests have passed.
    /// </summary>
    /// <returns>A tuple of (satisfied count, total count).</returns>
    public (int satisfied, int total) CalculateSatisfiedRequirements()
    {
        return CalculateSatisfiedRequirements(_requirements);
    }

    /// <summary>
    ///     Gets a list of requirement IDs that are not satisfied.
    ///     A requirement is not satisfied if it has no tests or any of its tests have not been executed or have failed.
    /// </summary>
    /// <returns>A list of unsatisfied requirement IDs.</returns>
    public List<string> GetUnsatisfiedRequirements()
    {
        var unsatisfied = new List<string>();
        CollectUnsatisfiedRequirements(_requirements, unsatisfied);
        return unsatisfied;
    }

    /// <summary>
    ///     Collects unsatisfied requirement IDs from a section and its subsections.
    /// </summary>
    /// <param name="section">The section to analyze.</param>
    /// <param name="unsatisfied">The list to add unsatisfied requirement IDs to.</param>
    private void CollectUnsatisfiedRequirements(Section section, List<string> unsatisfied)
    {
        // Check requirements in this section using LINQ Where
        unsatisfied.AddRange(
            section.Requirements
                .Where(requirement => !IsRequirementSatisfied(requirement, _requirements))
                .Select(requirement => requirement.Id));

        // Recursively check child sections
        foreach (var childSection in section.Sections)
        {
            CollectUnsatisfiedRequirements(childSection, unsatisfied);
        }
    }

    /// <summary>
    ///     Calculates how many requirements are satisfied.
    ///     A requirement is satisfied if it has at least one test and all tests have passed.
    /// </summary>
    /// <param name="section">The section to analyze.</param>
    /// <returns>A tuple of (satisfied count, total count).</returns>
    private (int satisfied, int total) CalculateSatisfiedRequirements(Section section)
    {
        var satisfied = 0;
        var total = 0;

        // Check requirements in this section
        foreach (var requirement in section.Requirements)
        {
            total++;
            if (IsRequirementSatisfied(requirement, _requirements))
            {
                satisfied++;
            }
        }

        // Recursively check child sections
        foreach (var childSection in section.Sections)
        {
            var (childSatisfied, childTotal) = CalculateSatisfiedRequirements(childSection);
            satisfied += childSatisfied;
            total += childTotal;
        }

        return (satisfied, total);
    }

    /// <summary>
    ///     Determines if a requirement is satisfied.
    ///     A requirement is satisfied if analyzing its tests and all child-requirement tests
    ///     recursively shows at least one test, and all tests have passed.
    /// </summary>
    /// <param name="requirement">The requirement to check.</param>
    /// <param name="rootSection">The root section for looking up child requirements.</param>
    /// <returns>True if the requirement is satisfied, false otherwise.</returns>
    private bool IsRequirementSatisfied(Requirement requirement, Section rootSection)
    {
        var allTests = new HashSet<string>();
        CollectAllTests(requirement, rootSection, allTests);

        // Must have at least one test
        if (allTests.Count == 0)
        {
            return false;
        }

        // All tests must have been executed and passed
        return allTests
            .Select(testName => GetTestResult(testName))
            .All(result => result.AllPassed);
    }

    /// <summary>
    ///     Collects all tests from a requirement and its children recursively.
    /// </summary>
    /// <param name="requirement">The requirement to collect tests from.</param>
    /// <param name="rootSection">The root section for looking up child requirements.</param>
    /// <param name="allTests">The set to add tests to.</param>
    private static void CollectAllTests(Requirement requirement, Section rootSection, HashSet<string> allTests)
    {
        // Add direct tests
        foreach (var test in requirement.Tests)
        {
            allTests.Add(test);
        }

        // Recursively add tests from children
        foreach (var childReq in requirement.Children.Select(childId => FindRequirement(rootSection, childId)).Where(childReq => childReq != null))
        {
            CollectAllTests(childReq!, rootSection, allTests);
        }
    }

    /// <summary>
    ///     Finds a requirement by ID in the section tree.
    /// </summary>
    /// <param name="section">The section to search.</param>
    /// <param name="requirementId">The requirement ID to find.</param>
    /// <returns>The requirement if found, null otherwise.</returns>
    private static Requirement? FindRequirement(Section section, string requirementId)
    {
        // Search in current section
        var requirement = section.Requirements.FirstOrDefault(req => req.Id == requirementId);
        if (requirement != null)
        {
            return requirement;
        }

        // Search in child sections
        return section.Sections
            .Select(childSection => FindRequirement(childSection, requirementId))
            .FirstOrDefault(found => found != null);
    }

    /// <summary>
    ///     Exports the requirements section with test statistics.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="depth">The current depth for Markdown headers.</param>
    private void ExportRequirements(TextWriter writer, int depth)
    {
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} Requirements");
        writer.WriteLine();

        // Export all sections
        foreach (var section in _requirements.Sections)
        {
            ExportRequirementSection(writer, section, depth + 1);
        }
    }

    /// <summary>
    ///     Exports a requirements section with test statistics.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="section">The section to export.</param>
    /// <param name="depth">The current depth for Markdown headers.</param>
    private void ExportRequirementSection(TextWriter writer, Section section, int depth)
    {
        // Write section header
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} {section.Title}");
        writer.WriteLine();

        // If there are requirements, write them as a table
        if (section.Requirements.Count > 0)
        {
            // Write table header
            writer.WriteLine("| ID | Tests Linked | Passed | Failed | Not Executed |");
            writer.WriteLine("| :- | -----------: | :-: | :-: | :-: |");

            // Write each requirement
            foreach (var requirement in section.Requirements)
            {
                var (testsLinked, passed, failed, notExecuted) = GetRequirementTestStats(requirement);
                writer.WriteLine($"| {requirement.Id} | {testsLinked} | {passed} | {failed} | {notExecuted} |");
            }

            writer.WriteLine();
        }

        // Recursively export child sections
        foreach (var childSection in section.Sections)
        {
            ExportRequirementSection(writer, childSection, depth + 1);
        }
    }

    /// <summary>
    ///     Gets test statistics for a requirement.
    /// </summary>
    /// <param name="requirement">The requirement to analyze.</param>
    /// <returns>A tuple of (tests linked, passed, failed, not executed).</returns>
    private (int testsLinked, int passed, int failed, int notExecuted) GetRequirementTestStats(Requirement requirement)
    {
        var testsLinked = requirement.Tests.Count;
        var passed = 0;
        var failed = 0;
        var notExecuted = 0;

        foreach (var result in requirement.Tests.Select(testName => GetTestResult(testName)))
        {
            if (result.Executed == 0)
            {
                notExecuted++;
            }
            else if (result.Fails > 0)
            {
                failed++;
            }
            else
            {
                passed++;
            }
        }

        return (testsLinked, passed, failed, notExecuted);
    }

    /// <summary>
    ///     Exports the testing section showing test-to-requirement mappings.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="depth">The current depth for Markdown headers.</param>
    private void ExportTesting(TextWriter writer, int depth)
    {
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} Testing");
        writer.WriteLine();

        // Build a mapping of test names to requirements
        var testToRequirements = new Dictionary<string, List<string>>();
        BuildTestToRequirementsMap(_requirements, testToRequirements);

        // Write table header
        writer.WriteLine("| Test | Requirement | Passed | Failed |");
        writer.WriteLine("|------|-------------|--------|--------|");

        // Write each test-to-requirement mapping
        foreach (var (testName, requirementIds) in testToRequirements.OrderBy(kvp => kvp.Key))
        {
            var result = GetTestResult(testName);
            var passed = result.Passes;
            var failed = result.Fails;

            foreach (var reqId in requirementIds.OrderBy(id => id))
            {
                writer.WriteLine($"| {testName} | {reqId} | {passed} | {failed} |");
            }
        }

        writer.WriteLine();
    }

    /// <summary>
    ///     Builds a mapping from test names to requirement IDs.
    /// </summary>
    /// <param name="section">The section to scan.</param>
    /// <param name="testToRequirements">The dictionary to populate.</param>
    private static void BuildTestToRequirementsMap(Section section, Dictionary<string, List<string>> testToRequirements)
    {
        // Process requirements in this section
        foreach (var requirement in section.Requirements)
        {
            foreach (var testName in requirement.Tests)
            {
                if (!testToRequirements.TryGetValue(testName, out var requirementIds))
                {
                    requirementIds = [];
                    testToRequirements[testName] = requirementIds;
                }
                requirementIds.Add(requirement.Id);
            }
        }

        // Recursively process child sections
        foreach (var childSection in section.Sections)
        {
            BuildTestToRequirementsMap(childSection, testToRequirements);
        }
    }

    /// <summary>
    ///     Processes a test result file and updates test execution counts.
    /// </summary>
    /// <param name="filePath">Path to the test result file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    private void ProcessTestResultFile(string filePath)
    {
        // Verify file exists
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Test result file not found: {filePath}", filePath);
        }

        // Extract the base filename (without extension) for source matching
        var fileBaseName = Path.GetFileNameWithoutExtension(filePath);

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

        // Aggregate test results by test name (collapse duplicate results from different classes)
        var testAggregates = new Dictionary<string, (int passes, int fails)>();
        foreach (var result in testResults.Results)
        {
            // Skip non-executed tests (e.g., filtered by OS/Runtime conditions)
            if (!result.Outcome.IsExecuted())
            {
                continue;
            }

            // Aggregate by test name
            if (!testAggregates.TryGetValue(result.Name, out var aggregate))
            {
                aggregate = (0, 0);
            }

            if (result.Outcome.IsPassed())
            {
                aggregate.passes++;
            }
            else
            {
                aggregate.fails++;
            }

            testAggregates[result.Name] = aggregate;
        }

        // Create TestExecution records and add to the dictionary
        foreach (var (testName, (passes, fails)) in testAggregates)
        {
            var execution = new TestExecution(fileBaseName, testName, new TestMetrics(passes, fails));

            // Add to the executions dictionary
            if (!_testExecutions.TryGetValue(testName, out var executions))
            {
                executions = [];
                _testExecutions[testName] = executions;
            }
            executions.Add(execution);
        }
    }

    /// <summary>
    ///     Parses a test name to extract the optional file part and the actual test name.
    ///     Format: [filepart@]testname
    /// </summary>
    /// <param name="testName">The test name from requirements.</param>
    /// <returns>A tuple of (filePart, testName). filePart is null if not specified.</returns>
    private static (string? filePart, string testName) ParseTestName(string testName)
    {
        var atIndex = testName.IndexOf('@');
        if (atIndex > 0 && atIndex < testName.Length - 1)
        {
            var filePart = testName[..atIndex];
            var actualTestName = testName[(atIndex + 1)..];
            return (filePart, actualTestName);
        }

        return (null, testName);
    }
}
