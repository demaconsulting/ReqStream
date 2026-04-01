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

namespace DemaConsulting.ReqStream.Modeling;

/// <summary>
///     Represents the complete requirements document tree.
/// </summary>
public class Requirements : Section
{
    /// <summary>
    ///     Reads one or more requirements YAML files and returns the parsed Requirements object.
    ///     Throws an exception if any error-level issues are found during loading.
    /// </summary>
    /// <param name="paths">One or more paths to YAML files to read.</param>
    /// <returns>A Requirements object containing the parsed requirements from all files.</returns>
    /// <exception cref="ArgumentException">Thrown when no paths are provided.</exception>
    /// <exception cref="FileNotFoundException">Thrown when a specified file does not exist.</exception>
    /// <exception cref="InvalidOperationException">Thrown when any other error-level issue is found during loading.</exception>
    public static Requirements Read(params string[] paths)
    {
        var (requirements, issues) = Load(paths);
        if (requirements != null)
        {
            return requirements;
        }

        // Throw an exception conveying the first error-level issue
        var firstError = issues.First(i => i.Severity == LintSeverity.Error);

        // Preserve FileNotFoundException semantics for missing-file errors
        if (firstError.Description == "File not found")
        {
            throw new FileNotFoundException(
                $"Requirements file not found: {firstError.Location}",
                firstError.Location);
        }

        throw new InvalidOperationException(firstError.ToString());
    }

    /// <summary>
    ///     Loads one or more requirements YAML files using a single YAML DOM tree walk that
    ///     simultaneously builds the requirements model and collects lint issues.
    /// </summary>
    /// <param name="paths">One or more paths to YAML files to load.</param>
    /// <returns>
    ///     A tuple of the parsed <see cref="Requirements"/> (or <c>null</c> when error-level issues
    ///     are present) and a read-only list of <see cref="LintIssue"/> objects.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when no paths are provided.</exception>
    public static (Requirements? Requirements, IReadOnlyList<LintIssue> Issues) Load(params string[] paths)
    {
        return RequirementsLoader.Load(paths);
    }

    /// <summary>
    ///     Exports the requirements to a Markdown file.
    /// </summary>
    /// <param name="filePath">The path to the output Markdown file.</param>
    /// <param name="depth">The starting depth for Markdown headers (default: 1).</param>
    /// <param name="filterTags">Optional set of tags to filter requirements. If provided, only requirements with matching tags are exported.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null or empty.</exception>
    public void Export(string filePath, int depth = 1, HashSet<string>? filterTags = null)
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
            ExportSection(writer, section, depth, filterTags);
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
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    private static void ExportSection(TextWriter writer, Section section, int depth, HashSet<string>? filterTags)
    {
        // Filter requirements if filter tags are provided
        var filteredRequirements = FilterRequirements(section.Requirements, filterTags);

        // Check if section has any content (filtered requirements or child sections with content)
        if (filteredRequirements.Count == 0 && !HasFilteredContent(section, filterTags))
        {
            return;
        }

        // Write section header
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} {section.Title}");
        writer.WriteLine();

        // If there are requirements, write them as a table
        if (filteredRequirements.Count > 0)
        {
            // Write table header
            writer.WriteLine("| ID | Title |");
            writer.WriteLine("| :- | :---- |");

            // Write each requirement
            foreach (var requirement in filteredRequirements)
            {
                writer.WriteLine($"| {requirement.Id} | {requirement.Title} |");
            }

            writer.WriteLine();
        }

        // Recursively export child sections
        foreach (var childSection in section.Sections)
        {
            ExportSection(writer, childSection, depth + 1, filterTags);
        }
    }

    /// <summary>
    ///     Exports requirements justifications to a Markdown file.
    /// </summary>
    /// <param name="filePath">The path to the output file.</param>
    /// <param name="depth">The starting depth for Markdown headers (default is 1).</param>
    /// <param name="filterTags">Optional set of tags to filter requirements. If provided, only requirements with matching tags are exported.</param>
    /// <exception cref="ArgumentException">Thrown when the file path is null or empty.</exception>
    public void ExportJustifications(string filePath, int depth = 1, HashSet<string>? filterTags = null)
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
            ExportJustificationsSection(writer, section, depth, filterTags);
        }

        // Write the content to the file
        File.WriteAllText(filePath, writer.ToString());
    }

    /// <summary>
    ///     Exports a section's justifications to the markdown writer.
    /// </summary>
    /// <param name="writer">The text writer to write to.</param>
    /// <param name="section">The section to export.</param>
    /// <param name="depth">The current depth for Markdown headers.</param>
    /// <param name="filterTags">Optional set of tags to filter requirements.</param>
    private static void ExportJustificationsSection(TextWriter writer, Section section, int depth, HashSet<string>? filterTags)
    {
        // Filter requirements if filter tags are provided
        var filteredRequirements = FilterRequirements(section.Requirements, filterTags);

        // Check if section has any content (filtered requirements or child sections with content)
        if (filteredRequirements.Count == 0 && !HasFilteredContent(section, filterTags))
        {
            return;
        }

        // Write section header
        var headerPrefix = new string('#', depth);
        writer.WriteLine($"{headerPrefix} {section.Title}");
        writer.WriteLine();

        // Write each requirement with justification
        foreach (var requirement in filteredRequirements)
        {
            // Write requirement ID as a subheader
            var reqHeaderPrefix = new string('#', depth + 1);
            writer.WriteLine($"{reqHeaderPrefix} {requirement.Id}");
            writer.WriteLine();

            // Write requirement title in bold
            writer.WriteLine($"**{requirement.Title}**");
            writer.WriteLine();

            // Write justification if present
            if (!string.IsNullOrWhiteSpace(requirement.Justification))
            {
                writer.WriteLine(requirement.Justification);
                writer.WriteLine();
            }
        }

        // Recursively export child sections
        foreach (var childSection in section.Sections)
        {
            ExportJustificationsSection(writer, childSection, depth + 1, filterTags);
        }
    }

    /// <summary>
    ///     Filters requirements based on tags.
    /// </summary>
    /// <param name="requirements">The list of requirements to filter.</param>
    /// <param name="filterTags">The set of filter tags. If null or empty, all requirements are returned.</param>
    /// <returns>A filtered list of requirements.</returns>
    private static List<Requirement> FilterRequirements(List<Requirement> requirements, HashSet<string>? filterTags)
    {
        // If no filter tags specified, return all requirements
        if (filterTags == null || filterTags.Count == 0)
        {
            return requirements;
        }

        // Return requirements that have at least one matching tag
        return requirements.Where(req => req.Tags.Any(tag => filterTags.Contains(tag))).ToList();
    }

    /// <summary>
    ///     Checks if a section has any filtered content (requirements or child sections with content).
    /// </summary>
    /// <param name="section">The section to check.</param>
    /// <param name="filterTags">The set of filter tags.</param>
    /// <returns>True if the section has filtered content, false otherwise.</returns>
    private static bool HasFilteredContent(Section section, HashSet<string>? filterTags)
    {
        // Check if any child section has filtered content
        foreach (var childSection in section.Sections)
        {
            var filteredRequirements = FilterRequirements(childSection.Requirements, filterTags);
            if (filteredRequirements.Count > 0 || HasFilteredContent(childSection, filterTags))
            {
                return true;
            }
        }

        return false;
    }
}
