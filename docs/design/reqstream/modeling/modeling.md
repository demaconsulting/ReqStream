# Modeling Subsystem Design

The `Modeling` subsystem provides the data model and YAML parsing for ReqStream requirements
documents. It is responsible for reading, validating, and structuring requirement data for use
by the tracing, reporting, and enforcement subsystems.

## Overview

The `Modeling` subsystem handles all YAML file parsing and requirement data structures. It
reads one or more requirement YAML files (including those referenced via `includes`), merges
them into a unified requirement tree, and exposes that tree to the rest of the tool.

## Units

The `Modeling` subsystem contains the following software units:

| Unit           | File                       | Responsibility                                               |
|----------------|----------------------------|--------------------------------------------------------------|
| `Requirements` | `Modeling/Requirements.cs` | YAML parsing, section merging, and requirements document.    |
| `Section`      | `Modeling/Section.cs`      | Named group of requirements within a requirements document.  |
| `Requirement`  | `Modeling/Requirement.cs`  | Single requirement with ID, title, tags, and test links.     |

## Interfaces

The `Modeling` subsystem exposes the following interface to the rest of the tool:

| Interface              | Direction | Description                                                           |
|------------------------|-----------|-----------------------------------------------------------------------|
| `Requirements.Read`    | Outbound  | Reads and merges YAML requirement files into a requirement tree.      |
| `Requirements.Export`  | Outbound  | Exports requirements to a Markdown report.                            |

## Interactions

| Dependency    | Direction | Purpose                                                             |
|---------------|-----------|---------------------------------------------------------------------|
| `Context`     | Uses      | Receives file paths from `Context.RequirementsFiles`.               |
| `TraceMatrix` | Used by   | Receives the requirement tree to map test results to requirements.  |
| `Program`     | Used by   | Calls `Requirements.Read` to load requirements.                     |

## References

- [ReqStream System Design][arch]
- [ReqStream Repository][repo]

[arch]: ../reqstream.md
[repo]: https://github.com/demaconsulting/ReqStream
