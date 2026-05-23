# OTS Dependencies

This chapter covers the design integration of Off-The-Shelf (OTS) software packages used by
ReqStream. Each OTS item is accessed through a single dedicated wrapper or utility class within
the system, ensuring minimal coupling and straightforward replacement. CI pipeline tools
(BuildMark, FileAssert, xUnit, Pandoc, ReviewMark, SarifMark, SonarMark, VersionMark,
WeasyPrint) are excluded from integration design documentation because they are not accessed
through source code APIs; their integration is through the CI configuration files.

## Selection Criteria

OTS items for ReqStream are selected based on the following criteria:

- **License compatibility** — only packages with MIT, Apache-2.0, or BSD-style licenses are used,
  consistent with the MIT license of ReqStream itself.
- **Community support and maturity** — packages must have active maintenance, stable versioning,
  and a history of production use within the .NET ecosystem.
- **Security track record** — packages must not carry known critical vulnerabilities; advisory
  notices are reviewed before any version is adopted.
- **Minimal API surface** — preference is given to packages with a focused API that closely
  matches the specific local need, reducing the risk of unintentional coupling to package
  internals.
- **NuGet availability** — all OTS items must be available as NuGet packages and support
  deterministic builds to ensure reproducibility.

## Version Management Policy

OTS package versions are tracked in the project file (`DemaConsulting.ReqStream.csproj`) and kept
current through automated Dependabot pull requests. The following policies apply:

- Minor and patch version upgrades are applied when Dependabot raises them and all CI checks pass.
- Major version upgrades require a design review to assess API changes; integration documentation
  is updated before the upgrade is merged.
- Version numbers are not recorded in design documentation; authoritative version information is
  maintained in the project file and in published Software Bill of Materials (SBOM) artifacts.
- Reproducible builds are ensured through lock files and pinned package versions in the project
  file.

## General Integration Approach

OTS items are integrated directly via their NuGet-published APIs. Each OTS item is accessed
through a single dedicated wrapper or utility class within ReqStream:

- **YamlDotNet** is accessed exclusively through `RequirementsLoader`, which isolates all YAML
  parsing logic and converts library-specific exceptions to `LintIssue` objects before they
  propagate to callers.
- **Microsoft.Extensions.FileSystemGlobbing** is accessed exclusively through `GlobMatcher`,
  which encapsulates all glob-pattern matching and exposes a simple `FindMatchingFiles` API to
  the rest of the system.
- **DemaConsulting.TestResults** is accessed through `TraceMatrix` and `Validation`, which are
  the only units that call the package's deserialization and serialization APIs directly.

This single-use-site pattern means that replacing an OTS package requires changes to only one
unit in the codebase, minimizing the impact of future upgrades or substitutions.

## Qualification Strategy

OTS items are qualified through integration tests in the main test project
(`DemaConsulting.ReqStream.Tests`). Each OTS item has dedicated integration tests that exercise
the specific features consumed by ReqStream and confirm that they behave as expected in the
local execution environment. These tests also serve as regression evidence when OTS package
versions are upgraded via Dependabot.
