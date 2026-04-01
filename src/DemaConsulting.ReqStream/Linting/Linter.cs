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

using DemaConsulting.ReqStream.Cli;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace DemaConsulting.ReqStream.Linting;

/// <summary>
///     Provides linting functionality for ReqStream requirement YAML files.
/// </summary>
public static class Linter
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
    ///     Known fields within a mapping.
    /// </summary>
    private static readonly HashSet<string> KnownMappingFields =
        new(StringComparer.Ordinal) { "id", "tests" };

    /// <summary>
    ///     Lints a list of requirement files and returns all issues found.
    /// </summary>
    /// <param name="files">The list of requirement files to lint.</param>
    /// <returns>A read-only list of lint issues found across all files and their includes.</returns>
    public static IReadOnlyList<LintIssue> Lint(IReadOnlyList<string> files)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(files);

        // No files to lint
        if (files.Count == 0)
        {
            return [];
        }

        // Collect issues
        var issues = new List<LintIssue>();

        // Track duplicate requirement IDs across all linted files: ID -> source file path
        var seenIds = new Dictionary<string, string>(StringComparer.Ordinal);

        // Track all visited files to avoid linting the same file twice (following includes)
        var visitedFiles = new HashSet<string>(StringComparer.Ordinal);

        // Lint each file, following includes
        foreach (var file in files)
        {
            LintFile(issues, file, seenIds, visitedFiles);
        }

        return issues;
    }

    /// <summary>
    ///     Lints a list of requirement files and reports all issues found.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="files">The list of requirement files to lint.</param>
    public static void Lint(Context context, IReadOnlyList<string> files)
    {
        // Validate input
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(files);

        // No files to lint
        if (files.Count == 0)
        {
            context.WriteLine("No requirements files specified.");
            return;
        }

        // Collect and report issues
        var issues = Lint(files);
        foreach (var issue in issues)
        {
            context.WriteError(issue.ToString());
        }

        // If no issues found, print success message using first file as root
        if (issues.Count == 0)
        {
            context.WriteLine($"{files[0]}: No issues found");
        }
    }

    /// <summary>
    ///     Lints a single requirements file, following includes.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The path to the file to lint.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen and the file they came from.</param>
    /// <param name="visitedFiles">Set of files already visited to avoid re-linting.</param>
    private static void LintFile(
        List<LintIssue> issues,
        string path,
        Dictionary<string, string> seenIds,
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

        // Skip already-visited files
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

        // Parse the YAML into a node tree
        YamlNode? rawRoot;
        try
        {
            rawRoot = ParseYaml(yaml);
        }
        catch (Exception ex)
        {
            // YamlDotNet may throw YamlException or InvalidOperationException for malformed YAML
            var location = ex is YamlException yamlEx
                ? $"{path}({yamlEx.Start.Line},{yamlEx.Start.Column})"
                : path;
            issues.Add(new LintIssue(location, LintSeverity.Error, $"Malformed YAML: {ex.Message}"));
            return;
        }

        // Empty documents are valid
        if (rawRoot == null)
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

        // Lint document root fields
        LintDocumentRoot(issues, path, root, seenIds);

        // Follow includes
        LintIncludes(issues, fullPath, GetStringList(root, "includes"), seenIds, visitedFiles);
    }

    /// <summary>
    ///     Lints all included files referenced from a parent file.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="parentFullPath">The resolved full path of the parent file.</param>
    /// <param name="includes">The list of include paths, or null if none.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen and the file they came from.</param>
    /// <param name="visitedFiles">Set of files already visited to avoid re-linting.</param>
    private static void LintIncludes(
        List<LintIssue> issues,
        string parentFullPath,
        List<string>? includes,
        Dictionary<string, string> seenIds,
        HashSet<string> visitedFiles)
    {
        if (includes == null)
        {
            return;
        }

        var baseDirectory = Path.GetDirectoryName(parentFullPath) ?? string.Empty;

        foreach (var include in includes.Where(includePath => !string.IsNullOrWhiteSpace(includePath)))
        {
            LintFile(issues, Path.Combine(baseDirectory, include), seenIds, visitedFiles);
        }
    }

    /// <summary>
    ///     Parses YAML text and returns the root node, or returns null for empty documents.
    /// </summary>
    /// <param name="yaml">The YAML text to parse.</param>
    /// <returns>The root node, or null if the document is empty.</returns>
    /// <exception cref="YamlException">Thrown when the YAML is malformed.</exception>
    private static YamlNode? ParseYaml(string yaml)
    {
        var stream = new YamlStream();
        using var reader = new StringReader(yaml);
        stream.Load(reader);

        if (stream.Documents.Count == 0)
        {
            return null;
        }

        return stream.Documents[0].RootNode;
    }

    /// <summary>
    ///     Lints the document root mapping node.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="root">The root mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    private static void LintDocumentRoot(
        List<LintIssue> issues,
        string path,
        YamlMappingNode root,
        Dictionary<string, string> seenIds)
    {
        // Check for unknown fields at document root
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

        // Lint sections
        LintDocumentSections(issues, path, root, seenIds);

        // Lint mappings
        LintDocumentMappings(issues, path, root);
    }

    /// <summary>
    ///     Lints the sections sequence within a document root.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="root">The root mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    private static void LintDocumentSections(
        List<LintIssue> issues,
        string path,
        YamlMappingNode root,
        Dictionary<string, string> seenIds)
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
                LintSection(issues, path, sectionMapping, seenIds);
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
    ///     Lints the mappings sequence within a document root.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="root">The root mapping node.</param>
    private static void LintDocumentMappings(
        List<LintIssue> issues,
        string path,
        YamlMappingNode root)
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
                LintMapping(issues, path, mappingMapping);
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
    ///     Lints a section mapping node.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="section">The section mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    private static void LintSection(
        List<LintIssue> issues,
        string path,
        YamlMappingNode section,
        Dictionary<string, string> seenIds)
    {
        // Check for unknown fields in section
        foreach (var key in section.Children.Keys.OfType<YamlScalarNode>())
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

        // Check required 'title' field
        var titleNode = GetScalar(section, "title");
        if (titleNode == null)
        {
            issues.Add(new LintIssue(
                $"{path}({section.Start.Line},{section.Start.Column})",
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

        // Lint requirements
        LintSectionRequirements(issues, path, section, seenIds);

        // Lint child sections
        LintSectionChildren(issues, path, section, seenIds);
    }

    /// <summary>
    ///     Lints the requirements sequence within a section.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="section">The section mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    private static void LintSectionRequirements(
        List<LintIssue> issues,
        string path,
        YamlMappingNode section,
        Dictionary<string, string> seenIds)
    {
        var requirements = GetSequenceChecked(issues, path, section, "requirements");
        if (requirements == null)
        {
            return;
        }

        foreach (var reqNode in requirements.Children)
        {
            if (reqNode is YamlMappingNode reqMapping)
            {
                LintRequirement(issues, path, reqMapping, seenIds);
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
    ///     Lints the child sections sequence within a section.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="section">The section mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    private static void LintSectionChildren(
        List<LintIssue> issues,
        string path,
        YamlMappingNode section,
        Dictionary<string, string> seenIds)
    {
        var sections = GetSequenceChecked(issues, path, section, "sections");
        if (sections == null)
        {
            return;
        }

        foreach (var childNode in sections.Children)
        {
            if (childNode is YamlMappingNode childMapping)
            {
                LintSection(issues, path, childMapping, seenIds);
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
    ///     Lints a requirement mapping node.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="requirement">The requirement mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    private static void LintRequirement(
        List<LintIssue> issues,
        string path,
        YamlMappingNode requirement,
        Dictionary<string, string> seenIds)
    {
        // Check for unknown fields in requirement
        foreach (var key in requirement.Children.Keys.OfType<YamlScalarNode>())
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

        // Check required 'id' field
        var reqId = LintRequirementId(issues, path, requirement, seenIds);

        // Check required 'title' field
        LintRequirementTitle(issues, path, requirement, reqId);

        // Check tests list for blank entries
        var tests = GetSequence(requirement, "tests");
        if (tests != null)
        {
            var blankTestStarts = tests.Children
                .OfType<YamlScalarNode>()
                .Where(s => string.IsNullOrWhiteSpace(s.Value))
                .Select(s => s.Start);
            foreach (var start in blankTestStarts)
            {
                issues.Add(new LintIssue(
                    $"{path}({start.Line},{start.Column})",
                    LintSeverity.Error,
                    "Test name cannot be blank"));
            }
        }

        // Check tags list for blank entries
        var tags = GetSequence(requirement, "tags");
        if (tags != null)
        {
            var blankTagStarts = tags.Children
                .OfType<YamlScalarNode>()
                .Where(s => string.IsNullOrWhiteSpace(s.Value))
                .Select(s => s.Start);
            foreach (var start in blankTagStarts)
            {
                issues.Add(new LintIssue(
                    $"{path}({start.Line},{start.Column})",
                    LintSeverity.Error,
                    "Tag name cannot be blank"));
            }
        }
    }

    /// <summary>
    ///     Validates the 'id' field of a requirement, checks for duplicates, and registers the ID.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="requirement">The requirement mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen and the file they came from.</param>
    /// <returns>The requirement ID if it can be parsed, or null if the ID is missing or blank.</returns>
    private static string? LintRequirementId(
        List<LintIssue> issues,
        string path,
        YamlMappingNode requirement,
        Dictionary<string, string> seenIds)
    {
        var idNode = GetScalar(requirement, "id");
        if (idNode == null)
        {
            issues.Add(new LintIssue(
                $"{path}({requirement.Start.Line},{requirement.Start.Column})",
                LintSeverity.Error,
                "Requirement missing required field 'id'"));
            return null;
        }

        if (string.IsNullOrWhiteSpace(idNode.Value))
        {
            issues.Add(new LintIssue(
                $"{path}({idNode.Start.Line},{idNode.Start.Column})",
                LintSeverity.Error,
                "Requirement 'id' cannot be blank"));
            return null;
        }

        var reqId = idNode.Value;
        if (seenIds.TryGetValue(reqId, out var firstFile))
        {
            issues.Add(new LintIssue(
                $"{path}({idNode.Start.Line},{idNode.Start.Column})",
                LintSeverity.Error,
                $"Duplicate requirement ID '{reqId}' (first seen in {firstFile})"));
            return reqId;
        }

        seenIds[reqId] = path;
        return reqId;
    }

    /// <summary>
    ///     Validates the 'title' field of a requirement.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="requirement">The requirement mapping node.</param>
    /// <param name="reqId">The requirement ID, used for error messages.</param>
    private static void LintRequirementTitle(
        List<LintIssue> issues,
        string path,
        YamlMappingNode requirement,
        string? reqId)
    {
        var titleNode = GetScalar(requirement, "title");
        if (titleNode == null)
        {
            var location = reqId != null ? $"requirement '{reqId}'" : "requirement";
            issues.Add(new LintIssue(
                $"{path}({requirement.Start.Line},{requirement.Start.Column})",
                LintSeverity.Error,
                $"{location} missing required field 'title'"));
            return;
        }

        if (string.IsNullOrWhiteSpace(titleNode.Value))
        {
            issues.Add(new LintIssue(
                $"{path}({titleNode.Start.Line},{titleNode.Start.Column})",
                LintSeverity.Error,
                "Requirement 'title' cannot be blank"));
        }
    }

    /// <summary>
    ///     Lints a test mapping node.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="mapping">The mapping node to lint.</param>
    private static void LintMapping(
        List<LintIssue> issues,
        string path,
        YamlMappingNode mapping)
    {
        // Check for unknown fields in mapping
        foreach (var key in mapping.Children.Keys.OfType<YamlScalarNode>())
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

        // Check required 'id' field
        var idNode = GetScalar(mapping, "id");
        if (idNode == null)
        {
            issues.Add(new LintIssue(
                $"{path}({mapping.Start.Line},{mapping.Start.Column})",
                LintSeverity.Error,
                "Mapping missing required field 'id'"));
        }
        else if (string.IsNullOrWhiteSpace(idNode.Value))
        {
            issues.Add(new LintIssue(
                $"{path}({idNode.Start.Line},{idNode.Start.Column})",
                LintSeverity.Error,
                "Mapping 'id' cannot be blank"));
        }

        // Check tests list for blank entries
        var tests = GetSequence(mapping, "tests");
        if (tests != null)
        {
            var blankTestStarts = tests.Children
                .OfType<YamlScalarNode>()
                .Where(s => string.IsNullOrWhiteSpace(s.Value))
                .Select(s => s.Start);
            foreach (var start in blankTestStarts)
            {
                issues.Add(new LintIssue(
                    $"{path}({start.Line},{start.Column})",
                    LintSeverity.Error,
                    "Test name cannot be blank in mapping"));
            }
        }
    }

    /// <summary>
    ///     Gets a sequence node from a mapping node by key, adding a type-mismatch issue if the
    ///     key exists but the value is not a sequence.
    /// </summary>
    /// <param name="issues">The list to add lint issues to.</param>
    /// <param name="path">The file path for error messages.</param>
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
    ///     Gets a scalar node value from a mapping node by key.
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
    ///     Gets a sequence node from a mapping node by key.
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
    ///     Gets a list of string values from a sequence node within a mapping.
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
