## YamlDotNet

`YamlDotNet` provides YAML parsing for the ReqStream Modeling subsystem. It converts YAML text
from requirements files into a DOM that `RequirementsLoader` traverses to build the requirements
tree.

### Purpose

`YamlDotNet` was chosen because it is the de-facto standard YAML library for .NET, is actively
maintained on NuGet, carries an MIT license compatible with ReqStream's own license, and exposes
a representation-model DOM API that gives `RequirementsLoader` full control over node traversal
and type-checking without requiring a rigid deserialization contract.

### Features Used

- **`YamlStream`** — top-level container; one instance is created per requirements file and
  populated via `YamlStream.Load(TextReader)`.
- **`YamlDocument`** — represents a single YAML document within the stream; accessed via
  `YamlStream.Documents[0]`.
- **`YamlMappingNode`** — represents a YAML mapping (dictionary); used to access named fields at
  the document root (`sections`, `includes`, `mappings`), within each section, and within each
  requirement.
- **`YamlSequenceNode`** — represents a YAML sequence (list); used to iterate requirement lists
  and nested section lists.
- **`YamlScalarNode`** — represents a scalar string value; used to read IDs, titles, tags, and
  other string fields from the DOM.
- **`YamlException`** — thrown by `YamlDotNet` on parse errors; caught by `RequirementsLoader`
  and converted to a `LintIssue` with `LintSeverity.Error`.

### Integration Pattern

`RequirementsLoader` uses the `YamlDotNet.RepresentationModel` namespace exclusively. The
serialization namespace (`YamlDotNet.Serialization`) is not used. The integration follows these
steps:

1. `new YamlStream()` is instantiated per requirements file.
2. `yamlStream.Load(reader)` is called with a `StringReader` over the file content.
3. The root document's root node is cast to `YamlMappingNode` to access document-level fields.
4. Nested nodes are walked recursively by `RequirementsLoader` to populate `Section` and
   `Requirement` objects.

`YamlDotNet` throws `YamlException` for malformed YAML. `RequirementsLoader` catches this and
converts it to a `LintIssue` with `LintSeverity.Error`, including the file path and line/column
from the exception's `Start` property. No other ReqStream unit depends on `YamlDotNet` directly;
all YAML parsing is encapsulated within `RequirementsLoader`.
