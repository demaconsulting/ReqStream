// Copyright (c) 2025 DEMA Consulting
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
///     Entry point and top-level dispatch controller for the ReqStream tool.
///     Separated into <see cref="Run"/> so that tests can construct a <see cref="Cli.Context"/>
///     and invoke the program logic directly without spawning a new process.
/// </summary>
internal static class Program
{
    /// <summary>
    ///     Cached application version, resolved once at class initialization to avoid
    ///     repeated reflection on every <see cref="Version"/> access.
    /// </summary>
    private static readonly string _version =
        typeof(Program).Assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion
        ?? typeof(Program).Assembly.GetName().Version?.ToString()
        ?? "Unknown";

    /// <summary>
    ///     Gets the application version string. Backed by the <see cref="_version"/> field to
    ///     avoid repeated reflection calls. Prefers the informational version (which carries
    ///     pre-release labels and build metadata) over the numeric assembly version, and falls
    ///     back to <c>"Unknown"</c> so the property never returns <c>null</c>.
    /// </summary>
    public static string Version => _version;

    /// <summary>
    ///     Process entry point. Responsible solely for creating the <see cref="Cli.Context"/>,
    ///     delegating all program logic to <see cref="Run"/>, and mapping exceptions to exit
    ///     codes so that callers receive a well-defined exit code regardless of failure mode.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    /// <returns>Exit code.</returns>
    /// <exception cref="Exception">Thrown when an unexpected error occurs that is not an <see cref="ArgumentException"/> or <see cref="InvalidOperationException"/>.</exception>
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
    ///     Implements the priority-ordered dispatch for all program modes. Separated from
    ///     <see cref="Main"/> so that tests can supply a pre-constructed <see cref="Cli.Context"/>
    ///     and exercise the full dispatch path without spawning a child process.
    /// </summary>
    /// <remarks>
    ///     This method writes output via <see cref="Cli.Context.WriteLine"/> and
    ///     <see cref="Cli.Context.WriteError"/>. A non-zero <see cref="Cli.Context.ExitCode"/>
    ///     after this method returns signals that an error was reported during execution.
    /// </remarks>
    /// <param name="context">The context containing command line arguments and program state.</param>
    /// <exception cref="System.IO.IOException">Propagates from <see cref="Modeling.Requirements.Load"/> or file export methods if file I/O fails.</exception>
    /// <exception cref="System.Xml.XmlException">Propagates from <see cref="Modeling.Requirements.Load"/> if a requirements file contains malformed XML.</exception>
    /// <exception cref="System.Exception">Any exception not caught internally by <see cref="Modeling.Requirements.Load"/>, <see cref="SelfTest.Validation.Run"/>, <see cref="Tracing.TraceMatrix"/> construction, or file export methods will propagate to <see cref="Main"/> where it is handled by the unexpected-exception catch block.</exception>
    public static void Run(Context context)
    {
        // Priority 1: Version query
        if (context.Version)
        {
            context.WriteLine(Version);
            return;
        }

        // Priority 2: Print application banner (suppressed during lint for cleaner script integration)
        if (!context.Lint)
        {
            PrintBanner(context);
        }

        // Priority 3: Help
        if (context.Help)
        {
            PrintHelp(context);
            return;
        }

        // Priority 4: Self-Validation
        if (context.Validate)
        {
            Validation.Run(context);
            return;
        }

        // Priority 5: Lint requirements files - early exit when no files specified
        if (context.Lint && context.RequirementsFiles.Count == 0)
        {
            context.WriteLine("No requirements files specified.");
            return;
        }

        // Priority 6: Lint requirements files - load and report issues
        if (context.Lint)
        {
            var result = Requirements.Load(context.RequirementsFiles.ToArray());
            result.ReportIssues(context);
            return;
        }

        // Priority 7: Requirements processing
        ProcessRequirements(context);
    }

    /// <summary>
    ///     Writes the tool identity banner to support compliance audits: every non-trivial
    ///     invocation records which version was used, satisfying traceability requirements.
    ///     The banner is suppressed during lint runs so that only actionable issue lines appear
    ///     in lint output.
    /// </summary>
    /// <param name="context">The context for output.</param>
    private static void PrintBanner(Context context)
    {
        context.WriteLine($"ReqStream version {Version}");
        context.WriteLine("Copyright (c) 2025 DEMA Consulting");
        context.WriteLine("");
    }

