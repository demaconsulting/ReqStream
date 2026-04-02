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

using System.Reflection;
using DemaConsulting.ReqStream.Cli;
using DemaConsulting.ReqStream.Modeling;
using DemaConsulting.ReqStream.SelfTest;
using DemaConsulting.ReqStream.Tracing;

namespace DemaConsulting.ReqStream;

/// <summary>
/// Main program entry point for the ReqStream tool.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Gets the application version string.
    /// </summary>
    public static string Version
    {
        get
        {
            var assembly = typeof(Program).Assembly;
            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                   ?? assembly.GetName().Version?.ToString()
                   ?? "Unknown";
        }
    }

    /// <summary>
    ///     Main entry point for the application.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Exit code.</returns>
    private static int Main(string[] args)
    {
        try
        {
            // Create context from arguments
            using var context = Context.Create(args);

            // Run the program logic
            Run(context);

            // Return the exit code from the context
            return context.ExitCode;
        }
        catch (ArgumentException ex)
        {
            // Print expected argument exceptions and return error code
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            // Print expected operation exceptions and return error code
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            // Print unexpected exceptions and re-throw to generate event logs
            Console.Error.WriteLine($"Unexpected error: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    ///     Runs the program logic based on the provided context.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    public static void Run(Context context)
    {
        // Priority 1: Version query
        if (context.Version)
        {
            Console.WriteLine(Version);
            return;
        }

        // Print application banner
        PrintBanner(context);

        // Priority 2: Help
        if (context.Help)
        {
            PrintHelp(context);
            return;
        }

        // Priority 3: Self-Validation
        if (context.Validate)
        {
            Validation.Run(context);
            return;
        }

        // Priority 4: Lint requirements files
        if (context.Lint)
        {
            if (context.RequirementsFiles.Count == 0)
            {
                context.WriteLine("No requirements files specified.");
                return;
            }

            var result = Requirements.Load(context.RequirementsFiles.ToArray());
            result.ReportIssues(context);

            if (result.Issues.Count == 0)
            {
                context.WriteLine("No issues found");
            }

            return;
        }

        // Priority 5: Requirements processing
        ProcessRequirements(context);
    }

    /// <summary>
    ///     Prints the application banner.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintBanner(Context context)
    {
        context.WriteLine($"ReqStream version {Version}");
        context.WriteLine("Copyright (c) 2026 DEMA Consulting");
        context.WriteLine("");
    }

    /// <summary>
    ///     Prints usage information.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintHelp(Context context)
    {
        context.WriteLine("Usage: reqstream [options]");
        context.WriteLine("");
        context.WriteLine("Options:");
        context.WriteLine("  -v, --version              Display version information");
        context.WriteLine("  -?, -h, --help             Display this help message");
        context.WriteLine("  --silent                   Suppress console output");
        context.WriteLine("  --validate                 Run self-validation");
        context.WriteLine("  --results <file>           Write validation results to file (TRX or JUnit format)");
        context.WriteLine("  --lint                     Lint requirements files for issues");
        context.WriteLine("  --log <file>               Write output to log file");
        context.WriteLine("  --requirements <pattern>   Requirements files glob pattern");
        context.WriteLine("  --report <file>            Export requirements to markdown file");
        context.WriteLine("  --report-depth <depth>     Markdown header depth for requirements report (default: 1)");
        context.WriteLine("  --filter <tags>            Filter requirements by comma-separated tags");
        context.WriteLine("  --justifications <file>    Export justifications to markdown file");
        context.WriteLine("  --justifications-depth <depth>");
        context.WriteLine("                             Markdown header depth for justifications (default: 1)");
        context.WriteLine("  --tests <pattern>          Test result files glob pattern (TRX or JUnit)");
        context.WriteLine("  --matrix <file>            Export trace matrix to markdown file");
        context.WriteLine("  --matrix-depth <depth>     Markdown header depth for trace matrix (default: 1)");
        context.WriteLine("  --enforce                  Fail if requirements are not fully tested");
    }

    /// <summary>
    ///     Processes requirements files and generates reports as requested.
    /// </summary>
    /// <param name="context">The context containing command line arguments and program state.</param>
    private static void ProcessRequirements(Context context)
    {
        // Check if we have requirements files to process
        if (context.RequirementsFiles.Count == 0)
        {
            context.WriteLine("No requirements files specified.");
            return;
        }

        // Read requirements from files
        context.WriteLine($"Reading {context.RequirementsFiles.Count} requirements file(s)...");
        var result = Requirements.Load(context.RequirementsFiles.ToArray());

        // Report any lint issues found during loading
        result.ReportIssues(context);

        // Abort if loading failed due to lint errors
        if (result.Requirements == null)
        {
            return;
        }

        var requirements = result.Requirements;

        context.WriteLine("Requirements loaded successfully.");

        // Export requirements report if requested
        if (context.RequirementsReport != null)
        {
            context.WriteLine($"Exporting requirements to {context.RequirementsReport}...");
            requirements.Export(context.RequirementsReport, context.ReportDepth, context.FilterTags);
            context.WriteLine("Requirements report generated successfully.");
        }

        // Export justifications if requested
        if (context.JustificationsFile != null)
        {
            context.WriteLine($"Exporting justifications to {context.JustificationsFile}...");
            requirements.ExportJustifications(context.JustificationsFile, context.JustificationsDepth, context.FilterTags);
            context.WriteLine("Justifications report generated successfully.");
        }

        // Create trace matrix if test files are specified
        TraceMatrix? traceMatrix = null;
        if (context.TestFiles.Count > 0)
        {
            context.WriteLine($"Processing {context.TestFiles.Count} test result file(s)...");
            traceMatrix = new TraceMatrix(requirements, context.TestFiles.ToArray());
            context.WriteLine("Trace matrix created successfully.");

            // Export trace matrix if requested
            if (context.Matrix != null)
            {
                context.WriteLine($"Exporting trace matrix to {context.Matrix}...");
                traceMatrix.Export(context.Matrix, context.MatrixDepth, context.FilterTags);
                context.WriteLine("Trace matrix report generated successfully.");
            }
        }

        // Enforce requirements coverage if requested
        if (context.Enforce)
        {
            EnforceRequirementsCoverage(context, traceMatrix);
        }
    }

    /// <summary>
    ///     Enforces that all requirements are satisfied with passing tests.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="traceMatrix">The trace matrix containing test results, or null if no tests were provided.</param>
    private static void EnforceRequirementsCoverage(Context context, TraceMatrix? traceMatrix)
    {
        if (traceMatrix == null)
        {
            context.WriteError("Error: Cannot enforce requirements without test results. Use --tests to specify test result files.");
            return;
        }

        var (satisfied, total) = traceMatrix.CalculateSatisfiedRequirements(context.FilterTags);
        if (satisfied < total)
        {
            var unsatisfied = traceMatrix.GetUnsatisfiedRequirements(context.FilterTags);
            context.WriteError($"Error: Only {satisfied} of {total} requirements are satisfied with tests.");
            context.WriteError("Unsatisfied requirements:");
            foreach (var reqId in unsatisfied)
            {
                context.WriteError($"  - {reqId}");
            }
        }
    }
}
