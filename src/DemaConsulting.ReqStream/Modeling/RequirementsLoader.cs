// Copyright (c) 2026 DEMA Consulting
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using DemaConsulting.ReqStream.Utilities;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.ReqStream.Modeling;

/// <summary>
///     Loads requirements from YAML files using a single DOM parse per file (each file's YAML is
///     parsed exactly once), followed by two logical passes over the resulting DOM trees: a tree
///     build pass that builds the requirements model, and a mapping resolution pass that applies
///     test mappings once the full requirements tree (across all included files) is known.
/// </summary>
/// <remarks>
///     Internal static class that is the sole reader of YAML from disk for requirements data.
///     Isolated behind <see cref="Requirements.Load"/>. Not thread-safe; designed for
///     single-threaded loading.
/// </remarks>
internal static class RequirementsLoader
{
    /// <summary>
    ///     Known fields at the document root level.
    /// </summary>
    private static readonly HashSet<string> KnownDocumentFields =
        new(StringComparer.Ordinal) { "sections", "mappings", "includes" };

    /// <summary>
    ///     Known fields within a section.
    /// </summary>
    private static readonly HashSet<string> KnownSectionFields =
        new(StringComparer.Ordinal) { "title", "requirements", "sections" };

    /// <summary>
    ///     Known fields within a requirement.
    /// </summary>
    private static readonly HashSet<string> KnownRequirementFields =
        new(StringComparer.Ordinal) { "id", "title", "justification", "tests", "children", "tags" };

    /// <summary>
    ///     Known fields within a test mapping.
    /// </summary>
    private static readonly HashSet<string> KnownMappingFields =
        new(StringComparer.Ordinal) { "id", "tests" };

    /// <summary>
    ///     Loads one or more requirements YAML files by walking each file's DOM tree, simultaneously
    ///     building the requirements model and collecting lint issues.
    /// </summary>
    /// <param name="paths">One or more paths to YAML files to load.</param>
    /// <returns>
    ///     A <see cref="LoadResult"/> containing the parsed <see cref="Requirements"/> (or <c>null</c>
    ///     when error-level issues are present) and a read-only list of <see cref="LintIssue"/> objects
    ///     describing all issues found.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when no paths are provided.</exception>
    /// <remarks>
    ///     Initializes all shared state for one load session (requirements tree, seenIds,
    ///     allRequirements, visitedFiles, activeFiles, pendingMappings). Runs two passes:
    ///     Pass 1 builds the full merged requirements tree across all files reachable via
    ///     <c>includes:</c>, by delegating to <see cref="LoadFile"/>, while <c>mappings:</c>
    ///     blocks are deferred (not yet resolved) into <c>pendingMappings</c>. Pass 2, run only
    ///     after the entire tree (across all included files) has been built, resolves every
    ///     deferred mapping via <see cref="LoadDocumentMappings"/> so that mappings may reference
    ///     requirements defined in any file, regardless of include order. Cycle detection via
    ///     <see cref="ValidateCycles"/> runs last, once mappings are resolved.
    ///     Returns null <see cref="Requirements"/> when any error-level issue is found,
    ///     allowing callers to detect failure without exception handling.
    /// </remarks>
    internal static LoadResult Load(string[] paths)
    {
        if (paths == null || paths.Length == 0)
        {
            throw new ArgumentException("At least one file path must be provided", nameof(paths));
        }

        var issues = new List<LintIssue>();
        var requirements = new Requirements();

        // seenIds tracks requirement IDs to detect duplicates: id -> first "path(line,col)" location
        var seenIds = new Dictionary<string, string>(StringComparer.Ordinal);

        // allRequirements collects built requirement objects for cycle detection and mapping resolution
        var allRequirements = new Dictionary<string, Requirement>(StringComparer.Ordinal);

        // visitedFiles prevents re-processing the same file via include loops
        var visitedFiles = new HashSet<string>(StringComparer.Ordinal);

        // activeFiles tracks the current include call stack to detect circular includes
        var activeFiles = new HashSet<string>(StringComparer.Ordinal);

        // pendingMappings collects each file's 'mappings:' root, deferred until the full
        // requirements tree (across all included files) has been built
        var pendingMappings = new List<(string Path, YamlMappingNode Root)>();

        // Pass 1: walk each file, building the model (deferring mapping resolution) and collecting issues
        foreach (var path in paths)
        {
            LoadFile(requirements, issues, path, seenIds, allRequirements, visitedFiles, activeFiles, pendingMappings);
        }

        // Pass 2: now that the full requirements tree is built, resolve deferred test mappings
        foreach (var (mappingPath, mappingRoot) in pendingMappings)
        {
            LoadDocumentMappings(issues, mappingPath, mappingRoot, allRequirements);
        }

        // Validate cycle-free requirement references on a best-effort basis, even if other errors exist
        if (allRequirements.Count > 0)
        {
            ValidateCycles(allRequirements, issues);
        }

        // Return null requirements if any error-level issues were found
        var hasErrors = issues.Any(i => i.Severity == LintSeverity.Error);
        return new LoadResult(hasErrors ? null : requirements, issues);
    }

