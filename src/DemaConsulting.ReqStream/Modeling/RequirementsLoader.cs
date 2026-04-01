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

using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.ReqStream.Modeling;

/// <summary>
///     Loads requirements from YAML files using a single DOM tree walk that simultaneously
///     builds the requirements model and collects lint issues.
/// </summary>
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
    ///     A tuple of the parsed <see cref="Requirements"/> (or <c>null</c> when error-level issues
    ///     are present) and a read-only list of <see cref="LintIssue"/> objects describing all issues found.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when no paths are provided.</exception>
    internal static (Requirements? Requirements, IReadOnlyList<LintIssue> Issues) Load(string[] paths)
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

        // Walk each file, building the model and collecting issues
        foreach (var path in paths)
        {
            LoadFile(requirements, issues, path, seenIds, allRequirements, visitedFiles);
        }

        // Validate cycle-free requirement references on a best-effort basis, even if other errors exist
        if (allRequirements.Count > 0)
        {
            ValidateCycles(allRequirements, issues);
        }

        // Return null requirements if any error-level issues were found
        return issues.Any(i => i.Severity == LintSeverity.Error)
            ? (null, issues)
            : (requirements, issues);
    }

    /// <summary>
    ///     Reads and processes a single YAML file, walking its DOM tree to build model objects
    ///     and collect lint issues. Follows include directives recursively.
    /// </summary>
    /// <param name="requirements">The requirements tree being built.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The path to the YAML file.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen and the file they came from.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects, keyed by ID.</param>
    /// <param name="visitedFiles">Set of fully-resolved file paths already processed.</param>
    private static void LoadFile(
        Requirements requirements,
        List<LintIssue> issues,
        string path,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements,
        HashSet<string> visitedFiles)
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

        // Skip already-visited files (prevents infinite loops on cyclic includes)
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

        // Walk the document, building model objects and collecting issues
        LoadDocument(requirements, issues, path, root, seenIds, allRequirements);

        // Follow include directives recursively
        var baseDirectory = Path.GetDirectoryName(fullPath) ?? string.Empty;
        var includes = GetStringList(root, "includes");
        if (includes != null)
        {
            foreach (var include in includes.Where(s => !string.IsNullOrWhiteSpace(s)))
            {
                LoadFile(requirements, issues, Path.Combine(baseDirectory, include), seenIds, allRequirements, visitedFiles);
            }
        }
    }

    /// <summary>
    ///     Walks a document root mapping node, checking for unknown fields and loading
    ///     sections and test mappings.
    /// </summary>
    /// <param name="requirements">The requirements tree being built.</param>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="root">The document root mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
    private static void LoadDocument(
        Requirements requirements,
        List<LintIssue> issues,
        string path,
        YamlMappingNode root,
        Dictionary<string, string> seenIds,
        Dictionary<string, Requirement> allRequirements)
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

        // Apply test mappings to already-loaded requirements
        LoadDocumentMappings(issues, path, root, allRequirements);
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
        var tests = new List<string>();
        var testsNode = GetSequence(node, "tests");
        if (testsNode != null)
        {
            foreach (var testNode in testsNode.Children.OfType<YamlScalarNode>())
            {
                if (string.IsNullOrWhiteSpace(testNode.Value))
                {
                    issues.Add(new LintIssue(
                        $"{path}({testNode.Start.Line},{testNode.Start.Column})",
                        LintSeverity.Error,
                        "Test name cannot be blank"));
                }
                else
                {
                    tests.Add(testNode.Value!);
                }
            }
        }

        // Extract 'children' list (requirement ID references for hierarchical decomposition)
        var children = new List<string>();
        var childrenNode = GetSequence(node, "children");
        if (childrenNode != null)
        {
            children.AddRange(childrenNode.Children
                .OfType<YamlScalarNode>()
                .Where(s => s.Value != null)
                .Select(s => s.Value!));
        }

        // Extract and validate 'tags' list
        var tags = new List<string>();
        var tagsNode = GetSequence(node, "tags");
        if (tagsNode != null)
        {
            foreach (var tagNode in tagsNode.Children.OfType<YamlScalarNode>())
            {
                if (string.IsNullOrWhiteSpace(tagNode.Value))
                {
                    issues.Add(new LintIssue(
                        $"{path}({tagNode.Start.Line},{tagNode.Start.Column})",
                        LintSeverity.Error,
                        "Tag name cannot be blank"));
                }
                else
                {
                    tags.Add(tagNode.Value!);
                }
            }
        }

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
    ///     Loads test mappings from the document root and applies them to already-built requirements.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error locations.</param>
    /// <param name="root">The document root mapping node.</param>
    /// <param name="allRequirements">Dictionary of all built requirement objects.</param>
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

        // Resolve the referenced requirement (silently skip mappings to unknown IDs)
        allRequirements.TryGetValue(idNode.Value, out var requirement);

        // Extract 'tests' and apply to the requirement
        var testsNode = GetSequence(node, "tests");
        if (testsNode == null)
        {
            return;
        }

        foreach (var testNode in testsNode.Children.OfType<YamlScalarNode>())
        {
            if (string.IsNullOrWhiteSpace(testNode.Value))
            {
                issues.Add(new LintIssue(
                    $"{path}({testNode.Start.Line},{testNode.Start.Column})",
                    LintSeverity.Error,
                    "Test name cannot be blank in mapping"));
            }
            else if (requirement != null)
            {
                requirement.Tests.Add(testNode.Value!);
            }
        }
    }

    /// <summary>
    ///     Validates that no cyclic requirement references exist in the built model.
    ///     Adds error-level <see cref="LintIssue"/> objects for any cycles found.
    /// </summary>
    /// <param name="allRequirements">All built requirements, keyed by ID.</param>
    /// <param name="issues">The list to add lint issues to.</param>
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
            var cycleId = requirement.Children.FirstOrDefault(visiting.Contains);
            if (cycleId != null)
            {
                var cycleStart = currentPath.IndexOf(cycleId);
                var cyclePath = string.Join(" -> ", currentPath.Skip(cycleStart).Append(cycleId));
                var location = allRequirements.TryGetValue(cycleId, out var cycleReq) && cycleReq.Location != null
                    ? cycleReq.Location
                    : cycleId;
                issues.Add(new LintIssue(
                    location,
                    LintSeverity.Error,
                    $"Circular requirement reference detected: {cyclePath}"));
            }
            else
            {
                foreach (var childId in requirement.Children.Where(id => !visited.Contains(id)))
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
    ///     Gets a sequence node from a mapping node by key, without reporting an error on type mismatch.
    /// </summary>
    /// <param name="mapping">The mapping node to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>The sequence node, or null if not found or not a sequence.</returns>
    private static YamlSequenceNode? GetSequence(YamlMappingNode mapping, string key)
    {
        var keyNode = new YamlScalarNode(key);
        return mapping.Children.TryGetValue(keyNode, out var value) ? value as YamlSequenceNode : null;
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
    ///     Gets a list of string values from a sequence within a mapping node.
    /// </summary>
    /// <param name="mapping">The mapping node to search.</param>
    /// <param name="key">The key to look up.</param>
    /// <returns>A list of string values, or null if the key is not found.</returns>
    private static List<string>? GetStringList(YamlMappingNode mapping, string key)
    {
        var sequence = GetSequence(mapping, key);
        if (sequence == null)
        {
            return null;
        }

        return sequence.Children
            .OfType<YamlScalarNode>()
            .Where(s => s.Value != null)
            .Select(s => s.Value!)
            .ToList();
    }
}
