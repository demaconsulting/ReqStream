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

using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;

namespace DemaConsulting.ReqStream;

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
    ///     Gets the list of requirements files found from the --requirements glob pattern.
    /// </summary>
    public List<string> RequirementsFiles { get; private init; } = new();

    /// <summary>
    ///     Gets the list of test files found from the --tests glob pattern.
    /// </summary>
    public List<string> TestFiles { get; private init; } = new();

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
    /// <exception cref="ArgumentException">Thrown when arguments are invalid.</exception>
    public static Context Create(string[] args)
    {
        var version = false;
        var help = false;
        var silent = false;
        var validate = false;
        var requirementsFiles = new List<string>();
        var testFiles = new List<string>();
        string? requirementsReport = null;
        var reportDepth = 1;
        string? matrix = null;
        var matrixDepth = 1;
        string? logFile = null;

        // Parse command-line arguments
        int i = 0;
        while (i < args.Length)
        {
            var arg = args[i];
            i++; // Increment at start, will be incremented again for arguments that consume next value

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

                case "--log":
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a filename argument", nameof(args));
                    }
                    logFile = args[i++];
                    break;

                case "--requirements":
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a pattern argument", nameof(args));
                    }
                    requirementsFiles.AddRange(ExpandGlobPattern(args[i++]));
                    break;

                case "--tests":
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a pattern argument", nameof(args));
                    }
                    testFiles.AddRange(ExpandGlobPattern(args[i++]));
                    break;

                case "--report":
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a filename argument", nameof(args));
                    }
                    requirementsReport = args[i++];
                    break;

                case "--report-depth":
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a depth argument", nameof(args));
                    }
                    if (!int.TryParse(args[i++], out reportDepth) || reportDepth < 1)
                    {
                        throw new ArgumentException($"{arg} requires a positive integer", nameof(args));
                    }
                    break;

                case "--matrix":
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a filename argument", nameof(args));
                    }
                    matrix = args[i++];
                    break;

                case "--matrix-depth":
                    if (i >= args.Length)
                    {
                        throw new ArgumentException($"{arg} requires a depth argument", nameof(args));
                    }
                    if (!int.TryParse(args[i++], out matrixDepth) || matrixDepth < 1)
                    {
                        throw new ArgumentException($"{arg} requires a positive integer", nameof(args));
                    }
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
            RequirementsFiles = requirementsFiles,
            TestFiles = testFiles,
            RequirementsReport = requirementsReport,
            ReportDepth = reportDepth,
            Matrix = matrix,
            MatrixDepth = matrixDepth
        };

        // Open log file if specified
        if (logFile != null)
        {
            try
            {
                result._logWriter = new StreamWriter(logFile, append: false);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Failed to open log file '{logFile}': {ex.Message}", nameof(args), ex);
            }
        }

        return result;
    }

    /// <summary>
    ///     Expands a glob pattern to a list of matching file paths.
    /// </summary>
    /// <param name="pattern">The glob pattern.</param>
    /// <returns>A list of matching file paths.</returns>
    private static List<string> ExpandGlobPattern(string pattern)
    {
        var matcher = new Matcher();
        matcher.AddInclude(pattern);

        var currentDirectory = Directory.GetCurrentDirectory();
        var result = matcher.Execute(new DirectoryInfoWrapper(new DirectoryInfo(currentDirectory)));

        return result.Files.Select(f => Path.Combine(currentDirectory, f.Path)).ToList();
    }

    /// <summary>
    ///     Writes a line of output to the console and log file (if logging is enabled).
    /// </summary>
    /// <param name="message">The message to write.</param>
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
    public void WriteError(string message)
    {
        _hasErrors = true;

        // Write to error console unless silent mode is enabled
        if (!Silent)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ForegroundColor = previousColor;
        }

        // Write to log file if logging is enabled
        _logWriter?.WriteLine(message);
    }

    /// <summary>
    ///     Disposes resources used by the Context.
    /// </summary>
    public void Dispose()
    {
        _logWriter?.Dispose();
        _logWriter = null;
    }
}
