---
name: ReqStream Usage
description: Follow these standards when managing requirements with ReqStream.
globs: ["requirements.yaml", "docs/reqstream/**/*.yaml"]
---

# Required Standards

Read these standards first before applying this standard:

- **`requirements-principles.md`** - Requirements principles and unidirectionality
- **`software-items.md`** - Software categorization (System/Subsystem/Unit/OTS/Shared Package)

# Requirements Organization

Organize requirements under `docs/reqstream/` mirroring the source code structure
because ReqStream discovers files via the includes chain in `requirements.yaml`
and organizes report output by this hierarchy:

```text
requirements.yaml                    # Root file (includes only)
docs/reqstream/
├── {system-name}.yaml               # System-level requirements
├── {system-name}/                   # System folder (one per system)
│   ├── platform-requirements.yaml  # Platform support requirements
│   ├── {subsystem-name}.yaml        # Subsystem requirements
│   ├── {subsystem-name}/            # Subsystem folder (kebab-case); may nest recursively
│   │   ├── {subsystem-name}.yaml    # Child subsystem requirements
│   │   ├── {subsystem-name}/        # Child subsystem folder
│   │   └── {unit-name}.yaml         # Unit requirements
│   └── {unit-name}.yaml             # System-level unit requirements
├── ots/                             # OTS items appear as a distinct section in reports
│   └── {ots-name}.yaml              # Requirements for OTS components
└── shared/                          # Shared Packages appear as a distinct section in reports
    └── {package-name}.yaml          # Requirements for Shared Package dependencies
```

Local items have matching relative paths across `docs/reqstream/`, `docs/design/`, and `docs/verification/`:

- Requirements: `{system-name}[/{subsystem-name}...]/{item-name}.yaml`
- Design: `{system-name}[/{subsystem-name}...]/{item-name}.md`
- Verification: `{system-name}[/{subsystem-name}...]/{item-name}.md`

# Requirements File Format

Each file adds requirements at exactly one level of the hierarchy. The file spells out
its full ancestry as nested `{ItemName} Requirements` sections down to that level, then
places requirements there. ReqStream merges identical section title paths across included
files automatically. Always determine item classification from `docs/design/introduction.md` -
folder depth does not determine whether an item is a subsystem or unit.

Valid section nestings (names in `{braces}` are placeholders):

```text
{SystemName} Requirements              # system-level requirements
├── {SubsystemName} Requirements       # root subsystem requirements
│   ├── {SubsystemName} Requirements   # nested subsystem (may recurse)
│   │   └── {UnitName} Requirements    # unit under a nested subsystem
│   └── {UnitName} Requirements        # unit under a root subsystem
└── {UnitName} Requirements            # unit directly under the system
OTS Software Requirements          # OTS root section (fixed title)
└── {OtsName} Requirements         # requirements for one OTS item
Shared Package Requirements        # shared package root section (fixed title)
└── {PackageName} Requirements     # requirements for one shared package
```

Each file implements one path through this tree:

```yaml
sections:
  - title: '{SystemName} Requirements'
    sections:
      - title: '{SubsystemName} Requirements'
        requirements:
          - id: System-Subsystem-Feature    # Used as-is in all reports - make it readable
            title: The subsystem shall perform the required function.
            justification: |              # ReqStream extracts this into the justifications report (--justifications)
              Business rationale and any regulatory references.
            tags:                         # Optional: categorize for filtering with --filter
              - security
            children:                     # Optional: ReqStream validates this decomposition chain
              - System-Subsystem-Unit-Feat  # Downward links only (see requirements-principles.md)
            tests:                        # ReqStream matches these by method name in test results
              - TestMethodName
              - windows@PlatformSpecificTest  # Only test runs on Windows count as evidence
```

# Tags (OPTIONAL)

Tags are free-form - no mandatory vocabulary. Common tags: `security`, `safety`, `performance`,
`compliance`, `reliability`, `critical`. Use `--filter` to selectively export or enforce subsets
(OR logic across comma-separated tags):

```bash
dotnet reqstream --requirements requirements.yaml \
  --filter security,critical \
  --report docs/requirements_doc/generated/security_requirements.md
```

# Root Tags and Orphan Checking (OPTIONAL)

ReqStream can detect "orphaned" requirements - well-formed requirements that do not trace
downward from any recognized product/quality need - because AI coding agents and human
contributors can otherwise add plausible-looking, well-justified, fully-tested requirements
that are disconnected from any real requirement, feeding CI green without adding real value.

Declare which tag(s) mark trusted starting points ("roots") using the document-level
`root-tags:` key, a peer of `includes:`/`sections:`/`mappings:`:

```yaml
root-tags:
  - product
```

- **Root-ness** is determined purely by whether a requirement's `tags:` intersects the
  configured `root-tags` set - no other property makes a requirement a root.
- `root-tags:` declared in any included file accumulates: the effective root-tag set is the
  UNION across every loaded file, not just the top-level file.
- A matching CLI flag `--root-tags <tag1,tag2,...>` (same comma-separated parsing convention as
  `--filter`) UNIONS with (never replaces) any YAML-declared root tags, letting root tags be
  extended at invocation time without editing YAML.
