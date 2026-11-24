### HelpCommand

![Cli Structure](CliView.svg)

#### Purpose

`HelpCommand` gives the tool a built-in `help` command that teaches a person or an AI agent how to use
ReqStream from the installed tool alone. It answers `reqstream help`, `reqstream help <topic>`,
`reqstream help --search <words>`, and the `--json` form of each from Markdown help cards embedded in the tool
assembly. It is the only unit that references the HelpCards library (see *DemaConsulting.HelpCards Integration
Design*).

The help verb is not an option, so `Context.Create` would reject it as an unsupported argument. `Program.Main`
therefore offers the arguments to `HelpCommand` before it creates a `Context`. `HelpCommand` answers only when the
first argument is `help` (compared without regard to case); for every other run it returns at once and the tool
continues with its own parser.

#### Data Model

**`ToolName`**: `const string` — `reqstream`, the name the user types; used in usage text, hints, command examples,
and the `{{tool}}` card token.

**`CardTags`**: `string[]` — the closed tag vocabulary of the cards: `tutorial`, `reference`, `troubleshooting`,
`ci`.

**`RequiredCardTags`**: `string[]` — the tags of which every card carries at least one: `tutorial`, `reference`,
`troubleshooting`.

**`RootCardIds`**: `string[]` — the ids of the root cards, which are the topics `reqstream help` lists:
`getting-started`, `requirements-file`, `includes`, `test-linking`, `test-results`, `reports`, `lint`, `orphans`,
`enforcement`, `command-line`, `troubleshooting`.

**`Cards`**: `HelpSource` — the cards embedded in the tool assembly, created by `HelpSource.FromAssembly`. Creating
the source reads nothing.

**`Default`**: `HelpCommand` — the instance the tool uses; it writes to the console at the time of the command.

**`_cli`**: `HelpCli` — the HelpCards command-line helper configured with the tool name, the version, the usage
lines, an index preamble, the output writers, and the catalog factory.

The cards are Markdown files with front matter in the `Help/` folder of the tool project, one file per topic. The
path of a card is its id, so `Help/test-linking/source-filters.md` is the card `test-linking/source-filters`, a
child of `test-linking`. Example data files live beside the cards.

#### Key Methods

**TryRun(args, exitCode)**: Offers the arguments of the tool to the help command.

- *Parameters*: `string[] args` — the command-line arguments; `out int exitCode`.
- *Returns*: `bool` — `true` when the first argument was the help verb and the command was answered.
- *Preconditions*: None.
- *Postconditions*: When `true`, the answer was written to the output or error writer and `exitCode` holds 0 for
  success, 2 for a wrong help argument or an unknown topic, or 70 for a defective card catalog. When `false`,
  nothing was written and the catalog was not built.

**Footer**: Read-only property returning the constant sentence `Run 'reqstream help' for topics and examples.`
Reading it never builds the catalog; `Program.PrintHelp` ends the usage text with it.

**CreateCatalog(culture)**: Builds the validated catalog. It is the catalog factory of the helper and is called
only after the help verb was recognized, once per help command. It delegates to `Configure`.

**Configure(builder, culture)**: Applies the catalog configuration shared by the tool and the tests: the embedded
`Cards` source, the tool name, the tool version (`Program.Version`, which supplies the `{{version}}` token), the
culture, the closed tag vocabulary with the required purpose tags, and the expected set of root cards. Tests
validate exactly this composition.

**Create(output, error, catalogFactory)**: Creates an instance that writes to the given writers, optionally with a
replacement catalog factory. Used by tests to capture output and to count catalog builds.

#### Error Handling

`HelpCommand` never throws for user input. A wrong help argument or an unknown topic exits with 2 and writes
`Error: ...` and a hint to the error writer (or the error document to the output when `--json` was given). A defective
catalog — a card that fails validation, a missing source name, or a root card that is missing or unexpected — exits
with 70 after writing `Help is unavailable` and the reason to the error writer.

`help` bypasses `Context`: `--silent` and `--log` do not apply to it and it prints no banner. This is deliberate,
because the help answer is the product of the command, and an agent that asks for help must always receive it.

#### Interactions

##### Dependencies

- **DemaConsulting.HelpCards** (OTS) — `HelpCli`, `HelpCliOptions`, `HelpSource`, `HelpCatalog`, and
  `HelpCatalogBuilder`.
- **Program** — `Program.Version` supplies the tool version for the `{{version}}` token.

##### Callers

- **Program** — `Main` calls `TryRun` before creating the `Context`; `PrintHelp` reads `Footer`.
- **Test code** — creates instances with `Create` and validates the catalog configured by `Configure`.
