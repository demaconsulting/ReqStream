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

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DemaConsulting.ReqStream;

/// <summary>
/// Represents the complete requirements document tree.
/// </summary>
public class Requirements : Section
{
    private readonly HashSet<string> _includedFiles = new();
    private readonly Dictionary<string, Requirement> _allRequirements = new();

    /// <summary>
    /// Reads a requirements YAML file and returns the parsed Requirements object.
    /// </summary>
    /// <param name="path">The path to the YAML file to read.</param>
    /// <returns>A Requirements object containing the parsed requirements.</returns>
    /// <exception cref="FileNotFoundException">Thrown when the specified file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when duplicate requirement IDs are found.</exception>
    public static Requirements Read(string path)
    {
        var requirements = new Requirements();
        requirements.ReadFile(path);
        return requirements;
    }

    /// <summary>
    /// Reads and processes a YAML file, including any referenced include files.
    /// </summary>
    /// <param name="path">The path to the YAML file to read.</param>
    private void ReadFile(string path)
    {
        var fullPath = Path.GetFullPath(path);

        if (_includedFiles.Contains(fullPath))
        {
            return;
        }

        _includedFiles.Add(fullPath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException($"Requirements file not found: {path}", path);
        }

        var yaml = File.ReadAllText(fullPath);
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .Build();

        var document = deserializer.Deserialize<YamlDocument>(yaml);
        if (document == null)
        {
            return;
        }

        var baseDirectory = Path.GetDirectoryName(fullPath) ?? string.Empty;

        if (document.Sections != null)
        {
            foreach (var section in document.Sections)
            {
                MergeSection(this, section);
            }
        }

        if (document.Mappings != null)
        {
            foreach (var mapping in document.Mappings)
            {
                if (_allRequirements.TryGetValue(mapping.Id, out var requirement))
                {
                    if (mapping.Tests != null)
                    {
                        requirement.Tests.AddRange(mapping.Tests);
                    }
                }
            }
        }

        if (document.Includes != null)
        {
            foreach (var include in document.Includes)
            {
                var includePath = Path.Combine(baseDirectory, include);
                ReadFile(includePath);
            }
        }
    }

    /// <summary>
    /// Merges a YAML section into the target section.
    /// </summary>
    /// <param name="target">The target section to merge into.</param>
    /// <param name="source">The source YAML section to merge from.</param>
    private void MergeSection(Section target, YamlSection source)
    {
        var existingSection = target.Sections.FirstOrDefault(s => s.Title == source.Title);
        if (existingSection == null)
        {
            existingSection = new Section { Title = source.Title };
            target.Sections.Add(existingSection);
        }

        if (source.Requirements != null)
        {
            foreach (var req in source.Requirements)
            {
                var requirement = new Requirement
                {
                    Id = req.Id,
                    Title = req.Title
                };

                if (req.Tests != null)
                {
                    requirement.Tests.AddRange(req.Tests);
                }

                if (req.Children != null)
                {
                    requirement.Children.AddRange(req.Children);
                }

                if (_allRequirements.ContainsKey(requirement.Id))
                {
                    throw new InvalidOperationException($"Duplicate requirement ID found: {requirement.Id}");
                }

                _allRequirements[requirement.Id] = requirement;
                existingSection.Requirements.Add(requirement);
            }
        }

        if (source.Sections != null)
        {
            foreach (var childSection in source.Sections)
            {
                MergeSection(existingSection, childSection);
            }
        }
    }

    /// <summary>
    /// Internal class for deserializing the YAML document structure.
    /// </summary>
    private class YamlDocument
    {
        /// <summary>
        /// Gets or sets the sections in the document.
        /// </summary>
        public List<YamlSection>? Sections { get; set; }

        /// <summary>
        /// Gets or sets the test mappings in the document.
        /// </summary>
        public List<YamlMapping>? Mappings { get; set; }

        /// <summary>
        /// Gets or sets the list of include files.
        /// </summary>
        public List<string>? Includes { get; set; }
    }

    /// <summary>
    /// Internal class for deserializing a YAML section.
    /// </summary>
    private class YamlSection
    {
        /// <summary>
        /// Gets or sets the title of the section.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the requirements in this section.
        /// </summary>
        public List<YamlRequirement>? Requirements { get; set; }

        /// <summary>
        /// Gets or sets the child sections.
        /// </summary>
        public List<YamlSection>? Sections { get; set; }
    }

    /// <summary>
    /// Internal class for deserializing a YAML requirement.
    /// </summary>
    private class YamlRequirement
    {
        /// <summary>
        /// Gets or sets the requirement ID.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the requirement title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of tests.
        /// </summary>
        public List<string>? Tests { get; set; }

        /// <summary>
        /// Gets or sets the list of child requirement IDs.
        /// </summary>
        public List<string>? Children { get; set; }
    }

    /// <summary>
    /// Internal class for deserializing a YAML test mapping.
    /// </summary>
    private class YamlMapping
    {
        /// <summary>
        /// Gets or sets the requirement ID for this mapping.
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the list of tests.
        /// </summary>
        public List<string>? Tests { get; set; }
    }
}
