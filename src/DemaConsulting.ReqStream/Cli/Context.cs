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

using DemaConsulting.ReqStream.Utilities;

namespace DemaConsulting.ReqStream.Cli;

/// <summary>
///     Context class that handles command-line arguments and program output.
/// </summary>
public sealed class Context : IDisposable
{
    /// <summary>
    ///     Log file stream writer (if logging is enabled).
    /// </summary>
    private StreamWriter? _logWriter;

    /// <summary>
    ///     Indicates whether errors have been reported.
    /// </summary>
    private bool _hasErrors;

    /// <summary>
    ///     Gets a value indicating whether the version flag was specified.
    /// </summary>
    public bool Version { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether the help flag was specified.
    /// </summary>
    public bool Help { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether the silent flag was specified.
    /// </summary>
    public bool Silent { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether the validate flag was specified.
    /// </summary>
    public bool Validate { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether the lint flag was specified.
    /// </summary>
    public bool Lint { get; private init; }

    /// <summary>
    ///     Gets the validation results output file path.
    /// </summary>
    public string? ResultsFile { get; private init; }

    /// <summary>
    ///     Gets a value indicating whether the enforce flag was specified.
    /// </summary>
    public bool Enforce { get; private init; }

    /// <summary>
    ///     Gets the set of filter tags for filtering requirements during export.
    ///     Returns null if no filter tags are specified.
    /// </summary>
    public HashSet<string>? FilterTags { get; private init; }

    /// <summary>
    ///     Gets the list of requirements files found from the --requirements glob pattern.
    /// </summary>
    public List<string> RequirementsFiles { get; private init; } = [];

    /// <summary>
    ///     Gets the list of test files found from the --tests glob pattern.
    /// </summary>
    public List<string> TestFiles { get; private init; } = [];

    /// <summary>
    ///     Gets the default markdown header depth for all reports.
    /// </summary>
    public int Depth { get; private init; } = 1;

    /// <summary>
    ///     Gets the requirements report output file path.
    /// </summary>
    public string? RequirementsReport { get; private init; }

    /// <summary>
    ///     Gets the report markdown depth.
    /// </summary>
    public int ReportDepth { get; private init; } = 1;

    /// <summary>
    ///     Gets the trace matrix output file path.
    /// </summary>
    public string? Matrix { get; private init; }

    /// <summary>
    ///     Gets the trace matrix markdown depth.
    /// </summary>
    public int MatrixDepth { get; private init; } = 1;

    /// <summary>
    ///     Gets the justifications export output file path.
    /// </summary>
    public string? JustificationsFile { get; private init; }

    /// <summary>
    ///     Gets the justifications markdown depth.
    /// </summary>
    public int JustificationsDepth { get; private init; } = 1;

    /// <summary>
    ///     Gets the proposed exit code for the application (0 for success, 1 for errors).
    /// </summary>
    public int ExitCode => _hasErrors ? 1 : 0;

    /// <summary>
    ///     Private constructor - use Create factory method instead.
    /// </summary>
    private Context()
    {
    }

    /// <summary>
    ///     Creates a Context instance from command-line arguments.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A new Context instance.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is null.</exception>
    /// <exception cref="ArgumentException">
    ///     Thrown under any of the following conditions:
    ///     <list type="bullet">
    ///       <item><description>
    ///         <b>Unknown argument</b> — an unrecognized flag is present in <paramref name="args"/>.
    ///       </description></item>
    ///       <item><description>
    ///         <b>Missing argument value</b> — a flag that requires a value (e.g. <c>--log</c>,
    ///         <c>--depth</c>) is the last element in <paramref name="args"/> with no value following it.
    ///       </description></item>
    ///       <item><description>
    ///         <b>Invalid depth value</b> — a <c>--depth</c>, <c>--report-depth</c>,
    ///         <c>--matrix-depth</c>, or <c>--justifications-depth</c> value is not a positive integer.
    ///       </description></item>
    ///       <item><description>
    ///         <b>Log file open failure</b> — the path supplied to <c>--log</c> cannot be opened
    ///         for writing (e.g. invalid path, missing parent directory, or insufficient permissions).
    ///       </description></item>
    ///     </list>
    /// </exception>
    public static Context Create(string[] args)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(args);

        // Initialize flag variables
        var version = false;
        var help = false;
        var silent = false;
        var validate = false;
        var lint = false;
        var enforce = false;

        // Initialize collection variables
        var requirementsPatterns = new List<string>();
        var testPatterns = new List<string>();
        HashSet<string>? filterTags = null;

        // Initialize optional parameters
        string? requirementsReport = null;
        int? reportDepth = null;
        string? matrix = null;
        int? matrixDepth = null;
        string? justificationsFile = null;
        int? justificationsDepth = null;
        string? logFile = null;
        string? resultsFile = null;
        var depth = 1;

        // Parse command-line arguments
        int i = 0;
        while (i < args.Length)
        {
            // Get current argument and advance index
            var arg = args[i++];

            switch (arg)
            {
                case "-v":
                case "--version":
                    version = true;
                    break;

                case "-?":
                case "-h":
                case "--help":
                    help = true;
                    break;

                case "--silent":
                    silent = true;
                    break;

                case "--validate":
                    validate = true;
                    break;

                case "--lint":
                    lint = true;
                    break;

                case "--depth":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a depth argument", nameof(args));
                    }

                    // Parse and validate depth value
                    if (!int.TryParse(args[i++], out depth) || depth < 1)
                    {
                        throw new ArgumentException($"{arg} requires a positive integer", nameof(args));
                    }

                    break;

                case "--result":
                case "--results":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a filename argument", nameof(args));
                    }

                    resultsFile = args[i++];
                    break;

                case "--enforce":
                    enforce = true;
                    break;

                case "--filter":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a comma-separated list of tags", nameof(args));
                    }

                    // Split comma-separated tags and add to the filter set
                    var tags = args[i++].Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    filterTags ??= [];
                    foreach (var tag in tags)
                    {
                        filterTags.Add(tag);
                    }

                    break;

                case "--log":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a filename argument", nameof(args));
                    }

                    logFile = args[i++];
                    break;

                case "--requirements":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a pattern argument", nameof(args));
                    }

                    requirementsPatterns.Add(args[i++]);
                    break;

                case "--tests":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a pattern argument", nameof(args));
                    }

