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
/// Context class that handles command-line arguments and program output.
/// </summary>
public sealed class Context : IDisposable
{
    /// <summary>
    /// Output writer for normal program output.
    /// </summary>
    private readonly TextWriter _outputWriter;

    /// <summary>
    /// Output writer for error output.
    /// </summary>
    private readonly TextWriter _errorWriter;

    /// <summary>
    /// Log file stream writer (if logging is enabled).
    /// </summary>
    private StreamWriter? _logWriter;

    /// <summary>
    /// Indicates whether errors have been reported.
    /// </summary>
    private bool _hasErrors;

    /// <summary>
    /// Gets a value indicating whether the version flag was specified.
    /// </summary>
    public bool Version { get; private init; }

    /// <summary>
    /// Gets a value indicating whether the help flag was specified.
    /// </summary>
    public bool Help { get; private init; }

    /// <summary>
    /// Gets a value indicating whether the silent flag was specified.
    /// </summary>
    public bool Silent { get; private init; }

    /// <summary>
    /// Gets a value indicating whether the validate flag was specified.
    /// </summary>
    public bool Validate { get; private init; }

    /// <summary>
    /// Gets the list of requirements files found from the --requirements glob pattern.
    /// </summary>
    public List<string> RequirementsFiles { get; private init; } = new();

    /// <summary>
    /// Gets the list of test files found from the --tests glob pattern.
    /// </summary>
    public List<string> TestFiles { get; private init; } = new();

    /// <summary>
    /// Gets the requirements report output file path.
    /// </summary>
    public string? RequirementsReport { get; private init; }

    /// <summary>
    /// Gets the report markdown depth.
    /// </summary>
    public int ReportDepth { get; private init; } = 1;

    /// <summary>
    /// Gets the trace matrix output file path.
    /// </summary>
    public string? Matrix { get; private init; }

    /// <summary>
    /// Gets the trace matrix markdown depth.
    /// </summary>
    public int MatrixDepth { get; private init; } = 1;

    /// <summary>
    /// Gets the proposed exit code for the application (0 for success, 1 for errors).
    /// </summary>
    public int ExitCode => _hasErrors ? 1 : 0;

    /// <summary>
    /// Private constructor - use Create factory method instead.
    /// </summary>
    /// <param name="outputWriter">The output writer.</param>
    /// <param name="errorWriter">The error writer.</param>
    private Context(TextWriter outputWriter, TextWriter errorWriter)
    {
        _outputWriter = outputWriter;
        _errorWriter = errorWriter;
    }

    /// <summary>
    /// Creates a Context instance from command-line arguments.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <returns>A new Context instance.</returns>
    public static Context Create(string[] args)
    {
        return Create(args, Console.Out, Console.Error);
    }

    /// <summary>
    /// Creates a Context instance from command-line arguments with custom output writers.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    /// <param name="outputWriter">The output writer.</param>
    /// <param name="errorWriter">The error writer.</param>
    /// <returns>A new Context instance.</returns>
    internal static Context Create(string[] args, TextWriter outputWriter, TextWriter errorWriter)
    {
        var context = new Context(outputWriter, errorWriter);

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
        for (int i = 0; i < args.Length; i++)
        {
            var arg = args[i];

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
                    if (i + 1 >= args.Length)
                    {
                        context.WriteError($"Error: {arg} requires a filename argument");
                        return context;
                    }
                    logFile = args[++i];
                    break;

                case "--requirements":
                    if (i + 1 >= args.Length)
                    {
                        context.WriteError($"Error: {arg} requires a pattern argument");
                        return context;
                    }
                    requirementsFiles.AddRange(ExpandGlobPattern(args[++i]));
                    break;

                case "--tests":
                    if (i + 1 >= args.Length)
                    {
                        context.WriteError($"Error: {arg} requires a pattern argument");
                        return context;
                    }
                    testFiles.AddRange(ExpandGlobPattern(args[++i]));
                    break;

                case "--report":
                    if (i + 1 >= args.Length)
                    {
                        context.WriteError($"Error: {arg} requires a filename argument");
                        return context;
                    }
                    requirementsReport = args[++i];
                    break;

                case "--report-depth":
                    if (i + 1 >= args.Length)
                    {
                        context.WriteError($"Error: {arg} requires a depth argument");
                        return context;
                    }
                    if (!int.TryParse(args[++i], out reportDepth) || reportDepth < 1)
                    {
                        context.WriteError($"Error: {arg} requires a positive integer");
                        return context;
                    }
                    break;

                case "--matrix":
                    if (i + 1 >= args.Length)
                    {
                        context.WriteError($"Error: {arg} requires a filename argument");
                        return context;
                    }
                    matrix = args[++i];
                    break;

                case "--matrix-depth":
                    if (i + 1 >= args.Length)
                    {
                        context.WriteError($"Error: {arg} requires a depth argument");
                        return context;
                    }
                    if (!int.TryParse(args[++i], out matrixDepth) || matrixDepth < 1)
                    {
                        context.WriteError($"Error: {arg} requires a positive integer");
                        return context;
                    }
                    break;

                default:
                    context.WriteError($"Error: Unsupported argument '{arg}'");
                    return context;
            }
        }

        // Create the context with parsed values
        var result = new Context(outputWriter, errorWriter)
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

        // Transfer error state
        result._hasErrors = context._hasErrors;

        // Open log file if specified
        if (logFile != null)
        {
            try
            {
                result._logWriter = new StreamWriter(logFile, append: false);
            }
            catch (Exception ex)
            {
                result.WriteError($"Error: Failed to open log file '{logFile}': {ex.Message}");
            }
        }

        return result;
    }

    /// <summary>
    /// Expands a glob pattern to a list of matching file paths.
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
    /// Writes a line of output to the console and log file (if logging is enabled).
    /// </summary>
    /// <param name="message">The message to write.</param>
    public void WriteLine(string message)
    {
        // Write to console unless silent mode is enabled
        if (!Silent)
        {
            _outputWriter.WriteLine(message);
        }

        // Write to log file if logging is enabled
        _logWriter?.WriteLine(message);
    }

    /// <summary>
    /// Writes an error message to the error console and log file (if logging is enabled).
    /// </summary>
    /// <param name="message">The error message to write.</param>
    public void WriteError(string message)
    {
        _hasErrors = true;

        // Write to error console unless silent mode is enabled
        if (!Silent)
        {
            _errorWriter.WriteLine(message);
        }

        // Write to log file if logging is enabled
        _logWriter?.WriteLine(message);
    }

    /// <summary>
    /// Disposes resources used by the Context.
    /// </summary>
    public void Dispose()
    {
        _logWriter?.Dispose();
        _logWriter = null;
    }
}
