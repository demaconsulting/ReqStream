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
using DemaConsulting.HelpCards.Testing;
using DemaConsulting.ReqStream.Cli;

namespace DemaConsulting.ReqStream.Tests.Cli;

/// <summary>
/// Unit tests for the HelpCommand class: the dispatch of the help verb, the deferred creation of
/// the catalog, and the validity of the embedded help cards.
/// </summary>
public sealed class HelpCommandTests
{
    /// <summary>
    /// Runs a help command with captured writers and a counting catalog factory.
    /// </summary>
    /// <param name="args">The command-line arguments.</param>
    /// <param name="answered">Receives whether the help command answered the arguments.</param>
    /// <param name="exitCode">Receives the exit code of the help command.</param>
    /// <param name="output">Receives the text written to the output writer.</param>
    /// <param name="error">Receives the text written to the error writer.</param>
    /// <param name="factoryCalls">Receives the number of times the catalog was built.</param>
    private static void Run(
        string[] args,
        out bool answered,
        out int exitCode,
        out string output,
        out string error,
        out int factoryCalls)
    {
        var outputWriter = new StringWriter();
        var errorWriter = new StringWriter();
        var calls = 0;
        var command = HelpCommand.Create(outputWriter, errorWriter, culture =>
        {
            calls++;
            return HelpCommand.CreateCatalog(culture);
        });

        answered = command.TryRun(args, out exitCode);
        output = outputWriter.ToString();
        error = errorWriter.ToString();
        factoryCalls = calls;
    }

    /// <summary>
    /// Test that the help verb alone prints the topic index and exits with code 0.
    /// </summary>
    [Fact]
    public void HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero()
    {
        // Arrange: nothing to arrange - the bare help verb is the input

        // Act: run the help verb
        Run(["help"], out var answered, out var exitCode, out var output, out _, out _);

        // Assert: the command answered, succeeded, and listed every root topic with its command
        Assert.True(answered);
        Assert.Equal(0, exitCode);
        foreach (var topic in HelpCommand.RootCardIds)
        {
            Assert.Contains($"reqstream help {topic}", output);
        }
    }

    /// <summary>
    /// Test that a card path prints the card with its examples.
    /// </summary>
    [Fact]
    public void HelpCommand_TryRun_CardPath_PrintsCardWithExamples()
    {
        // Arrange: nothing to arrange - the card path is the input

        // Act: print a child card by its path
        Run(["help", "test-linking", "source-filters"], out var answered, out var exitCode, out var output, out _, out _);

        // Assert: the card text and its examples are printed
        Assert.True(answered);
        Assert.Equal(0, exitCode);
        Assert.Contains("# Source filters", output);
        Assert.Contains("windows@Paths_Read_HandlesBackslashes", output);
        Assert.Contains("## Examples", output);
    }

    /// <summary>
    /// Test that --search ranks cards that match the search words.
    /// </summary>
    [Fact]
    public void HelpCommand_TryRun_SearchOption_ListsMatchingCards()
    {
        // Arrange: nothing to arrange - the search words are the input

        // Act: search for a word that names a topic
        Run(["help", "--search", "orphan"], out var answered, out var exitCode, out var output, out _, out _);

        // Assert: the orphans card is found
        Assert.True(answered);
        Assert.Equal(0, exitCode);
        Assert.Contains("reqstream help orphans", output);
    }

    /// <summary>
    /// Test that --json writes a versioned JSON document.
    /// </summary>
    [Fact]
    public void HelpCommand_TryRun_JsonOption_WritesJsonDocument()
    {
        // Arrange: nothing to arrange - the card path and --json are the input

        // Act: print a card as JSON
        Run(["help", "orphans", "--json"], out var answered, out var exitCode, out var output, out _, out _);

        // Assert: the output is a JSON document of schema version 1 that names the card
        Assert.True(answered);
        Assert.Equal(0, exitCode);
        Assert.StartsWith("{", output);
        Assert.Contains("\"helpSchemaVersion\":1", output);
        Assert.Contains("\"id\":\"orphans\"", output);
    }

    /// <summary>
    /// Test that an unknown topic exits with code 2 and prints a hint to the error writer.
    /// </summary>
    [Fact]
    public void HelpCommand_TryRun_UnknownTopic_ReturnsTwo()
    {
        // Arrange: nothing to arrange - an unknown topic is the input

        // Act: ask for a card that does not exist
        Run(["help", "no-such-topic"], out var answered, out var exitCode, out var output, out var error, out _);

        // Assert: the command answered with the usage-error code and a hint on the error stream
        Assert.True(answered);
        Assert.Equal(2, exitCode);
        Assert.Equal(string.Empty, output);
        Assert.Contains("no-such-topic", error);
        Assert.Contains("reqstream help", error);
    }

