## DemaConsulting.HelpCards

### Required Functionality

DemaConsulting.HelpCards (`ReqStream-OTS-HelpCards-Cli`, `ReqStream-OTS-HelpCards-Catalog`,
`ReqStream-OTS-HelpCards-Validator`, `ReqStream-OTS-HelpCards-Embedding`) shall answer a help command
from a catalog of Markdown help cards, build that catalog only after the help verb is recognized,
support search and JSON output, report defects in cards and command examples to tests, and embed the
card folder in the tool assembly. DemaConsulting.HelpCards is the library that serves
`reqstream help` to people and AI agents.

### Verification Approach

DemaConsulting.HelpCards is verified by integration test evidence. The `HelpCommand` unit tests drive
the package's command-line helper with the cards embedded in the tool assembly and check the answers,
exit codes, and the deferred creation of the catalog. The same tests run the package's validator,
root-card check, and command-example parser against the shipped cards. The system
tests run the built tool to confirm that the same behavior holds in a separate process and that every
command example of every card runs.

### Test Scenarios

**Command Answers**: Verifies that the helper prints the topic index, a card with its examples, and a
usage error with exit code 2 for an unknown topic, and that arguments other than the help verb are not
answered and never build the catalog. This scenario is tested by
`HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero`,
`HelpCommand_TryRun_CardPath_PrintsCardWithExamples`, `HelpCommand_TryRun_UnknownTopic_ReturnsTwo`, and
`HelpCommand_TryRun_NonHelpArguments_ReturnsFalseWithoutBuildingCatalog`.

**Catalog, Search, and JSON**: Verifies that the catalog loads the cards, enforces the expected topics,
ranks cards for a search, and writes a versioned JSON document. This scenario is tested by
`HelpCommand_TryRun_SearchOption_ListsMatchingCards`, `HelpCommand_TryRun_JsonOption_WritesJsonDocument`,
and `HelpCommand_Cards_RootsMatchExpectedTopics`.

**Validation**: Verifies that the validator accepts the shipped cards with warnings treated as errors
and that every command example parses. This scenario is tested by `HelpCommand_Cards_AreValid` and
`HelpCommand_Cards_CommandExamplesParse`.

**Embedding**: Verifies that the cards embedded in the assembly by the package build targets are complete
and usable. The test output holds no card files, so a validated catalog, a printed card, and every
command example of every card running against the built tool prove that the embedded copy is complete.
This scenario is tested by `HelpCommand_Cards_AreValid`, `HelpCommand_TryRun_CardPath_PrintsCardWithExamples`,
and `ReqStream_System_HelpExamples_AllCommandExamples_RunAgainstScratchSet`.

### Requirements Coverage

Each entry gives the requirement, its scenario in parentheses, and the test methods that verify it.

- ReqStream-OTS-HelpCards-Cli (Command Answers): `HelpCommand_TryRun_HelpVerb_PrintsTopicIndexAndReturnsZero`,
  `HelpCommand_TryRun_CardPath_PrintsCardWithExamples`, `HelpCommand_TryRun_UnknownTopic_ReturnsTwo`,
  `HelpCommand_TryRun_NonHelpArguments_ReturnsFalseWithoutBuildingCatalog`
- ReqStream-OTS-HelpCards-Catalog (Catalog, Search, and JSON): `HelpCommand_TryRun_SearchOption_ListsMatchingCards`,
  `HelpCommand_TryRun_JsonOption_WritesJsonDocument`, `HelpCommand_Cards_RootsMatchExpectedTopics`
- ReqStream-OTS-HelpCards-Validator (Validation): `HelpCommand_Cards_AreValid`,
  `HelpCommand_Cards_CommandExamplesParse`
- ReqStream-OTS-HelpCards-Embedding (Embedding): `HelpCommand_Cards_AreValid`,
  `HelpCommand_TryRun_CardPath_PrintsCardWithExamples`,
  `ReqStream_System_HelpExamples_AllCommandExamples_RunAgainstScratchSet`