    /// <summary>
    ///     Writes the full option listing so users can discover all supported flags without
    ///     consulting external documentation. Invoked only at dispatch priority 3,
    ///     when <c>--help</c> is present.
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
        context.WriteLine("  --results <file>           Write validation results to file (.trx or .xml extension required)");
        context.WriteLine("  --lint                     Lint requirements files for issues");
        context.WriteLine("  --log <file>               Write output to log file");
        context.WriteLine("  --depth <depth>            Default markdown header depth for all reports (default: 1)");
        context.WriteLine("  --requirements <pattern>   Requirements files glob pattern");
        context.WriteLine("  --report <file>            Export requirements to markdown file");
        context.WriteLine("  --report-depth <depth>     Markdown header depth for requirements report (overrides --depth)");
        context.WriteLine("  --filter <tags>            Filter requirements by comma-separated tags");
        context.WriteLine("  --root-tags <tags>         Comma-separated tags marking root requirements for orphan detection");
        context.WriteLine("  --justifications <file>    Export justifications to markdown file");
        context.WriteLine("  --justifications-depth <depth>");
        context.WriteLine("                             Markdown header depth for justifications (overrides --depth)");
        context.WriteLine("  --tests <pattern>          Test result files glob pattern (TRX or JUnit)");
        context.WriteLine("  --matrix <file>            Export trace matrix to markdown file");
        context.WriteLine("  --matrix-depth <depth>     Markdown header depth for trace matrix (overrides --depth)");
        context.WriteLine("  --enforce                  Fail if requirements are not fully tested or are orphaned (when root tags are configured)");
    }

    /// <summary>
    ///     Orchestrates the normal (non-version, non-help, non-validate, non-lint) run, acting
    ///     as the requirements-processing stage of the dispatch chain. Delegates all domain work
    ///     to <see cref="Modeling.Requirements"/>, <see cref="Tracing.TraceMatrix"/>, and
    ///     <see cref="EnforceRequirementsCoverage"/> so that <c>Program</c> itself contains no
    ///     domain logic.
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

        // Compute the effective (merged) root-tag set: YAML-declared root-tags (combined across
        // every loaded/included file) union any CLI --root-tags flag(s). Orphan-checking runs
        // against the full, unfiltered requirement graph — independent of --filter, which only
        // narrows report/matrix output below.
        var mergedRootTags = new HashSet<string>(requirements.RootTags, StringComparer.Ordinal);
        if (context.RootTags != null)
        {
            mergedRootTags.UnionWith(context.RootTags);
        }

        var orphanResult = requirements.FindOrphans(mergedRootTags);

        // Warn (non-fatal) about orphaned requirements when enforcement is not active; when
        // enforcement is active, orphan-freedom is instead enforced as an error below.
        if (orphanResult.OrphanIds.Count > 0 && !context.Enforce)
        {
            ReportOrphans(orphanResult, mergedRootTags, context.WriteWarning, "Warning");
        }

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

        // Report error if matrix was requested but no test files were provided
        if (context.Matrix != null && traceMatrix == null)
        {
            context.WriteError("Error: No test result files were provided or matched. Ensure the --tests pattern matches at least one file.");
        }

        // Enforce requirements coverage if requested. This must run even when the --matrix
        // guard above reported an error, because orphan-freedom enforcement is independent of
        // the trace matrix (see EnforceRequirementsCoverage remarks) and must still be evaluated
        // when root tags are configured, regardless of the --matrix outcome.
        if (context.Enforce)
        {
            EnforceRequirementsCoverage(context, traceMatrix, orphanResult, mergedRootTags);
        }
    }

    /// <summary>
    ///     Formats and writes the orphan summary line and per-requirement listing, shared by
    ///     the non-fatal warning path and the <c>--enforce</c> error path so both stay in sync.
    /// </summary>
    /// <remarks>
    ///     Extracted so the warning (<see cref="ProcessRequirements"/>) and error
    ///     (<see cref="EnforceRequirementsCoverage"/>) call sites produce byte-for-byte
    ///     identical formatting aside from severity label and write method.
    /// </remarks>
    /// <param name="orphanResult">The orphan-detection result to report.</param>
    /// <param name="rootTags">The effective root-tag set used for the scan.</param>
    /// <param name="write">The write method to use (<see cref="Cli.Context.WriteWarning"/> or <see cref="Cli.Context.WriteError"/>).</param>
    /// <param name="severity">The severity label to prefix the summary line with ("Warning" or "Error").</param>
    private static void ReportOrphans(
        OrphanResult orphanResult,
        HashSet<string> rootTags,
        Action<string> write,
        string severity)
    {
        var tagList = string.Join(", ", rootTags.OrderBy(tag => tag, StringComparer.Ordinal));
        write($"{severity}: {orphanResult.OrphanIds.Count} of {orphanResult.TotalRequirements} requirements are orphaned " +
              $"(not reachable from any requirement tagged: {tagList}).");
        foreach (var orphanId in orphanResult.OrphanIds)
        {
            write($"  - {orphanId}");
        }
    }

    /// <summary>
    ///     Enforces the compliance contract that every requirement must be backed by at least one
    ///     passing test, and (independently) that no requirement is orphaned when root tags are
    ///     configured. Separated from <see cref="ProcessRequirements"/> to keep enforcement
    ///     logic isolated and to allow all reports to be generated before a coverage failure
    ///     is signalled.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="traceMatrix">The trace matrix containing test results, or null if no tests were provided.</param>
    /// <param name="orphanResult">The orphan-detection result computed against the full requirement graph.</param>
    /// <param name="rootTags">The effective root-tag set used to compute <paramref name="orphanResult"/>.</param>
    /// <remarks>
    ///     Test-coverage enforcement and orphan-freedom enforcement are independent of one
    ///     another: either, both, or neither may apply on a given invocation. The
    ///     "nothing to enforce" error is only reported when neither a trace matrix exists
    ///     (no <c>--tests</c> supplied) nor root tags are configured (no <c>root-tags:</c>/
    ///     <c>--root-tags</c>) — i.e. there is genuinely nothing for <c>--enforce</c> to check.
    /// </remarks>
    private static void EnforceRequirementsCoverage(
        Context context,
        TraceMatrix? traceMatrix,
        OrphanResult orphanResult,
        HashSet<string> rootTags)
    {
        // Phase 0: Guard - enforcement requires either a trace matrix or configured root tags
        // (report error only when neither check has anything to enforce)
        if (traceMatrix == null && rootTags.Count == 0)
        {
            context.WriteError("Error: Cannot enforce requirements without test results or root tags. Use --tests to specify test result files or --root-tags/root-tags: to configure orphan checking.");
            return;
        }

        // Phase 1: Test coverage enforcement (unchanged), only when a trace matrix exists
        if (traceMatrix != null)
        {
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

        // Phase 2: Orphan-freedom enforcement (new), independent of test coverage enforcement
        if (rootTags.Count > 0 && orphanResult.OrphanIds.Count > 0)
        {
            ReportOrphans(orphanResult, rootTags, context.WriteError, "Error");
        }
    }
}
