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

namespace DemaConsulting.ReqStream;

/// <summary>
/// Main program entry point for the ReqStream tool.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Gets the application version string.
    /// </summary>
    private static string Version
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
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (InvalidOperationException ex)
        {
            // Print expected operation exceptions and return error code
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        catch (Exception ex)
        {
            // Print unexpected exceptions and re-throw to generate event logs
            Console.WriteLine($"Unexpected error: {ex.Message}");
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
        PrintBanner();

        // Priority 2: Help
        if (context.Help)
        {
            PrintHelp();
            return;
        }

        // Priority 3: Self-Validation (placeholder for now)
        if (context.Validate)
        {
            context.WriteLine("Self-validation not yet implemented.");
            return;
        }

        // Priority 4: Requirements processing
        ProcessRequirements(context);
    }

    /// <summary>
    ///     Prints the application banner to the console.
    /// </summary>
    private static void PrintBanner()
    {
        Console.WriteLine($"ReqStream version {Version}");
        Console.WriteLine("Copyright (c) 2026 DEMA Consulting");
        Console.WriteLine();
    }

    /// <summary>
    ///     Prints usage information to the console.
    /// </summary>
    private static void PrintHelp()
    {
        Console.WriteLine("Usage: reqstream [options]");
        Console.WriteLine();
        Console.WriteLine("Options:");
        Console.WriteLine("  -v, --version              Display version information");
        Console.WriteLine("  -?, -h, --help             Display this help message");
        Console.WriteLine("  --silent                   Suppress console output");
        Console.WriteLine("  --validate                 Run self-validation");
        Console.WriteLine("  --log <file>               Write output to log file");
        Console.WriteLine("  --requirements <pattern>   Requirements files glob pattern");
        Console.WriteLine("  --report <file>            Export requirements to markdown file");
        Console.WriteLine("  --report-depth <depth>     Markdown header depth for requirements report (default: 1)");
        Console.WriteLine("  --tests <pattern>          Test result files glob pattern (TRX or JUnit)");
        Console.WriteLine("  --matrix <file>            Export trace matrix to markdown file");
        Console.WriteLine("  --matrix-depth <depth>     Markdown header depth for trace matrix (default: 1)");
        Console.WriteLine("  --enforce                  Fail if requirements are not fully tested");
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
        var requirements = Requirements.Read(context.RequirementsFiles.ToArray());
        context.WriteLine("Requirements loaded successfully.");

        // Export requirements report if requested
        if (context.RequirementsReport != null)
        {
            context.WriteLine($"Exporting requirements to {context.RequirementsReport}...");
            requirements.Export(context.RequirementsReport, context.ReportDepth);
            context.WriteLine("Requirements report generated successfully.");
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
                traceMatrix.Export(context.Matrix, context.MatrixDepth);
                context.WriteLine("Trace matrix report generated successfully.");
            }
        }

        // Enforce requirements coverage if requested
        if (context.Enforce)
        {
            if (traceMatrix != null)
            {
                var (satisfied, total) = traceMatrix.CalculateSatisfiedRequirements();
                if (satisfied < total)
                {
                    context.WriteError($"Error: Only {satisfied} of {total} requirements are satisfied with tests.");
                }
            }
            else
            {
                context.WriteError("Error: Cannot enforce requirements without test results. Use --tests to specify test result files.");
            }
        }
    }
}