                    testPatterns.Add(args[i++]);
                    break;

                case "--report":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a filename argument", nameof(args));
                    }

                    requirementsReport = args[i++];
                    break;

                case "--report-depth":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a depth argument", nameof(args));
                    }

                    // Parse and validate depth value
                    if (!int.TryParse(args[i++], out var parsedReportDepth) || parsedReportDepth < 1)
                    {
                        throw new ArgumentException($"{arg} requires a positive integer", nameof(args));
                    }

                    reportDepth = parsedReportDepth;
                    break;

                case "--matrix":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a filename argument", nameof(args));
                    }

                    matrix = args[i++];
                    break;

                case "--matrix-depth":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a depth argument", nameof(args));
                    }

                    // Parse and validate depth value
                    if (!int.TryParse(args[i++], out var parsedMatrixDepth) || parsedMatrixDepth < 1)
                    {
                        throw new ArgumentException($"{arg} requires a positive integer", nameof(args));
                    }

                    matrixDepth = parsedMatrixDepth;
                    break;

                case "--justifications":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a filename argument", nameof(args));
                    }

                    justificationsFile = args[i++];
                    break;

                case "--justifications-depth":
                    // Ensure argument has a value
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a depth argument", nameof(args));
                    }

                    // Parse and validate depth value
                    if (!int.TryParse(args[i++], out var parsedJustificationsDepth) || parsedJustificationsDepth < 1)
                    {
                        throw new ArgumentException($"{arg} requires a positive integer", nameof(args));
                    }

                    justificationsDepth = parsedJustificationsDepth;
                    break;

                default:
                    throw new ArgumentException($"Unsupported argument '{arg}'", nameof(args));
            }
        }

        // Create the context with parsed values
        var result = new Context
        {
            Version = version,
            Help = help,
            Silent = silent,
            Validate = validate,
            Lint = lint,
            ResultsFile = resultsFile,
            Enforce = enforce,
            FilterTags = filterTags,
            RequirementsFiles = GlobMatcher.FindMatchingFiles(requirementsPatterns),
            TestFiles = GlobMatcher.FindMatchingFiles(testPatterns),
            RequirementsReport = requirementsReport,
            Depth = depth,
            ReportDepth = reportDepth ?? depth,
            Matrix = matrix,
            MatrixDepth = matrixDepth ?? depth,
            JustificationsFile = justificationsFile,
            JustificationsDepth = justificationsDepth ?? depth
        };

        // Open log file if specified
        if (logFile != null)
        {
            try
            {
                result._logWriter = new StreamWriter(logFile, append: false) { AutoFlush = true };
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to open log file '{logFile}': {ex.Message}", nameof(args), ex);
            }
        }

        return result;
    }

    /// <summary>
    ///     Writes a line of output to the console and log file (if logging is enabled).
    /// </summary>
    /// <param name="message">The message to write.</param>
    /// <remarks>
    ///     Console output is suppressed when <see cref="Silent"/> is <see langword="true"/>;
    ///     log file output is always written when a log file is open.
    /// </remarks>
    public void WriteLine(string message)
    {
        // Write to console unless silent mode is enabled
        if (!Silent)
        {
            Console.WriteLine(message);
        }

        // Write to log file if logging is enabled
        _logWriter?.WriteLine(message);
    }

    /// <summary>
    ///     Writes an error message to the error console and log file (if logging is enabled).
    /// </summary>
    /// <param name="message">The error message to write.</param>
    /// <remarks>
    ///     Sets the internal error flag, causing <see cref="ExitCode"/> to return 1 for the
    ///     lifetime of this context.
    /// </remarks>
    public void WriteError(string message)
    {
        // Mark that we have encountered errors
        _hasErrors = true;

        // Write to error console unless silent mode is enabled
        if (!Silent)
        {
            var previousColor = Console.ForegroundColor;
            try
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Error.WriteLine(message);
            }
            finally
            {
                Console.ForegroundColor = previousColor;
            }
        }

        // Write to log file if logging is enabled
        _logWriter?.WriteLine(message);
    }

    /// <summary>
    ///     Disposes resources used by the Context.
    /// </summary>
    /// <remarks>
    ///     Calling <see cref="Dispose"/> more than once is safe; subsequent calls are no-ops
    ///     (idempotent).
    /// </remarks>
    public void Dispose()
    {
        // Close and dispose the log file writer if it exists
        _logWriter?.Dispose();
        _logWriter = null;
    }
}
