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
using DemaConsulting.TestResults;
using DemaConsulting.TestResults.IO;

namespace DemaConsulting.ReqStream.Tracing;

/// <summary>
///     Represents test metrics for a single test execution.
/// </summary>
/// <remarks>
///     <para>
///         <see cref="TestMetrics"/> is an immutable record because metrics are computed once
///         from a test result file and never modified afterwards; immutability prevents accidental
///         mutation during aggregation and makes the type safe to share across threads without
///         synchronization.
///     </para>
///     <para>
///         The default instance <c>TestMetrics(0, 0)</c> is the intentional safe-return value for
///         tests that have no recorded executions. Callers receive a valid, non-null object regardless
///         of whether the test name was found, eliminating null-checks at every call site.
///     </para>
/// </remarks>
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
    /// <exception cref="InvalidOperationException">Thrown when a test result file cannot be parsed
    /// (malformed TRX or JUnit XML). The message includes the file path; the inner exception contains
    /// the parse failure.</exception>
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
    /// <remarks>
    ///     <para>
    ///         When <paramref name="testName"/> contains a <c>'@'</c> separator (not at position 0 or
    ///         end), source-specific filtering is applied: the part before <c>'@'</c> is matched
    ///         case-insensitively using <see cref="string.Contains(string, StringComparison)"/> against
    ///         each <see cref="TestExecution.FileBaseName"/>. <c>Contains</c> is used rather than exact
    ///         equality so that a partial qualifier such as <c>ubuntu</c> matches a file named
    ///         <c>ubuntu-results</c> without requiring the caller to know the full file name.
    ///     </para>
    ///     <para>
    ///         When the test name is not found, the method returns <c>TestMetrics(0, 0)</c>. This
    ///         safe-return contract means callers never need to null-check the result; a 0/0 metric
    ///         correctly propagates as "not executed" through <see cref="TestMetrics.AllPassed"/> and
    ///         <see cref="IsRequirementSatisfied"/>.
    ///     </para>
    /// </remarks>
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
    /// <remarks>
    ///     <para>
    ///         The output is structured in three sections written in order by
    ///         <c>ExportSummary</c>, <c>ExportRequirements</c>, and <c>ExportTesting</c>.
    ///         The three-section structure separates the summary sentence (a quick pass/fail signal),
    ///         the per-requirement detail table (direct-test counts for auditors), and the per-test
    ///         detail table (traceability links for engineers) so that each audience can locate the
    ///         information they need without reading the entire document.
    ///     </para>
    ///     <para>
    ///         There is a deliberate asymmetry between the Summary and Requirements sections: the
    ///         Requirements table shows only <em>direct</em> tests listed on each requirement, while
    ///         the Summary satisfied-count is computed by <see cref="CalculateSatisfiedRequirements(HashSet{string})"/>
    ///         using <see cref="IsRequirementSatisfied"/>, which recurses through the entire descendant
    ///         subtree. This gives the Summary an accurate compliance verdict while keeping the
    ///         Requirements table rows concise.
    ///     </para>
    /// </remarks>
    /// <param name="filePath">The path to the output Markdown file.</param>
    /// <param name="depth">The starting depth for Markdown headers (default: 1).</param>
    /// <param name="filterTags">Optional set of tags to filter requirements. If provided, only requirements with matching tags are included.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    public void Export(string filePath, int depth = 1, HashSet<string>? filterTags = null)
    {
        // Validate file path
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        // Create a string builder to build the markdown content
        using var writer = new StringWriter();

        // Export Summary section
        ExportSummary(writer, depth, filterTags);

        // Export Requirements section
        ExportRequirements(writer, depth, filterTags);

        // Export Testing section
        ExportTesting(writer, depth, filterTags);

        // Write the content to the file
        File.WriteAllText(filePath, writer.ToString());
    }

    /// <summary>
    ///     Exports the summary section showing satisfied requirements count.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="depth">The current depth for Markdown headers.</param>
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    private void ExportSummary(TextWriter writer, int depth, HashSet<string>? filterTags)
    {
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} Summary");
        writer.WriteLine();

        // Calculate satisfied requirements
        var (satisfied, total) = CalculateSatisfiedRequirements(_requirements, filterTags);

        writer.WriteLine($"{satisfied} of {total} requirements are satisfied with tests.");
        writer.WriteLine();
    }

    /// <summary>
    ///     Calculates how many requirements are satisfied.
    ///     A requirement is satisfied if it has at least one test and all tests have passed.
    /// </summary>
    /// <param name="filterTags">Optional set of tags to filter requirements. If provided, only requirements with matching tags are counted.</param>
    /// <returns>A tuple of (satisfied count, total count).</returns>
    public (int satisfied, int total) CalculateSatisfiedRequirements(HashSet<string>? filterTags = null)
    {
        return CalculateSatisfiedRequirements(_requirements, filterTags);
    }

    /// <summary>
    ///     Gets a list of requirement IDs that are not satisfied.
    ///     A requirement is not satisfied if it has no tests or any of its tests have not been executed or have failed.
    /// </summary>
    /// <param name="filterTags">Optional set of tags to filter requirements. If provided, only requirements with matching tags are checked.</param>
    /// <returns>A list of unsatisfied requirement IDs.</returns>
    public List<string> GetUnsatisfiedRequirements(HashSet<string>? filterTags = null)
    {
        var unsatisfied = new List<string>();
        CollectUnsatisfiedRequirements(_requirements, unsatisfied, filterTags);
        return unsatisfied;
    }

    /// <summary>
    ///     Collects unsatisfied requirement IDs from a section and its subsections.
    /// </summary>
    /// <param name="section">The section to analyze.</param>
    /// <param name="unsatisfied">The list to add unsatisfied requirement IDs to.</param>
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    private void CollectUnsatisfiedRequirements(Section section, List<string> unsatisfied, HashSet<string>? filterTags)
    {
        // Filter requirements if tags are specified
        var requirementsToCheck = section.Requirements;
        if (filterTags != null && filterTags.Count > 0)
        {
            requirementsToCheck = section.Requirements
                .Where(req => req.Tags.Any(tag => filterTags.Contains(tag)))
                .ToList();
        }

        // Check requirements in this section using LINQ Where
        unsatisfied.AddRange(
            requirementsToCheck
                .Where(requirement => !IsRequirementSatisfied(requirement, _requirements))
                .Select(requirement => requirement.Id));

        // Recursively check child sections
        foreach (var childSection in section.Sections)
        {
            CollectUnsatisfiedRequirements(childSection, unsatisfied, filterTags);
        }
    }

    /// <summary>
    ///     Calculates how many requirements are satisfied.
    ///     A requirement is satisfied if it has at least one test and all tests have passed.
    /// </summary>
    /// <param name="section">The section to analyze.</param>
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    /// <returns>A tuple of (satisfied count, total count).</returns>
    private (int satisfied, int total) CalculateSatisfiedRequirements(Section section, HashSet<string>? filterTags)
    {
        var satisfied = 0;
        var total = 0;

        // Filter requirements if tags are specified
        var requirementsToCheck = section.Requirements;
        if (filterTags != null && filterTags.Count > 0)
        {
            requirementsToCheck = section.Requirements
                .Where(req => req.Tags.Any(tag => filterTags.Contains(tag)))
                .ToList();
        }

        // Check requirements in this section
        foreach (var requirement in requirementsToCheck)
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
            var (childSatisfied, childTotal) = CalculateSatisfiedRequirements(childSection, filterTags);
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
    /// <remarks>
    ///     A requirement with no tests returns <see langword="false"/>. This is a deliberate
    ///     design decision: every requirement must be traceable to at least one passing test,
    ///     so a requirement that has never been linked to a test is treated as not satisfied
    ///     rather than vacuously satisfied. This ensures that untested requirements are always
    ///     surfaced as coverage gaps during enforcement.
    /// </remarks>
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
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    private void ExportRequirements(TextWriter writer, int depth, HashSet<string>? filterTags)
    {
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} Requirements");
        writer.WriteLine();

        // Export all sections
        foreach (var section in _requirements.Sections)
        {
            ExportRequirementSection(writer, section, depth + 1, filterTags);
        }
    }

    /// <summary>
    ///     Exports a requirements section with test statistics.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="section">The section to export.</param>
    /// <param name="depth">The current depth for Markdown headers.</param>
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    private void ExportRequirementSection(TextWriter writer, Section section, int depth, HashSet<string>? filterTags)
    {
        // Filter requirements if tags are specified
        var requirementsToExport = section.Requirements;
        if (filterTags != null && filterTags.Count > 0)
        {
            requirementsToExport = section.Requirements
                .Where(req => req.Tags.Any(tag => filterTags.Contains(tag)))
                .ToList();
        }

        // Check if section has any content to export
        var hasContent = requirementsToExport.Count > 0;
        if (!hasContent)
        {
            // Check if any child sections have content
            hasContent = section.Sections.Any(childSection => SectionHasFilteredContent(childSection, filterTags));
        }

        // Skip section if no content
        if (!hasContent)
        {
            return;
        }

        // Write section header
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} {section.Title}");
        writer.WriteLine();

        // If there are requirements, write them as a table
        if (requirementsToExport.Count > 0)
        {
            // Write table header
            writer.WriteLine("| ID | Tests Linked | Passed | Failed | Not Executed |");
            writer.WriteLine("| :- | -----------: | :-: | :-: | :-: |");

            // Write each requirement
            foreach (var requirement in requirementsToExport)
            {
                var (testsLinked, passed, failed, notExecuted) = GetRequirementTestStats(requirement);
                writer.WriteLine($"| {requirement.Id} | {testsLinked} | {passed} | {failed} | {notExecuted} |");
            }

            writer.WriteLine();
        }

        // Recursively export child sections
        foreach (var childSection in section.Sections)
        {
            ExportRequirementSection(writer, childSection, depth + 1, filterTags);
        }
    }

    /// <summary>
    ///     Checks if a section has any filtered content (requirements or child sections with content).
    /// </summary>
    /// <param name="section">The section to check.</param>
    /// <param name="filterTags">The set of filter tags.</param>
    /// <returns>True if the section has filtered content, false otherwise.</returns>
    private static bool SectionHasFilteredContent(Section section, HashSet<string>? filterTags)
    {
        // Check if section has any matching requirements
        if (filterTags == null || filterTags.Count == 0)
        {
            if (section.Requirements.Count > 0)
            {
                return true;
            }
        }
        else
        {
            if (section.Requirements.Any(req => req.Tags.Any(tag => filterTags.Contains(tag))))
            {
                return true;
            }
        }

        // Check if any child section has filtered content
        return section.Sections.Any(childSection => SectionHasFilteredContent(childSection, filterTags));
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
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    private void ExportTesting(TextWriter writer, int depth, HashSet<string>? filterTags)
    {
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} Testing");
        writer.WriteLine();

        // Build a mapping of test names to requirements
        var testToRequirements = new Dictionary<string, List<string>>();
        BuildTestToRequirementsMap(_requirements, testToRequirements, filterTags);

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
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    private static void BuildTestToRequirementsMap(Section section, Dictionary<string, List<string>> testToRequirements, HashSet<string>? filterTags)
    {
        // Filter requirements if tags are specified
        var requirementsToProcess = section.Requirements;
        if (filterTags != null && filterTags.Count > 0)
        {
            requirementsToProcess = section.Requirements
                .Where(req => req.Tags.Any(tag => filterTags.Contains(tag)))
                .ToList();
        }

        // Process requirements in this section
        foreach (var requirement in requirementsToProcess)
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
            BuildTestToRequirementsMap(childSection, testToRequirements, filterTags);
        }
    }

    /// <summary>
    ///     Processes a test result file and updates test execution counts.
    /// </summary>
    /// <remarks>
    ///     When the underlying <see cref="DemaConsulting.TestResults.IO.Serializer.Deserialize"/> call
    ///     throws, the exception is caught and re-thrown as an
    ///     <see cref="InvalidOperationException"/> that includes <paramref name="filePath"/> in its
    ///     message. This wrapping ensures that the caller (the constructor) can identify the offending
    ///     file by message text alone, without needing to inspect nested exception detail. The original
    ///     parse exception is preserved as the inner exception for diagnostics.
    /// </remarks>
    /// <param name="filePath">Path to the test result file.</param>
    /// <exception cref="FileNotFoundException">Thrown when the file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the file cannot be parsed (malformed
    /// TRX or JUnit XML). The message includes the file path; the inner exception contains the parse
    /// failure.</exception>
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

        // Deserialize test results (automatically detects TRX or JUnit format)
        DemaConsulting.TestResults.TestResults testResults;
        try
        {
            testResults = Serializer.Deserialize(content);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to parse test result file: {filePath}", ex);
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
