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

using System.Diagnostics.CodeAnalysis;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DemaConsulting.ReqStream;

/// <summary>
///     Represents the complete requirements document tree.
/// </summary>
public class Requirements : Section
{
    /// <summary>
    ///     Set of files that have already been included to prevent infinite loops.
    /// </summary>
    private readonly HashSet<string> _includedFiles = [];

    /// <summary>
    ///     Dictionary mapping requirement IDs to their Requirement objects for duplicate detection.
    /// </summary>
    private readonly Dictionary<string, Requirement> _allRequirements = [];

    /// <summary>
    ///     Reads one or more requirements YAML files and returns the parsed Requirements object.
    /// </summary>
    /// <param name="paths">One or more paths to YAML files to read.</param>
    /// <returns>A Requirements object containing the parsed requirements from all files.</returns>
    /// <exception cref="ArgumentException">Thrown when no paths are provided.</exception>
    /// <exception cref="FileNotFoundException">Thrown when a specified file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when duplicate requirement IDs are found or validation fails.</exception>
    public static Requirements Read(params string[] paths)
    {
        // Validate that at least one path is provided
        if (paths == null || paths.Length == 0)
        {
            throw new ArgumentException("At least one file path must be provided", nameof(paths));
        }

        // Create a new Requirements instance to hold the parsed data
        var requirements = new Requirements();

        // Read and process each file and any includes
        foreach (var path in paths)
        {
            requirements.ReadFile(path);
        }

        // Return the fully populated requirements tree
        return requirements;
    }

    /// <summary>
    ///     Exports the requirements to a Markdown file.
    /// </summary>
    /// <param name="filePath">The path to the output Markdown file.</param>
    /// <param name="depth">The starting depth for Markdown headers (default: 1).</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    public void Export(string filePath, int depth = 1)
    {
        // Validate file path
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        // Create a string builder to build the markdown content
        using var writer = new StringWriter();

        // Export all sections
        foreach (var section in Sections)
        {
            ExportSection(writer, section, depth);
        }

        // Write the content to the file
        File.WriteAllText(filePath, writer.ToString());
    }

    /// <summary>
    ///     Exports a section to the markdown writer.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="section">The section to export.</param>
    /// <param name="depth">The current depth for Markdown headers.</param>
    private static void ExportSection(TextWriter writer, Section section, int depth)
    {
        // Write section header
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} {section.Title}");
        writer.WriteLine();

        // If there are requirements, write them as a table
        if (section.Requirements.Count > 0)
        {
            // Write table header
            writer.WriteLine("| ID | Title |");
            writer.WriteLine("| :- | :---- |");

            // Write each requirement
            foreach (var requirement in section.Requirements)
            {
                writer.WriteLine($"| {requirement.Id} | {requirement.Title} |");
            }

            writer.WriteLine();
        }

