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

**`_cli`**: `HelpCli` — the HelpCards command-line helper configured by `CreateOptions` with the tool name, the
version, the usage lines, an index preamble, the output writers, and the catalog configuration.

The cards are Markdown files with front matter in the `Help/` folder of the tool project, one file per topic. The
path of a card is its id, so `Help/test-linking/source-filters.md` is the card `test-linking/source-filters`, a
child of `test-linking`. The source declares `schema: 2`: an inline example names its file with `save-as`, and a
command example states its result with `exit` (default 0) and `expect` (output fragments). Example names are unique
within a card only. Example requirements files that the cards show live beside the cards. Result files are not
shipped: they are produced by test runners, so the example tests keep them as test data.

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

**CreateCatalog(culture)**: Builds the validated catalog with `HelpCatalog.Create` from
`CreateOptions().GetCatalogConfiguration(culture)`, so it is exactly the catalog the helper builds. The helper
itself builds the catalog only after the help verb was recognized, once per help command.

**CreateOptions(output, error, configure)**: Creates the `HelpCliOptions` of the helper. The tool name and the
version (`Program.Version`, which supplies the `{{version}}` token and the `version` field of the JSON index) are
set here once; the helper gives both, and the culture of `--lang`, to the catalog builder.

**Configure(builder)**: Adds ReqStream's part of the catalog configuration: the embedded `Cards` source, the closed
tag vocabulary with the required purpose tags, and the expected set of root cards. Tests validate the composition
that `CreateOptions().GetCatalogConfiguration()` returns, which is this configuration plus the tool name and version.

**Create(output, error, configure)**: Creates an instance that writes to the given writers, optionally with a
replacement catalog configuration. Used by tests to capture output and to count catalog builds.

#### Error Handling

`HelpCommand` never throws for user input. A wrong help argument or an unknown topic exits with 2 and writes
`Error: ...` and a hint to the error writer (or the error document to the output when `--json` was given). A defective
catalog — a card that fails validation, a missing source name, or a root card that is missing or unexpected — exits
with 70 after writing `Help is unavailable` and the reason to the error writer.

`help` bypasses `Context`: `--silent` and `--log` do not apply to it and it prints no banner. This is deliberate,
because the help answer is the product of the command, and an agent that asks for help must always receive it. The
`OutputProvider` and `ErrorProvider` hooks of HelpCards could route the answer through a `Context`, but the verb is
answered before the options are parsed, so no `Context` exists; making one would mean parsing `--silent` and `--log`
twice or teaching `Context.Create` the help verb, for no gain.

#### Interactions

##### Dependencies

- **DemaConsulting.HelpCards** (OTS) — `HelpCli`, `HelpCliOptions`, `HelpSource`, `HelpCatalog`, and
  `HelpCatalogBuilder`.
- **Program** — `Program.Version` supplies the tool version for the `{{version}}` token.

##### Callers

- **Program** — `Main` calls `TryRun` before creating the `Context`; `PrintHelp` reads `Footer`.
- **Test code** — creates instances with `Create` and validates the catalog configured by `Configure`.