- If the final merged root-tag set is empty (no CLI flag, no YAML anywhere), orphan checking is
  skipped entirely - this feature is fully backward compatible with requirements files that
  predate it.

**How orphan checking works**: every requirement whose `tags:` intersects the root-tag set is a
root. ReqStream performs a downward graph traversal from every root via each requirement's
`children:` links (the requirement graph is a DAG, not a tree, so a requirement reachable via
multiple parents is only ever visited once). Any requirement reachable from a root - or that is
itself a root - is "rooted"; everything else in the fully-loaded requirement set is "orphaned".
This check always runs against the complete loaded requirement graph and is **independent of
`--filter`**, which only narrows report/matrix output elsewhere in the same invocation.

Orphan checking triggers automatically whenever the merged root-tag set is non-empty - there is
no separate `--orphans` flag, and it runs alongside `--lint` as well as full requirements
processing, so agents do not need to remember two separate invocations to see both structural
lint issues and orphaned requirements:

- **Without `--enforce`**: orphans are reported as a non-fatal warning (does not affect the exit
  code), e.g.:

  ```text
  Warning: 2 of 47 requirements orphaned (not reachable from any requirement tagged: product).
    - ReqStream-Unit-JsonEscapeHelper
    - ReqStream-Unit-RetryBackoffCalculator
  ```

- **With `--enforce`**: orphans found becomes a build-breaking error at the same severity tier as
  missing test coverage, regardless of whether `--tests` was supplied. `--enforce` independently
  enforces (a) test coverage if a trace matrix was built and (b) orphan-freedom if root tags are
  configured; the pre-existing "nothing to enforce" error is reported only when **neither**
  applies. `--lint --enforce` enforces orphan-freedom the same way, without requiring a full
  `--requirements`/`--report`/`--matrix` invocation.

```bash
dotnet reqstream --requirements requirements.yaml \
  --root-tags product \
  --enforce
```

## Do Not Create Unauthorized New Roots (MANDATORY when root-tags is configured)

AI coding agents and contributors **must not** create a new root-tagged requirement, or retag an
existing requirement to add a root tag, without explicit task authorization. New work must attach
as `children:` under an **existing, human-approved** root wherever a suitable one exists.
Introducing a new root is a deliberate, reviewable action with its own justification for why the
existing root set does not already cover the need - it must never be a side effect of routine
feature work.

This mirrors the "how vs what" convention used for Off-The-Shelf (OTS) dependencies: a dependency
or tool requirement is never itself a root - it is a `children:` entry under a requirement that
states the actual outcome/capability needed. See `docs/reqstream/reqstream/modeling.yaml` for a
worked example of this pattern: the requirement `ReqStream-Modeling-YamlParsing` describes *what*
capability is needed (parsing requirement YAML files into a structured data model), and lists the
specific OTS tool requirement `ReqStream-OTS-YamlDotNet` (defined in
`docs/reqstream/ots/yamldotnet.yaml`) only as a `children:` entry, never as an independent
root-level requirement. New low-level requirements/design/code/tests should follow the same
discipline - describe the *what*, and link *how* underneath an approved root.

# Semantic IDs (MANDATORY)

Use the `System-Component-Feature` pattern because ReqStream uses IDs as-is in
all generated reports and the trace matrix - opaque IDs make those outputs
unreadable without a separate lookup:

- **System-level**: `TemplateTool-Core-DisplayHelp`
- **Subsystem-level**: `TemplateTool-Parser-ParseYaml`
- **Unit-level**: `TemplateTool-Validator-CheckFormat`
- **Bad**: `REQ-042` (meaningless in report output)

# Source Filter Requirements (CRITICAL)

Platform-specific requirements MUST use source filters because without them
ReqStream accepts test results from any platform as evidence - a Windows-only
requirement would incorrectly pass on Linux:

```yaml
tests:
  - "windows@TestMethodName"    # Only Windows test runs count as evidence
  - "ubuntu@TestMethodName"     # Only Linux test runs count as evidence
  - "net8.0@TestMethodName"     # Only .NET 8 runs count as evidence
  - "TestMethodName"            # Any platform acceptable
```

**WARNING**: Removing source filters invalidates platform-specific compliance
evidence.

# ReqStream Commands

```bash
# Validate YAML syntax and requirement IDs before generating any reports
dotnet reqstream --requirements requirements.yaml --lint

# Generate requirements document for compliance record
dotnet reqstream --requirements requirements.yaml \
  --report docs/requirements_doc/generated/requirements.md

# Generate justifications document for compliance record
dotnet reqstream --requirements requirements.yaml \
  --justifications docs/requirements_doc/generated/justifications.md

# Generate trace matrix proving each requirement is covered by passing tests
dotnet reqstream --requirements requirements.yaml \
  --tests "artifacts/**/*.trx" \
  --matrix docs/requirements_report/generated/trace_matrix.md
```

# Quality Checks

Before submitting requirements, verify:

- [ ] All requirements have semantic IDs (`System-Component-Feature` pattern)
- [ ] Every requirement has a justification explaining business/regulatory need
- [ ] Every requirement links to at least one test
- [ ] Platform-specific requirements use source filters (`platform@TestName`)
- [ ] All files and folders use kebab-case names matching source code structure
- [ ] All files are organized under `docs/reqstream/` following the folder structure above
