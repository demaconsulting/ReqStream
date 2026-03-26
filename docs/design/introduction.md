# Introduction

This document describes the internal design of the ReqStream tool, a .NET command-line application for managing
software requirements in YAML format.

## Purpose

The purpose of this document is to define the design of each software unit that makes up the ReqStream tool.
It describes the structure, responsibilities, key design decisions, and relationships of each unit, providing
a reference for developers maintaining or extending the tool.

## Scope

This document covers the design of the following software units:

- **Context** — command-line argument parsing and program output
- **Program** — application entry point and top-level orchestration
- **Validation** — self-validation test execution
- **Requirements** — YAML requirements model and export
- **TraceMatrix** — test result integration and coverage computation

This document does not cover deployment, build procedures, or external tool integrations beyond their interface
to ReqStream.

## Document Conventions

Throughout this document:

- Software units correspond to one or more C# source files in `src/DemaConsulting.ReqStream/`
- Code elements such as class names, method names, and properties are written in `monospace`
- Each unit section describes: overview, structure, key design decisions, and relationships to other units

## References

- [ReqStream User Guide][guide]
- [ReqStream Requirements Specification][requirements]
- [ReqStream Repository][repo]

[guide]: https://github.com/demaconsulting/ReqStream/blob/main/docs/guide/guide.md
[requirements]: https://github.com/demaconsulting/ReqStream
[repo]: https://github.com/demaconsulting/ReqStream
