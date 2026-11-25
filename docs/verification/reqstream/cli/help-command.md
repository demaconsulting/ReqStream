### HelpCommand

#### Verification Approach

The HelpCommand unit is verified using xUnit unit tests in `HelpCommandTests.cs`. Each test creates a
`HelpCommand` with in-memory output and error writers and, where the test observes catalog creation,
a counting catalog configuration, then runs a set of arguments and asserts on the answer, the exit code, and
the number of times the catalog was built. Card validity is verified with the HelpCards test helpers
against the exact catalog configuration that the tool ships (`CreateOptions().GetCatalogConfiguration()`).
No component is mocked: the tests use the cards embedded in the tool assembly.

#### Test Environment

The HelpCommand unit tests require no setup beyond the standard xUnit test runner and .NET runtime.
No temporary files are created.

#### Acceptance Criteria

The HelpCommand unit verification is complete when all xUnit tests in `HelpCommandTests.cs` pass
without uncaught exceptions and all assertions succeed. The unit is considered verified when every
requirement in the Requirements Coverage is mapped to at least one passing test method.

#### Test Scenarios

**Help Verb**: Tests verify that the help verb answers. This scenario is tested by
`HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero`, which verifies that the topic index
lists every root card and exits 0; `HelpCommand_TryRun_CardPath_PrintsCardWithExamples`, which
verifies that a child card prints with its examples; and `HelpCommand_TryRun_UnknownTopic_ReturnsTwo`,
which verifies that an unknown topic exits 2 with a hint on the error writer.

**Search and JSON**: Tests verify search and structured output. This scenario is tested by
`HelpCommand_TryRun_SearchOption_ListsMatchingCards`, which verifies that `--search` ranks the card
that matches the words, and `HelpCommand_TryRun_JsonOption_WritesJsonDocument`, which verifies that
`--json` writes a versioned JSON document, and that the JSON index carries the tool name and the version
that were set once on the helper options.

**Deferred Catalog**: `HelpCommand_TryRun_NonHelpArguments_ReturnsFalseWithoutBuildingCatalog` verifies
with a counting catalog configuration that ordinary arguments, the `--help` flag, and no arguments are not
answered and never build the catalog, whereas the help verb builds it exactly once.

**Footer**: `HelpCommand_Footer_PointsToHelpCommand` verifies the constant pointer sentence.

**Valid Cards**: Tests verify the shipped cards. `HelpCommand_Cards_AreValid` validates the exact
catalog configuration with warnings treated as errors; `HelpCommand_Cards_RootsMatchExpectedTopics`
verifies that the root cards are the expected topics and that every card carries a purpose tag.

**Command Examples**: `HelpCommand_Cards_CommandExamplesParse` verifies that every command example
parses into arguments of the tool, starts with an option, and has a unique name. That the examples also
run is verified at the system level (see the system verification design).

#### Requirements Coverage

Each entry gives the requirement, its scenario in parentheses, and the test methods that verify it.

- `ReqStream-Cli-HelpCommand-HelpVerb` (Help Verb): `HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero`,
  `HelpCommand_TryRun_CardPath_PrintsCardWithExamples`, `HelpCommand_TryRun_UnknownTopic_ReturnsTwo`
- `ReqStream-Cli-HelpCommand-Search` (Search and JSON): `HelpCommand_TryRun_SearchOption_ListsMatchingCards`,
  `HelpCommand_TryRun_JsonOption_WritesJsonDocument`
- `ReqStream-Cli-HelpCommand-DeferredCatalog` (Deferred Catalog):
  `HelpCommand_TryRun_NonHelpArguments_ReturnsFalseWithoutBuildingCatalog`
- `ReqStream-Cli-HelpCommand-Footer` (Footer): `HelpCommand_Footer_PointsToHelpCommand`
- `ReqStream-Cli-HelpCommand-ValidCards` (Valid Cards): `HelpCommand_Cards_AreValid`,
  `HelpCommand_Cards_RootsMatchExpectedTopics`
- `ReqStream-Cli-HelpCommand-CommandExamples` (Command Examples): `HelpCommand_Cards_CommandExamplesParse`
