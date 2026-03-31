# Linting Subsystem Design

The `Linting` subsystem provides structural validation of requirement YAML files for ReqStream.
It checks that requirement files conform to expected structure, naming conventions, and
cross-reference integrity.

## Overview

The `Linting` subsystem is invoked when the user passes `--lint` on the command line. It
reads all specified requirement YAML files and reports structural errors such as missing IDs,
duplicate IDs, and malformed test references.

## Units

The `Linting` subsystem contains the following software unit:

| Unit     | File                  | Responsibility                                                        |
|----------|-----------------------|-----------------------------------------------------------------------|
| `Linter` | `Linting/Linter.cs`   | Structural validation of requirement YAML files.                      |

## Interfaces

The `Linting` subsystem exposes the following interface to the rest of the tool:

| Interface    | Direction | Description                                                         |
|--------------|-----------|---------------------------------------------------------------------|
| `Linter.Run` | Outbound  | Validates requirement files and reports errors through `Context`.   |

## Interactions

| Dependency     | Direction | Purpose                                                             |
|----------------|-----------|---------------------------------------------------------------------|
| `Context`      | Uses      | Reports linting errors via `Context.WriteError`.                   |
| `Requirements` | Uses      | Reads requirement data to validate structure.                       |
| `Program`      | Used by   | Calls `Linter.Run` when `--lint` is set.                           |

## References

- [ReqStream Architecture][arch]
- [ReqStream Repository][repo]

[arch]: ../../../ARCHITECTURE.md
[repo]: https://github.com/demaconsulting/ReqStream
