# Shared Package Design

This chapter covers the design integration of shared NuGet packages used by ReqStream that
are developed by DEMA Consulting but maintained as separate packages. Each section describes
how the package is integrated, which units call it, and how errors are handled.

## Shared Packages

The following shared packages are used by ReqStream:

| Package | Purpose |
| ------- | ------- |
| `DemaConsulting.TestResults` | Test result deserialization (TRX, JUnit) in the Tracing and SelfTest subsystems |

## Consumption Policy

ReqStream references shared packages as NuGet `PackageReference` entries in
`DemaConsulting.ReqStream.csproj`. Only the advertised public API surface of each package is
consumed; no internal or non-public types are accessed. Pre-release versions are not used in
production builds.

## Version Management Policy

Package version numbers are declared in `DemaConsulting.ReqStream.csproj` and managed by
Dependabot, which monitors the NuGet feed weekly (Mondays) and opens pull requests for all
`nuget-dependencies` grouped updates. Major-version upgrades that introduce breaking API changes
trigger a design review before the update is merged. Version numbers are captured in SBOMs
generated during the build and are not duplicated in design documentation.

## General Integration Approach

Shared package APIs are called directly; no dependency-injection container is used. Each
package's integration is encapsulated within one or two specific units that own the integration
boundary, keeping the rest of the codebase independent of the package's API surface. Exception
translation — converting package-specific exceptions into ReqStream's own error model — is
performed at the call site within those owning units.