        // Recursively export child sections
        foreach (var childSection in section.Sections)
        {
            ExportSection(writer, childSection, depth + 1);
        }
    }

    /// <summary>
    ///     Reads and processes a YAML file, including any referenced include files.
    /// </summary>
    /// <param name="path">The path to the YAML file to read.</param>
    private void ReadFile(string path)
    {
        // Convert to full path and check if already included to prevent loops
        var fullPath = Path.GetFullPath(path);

        // Skip if this file has already been included
        if (_includedFiles.Contains(fullPath))
        {
            return;
        }

        // Mark this file as included
        _includedFiles.Add(fullPath);

        // Verify the file exists before attempting to read
        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Requirements file not found: {path}", path);
        }

        // Read the entire YAML file as text
        var yaml = File.ReadAllText(fullPath);

        // Create a deserializer configured for hyphenated property names
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();

        // Deserialize the YAML into our document structure
        var document = deserializer.Deserialize<YamlDocument>(yaml);

        // Handle empty or null documents
        if (document == null)
        {
            return;
        }

        // Get the base directory for resolving relative include paths
        var baseDirectory = Path.GetDirectoryName(fullPath) ?? string.Empty;

        // Merge all sections from the document into the requirements tree
        if (document.Sections != null)
        {
            // Process each top-level section in the document
            foreach (var section in document.Sections)
            {
                MergeSection(this, section, fullPath);
            }
        }

        // Apply test mappings to existing requirements
        if (document.Mappings != null)
        {
            // Process each mapping to add tests to requirements
            foreach (var mapping in document.Mappings)
            {
                // Validate mapping ID is not blank
                if (string.IsNullOrWhiteSpace(mapping.Id))
                {
                    throw new InvalidOperationException($"Mapping requirement ID cannot be blank in file: {fullPath}");
                }

                // Find the requirement by ID and add tests if they exist
                if (_allRequirements.TryGetValue(mapping.Id, out var requirement) && mapping.Tests != null)
                {
                    // Validate no test names are blank
                    if (mapping.Tests.Any(string.IsNullOrWhiteSpace))
                    {
                        throw new InvalidOperationException(
                            $"Test name cannot be blank in mapping for requirement '{mapping.Id}' in file: {fullPath}");
                    }

                    requirement.Tests.AddRange(mapping.Tests);
                }
            }
        }

        // Recursively process any included files
        if (document.Includes != null)
        {
            // Process each included file by resolving paths and recursively reading
            foreach (var includePath in document.Includes.Select(include => Path.Combine(baseDirectory, include)))
            {
                ReadFile(includePath);
            }
        }
    }

    /// <summary>
    ///     Merges a YAML section into the target section.
    /// </summary>
    /// <param name="target">The target section to merge into.</param>
    /// <param name="source">The source YAML section to merge from.</param>
    /// <param name="filePath">The path to the file being processed for error messages.</param>
    private void MergeSection(Section target, YamlSection source, string filePath)
    {
        // Validate section title is not blank
        if (string.IsNullOrWhiteSpace(source.Title))
        {
            throw new InvalidOperationException($"Section title cannot be blank in file: {filePath}");
        }

        // Find or create the section with matching title
        var existingSection = target.Sections.FirstOrDefault(s => s.Title == source.Title);

        // Create the section if it doesn't exist
        if (existingSection == null)
        {
            existingSection = new Section { Title = source.Title };
            target.Sections.Add(existingSection);
        }

        // Add all requirements from the source section
        if (source.Requirements != null)
        {
            // Process each requirement in the source section
            foreach (var req in source.Requirements)
            {
                var requirement = CreateAndValidateRequirement(req, source.Title, filePath);
                existingSection.Requirements.Add(requirement);
            }
        }

        // Recursively merge any child sections
        if (source.Sections != null)
        {
            // Process each child section recursively
            foreach (var childSection in source.Sections)
            {
                MergeSection(existingSection, childSection, filePath);
            }
        }
    }

    /// <summary>
    ///     Creates and validates a requirement from YAML data.
    /// </summary>
    /// <param name="req">The YAML requirement to process.</param>
    /// <param name="sectionTitle">The title of the section containing this requirement.</param>
    /// <param name="filePath">The path to the file being processed for error messages.</param>
    /// <returns>A validated Requirement object.</returns>
    /// <exception cref="InvalidOperationException">Thrown when validation fails.</exception>
    private Requirement CreateAndValidateRequirement(YamlRequirement req, string sectionTitle, string filePath)
    {
        // Validate requirement ID is not blank
        if (string.IsNullOrWhiteSpace(req.Id))
        {
            throw new InvalidOperationException(
                $"Requirement ID cannot be blank in section '{sectionTitle}' in file: {filePath}");
        }

        // Validate requirement title is not blank
        if (string.IsNullOrWhiteSpace(req.Title))
        {
            throw new InvalidOperationException(
                $"Requirement title cannot be blank for ID '{req.Id}' in file: {filePath}");
        }

        // Create the requirement with its basic properties
        var requirement = new Requirement
        {
            Id = req.Id,
            Title = req.Title
        };

        // Add any inline tests
        if (req.Tests != null)
        {
            // Validate no test names are blank
            if (req.Tests.Any(string.IsNullOrWhiteSpace))
            {
                throw new InvalidOperationException(
                    $"Test name cannot be blank for requirement '{req.Id}' in file: {filePath}");
            }

            requirement.Tests.AddRange(req.Tests);
        }

        // Add any child requirement references
        if (req.Children != null)
        {
            requirement.Children.AddRange(req.Children);
        }

        // Check for duplicate requirement IDs and register the requirement
        if (!_allRequirements.TryAdd(requirement.Id, requirement))
        {
            throw new InvalidOperationException(
                $"Duplicate requirement ID found: '{requirement.Id}' in file: {filePath}");
        }

        return requirement;
    }

    /// <summary>
    ///     Internal class for deserializing the YAML document structure.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    [SuppressMessage("SonarAnalyzer.CSharp", "S3459:Unassigned members should be removed", Justification = "Properties are set by YamlDotNet deserializer via reflection")]
    [SuppressMessage("SonarAnalyzer.CSharp", "S1144:Unused private types or members should be removed", Justification = "Properties are accessed by YamlDotNet deserializer via reflection")]
    private sealed class YamlDocument
    {
        /// <summary>
        ///     Gets or sets the sections in the document.
        /// </summary>
        public List<YamlSection>? Sections { get; set; }

        /// <summary>
        ///     Gets or sets the test mappings in the document.
        /// </summary>
        public List<YamlMapping>? Mappings { get; set; }

        /// <summary>
        ///     Gets or sets the list of include files.
        /// </summary>
        public List<string>? Includes { get; set; }
    }

    /// <summary>
    ///     Internal class for deserializing a YAML section.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    [SuppressMessage("SonarAnalyzer.CSharp", "S3459:Unassigned members should be removed", Justification = "Properties are set by YamlDotNet deserializer via reflection")]
    [SuppressMessage("SonarAnalyzer.CSharp", "S1144:Unused private types or members should be removed", Justification = "Properties are accessed by YamlDotNet deserializer via reflection")]
    private sealed class YamlSection
    {
        /// <summary>
        ///     Gets or sets the title of the section.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the requirements in this section.
        /// </summary>
        public List<YamlRequirement>? Requirements { get; set; }

        /// <summary>
        ///     Gets or sets the child sections.
        /// </summary>
        public List<YamlSection>? Sections { get; set; }
    }

    /// <summary>
    ///     Internal class for deserializing a YAML requirement.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    [SuppressMessage("SonarAnalyzer.CSharp", "S3459:Unassigned members should be removed", Justification = "Properties are set by YamlDotNet deserializer via reflection")]
    [SuppressMessage("SonarAnalyzer.CSharp", "S1144:Unused private types or members should be removed", Justification = "Properties are accessed by YamlDotNet deserializer via reflection")]
    private sealed class YamlRequirement
    {
        /// <summary>
        ///     Gets or sets the requirement ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the requirement title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the list of tests.
        /// </summary>
        public List<string>? Tests { get; set; }

        /// <summary>
        ///     Gets or sets the list of child requirement IDs.
        /// </summary>
        public List<string>? Children { get; set; }
    }

    /// <summary>
    ///     Internal class for deserializing a YAML test mapping.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties)]
    [SuppressMessage("SonarAnalyzer.CSharp", "S3459:Unassigned members should be removed", Justification = "Properties are set by YamlDotNet deserializer via reflection")]
    [SuppressMessage("SonarAnalyzer.CSharp", "S1144:Unused private types or members should be removed", Justification = "Properties are accessed by YamlDotNet deserializer via reflection")]
    private sealed class YamlMapping
    {
        /// <summary>
        ///     Gets or sets the requirement ID for this mapping.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        ///     Gets or sets the list of tests.
        /// </summary>
        public List<string>? Tests { get; set; }
    }
}
