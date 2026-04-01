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

namespace DemaConsulting.ReqStream.Tests.Modeling;

/// <summary>
/// Integration tests for the Modeling subsystem, testing requirements loading and export
/// through the full tool executable.
/// </summary>
[TestClass]
public class ModelingIntegrationTests
{
    private string _dllPath = string.Empty;
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by locating the DLL and creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _dllPath = Path.Combine(AppContext.BaseDirectory, "DemaConsulting.ReqStream.dll");
        Assert.IsTrue(File.Exists(_dllPath), $"Could not find ReqStream DLL at {_dllPath}");

        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_modeling_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Clean up test by deleting the temporary test directory.
    /// </summary>
    [TestCleanup]
    public void TestCleanup()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, recursive: true);
        }
    }

    /// <summary>
    /// Integration test verifying that a requirements report Markdown file is generated correctly.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_RequirementsReport_GeneratesMarkdown()
    {
        // Arrange: create a requirements file with one testable requirement
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Modeling Test Requirements
                requirements:
                  - id: Modeling-Test-Req1
                    title: The system shall have a testable requirement.
                    justification: Test justification.
                    tests:
                      - SomeTest
            """);

        var reportFile = Path.Combine(_testDirectory, "requirements.md");

        // Act: invoke the tool to generate a requirements report
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--report", reportFile);

        // Assert: exit code is 0, report file exists, and contains the requirement ID and title
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
        Assert.IsTrue(File.Exists(reportFile), "Requirements report should be generated.");

        var content = File.ReadAllText(reportFile);
        Assert.Contains("Modeling-Test-Req1", content);
        Assert.Contains("The system shall have a testable requirement.", content);
    }

    /// <summary>
    /// Integration test verifying that a justifications report Markdown file is generated correctly.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_JustificationsReport_GeneratesMarkdown()
    {
        // Arrange: create a requirements file with one justified requirement
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Modeling Test Requirements
                requirements:
                  - id: Modeling-Test-Req2
                    title: The system shall have a justified requirement.
                    justification: This justification explains why the requirement is needed.
                    tests:
                      - SomeTest
            """);

        var justificationsFile = Path.Combine(_testDirectory, "justifications.md");

        // Act: invoke the tool to generate a justifications report
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--justifications", justificationsFile);

        // Assert: exit code is 0, report file exists, and contains the requirement ID and justification text
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 but got {exitCode}. Output: {output}");
        Assert.IsTrue(File.Exists(justificationsFile), "Justifications report should be generated.");

        var content = File.ReadAllText(justificationsFile);
        Assert.Contains("Modeling-Test-Req2", content);
        Assert.Contains("This justification explains why the requirement is needed.", content);
    }

    /// <summary>
    /// Integration test verifying that linting a valid requirements file reports no issues
    /// and exits with code 0.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_LintFlag_ValidFile_ReturnsSuccess()
    {
        // Arrange: create a structurally valid requirements YAML file with no duplicate IDs
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Lint Test Requirements
                requirements:
                  - id: Lint-Test-Req1
                    title: The system shall have a valid requirement.
                    justification: Lint test justification.
                    tests:
                      - LintTest1
            """);

        // Act: invoke the tool with --lint flag on the valid requirements file
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--lint");

        // Assert: exit code is 0 indicating no linting issues were found
        Assert.AreEqual(0, exitCode, $"Expected exit code 0 for valid file but got {exitCode}. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that linting a requirements file with duplicate IDs
    /// reports an error and exits with a non-zero code.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_LintFlag_InvalidFile_ReturnsError()
    {
        // Arrange: create a requirements YAML file containing duplicate requirement IDs
        var reqFile = Path.Combine(_testDirectory, "invalid.yaml");
        File.WriteAllText(reqFile, """
            sections:
              - title: Duplicate ID Test
                requirements:
                  - id: Lint-Duplicate-Req
                    title: The first requirement.
                    justification: First.
                    tests:
                      - Test1
                  - id: Lint-Duplicate-Req
                    title: The second requirement with duplicate ID.
                    justification: Second.
                    tests:
                      - Test2
            """);

        // Act: invoke the tool with --lint flag on the invalid requirements file
        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "invalid.yaml",
            "--lint");

        // Assert: exit code is non-zero indicating a linting error was detected
        Assert.AreNotEqual(0, exitCode, $"Expected non-zero exit code for invalid file but got 0. Output: {output}");
    }
}
