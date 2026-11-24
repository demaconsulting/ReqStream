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

using System.Globalization;
using DemaConsulting.HelpCards.Catalog;
using DemaConsulting.HelpCards.Integration;
using DemaConsulting.HelpCards.Loading;

namespace DemaConsulting.ReqStream.Cli;

/// <summary>
///     Answers the <c>reqstream help</c> command from the built-in help cards, before the regular
///     argument parser runs.
/// </summary>
/// <remarks>
///     <para>
///         <c>HelpCommand</c> is the only unit that references the HelpCards library. It wraps one
///         <see cref="HelpCli"/> and a <see cref="HelpSource"/> over the <c>Help</c> folder that the
///         HelpCards build targets embed in the tool assembly. Creating either object performs no
///         file or console I/O, so the static <see cref="Default"/> instance costs nothing at start-up.
///     </para>
///     <para>
///         The help catalog is built lazily: <see cref="TryRun"/> returns <see langword="false"/>
///         at once for every first argument other than <c>help</c> (compared without regard to case)
///         and calls the catalog factory only after the verb was recognized. A normal run of the tool
///         therefore never reads or parses a help card.
///     </para>
///     <para>
///         The command bypasses <see cref="Context"/>: <c>--silent</c> and <c>--log</c> do not apply to
///         it and no banner is printed. It writes to the console (or to the writers supplied to
///         <see cref="Create"/>) and reports an exit code of 0 for success, 2 for wrong arguments or an
///         unknown topic, and 70 for a defective card catalog.
///     </para>
/// </remarks>
internal sealed class HelpCommand
{
    /// <summary>
    ///     The name of the tool as the user types it; used in usage text, command examples, and tokens.
    /// </summary>
    internal const string ToolName = "reqstream";

    /// <summary>
    ///     Tags that classify a card by its purpose; every card carries exactly one of the first three.
    /// </summary>
    internal static readonly string[] CardTags = ["tutorial", "reference", "troubleshooting", "ci"];

    /// <summary>
    ///     Tags of which every card must carry at least one.
    /// </summary>
    internal static readonly string[] RequiredCardTags = ["tutorial", "reference", "troubleshooting"];

    /// <summary>
    ///     Ids of the root cards, which are the topics listed by <c>reqstream help</c>.
    /// </summary>
    internal static readonly string[] RootCardIds =
    [
        "getting-started",
        "requirements-file",
        "includes",
        "test-linking",
        "test-results",
        "reports",
        "lint",
        "orphans",
        "enforcement",
        "command-line",
        "troubleshooting"
    ];

    /// <summary>
    ///     The underlying command-line helper.
    /// </summary>
    private readonly HelpCli _cli;

    /// <summary>
    ///     Initializes a new instance of the <see cref="HelpCommand"/> class.
    /// </summary>
    /// <param name="output">The writer for output, or <see langword="null"/> for the console output.</param>
    /// <param name="error">The writer for errors, or <see langword="null"/> for the console error.</param>
    /// <param name="catalogFactory">
    ///     The function that builds the catalog from the culture the user asked for, or
    ///     <see langword="null"/> to use <see cref="CreateCatalog"/>. Supplied only by tests.
    /// </param>
    private HelpCommand(
        TextWriter? output,
        TextWriter? error,
        Func<CultureInfo?, HelpCatalog>? catalogFactory)
    {
        _cli = HelpCli.Create(new HelpCliOptions
        {
            ToolName = ToolName,
            Version = Program.Version,
            CatalogFactory = catalogFactory ?? CreateCatalog,
            UsageLines = ["reqstream help [topic]", "reqstream help --search <words>"],
            IndexPreamble = "Start with 'reqstream help getting-started'.",
            Output = output,
            Error = error
        });
    }

    /// <summary>
    ///     Gets the cards embedded in the tool assembly. Creating the source reads nothing.
    /// </summary>
    internal static HelpSource Cards { get; } = HelpSource.FromAssembly(typeof(HelpCommand).Assembly);

    /// <summary>
    ///     Gets the instance used by the tool, which writes to the console at the time of the command.
    /// </summary>
    internal static HelpCommand Default { get; } = new(null, null, null);

    /// <summary>
    ///     Gets the sentence that points a reader of the usage text to <c>reqstream help</c>.
    /// </summary>
    /// <remarks>
    ///     The sentence is a constant: reading it never builds the help catalog.
    /// </remarks>
    public string Footer => _cli.Footer;

    /// <summary>
    ///     Creates a help command that writes to the given writers.
    /// </summary>
    /// <param name="output">The writer for output.</param>
    /// <param name="error">The writer for errors.</param>
    /// <param name="catalogFactory">
    ///     Optional replacement for <see cref="CreateCatalog"/>, used by tests to observe when the
    ///     catalog is built.
    /// </param>
    /// <returns>The new help command.</returns>
    internal static HelpCommand Create(
        TextWriter output,
        TextWriter error,
        Func<CultureInfo?, HelpCatalog>? catalogFactory = null) =>
        new(output, error, catalogFactory);

    /// <summary>
    ///     Builds the help catalog of the tool: the embedded cards, the tool name and version, the
    ///     closed tag vocabulary, and the expected set of topics.
    /// </summary>
    /// <param name="culture">The culture of the <c>--lang</c> option, or <see langword="null"/>.</param>
    /// <returns>The validated catalog.</returns>
    /// <exception cref="HelpCatalogException">Thrown when a card is defective.</exception>
    internal static HelpCatalog CreateCatalog(CultureInfo? culture) =>
        HelpCatalog.Create(builder => Configure(builder, culture));

    /// <summary>
    ///     Applies the tool's catalog configuration to a builder. Shared by the tool and the tests so
    ///     both validate exactly the composition the tool ships.
    /// </summary>
    /// <param name="builder">The builder to configure.</param>
    /// <param name="culture">The culture of the catalog, or <see langword="null"/> for the invariant culture.</param>
    internal static void Configure(DemaConsulting.HelpCards.Composition.HelpCatalogBuilder builder, CultureInfo? culture) =>
        builder
            .AddSource(Cards)
            .ToolName(ToolName)
            .ToolVersion(Program.Version)
            .Culture(culture ?? CultureInfo.InvariantCulture)
            .TagVocabulary(CardTags, RequiredCardTags)
            .ExpectedRoots(RootCardIds);

    /// <summary>
    ///     Runs the help command when the arguments start with the <c>help</c> verb.
    /// </summary>
    /// <param name="args">The command-line arguments of the tool.</param>
    /// <param name="exitCode">Receives the exit code when the command was answered.</param>
    /// <returns>
    ///     <see langword="true"/> when the arguments started with the help verb and were answered;
    ///     <see langword="false"/> when the tool should continue with its own parser. Returning
    ///     <see langword="false"/> never builds the catalog.
    /// </returns>
    public bool TryRun(string[] args, out int exitCode) => _cli.TryRun(args, out exitCode);
}