    /// <summary>
    ///     Reads and processes a single YAML file (one DOM parse per file), walking its DOM tree
    ///     to build model objects and collect lint issues, deferring mapping resolution. Follows
    ///     include directives recursively so the tree-build pass covers the full include graph
    ///     before any mapping is resolved.
    /// </summary>
    /// <param name="requirements">The requirements tree being built.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The path to the YAML file.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen and the file they came from.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects, keyed by ID.</param>
    /// <param name="visitedFiles">Set of fully-resolved file paths already processed.</param>
    /// <param name="activeFiles">Set of fully-resolved file paths in the current include call stack.</param>
    /// <param name="pendingMappings">
    ///     Collects each file's <c>mappings:</c> document root, deferred for resolution in the
    ///     second pass (see <see cref="Load"/>) after the full requirements tree has been built.
    /// </param>
    /// <remarks>
    ///     Handles file-not-found and I/O errors by recording them as <see cref="LintIssue"/> objects
    ///     and returning — never throws for domain errors. Uses <c>activeFiles</c> to detect circular
    ///     file includes before following include directives. Uses <c>visitedFiles</c> to skip files
    ///     already processed (handles diamond-include patterns without re-processing). Mapping
    ///     resolution is intentionally deferred (see <c>pendingMappings</c>) so that mappings may
    ///     reference requirements defined in files not yet visited at this point in the recursion.
    /// </remarks>
    private static void LoadFile(
        Requirements requirements,
        List<LintIssue> issues,
        string path,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements,
        HashSet<string> visitedFiles,
        HashSet<string> activeFiles,
        List<(string Path, YamlMappingNode Root)> pendingMappings)
    {
        // Resolve to full path to detect duplicate includes
        string fullPath;
        try
        {
            fullPath = Path.GetFullPath(path);
        }
        catch (Exception ex)
        {
            issues.Add(new LintIssue(path, LintSeverity.Error, $"Invalid file path: {ex.Message}"));
            return;
        }

        // Detect circular includes: if this file is already in the active call stack, report it
        if (activeFiles.Contains(fullPath))
        {
            issues.Add(new LintIssue(path, LintSeverity.Error, $"Circular include detected: '{path}' is already being loaded"));
            return;
        }

        // Skip already-visited files (prevents re-processing the same file when included multiple times)
        if (!visitedFiles.Add(fullPath))
        {
            return;
        }

        // Verify the file exists
        if (!File.Exists(fullPath))
        {
            issues.Add(new LintIssue(path, LintSeverity.Error, "File not found"));
            return;
        }

        // Read the file content
        string yaml;
        try
        {
            yaml = File.ReadAllText(fullPath);
        }
        catch (Exception ex)
        {
            issues.Add(new LintIssue(path, LintSeverity.Error, $"Failed to read file: {ex.Message}"));
            return;
        }

        // Parse into a YAML DOM tree
        YamlNode? rawRoot;
        try
        {
            rawRoot = ParseYaml(yaml);
        }
        catch (Exception ex)
        {
            var location = ex is YamlException yamlEx
                ? $"{path}({yamlEx.Start.Line},{yamlEx.Start.Column})"
                : path;
            issues.Add(new LintIssue(location, LintSeverity.Error, $"Malformed YAML: {ex.Message}"));
            return;
        }

        // Empty documents are valid - nothing to do
        if (rawRoot == null)
        {
            return;
        }

        // A document-start marker (---) with no content produces a null-value scalar at root
        // rather than a null node; treat this as an empty document too.
        if (rawRoot is YamlScalarNode { Value: null or "" })
        {
            return;
        }

        // Document root must be a mapping node
        if (rawRoot is not YamlMappingNode root)
        {
            issues.Add(new LintIssue(
                $"{path}({rawRoot.Start.Line},{rawRoot.Start.Column})",
                LintSeverity.Error,
                "Document root must be a mapping"));
            return;
        }

        // Walk the document, building model objects and collecting issues (mapping resolution deferred)
        LoadDocument(requirements, issues, path, root, seenIds, allRequirements, pendingMappings);

        // Track this file as active during recursive include processing
        activeFiles.Add(fullPath);

        // Follow include directives recursively
        var baseDirectory = Path.GetDirectoryName(fullPath) ?? string.Empty;
        var includes = GetValidatedStringList(
            issues, path, root,
            "includes",
            "Each 'includes' entry must be a scalar string",
            "Each 'includes' entry cannot be blank");
        foreach (var include in includes)
        {
            string includePath;
            try
            {
                includePath = PathHelpers.SafePathCombine(baseDirectory, include);
            }
            catch (ArgumentException ex)
            {
                issues.Add(new LintIssue(path, LintSeverity.Error, $"Invalid 'includes' path '{include}': {ex.Message}"));
                continue;
            }

            LoadFile(requirements, issues, includePath, seenIds, allRequirements, visitedFiles, activeFiles, pendingMappings);
        }

        // Remove from active set after this file's includes are fully processed
        activeFiles.Remove(fullPath);
    }