    /// <summary>
    /// Test that arguments other than the help verb are left alone and never build the catalog.
    /// </summary>
    [Fact]
    public void HelpCommand_TryRun_NonHelpArguments_ReturnsFalseWithoutBuildingCatalog()
    {
        // Arrange: nothing to arrange - the arguments are the input

        // Act: run the arguments of ordinary commands, and one help command for comparison
        Run(["--lint", "--requirements", "requirements.yaml"], out var lintAnswered, out _, out var lintOutput, out _, out var lintCalls);
        Run(["--help"], out var helpFlagAnswered, out _, out _, out _, out var helpFlagCalls);
        Run([], out var emptyAnswered, out _, out _, out _, out var emptyCalls);
        Run(["help"], out var helpAnswered, out _, out _, out _, out var helpCalls);

        // Assert: ordinary commands are not answered and never build the catalog
        Assert.False(lintAnswered);
        Assert.Equal(string.Empty, lintOutput);
        Assert.Equal(0, lintCalls);
        Assert.False(helpFlagAnswered);
        Assert.Equal(0, helpFlagCalls);
        Assert.False(emptyAnswered);
        Assert.Equal(0, emptyCalls);

        // Assert: the help verb builds the catalog exactly once
        Assert.True(helpAnswered);
        Assert.Equal(1, helpCalls);
    }

    /// <summary>
    /// Test that the footer points readers of the usage text to the help command.
    /// </summary>
    [Fact]
    public void HelpCommand_Footer_PointsToHelpCommand()
    {
        // Arrange: nothing to arrange - the footer is a constant sentence

        // Act: read the footer
        var footer = HelpCommand.Default.Footer;

        // Assert: the footer names the tool and the help command
        Assert.Equal("Run 'reqstream help' for topics and examples.", footer);
    }

    /// <summary>
    /// Test that every embedded help card is valid, with warnings treated as errors.
    /// </summary>
    [Fact]
    public void HelpCommand_Cards_AreValid()
    {
        // Arrange: validate exactly the composition the tool ships
        var options = new HelpValidationOptions { WarningsAreErrors = true };

        // Act and Assert: the validator throws a HelpCatalogException that lists every defect
        HelpCardValidator.AssertValid(builder => HelpCommand.Configure(builder, null), options);
    }

    /// <summary>
    /// Test that the root cards are exactly the expected topics, each with a purpose tag.
    /// </summary>
    [Fact]
    public void HelpCommand_Cards_RootsMatchExpectedTopics()
    {
        // Arrange: build the catalog the tool ships
        var catalog = HelpCommand.CreateCatalog(CultureInfo.InvariantCulture);

        // Act: read the ids of the root cards in their listed order
        var roots = catalog.Roots.Select(card => card.Id).ToList();

        // Assert: the topics are exactly the expected ones, and each carries a purpose tag
        Assert.Equal(HelpCommand.RootCardIds.OrderBy(id => id), roots.OrderBy(id => id));
        foreach (var card in catalog.Cards)
        {
            Assert.Contains(card.Tags, tag => HelpCommand.RequiredCardTags.Contains(tag));
        }
    }

    /// <summary>
    /// Test that every command example parses into arguments of the tool and starts with a real option.
    /// </summary>
    [Fact]
    public void HelpCommand_Cards_CommandExamplesParse()
    {
        // Arrange: build the catalog the tool ships
        var catalog = HelpCommand.CreateCatalog(CultureInfo.InvariantCulture);
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var count = 0;

        // Act and Assert: every command example parses, starts with an option, and has a unique name in its card
        foreach (var (card, example) in HelpExamples.Enumerate(catalog, [HelpExample.CommandKind]))
        {
            var parsed = ExampleCommandLine.TryParse(example.Content, HelpCommand.ToolName, out var arguments, out var error);
            Assert.True(parsed, $"{card.Id}#{example.Name}: {error}");
            Assert.StartsWith("-", arguments[0]);
            Assert.True(seen.Add($"{card.Id}#{example.Name}"), $"Duplicate example {card.Id}#{example.Name}");
            count++;
        }

        // Assert: the cards contain command examples at all
        Assert.True(count > 20, $"Expected more than 20 command examples but found {count}.");
    }
}
