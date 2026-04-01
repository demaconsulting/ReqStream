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

namespace DemaConsulting.ReqStream.Tests.Linting;

/// <summary>
/// Integration tests for the Linting subsystem, testing requirement YAML structural
/// validation through the full tool executable.
/// </summary>
[TestClass]
public class LintingIntegrationTests
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

        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_linting_{Guid.NewGuid()}");
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
    /// Integration test verifying that linting a valid requirements file reports no issues
    /// and exits with code 0.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_LintFlag_ValidFile_ReturnsSuccess()
    {
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

        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "requirements.yaml",
            "--lint");

        Assert.AreEqual(0, exitCode, $"Expected exit code 0 for valid file but got {exitCode}. Output: {output}");
    }

    /// <summary>
    /// Integration test verifying that linting a requirements file with duplicate IDs
    /// reports an error and exits with a non-zero code.
    /// </summary>
    [TestMethod]
    public void IntegrationTest_LintFlag_InvalidFile_ReturnsError()
    {
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

        var exitCode = Runner.RunInDirectory(
            out var output,
            _testDirectory,
            "dotnet",
            _dllPath,
            "--requirements", "invalid.yaml",
            "--lint");

        Assert.AreNotEqual(0, exitCode, $"Expected non-zero exit code for invalid file but got 0. Output: {output}");
    }
}
