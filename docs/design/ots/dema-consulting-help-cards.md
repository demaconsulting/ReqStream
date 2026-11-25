## DemaConsulting.HelpCards

`DemaConsulting.HelpCards` provides the built-in help of ReqStream. It turns Markdown help cards
embedded in the tool assembly into a validated catalog, serves `reqstream help` from that catalog for
people and AI agents, and supplies the test helpers that prove the cards are valid. It is used by the
Cli subsystem.

### Purpose

`DemaConsulting.HelpCards` was chosen because ReqStream wants help of the quality of a skill file: a
short usage text that points to topics, and a focused card per topic with worked examples that a
person or an AI agent can read, search, and request as JSON. The package is maintained within the same
program (DEMA Consulting), has no dependencies beyond the .NET Base Class Library, carries a compatible
MIT license, and ships build targets that embed the cards in the assembly, so a .NET tool package
needs no loose files. ReqStream is the first tool to adopt it.

The package is classified as an OTS item, following the precedent of `DemaConsulting.TestResults`, the other DEMA
library that this repository consumes as a published NuGet package: it is produced by a different repository,
ReqStream does not change it, and ReqStream verifies it through integration tests of the features it uses. The
software item standard would also allow a Shared Package for a package from another repository of the same program;
this repository has no Shared Package artifact tree and models all such dependencies as OTS items, so HelpCards
follows the same pattern as `DemaConsulting.TestResults`.

### Features Used

- **`HelpSource.FromAssembly(assembly)`** — exposes the `Help` folder that the package build targets
  embed in the tool assembly. Creating the source reads nothing.
- **`HelpCli.Create(HelpCliOptions)`**, **`HelpCli.TryRun`**, and **`HelpCli.Footer`** — the command-line
  helper that answers `help`, `help <topic>`, `help --search`, `help --json`, and the related options,
  and the constant pointer sentence for the usage text. **`HelpCliOptions.ToolName`** and
  **`HelpCliOptions.Version`** are set once, and the helper gives both to the catalog builder;
  **`HelpCliOptions.ConfigureCatalog`** adds the sources and settings that belong to ReqStream, and
  **`HelpCliOptions.GetCatalogConfiguration`** gives the tests the identical composition.
- **`HelpCatalog.Create(configure)`** with **`HelpCatalogBuilder.AddSource`**, **`TagVocabulary`**, and
  **`ExpectedRoots`** — builds and validates the catalog, and fixes the allowed tags and the set of topics.
- **`HelpCardValidator.AssertValid`** with **`HelpValidationOptions`** — tests prove the cards are valid.
- **`HelpExamples.Enumerate`** and **`ExampleCommandLine.TryParse`** — tests enumerate every command
  example and prove it parses.
- **`HelpExampleRunner.Run`**, **`HelpExampleInvocation`**, **`HelpExampleOutcome`**, and
  **`HelpExampleRunReport.AssertSucceeded`** — the system test runs every command example against the
  built tool and compares the exit code (`exit`) and the output fragments (`expect`) that the card itself
  states. The runner saves the source examples of each card to a fresh scratch folder; the library never
  starts a process, so the test supplies a delegate that launches the built tool. The result files that
  some commands read are test data in the test project (`TestData/HelpExamples/<card id>`, with `/` in
  the id written as `__`); the delegate copies them into the working directory of the invocation the first
  time it sees that directory.
- **MSBuild targets** shipped in the package — embed `Help/**` as resources of the tool assembly.

### Integration Pattern

`HelpCommand` is the only unit that uses the package in the shipped tool. It keeps one static
`HelpSource` and one `HelpCli`; creating them performs no I/O. `Program.Main` calls
`HelpCommand.Default.TryRun(args, out exitCode)` before `Context.Create`. The package answers only when
the first argument is `help` and otherwise returns `false` without building the catalog, so no other
run reads or parses a card. The helper builds the catalog with `HelpCatalog.Create` when the
help verb has been recognized, after it has set the tool name and the version from the options.

The cards use schema 2 (`schema: 2` in `_source.md`): an inline example names the file to save it as with
`save-as`, and a command example states its result with `exit` and `expect`. Example names are unique
within a card, not within a family of cards.

The project file references the package directly, because only a project with its own
`PackageReference` embeds cards. The reference is a normal runtime dependency of the tool. Test code
uses the validator and example helpers from the same package, which flow to the test project through the
project reference.

The help answer is written by the package directly to the console (or to writers supplied at creation),
not through `Context`. `--silent` and `--log` therefore do not apply to the help command, and the
`OutputProvider` and `ErrorProvider` hooks of the package are not used: the verb is answered before the
options are parsed and no context exists yet, and a second parse of the options for the help verb would
duplicate `Context.Create`. See the *HelpCommand* unit design.

### Version Constraints

A pre-release version (`0.1.0-beta.3`) is pinned in the project file, because the package has no stable
release yet. The card format (restricted front matter, schema 2, ids from paths, `{{tool}}` and
`{{version}}` tokens) and the exit codes of the helper (0, 2, 70) are those documented in the package API
reference. Upgrading the package, in particular to a release with a changed card format, requires
re-running the card validity, command-example, and runnable-example tests, and a review of this design.
Version numbers are otherwise managed centrally in the project file and SBOM per the OTS Dependencies
Design policy.