    /// <summary>
    ///     Walks a document root mapping node, checking for unknown fields and loading
    ///     sections; defers mapping resolution to the second pass (see <see cref="Load"/>).
    /// </summary>
    /// <param name="requirements">The requirements tree being built.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="root">The document root mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    /// <param name="pendingMappings">
    ///     Collects this document's <c>mappings:</c> root for deferred resolution once the full
    ///     requirements tree (across all included files) has been built.
    /// </param>
    private static void LoadDocument(
        Requirements requirements,
        List<LintIssue> issues,
        string path,
        YamlMappingNode root,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements,
        List<(string Path, YamlMappingNode Root)> pendingMappings)
    {
        // Report unknown fields at document root
        foreach (var key in root.Children.Keys.OfType<YamlScalarNode>())
        {
            var keyValue = key.Value ?? string.Empty;
            if (!KnownDocumentFields.Contains(keyValue))
            {
                issues.Add(new LintIssue(
                    $"{path}({key.Start.Line},{key.Start.Column})",
                    LintSeverity.Error,
                    $"Unknown field '{keyValue}'"));
            }
        }

        // Load top-level sections into the requirements tree
        LoadDocumentSections(requirements, issues, path, root, seenIds, allRequirements);

        // Defer test mapping resolution to the second pass, once the full requirements tree
        // (across all included files) has been built
        pendingMappings.Add((path, root));
    }

