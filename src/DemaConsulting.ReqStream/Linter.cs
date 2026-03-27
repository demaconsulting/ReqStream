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

namespace DemaConsulting.ReqStream;

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

        // Track duplicate requirement IDs across all linted files: ID -> source file path
        var seenIds = new Dictionary<string, string>(StringComparer.Ordinal);

        // Track all visited files to avoid linting the same file twice (following includes)
        var visitedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Count total issues
        var issueCount = 0;

        // Lint each file, following includes
        foreach (var file in files)
        {
            issueCount += LintFile(context, file, seenIds, visitedFiles);
        }

        // If no issues found, print success message using first file as root
        if (issueCount == 0)
        {
            context.WriteLine($"{files[0]}: No issues found");
        }
    }

    /// <summary>
    ///     Lints a single requirements file, following includes.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="path">The path to the file to lint.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen and the file they came from.</param>
    /// <param name="visitedFiles">Set of files already visited to avoid re-linting.</param>
    /// <returns>The number of issues found in this file and its includes.</returns>
    private static int LintFile(
        Context context,
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
            context.WriteError($"{path}: error: Invalid file path: {ex.Message}");
            return 1;
        }

        // Skip already-visited files
        if (!visitedFiles.Add(fullPath))
        {
            return 0;
        }

        var issueCount = 0;

        // Verify the file exists
        if (!File.Exists(fullPath))
        {
            context.WriteError($"{path}: error: File not found");
            return 1;
        }

        // Read the file content
        string yaml;
        try
        {
            yaml = File.ReadAllText(fullPath);
        }
        catch (Exception ex)
        {
            context.WriteError($"{path}: error: Failed to read file: {ex.Message}");
            return 1;
        }

        // Parse the YAML into a node tree
        YamlMappingNode? root;
        try
        {
            root = ParseYaml(yaml);
        }
        catch (Exception ex)
        {
            // YamlDotNet may throw YamlException or InvalidOperationException for malformed YAML
            var location = ex is YamlException yamlEx
                ? $"{path}({yamlEx.Start.Line},{yamlEx.Start.Column})"
                : path;
            context.WriteError($"{location}: error: Malformed YAML: {ex.Message}");
            return 1;
        }

        // Empty documents are valid
        if (root == null)
        {
            return 0;
        }

        // Lint document root fields
        issueCount += LintDocumentRoot(context, path, root, seenIds);

        // Follow includes
        var includes = GetStringList(root, "includes");
        if (includes != null)
        {
            var baseDirectory = Path.GetDirectoryName(fullPath) ?? string.Empty;
            foreach (var include in includes)
            {
                if (string.IsNullOrWhiteSpace(include))
                {
                    continue;
                }

                var includePath = Path.Combine(baseDirectory, include);
                issueCount += LintFile(context, includePath, seenIds, visitedFiles);
            }
        }

        return issueCount;
    }

    /// <summary>
    ///     Parses YAML text into a mapping node, or returns null for empty documents.
    /// </summary>
    /// <param name="yaml">The YAML text to parse.</param>
    /// <returns>The root mapping node, or null if the document is empty.</returns>
    /// <exception cref="YamlException">Thrown when the YAML is malformed.</exception>
    private static YamlMappingNode? ParseYaml(string yaml)
    {
        var stream = new YamlStream();
        using var reader = new StringReader(yaml);
        stream.Load(reader);

        if (stream.Documents.Count == 0)
        {
            return null;
        }

        var rootNode = stream.Documents[0].RootNode;
        return rootNode as YamlMappingNode;
    }

    /// <summary>
    ///     Lints the document root mapping node.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="root">The root mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <returns>The number of issues found.</returns>
    private static int LintDocumentRoot(
        Context context,
        string path,
        YamlMappingNode root,
        Dictionary<string, string> seenIds)
    {
        var issueCount = 0;

        // Check for unknown fields at document root
        foreach (var key in root.Children.Keys.OfType<YamlScalarNode>())
        {
            var keyValue = key.Value ?? string.Empty;
            if (!KnownDocumentFields.Contains(keyValue))
            {
                context.WriteError(
                    $"{path}({key.Start.Line},{key.Start.Column}): error: Unknown field '{keyValue}'");
                issueCount++;
            }
        }

        // Lint sections
        var sections = GetSequence(root, "sections");
        if (sections != null)
        {
            foreach (var sectionNode in sections.Children)
            {
                if (sectionNode is YamlMappingNode sectionMapping)
                {
                    issueCount += LintSection(context, path, sectionMapping, seenIds);
                }
                else
                {
                    context.WriteError(
                        $"{path}({sectionNode.Start.Line},{sectionNode.Start.Column}): error: Section must be a mapping");
                    issueCount++;
                }
            }
        }

        // Lint mappings
        var mappings = GetSequence(root, "mappings");
        if (mappings != null)
        {
            foreach (var mappingNode in mappings.Children)
            {
                if (mappingNode is YamlMappingNode mappingMapping)
                {
                    issueCount += LintMapping(context, path, mappingMapping);
                }
                else
                {
                    context.WriteError(
                        $"{path}({mappingNode.Start.Line},{mappingNode.Start.Column}): error: Mapping must be a mapping node");
                    issueCount++;
                }
            }
        }

        return issueCount;
    }

    /// <summary>
    ///     Lints a section mapping node.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="section">The section mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <returns>The number of issues found.</returns>
    private static int LintSection(
        Context context,
        string path,
        YamlMappingNode section,
        Dictionary<string, string> seenIds)
    {
        var issueCount = 0;

        // Check for unknown fields in section
        foreach (var key in section.Children.Keys.OfType<YamlScalarNode>())
        {
            var keyValue = key.Value ?? string.Empty;
            if (!KnownSectionFields.Contains(keyValue))
            {
                context.WriteError(
                    $"{path}({key.Start.Line},{key.Start.Column}): error: Unknown field '{keyValue}' in section");
                issueCount++;
            }
        }

        // Check required 'title' field
        var titleNode = GetScalar(section, "title");
        if (titleNode == null)
        {
            context.WriteError(
                $"{path}({section.Start.Line},{section.Start.Column}): error: Section missing required field 'title'");
            issueCount++;
        }
        else if (string.IsNullOrWhiteSpace(titleNode.Value))
        {
            context.WriteError(
                $"{path}({titleNode.Start.Line},{titleNode.Start.Column}): error: Section 'title' cannot be blank");
            issueCount++;
        }

        // Lint requirements
        var requirements = GetSequence(section, "requirements");
        if (requirements != null)
        {
            foreach (var reqNode in requirements.Children)
            {
                if (reqNode is YamlMappingNode reqMapping)
                {
                    issueCount += LintRequirement(context, path, reqMapping, seenIds);
                }
                else
                {
                    context.WriteError(
                        $"{path}({reqNode.Start.Line},{reqNode.Start.Column}): error: Requirement must be a mapping");
                    issueCount++;
                }
            }
        }

        // Lint child sections
        var sections = GetSequence(section, "sections");
        if (sections != null)
        {
            foreach (var childNode in sections.Children)
            {
                if (childNode is YamlMappingNode childMapping)
                {
                    issueCount += LintSection(context, path, childMapping, seenIds);
                }
                else
                {
                    context.WriteError(
                        $"{path}({childNode.Start.Line},{childNode.Start.Column}): error: Section must be a mapping");
                    issueCount++;
                }
            }
        }

        return issueCount;
    }

    /// <summary>
    ///     Lints a requirement mapping node.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="requirement">The requirement mapping node.</param>
    /// <param name="seenIds">Dictionary of requirement IDs already seen.</param>
    /// <returns>The number of issues found.</returns>
    private static int LintRequirement(
        Context context,
        string path,
        YamlMappingNode requirement,
        Dictionary<string, string> seenIds)
    {
        var issueCount = 0;

        // Check for unknown fields in requirement
        foreach (var key in requirement.Children.Keys.OfType<YamlScalarNode>())
        {
            var keyValue = key.Value ?? string.Empty;
            if (!KnownRequirementFields.Contains(keyValue))
            {
                context.WriteError(
                    $"{path}({key.Start.Line},{key.Start.Column}): error: Unknown field '{keyValue}' in requirement");
                issueCount++;
            }
        }

        // Check required 'id' field
        var idNode = GetScalar(requirement, "id");
        string? reqId = null;
        if (idNode == null)
        {
            context.WriteError(
                $"{path}({requirement.Start.Line},{requirement.Start.Column}): error: Requirement missing required field 'id'");
            issueCount++;
        }
        else if (string.IsNullOrWhiteSpace(idNode.Value))
        {
            context.WriteError(
                $"{path}({idNode.Start.Line},{idNode.Start.Column}): error: Requirement 'id' cannot be blank");
            issueCount++;
        }
        else
        {
            reqId = idNode.Value;

            // Check for duplicate IDs
            if (seenIds.TryGetValue(reqId, out var firstFile))
            {
                context.WriteError(
                    $"{path}({idNode.Start.Line},{idNode.Start.Column}): error: Duplicate requirement ID '{reqId}' (first seen in {firstFile})");
                issueCount++;
            }
            else
            {
                seenIds[reqId] = path;
            }
        }

        // Check required 'title' field
        var titleNode = GetScalar(requirement, "title");
        if (titleNode == null)
        {
            var location = reqId != null ? $"requirement '{reqId}'" : "requirement";
            context.WriteError(
                $"{path}({requirement.Start.Line},{requirement.Start.Column}): error: {location} missing required field 'title'");
            issueCount++;
        }
        else if (string.IsNullOrWhiteSpace(titleNode.Value))
        {
            context.WriteError(
                $"{path}({titleNode.Start.Line},{titleNode.Start.Column}): error: Requirement 'title' cannot be blank");
            issueCount++;
        }

        // Check tests list for blank entries
        var tests = GetSequence(requirement, "tests");
        if (tests != null)
        {
            foreach (var testNode in tests.Children)
            {
                if (testNode is YamlScalarNode testScalar && string.IsNullOrWhiteSpace(testScalar.Value))
                {
                    context.WriteError(
                        $"{path}({testNode.Start.Line},{testNode.Start.Column}): error: Test name cannot be blank");
                    issueCount++;
                }
            }
        }

        // Check tags list for blank entries
        var tags = GetSequence(requirement, "tags");
        if (tags != null)
        {
            foreach (var tagNode in tags.Children)
            {
                if (tagNode is YamlScalarNode tagScalar && string.IsNullOrWhiteSpace(tagScalar.Value))
                {
                    context.WriteError(
                        $"{path}({tagNode.Start.Line},{tagNode.Start.Column}): error: Tag name cannot be blank");
                    issueCount++;
                }
            }
        }

        return issueCount;
    }

    /// <summary>
    ///     Lints a test mapping node.
    /// </summary>
    /// <param name="context">The context for output.</param>
    /// <param name="path">The file path for error messages.</param>
    /// <param name="mapping">The mapping node to lint.</param>
    /// <returns>The number of issues found.</returns>
    private static int LintMapping(
        Context context,
        string path,
        YamlMappingNode mapping)
    {
        var issueCount = 0;

        // Check for unknown fields in mapping
        foreach (var key in mapping.Children.Keys.OfType<YamlScalarNode>())
        {
            var keyValue = key.Value ?? string.Empty;
            if (!KnownMappingFields.Contains(keyValue))
            {
                context.WriteError(
                    $"{path}({key.Start.Line},{key.Start.Column}): error: Unknown field '{keyValue}' in mapping");
                issueCount++;
            }
        }

        // Check required 'id' field
        var idNode = GetScalar(mapping, "id");
        if (idNode == null)
        {
            context.WriteError(
                $"{path}({mapping.Start.Line},{mapping.Start.Column}): error: Mapping missing required field 'id'");
            issueCount++;
        }
        else if (string.IsNullOrWhiteSpace(idNode.Value))
        {
            context.WriteError(
                $"{path}({idNode.Start.Line},{idNode.Start.Column}): error: Mapping 'id' cannot be blank");
            issueCount++;
        }

        // Check tests list for blank entries
        var tests = GetSequence(mapping, "tests");
        if (tests != null)
        {
            foreach (var testNode in tests.Children)
            {
                if (testNode is YamlScalarNode testScalar && string.IsNullOrWhiteSpace(testScalar.Value))
                {
                    context.WriteError(
                        $"{path}({testNode.Start.Line},{testNode.Start.Column}): error: Test name cannot be blank in mapping");
                    issueCount++;
                }
            }
        }

        return issueCount;
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

        var result = new List<string>();
        foreach (var node in sequence.Children)
        {
            if (node is YamlScalarNode scalar && scalar.Value != null)
            {
                result.Add(scalar.Value);
            }
        }

        return result;
    }
}
