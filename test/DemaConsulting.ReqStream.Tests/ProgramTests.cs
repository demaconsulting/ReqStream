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

namespace DemaConsulting.ReqStream.Tests;

/// <summary>
/// Unit tests for the Program class Run method.
/// </summary>
[TestClass]
public class ProgramTests
{
    private string _testDirectory = string.Empty;

    /// <summary>
    /// Initialize test by creating a temporary test directory.
    /// </summary>
    [TestInitialize]
    public void TestInitialize()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"reqstream_test_{Guid.NewGuid()}");
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
    /// Test Run with version flag prints version information.
    /// </summary>
    [TestMethod]
    public void Run_WithVersionFlag_PrintsVersion()
    {
        var originalOut = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create(["--version"]);
            Program.Run(context);

            var outputText = output.ToString().Trim();
            // Version should be printed alone without any other text
            Assert.IsFalse(string.IsNullOrWhiteSpace(outputText));
            Assert.DoesNotContain("Copyright", outputText);
            Assert.DoesNotContain("Usage", outputText);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test Run with help flag prints help information.
    /// </summary>
    [TestMethod]
    public void Run_WithHelpFlag_PrintsHelp()
    {
        var originalOut = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create(["--help"]);
            Program.Run(context);

            var outputText = output.ToString();
            Assert.Contains("ReqStream version", outputText);
            Assert.Contains("Copyright", outputText);
            Assert.Contains("Usage:", outputText);
            Assert.Contains("Options:", outputText);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test Run with validate flag shows placeholder message.
    /// </summary>
    [TestMethod]
    public void Run_WithValidateFlag_ShowsPlaceholder()
    {
        using var context = Context.Create(["--validate"]);
        Program.Run(context);

        // Validate flag should not cause errors, just placeholder message
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test Run with no requirements files shows message.
    /// </summary>
    [TestMethod]
    public void Run_WithNoRequirementsFiles_ShowsMessage()
    {
        using var context = Context.Create([]);
        Program.Run(context);

        // Should complete without errors
        Assert.AreEqual(0, context.ExitCode);
    }

    /// <summary>
    /// Test Run with requirements files processes them successfully.
    /// </summary>
    [TestMethod]
    public void Run_WithRequirementsFiles_ProcessesSuccessfully()
    {
        // Create a test requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create(["--requirements", "*.yaml"]);
            Program.Run(context);

            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with requirements export generates report file.
    /// </summary>
    [TestMethod]
    public void Run_WithRequirementsExport_GeneratesReport()
    {
        // Create a test requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        var reportFile = Path.Combine(_testDirectory, "report.md");

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create(["--requirements", "*.yaml", "--report", reportFile]);
            Program.Run(context);

            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(reportFile));

            var reportContent = File.ReadAllText(reportFile);
            Assert.Contains("Test Section", reportContent);
            Assert.Contains("REQ-001", reportContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test Run with trace matrix export generates matrix file.
    /// </summary>
    [TestMethod]
    public void Run_WithTraceMatrixExport_GeneratesMatrix()
    {
        // Create a test requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        tests:
          - TestMethod1
");

        // Create a test TRX file
        var trxFile = Path.Combine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, @"<?xml version=""1.0"" encoding=""utf-8""?>
<TestRun xmlns=""http://microsoft.com/schemas/VisualStudio/TeamTest/2010"">
  <Results>
    <UnitTestResult testName=""TestMethod1"" outcome=""Passed"" />
  </Results>
</TestRun>");

        var matrixFile = Path.Combine(_testDirectory, "matrix.md");

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--tests", "*.trx",
                "--matrix", matrixFile
            ]);
            Program.Run(context);

            Assert.AreEqual(0, context.ExitCode);
            Assert.IsTrue(File.Exists(matrixFile));

            var matrixContent = File.ReadAllText(matrixFile);
            Assert.Contains("Summary", matrixContent);
            Assert.Contains("REQ-001", matrixContent);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test priority order: version takes precedence over help.
    /// </summary>
    [TestMethod]
    public void Run_WithVersionAndHelp_ProcessesVersionFirst()
    {
        var originalOut = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create(["--version", "--help"]);
            Program.Run(context);

            var outputText = output.ToString().Trim();
            // Version should be printed alone
            Assert.IsFalse(string.IsNullOrWhiteSpace(outputText));
            Assert.DoesNotContain("Usage:", outputText);
            Assert.DoesNotContain("Copyright", outputText);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test priority order: help takes precedence over validate.
    /// </summary>
    [TestMethod]
    public void Run_WithHelpAndValidate_ProcessesHelpFirst()
    {
        var originalOut = Console.Out;
        var output = new StringWriter();
        Console.SetOut(output);

        try
        {
            using var context = Context.Create(["--help", "--validate"]);
            Program.Run(context);

            var outputText = output.ToString();
            Assert.Contains("Usage:", outputText);
            Assert.DoesNotContain("Self-validation", outputText);
        }
        finally
        {
            Console.SetOut(originalOut);
        }
    }

    /// <summary>
    /// Test enforcement with fully satisfied requirements succeeds.
    /// </summary>
    [TestMethod]
    public void Run_WithEnforcementAndFullySatisfiedRequirements_Succeeds()
    {
        // Create a test requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        tests:
          - TestMethod1
");

        // Create a test TRX file with passing test using TestResults library
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TestMethod1",
            ClassName = "TestClass",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxFile = Path.Combine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--tests", "*.trx",
                "--enforce"
            ]);
            Program.Run(context);

            Assert.AreEqual(0, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test enforcement with unsatisfied requirements fails.
    /// </summary>
    [TestMethod]
    public void Run_WithEnforcementAndUnsatisfiedRequirements_Fails()
    {
        // Create a test requirements file with one tested and one untested requirement
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Tested Requirement
        tests:
          - TestMethod1
      - id: REQ-002
        title: Untested Requirement
");

        // Create a test TRX file with passing test using TestResults library
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TestMethod1",
            ClassName = "TestClass",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Passed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxFile = Path.Combine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--tests", "*.trx",
                "--enforce"
            ]);
            Program.Run(context);

            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test enforcement without test files fails.
    /// </summary>
    [TestMethod]
    public void Run_WithEnforcementAndNoTests_Fails()
    {
        // Create a test requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
");

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--enforce"
            ]);
            Program.Run(context);

            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }

    /// <summary>
    /// Test enforcement with failed tests fails.
    /// </summary>
    [TestMethod]
    public void Run_WithEnforcementAndFailedTests_Fails()
    {
        // Create a test requirements file
        var reqFile = Path.Combine(_testDirectory, "requirements.yaml");
        File.WriteAllText(reqFile, @"
sections:
  - title: Test Section
    requirements:
      - id: REQ-001
        title: Test Requirement
        tests:
          - TestMethod1
");

        // Create a test TRX file with failing test using TestResults library
        var testResults = new DemaConsulting.TestResults.TestResults { Name = "TestRun" };
        testResults.Results.Add(new DemaConsulting.TestResults.TestResult
        {
            Name = "TestMethod1",
            ClassName = "TestClass",
            CodeBase = "Tests.dll",
            Outcome = DemaConsulting.TestResults.TestOutcome.Failed,
            Duration = TimeSpan.FromSeconds(1)
        });

        var trxFile = Path.Combine(_testDirectory, "tests.trx");
        File.WriteAllText(trxFile, DemaConsulting.TestResults.IO.TrxSerializer.Serialize(testResults));

        // Save current directory and change to test directory
        var originalDir = Directory.GetCurrentDirectory();
        try
        {
            Directory.SetCurrentDirectory(_testDirectory);

            using var context = Context.Create([
                "--requirements", "*.yaml",
                "--tests", "*.trx",
                "--enforce"
            ]);
            Program.Run(context);

            Assert.AreEqual(1, context.ExitCode);
        }
        finally
        {
            Directory.SetCurrentDirectory(originalDir);
        }
    }
}