    /// <summary>
    ///     Loads sections from the document root into the requirements tree.
    /// </summary>
    /// <param name="parent">The parent section to add loaded sections to.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="root">The mapping node containing the sections sequence.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    private static void LoadDocumentSections(
        Section parent,
        List<LintIssue> issues,
        string path,
        YamlMappingNode root,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements)
    {
        var sections = GetSequenceChecked(issues, path, root, "sections");
        if (sections == null)
        {
            return;
        }

        foreach (var sectionNode in sections.Children)
        {
            if (sectionNode is YamlMappingNode sectionMapping)
            {
                LoadSection(parent, issues, path, sectionMapping, seenIds, allRequirements);
            }
            else
            {
                issues.Add(new LintIssue(
                    $"{path}({sectionNode.Start.Line},{sectionNode.Start.Column})",
                    LintSeverity.Error,
                    "Section must be a mapping"));
            }
        }
    }

    /// <summary>
    ///     Walks a section mapping node, building a <see cref="Section"/> model object while
    ///     checking for structural issues, then recursively loading its requirements and child sections.
    /// </summary>
    /// <param name="parent">The parent section to add the built section to.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="node">The section mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    private static void LoadSection(
        Section parent,
        List<LintIssue> issues,
        string path,
        YamlMappingNode node,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements)
    {
        // Report unknown fields in section
        foreach (var key in node.Children.Keys.OfType<YamlScalarNode>())
        {
            var keyValue = key.Value ?? string.Empty;
            if (!KnownSectionFields.Contains(keyValue))
            {
                issues.Add(new LintIssue(
                    $"{path}({key.Start.Line},{key.Start.Column})",
                    LintSeverity.Error,
                    $"Unknown field '{keyValue}' in section"));
            }
        }

        // Extract and validate the title
        var titleNode = GetScalar(node, "title");
        Section? section = null;

        if (titleNode == null)
        {
            issues.Add(new LintIssue(
                $"{path}({node.Start.Line},{node.Start.Column})",
                LintSeverity.Error,
                "Section missing required field 'title'"));
        }
        else if (string.IsNullOrWhiteSpace(titleNode.Value))
        {
            issues.Add(new LintIssue(
                $"{path}({titleNode.Start.Line},{titleNode.Start.Column})",
                LintSeverity.Error,
                "Section 'title' cannot be blank"));
        }
        else
        {
            // Find an existing section with the same title (section merging) or create a new one
            section = parent.Sections.FirstOrDefault(s => s.Title == titleNode.Value);
            if (section == null)
            {
                section = new Section { Title = titleNode.Value };
                parent.Sections.Add(section);
            }
        }

        // Continue walking into requirements and child sections even if the title is invalid,
        // so we collect all issues. Use parent as fallback when section could not be created.
        var effectiveSection = section ?? parent;
        LoadSectionRequirements(effectiveSection, issues, path, node, seenIds, allRequirements);
        LoadChildSections(effectiveSection, issues, path, node, seenIds, allRequirements);
    }

    /// <summary>
    ///     Loads requirements from a section node, walking each requirement node to build
    ///     <see cref="Requirement"/> model objects and collect issues.
    /// </summary>
    /// <param name="section">The section to add loaded requirements to.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="node">The section mapping node containing the requirements sequence.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    private static void LoadSectionRequirements(
        Section section,
        List<LintIssue> issues,
        string path,
        YamlMappingNode node,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements)
    {
        var requirements = GetSequenceChecked(issues, path, node, "requirements");
        if (requirements == null)
        {
            return;
        }

        foreach (var reqNode in requirements.Children)
        {
            if (reqNode is YamlMappingNode reqMapping)
            {
                LoadRequirement(section, issues, path, reqMapping, seenIds, allRequirements);
            }
            else
            {
                issues.Add(new LintIssue(
                    $"{path}({reqNode.Start.Line},{reqNode.Start.Column})",
                    LintSeverity.Error,
                    "Requirement must be a mapping"));
            }
        }
    }

    /// <summary>
    ///     Loads child sections from a section node.
    /// </summary>
    /// <param name="parent">The parent section to add child sections to.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="node">The section mapping node containing the child sections sequence.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    private static void LoadChildSections(
        Section parent,
        List<LintIssue> issues,
        string path,
        YamlMappingNode node,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements)
    {
        var sections = GetSequenceChecked(issues, path, node, "sections");
        if (sections == null)
        {
            return;
        }

        foreach (var childNode in sections.Children)
        {
            if (childNode is YamlMappingNode childMapping)
            {
                LoadSection(parent, issues, path, childMapping, seenIds, allRequirements);
            }
            else
            {
                issues.Add(new LintIssue(
                    $"{path}({childNode.Start.Line},{childNode.Start.Column})",
                    LintSeverity.Error,
                    "Section must be a mapping"));
            }
        }
    }

    /// <summary>
    ///     Walks a requirement mapping node, building a <see cref="Requirement"/> model object while
    ///     checking for structural issues. Adds the requirement to the section and registers it for
    ///     duplicate detection and test-mapping resolution.
    /// </summary>
    /// <param name="section">The section to add the requirement to.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="node">The requirement mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    private static void LoadRequirement(
        Section section,
        List<LintIssue> issues,
        string path,
        YamlMappingNode node,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements)
    {
        // Report unknown fields in requirement
        foreach (var key in node.Children.Keys.OfType<YamlScalarNode>())
        {
            var keyValue = key.Value ?? string.Empty;
            if (!KnownRequirementFields.Contains(keyValue))
            {
                issues.Add(new LintIssue(
                    $"{path}({key.Start.Line},{key.Start.Column})",
                    LintSeverity.Error,
                    $"Unknown field '{keyValue}' in requirement"));
            }
        }

        // Extract and validate 'id'
        var idNode = GetScalar(node, "id");
        string? reqId = null;

        if (idNode == null)
        {
            issues.Add(new LintIssue(
                $"{path}({node.Start.Line},{node.Start.Column})",
                LintSeverity.Error,
                "Requirement missing required field 'id'"));
        }
        else if (string.IsNullOrWhiteSpace(idNode.Value))
        {
            issues.Add(new LintIssue(
                $"{path}({idNode.Start.Line},{idNode.Start.Column})",
                LintSeverity.Error,
                "Requirement 'id' cannot be blank"));
        }
        else if (seenIds.TryGetValue(idNode.Value, out var firstLocation))
        {
            issues.Add(new LintIssue(
                $"{path}({idNode.Start.Line},{idNode.Start.Column})",
                LintSeverity.Error,
                $"Duplicate requirement ID '{idNode.Value}' (first seen at {firstLocation})"));
        }
        else
        {
            reqId = idNode.Value;
            seenIds[reqId] = $"{path}({idNode.Start.Line},{idNode.Start.Column})";
        }

        // Extract and validate 'title'
        var titleNode = GetScalar(node, "title");
        string? reqTitle = null;

        if (titleNode == null)
        {
            var label = reqId != null ? $"requirement '{reqId}'" : "requirement";
            issues.Add(new LintIssue(
                $"{path}({node.Start.Line},{node.Start.Column})",
                LintSeverity.Error,
                $"{label} missing required field 'title'"));
        }
        else if (string.IsNullOrWhiteSpace(titleNode.Value))
        {
            issues.Add(new LintIssue(
                $"{path}({titleNode.Start.Line},{titleNode.Start.Column})",
                LintSeverity.Error,
                "Requirement 'title' cannot be blank"));
        }
        else
        {
            reqTitle = titleNode.Value;
        }

        // Extract optional 'justification' (plain scalar or block scalar)
        var justificationNode = GetScalar(node, "justification");
        var justification = justificationNode?.Value;

        // Extract and validate 'tests' list
        var tests = GetValidatedStringList(
            issues, path, node,
            "tests",
            "Test entry must be a scalar value",
            "Test name cannot be blank");

        // Extract and validate 'children' list (requirement ID references for hierarchical decomposition)
        var children = GetValidatedStringList(
            issues, path, node,
            "children",
            "Child requirement reference must be a scalar string",
            "Child requirement reference cannot be blank");

        // Extract and validate 'tags' list
        var tags = GetValidatedStringList(
            issues, path, node,
            "tags",
            "Tag entry must be a scalar value",
            "Tag name cannot be blank");

        // Build the Requirement model object only when we have a valid id and title
        if (reqId == null || reqTitle == null)
        {
            return;
        }

        var requirement = new Requirement
        {
            Id = reqId,
            Title = reqTitle,
            Justification = justification,
            Location = seenIds[reqId]
        };
        requirement.Tests.AddRange(tests);
        requirement.Children.AddRange(children);
        requirement.Tags.AddRange(tags);

        section.Requirements.Add(requirement);
        allRequirements[reqId] = requirement;
    }

    /// <summary>
    ///     Loads test mappings from a document root and applies them to already-built requirements.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="root">The document root mapping node.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    /// <remarks>
    ///     Called during the second pass (see <see cref="Load"/>), once the full requirements tree
    ///     across all included files has been built, so mappings may reference requirements defined
    ///     in any file regardless of include order.
    /// </remarks>
    private static void LoadDocumentMappings(
        List<LintIssue> issues,
        string path,
        YamlMappingNode root,
        Dictionary<string, Requirement> allRequirements)
    {
        var mappings = GetSequenceChecked(issues, path, root, "mappings");
        if (mappings == null)
        {
            return;
        }

        foreach (var mappingNode in mappings.Children)
        {
            if (mappingNode is YamlMappingNode mappingMapping)
            {
                LoadMapping(issues, path, mappingMapping, allRequirements);
            }
            else
            {
                issues.Add(new LintIssue(
                    $"{path}({mappingNode.Start.Line},{mappingNode.Start.Column})",
                    LintSeverity.Error,
                    "Mapping must be a mapping node"));
            }
        }
    }

    /// <summary>
    ///     Walks a test mapping node, checking for structural issues and adding tests to the
    ///     referenced requirement.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="node">The mapping node.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    private static void LoadMapping(
        List<LintIssue> issues,
        string path,
        YamlMappingNode node,
        Dictionary<string, Requirement> allRequirements)
    {
        // Report unknown fields in mapping
        foreach (var key in node.Children.Keys.OfType<YamlScalarNode>())
        {
            var keyValue = key.Value ?? string.Empty;
            if (!KnownMappingFields.Contains(keyValue))
            {
                issues.Add(new LintIssue(
                    $"{path}({key.Start.Line},{key.Start.Column})",
                    LintSeverity.Error,
                    $"Unknown field '{keyValue}' in mapping"));
            }
        }

        // Extract and validate mapping 'id'
        var idNode = GetScalar(node, "id");
        if (idNode == null)
        {
            issues.Add(new LintIssue(
                $"{path}({node.Start.Line},{node.Start.Column})",
                LintSeverity.Error,
                "Mapping missing required field 'id'"));
            return;
        }

        if (string.IsNullOrWhiteSpace(idNode.Value))
        {
            issues.Add(new LintIssue(
                $"{path}({idNode.Start.Line},{idNode.Start.Column})",
                LintSeverity.Error,
                "Mapping 'id' cannot be blank"));
            return;
        }

        // Resolve the referenced requirement, reporting an error for unknown mapping IDs
        if (!allRequirements.TryGetValue(idNode.Value, out var requirement))
        {
            issues.Add(new LintIssue(
                $"{path}({idNode.Start.Line},{idNode.Start.Column})",
                LintSeverity.Error,
                $"Mapping references unknown requirement id '{idNode.Value}'"));
        }

        // Extract and validate 'tests', then apply to the requirement
        var tests = GetValidatedStringList(
            issues, path, node,
            "tests",
            "Test entry must be a scalar value in mapping",
            "Test name cannot be blank in mapping");

        if (requirement != null)
        {
            requirement.Tests.AddRange(tests);
        }
    }

    /// <summary>
    ///     Validates that no cyclic requirement references exist in the built model.
    ///     Adds error-level <see cref="LintIssue"/> objects for any cycles found.
    /// </summary>
    /// <param name="allRequirements">All built requirements, keyed by ID.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <remarks>
    ///     Called once after all files are loaded. Skipped entirely when no requirements were
    ///     loaded. Uses a three-set DFS (visiting/currentPath/visited) that is safe to call
    ///     multiple times from the outer loop.
    /// </remarks>
    private static void ValidateCycles(
        Dictionary<string, Requirement> allRequirements,
        List<LintIssue> issues)
    {
        var visiting = new HashSet<string>();
        var currentPath = new List<string>();
        var visited = new HashSet<string>();

        foreach (var reqId in allRequirements.Keys.Where(id => !visited.Contains(id)))
        {
            ValidateCyclesFrom(reqId, allRequirements, issues, visiting, currentPath, visited);
        }
    }

    /// <summary>
    ///     Recursively checks a requirement and its children for cyclic references using DFS.
    /// </summary>
    /// <param name="reqId">The requirement ID to start from.</param>
    /// <param name="allRequirements">All built requirements, keyed by ID.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="visiting">IDs currently on the active DFS stack.</param>
    /// <param name="currentPath">Ordered list of IDs on the active DFS path (for cycle reporting).</param>
    /// <param name="visited">IDs already fully processed.</param>
    /// <remarks>
    ///     Standard DFS cycle-detection algorithm. A hit in <c>visiting</c> indicates a back-edge
    ///     (cycle). The <c>currentPath</c> list reconstructs the human-readable cycle path for the
    ///     error message. After processing all children, the ID moves from <c>visiting</c> to
    ///     <c>visited</c> to prevent redundant traversal.
    /// </remarks>
    private static void ValidateCyclesFrom(
        string reqId,
        Dictionary<string, Requirement> allRequirements,
        List<LintIssue> issues,
        HashSet<string> visiting,
        List<string> currentPath,
        HashSet<string> visited)
    {
        visiting.Add(reqId);
        currentPath.Add(reqId);

        if (allRequirements.TryGetValue(reqId, out var requirement))
        {
            foreach (var childId in requirement.Children)
            {
                if (!allRequirements.TryGetValue(childId, out var childReq))
                {
                    issues.Add(new LintIssue(
                        requirement.Location ?? reqId,
                        LintSeverity.Error,
                        $"Requirement '{reqId}' references unknown child '{childId}'"));
                    continue;
                }

                if (visiting.Contains(childId))
                {
                    var cycleStart = currentPath.IndexOf(childId);
                    var cyclePath = string.Join(" -> ", currentPath.Skip(cycleStart).Append(childId));
                    var location = childReq.Location ?? childId;
                    issues.Add(new LintIssue(
                        location,
                        LintSeverity.Error,
                        $"Circular requirement reference detected: {cyclePath}"));
                }
                else if (!visited.Contains(childId))
                {
                    ValidateCyclesFrom(childId, allRequirements, issues, visiting, currentPath, visited);
                }
            }
        }

        visiting.Remove(reqId);
        currentPath.RemoveAt(currentPath.Count - 1);
        visited.Add(reqId);
    }

    /// <summary>
    ///     Parses a YAML string and returns the root node, or null for empty documents.
    /// </summary>
    /// <param name="yaml">The YAML text to parse.</param>
    /// <returns>The root node, or null for an empty document.</returns>
    /// <exception cref="YamlException">Thrown when the YAML is malformed.</exception>
    private static YamlNode? ParseYaml(string yaml)
    {
        var stream = new YamlStream();
        using var reader = new StringReader(yaml);
        stream.Load(reader);
        return stream.Documents.Count == 0 ? null : stream.Documents[0].RootNode;
    }

    /// <summary>
    ///     Gets a scalar node from a mapping node by key.
    /// </summary>
    /// <param name="mapping">The mapping node to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The scalar node, or null if not found or not a scalar.</returns>
    private static YamlScalarNode? GetScalar(YamlMappingNode mapping, string key)
    {
        var keyNode = new YamlScalarNode(key);
        return mapping.Children.TryGetValue(keyNode, out var value) ? value as YamlScalarNode : null;
    }

    /// <summary>
    ///     Gets a sequence node from a mapping node by key, adding a type-mismatch lint issue if
    ///     the key exists but its value is not a sequence.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="mapping">The mapping node to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The sequence node, or null if not found or a type error was reported.</returns>
    private static YamlSequenceNode? GetSequenceChecked(
        List<LintIssue> issues,
        string path,
        YamlMappingNode mapping,
        string key)
    {
        var keyNode = new YamlScalarNode(key);
        if (!mapping.Children.TryGetValue(keyNode, out var value))
        {
            return null;
        }

        if (value is YamlSequenceNode seq)
        {
            return seq;
        }

        issues.Add(new LintIssue(
            $"{path}({value.Start.Line},{value.Start.Column})",
            LintSeverity.Error,
            $"Field '{key}' must be a sequence"));
        return null;
    }

    /// <summary>
    ///     Gets a validated list of string values from a sequence field in a mapping node.
    ///     Adds a type-mismatch lint issue if the field is not a sequence. Adds an error lint
    ///     issue for each non-scalar entry. Optionally adds an error for blank entries.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="mapping">The mapping node to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <param name="nonScalarMessage">Error message to emit when an entry is not a scalar.</param>
    /// <param name="blankMessage">Error message to emit when an entry is blank, or null to allow blanks.</param>
    /// <returns>The list of valid string values (never null).</returns>
    private static List<string> GetValidatedStringList(
        List<LintIssue> issues,
        string path,
        YamlMappingNode mapping,
        string key,
        string nonScalarMessage,
        string? blankMessage = null)
    {
        var result = new List<string>();
        var sequence = GetSequenceChecked(issues, path, mapping, key);
        if (sequence == null)
        {
            return result;
        }

        foreach (var child in sequence.Children)
        {
            if (child is not YamlScalarNode scalar)
            {
                issues.Add(new LintIssue(
                    $"{path}({child.Start.Line},{child.Start.Column})",
                    LintSeverity.Error,
                    nonScalarMessage));
                continue;
            }

            if (blankMessage != null && string.IsNullOrWhiteSpace(scalar.Value))
            {
                issues.Add(new LintIssue(
                    $"{path}({scalar.Start.Line},{scalar.Start.Column})",
                    LintSeverity.Error,
                    blankMessage));
                continue;
            }

            // Use pattern matching to add only non-null values; null scalars (e.g. from '- ~'
            // or '- null' in YAML) are silently skipped when blank reporting is not requested.
            if (scalar.Value is { } value)
            {
                result.Add(value);
            }
        }

        return result;
    }
}
