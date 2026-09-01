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

namespace DemaConsulting.ReqStream.Modeling;

/// <summary>
///     Represents the complete requirements document tree.
/// </summary>
/// <remarks>
///     <para>
///         <b>Why extends Section:</b> <c>Requirements</c> is the root of the requirements
///         section tree. Inheriting from <see cref="Section"/> reuses the container tree
///         (title, requirements list, child sections) without duplication. The root node is
///         identical in structure to any other section node; only its role is different.
///     </para>
///     <para>
///         <b>Public API surface:</b> <c>Requirements</c> is the Modeling subsystem's public
///         API entry point. It exposes <see cref="Load"/>, <see cref="Export"/>, and
///         <see cref="ExportJustifications"/> to callers while hiding
///         <see cref="RequirementsLoader"/> entirely.
///     </para>
/// </remarks>
public class Requirements : Section
{
    /// <summary>
    ///     Gets the set of tags that mark a requirement as a "root" for orphan detection.
    /// </summary>
    /// <remarks>
    ///     Pre-initialized to an empty set (never <c>null</c>). Populated by
    ///     <see cref="RequirementsLoader"/> from each loaded document's <c>root-tags:</c> key,
    ///     combining values declared across every included file - no single file's declaration
    ///     overwrites another's.
    /// </remarks>
    public HashSet<string> RootTags { get; } = new(StringComparer.Ordinal);

    /// <summary>
    ///     Provides the single public entry point for loading YAML requirements files,
    ///     insulating callers from the loader and lint pipeline.
    /// </summary>
    /// <param name="paths">One or more paths to YAML files to load.</param>
    /// <returns>
    ///     A <see cref="LoadResult"/> containing the parsed <see cref="Requirements"/> (or <c>null</c>
    ///     when error-level issues are present) and all lint issues found during loading.
    /// </returns>
    /// <exception cref="ArgumentException">Thrown when no paths are provided.</exception>
    /// <remarks>
    ///     Delegates to <see cref="RequirementsLoader.Load"/> which performs a single YAML DOM
    ///     tree walk that simultaneously builds the requirements model and collects lint issues.
    ///     Returns <c>null</c> requirements when any error-level issue is found, allowing callers
    ///     to detect failure without exception handling.
    /// </remarks>
    public static LoadResult Load(params string[] paths)
    {
        return RequirementsLoader.Load(paths);
    }

    /// <summary>
    ///     Exports the requirements to a Markdown file.
    /// </summary>
    /// <param name="filePath">The path to the output Markdown file.</param>
    /// <param name="depth">The starting depth for Markdown headers (default: 1).</param>
    /// <param name="filterTags">Optional set of tags to filter requirements. If provided, only requirements with matching tags are exported.</param>
    /// <exception cref="ArgumentException">Thrown when filePath is null, empty, or whitespace-only.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when depth is less than 1.</exception>
    /// <remarks>
    ///     <b>File-write side effect:</b> Overwrites any existing file at <paramref name="filePath"/>.
    ///     Any <see cref="IOException"/> or <see cref="UnauthorizedAccessException"/> from the
    ///     underlying file-write operations propagates to the caller (<c>Program</c>) without
    ///     wrapping; this method does not catch or suppress I/O exceptions.
    /// </remarks>
    public void Export(string filePath, int depth = 1, HashSet<string>? filterTags = null)
    {
        // Validate file path
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        // Validate depth
        if (depth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), depth, "Depth must be at least 1");
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
    /// <remarks>
    ///     Extracted as a recursive entry point so <see cref="Export"/> remains non-recursive
    ///     while the tree walk handles depth tracking. Each recursive call increments
    ///     <paramref name="depth"/> by one, producing progressively deeper ATX headings.
    /// </remarks>
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
    /// <exception cref="ArgumentException">Thrown when the file path is null, empty, or whitespace-only.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when depth is less than 1.</exception>
    /// <remarks>
    ///     <b>File-write side effect:</b> Overwrites any existing file at <paramref name="filePath"/>.
    ///     Any <see cref="IOException"/> or <see cref="UnauthorizedAccessException"/> from the
    ///     underlying file-write operations propagates to the caller (<c>Program</c>) without
    ///     wrapping; this method does not catch or suppress I/O exceptions.
    /// </remarks>
    public void ExportJustifications(string filePath, int depth = 1, HashSet<string>? filterTags = null)
    {
        // Validate file path
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path cannot be null or empty", nameof(filePath));
        }

        // Validate depth
        if (depth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), depth, "Depth must be at least 1");
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
    /// <remarks>
    ///     Extracted to mirror the structure of <see cref="ExportSection"/>, keeping
    ///     justification export parallel to standard report export. Applying the same
    ///     recursive depth-tracking pattern ensures both export paths remain structurally
    ///     consistent and independently maintainable.
    /// </remarks>
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
    ///     Finds requirements that are not reachable, via <see cref="Requirement.Children"/>
    ///     references, from any requirement tagged with one of the given root tags.
    /// </summary>
    /// <param name="rootTags">
    ///     The effective (merged) set of root tags. When empty, no requirement can ever be
    ///     orphaned and this method returns an empty result immediately (backward-compatible
    ///     no-op path).
    /// </param>
    /// <returns>
    ///     An <see cref="OrphanResult"/> containing the orphaned requirement ids (in tree
    ///     declaration order) and the total number of requirements considered.
    /// </returns>
    /// <remarks>
    ///     <para>
    ///         <b>Why downward flood-fill, not upward ancestor-walk:</b> requirements form a DAG
    ///         (a requirement's <see cref="Requirement.Children"/> may be referenced from
    ///         multiple parents), not a tree. Walking upward would require a reverse-children
    ///         (parent) index that does not otherwise exist in this model. Instead, this method
    ///         seeds a breadth-first search from every root-tagged requirement and floods
    ///         downward through <see cref="Requirement.Children"/>, using a visited set to
    ///         guarantee each requirement is processed at most once - O(V+E) regardless of how
    ///         many parents reference a given child.
    ///     </para>
    ///     <para>
    ///         <b>Side-effect-free:</b> this method never mutates the requirements tree; it only
    ///         reads <see cref="Requirement.Tags"/> and <see cref="Requirement.Children"/>.
    ///     </para>
    /// </remarks>
    public OrphanResult FindOrphans(IReadOnlySet<string> rootTags)
    {
        // Backward-compatible no-op: with no root tags configured, nothing can be orphaned
        if (rootTags.Count == 0)
        {
            return new OrphanResult([], 0);
        }

        // Flatten the full tree into declaration order, keyed by id for child-reference lookup
        var flattened = new List<Requirement>();
        FlattenRequirements(this, flattened);
        var byId = flattened.ToDictionary(r => r.Id, StringComparer.Ordinal);

        // Seed the visited set and BFS queue with every root-tagged requirement
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var queue = new Queue<string>();
        foreach (var requirement in flattened.Where(r => r.Tags.Any(rootTags.Contains) && visited.Add(r.Id)))
        {
            queue.Enqueue(requirement.Id);
        }

        // BFS-flood downward through child references, guarded by the visited set
        while (queue.Count > 0)
        {
            var id = queue.Dequeue();
            if (!byId.TryGetValue(id, out var requirement))
            {
                continue;
            }

            foreach (var childId in requirement.Children.Where(visited.Add))
            {
                queue.Enqueue(childId);
            }
        }

        // Orphans are every flattened requirement not reached, preserving declaration order
        var orphanIds = flattened
            .Where(r => !visited.Contains(r.Id))
            .Select(r => r.Id)
            .ToList();

        return new OrphanResult(orphanIds, flattened.Count);
    }

    /// <summary>
    ///     Recursively flattens a section's requirements (and its child sections' requirements)
    ///     into a single ordered list.
    /// </summary>
    /// <remarks>
    ///     Extracted so <see cref="FindOrphans"/> can build a single declaration-ordered,
    ///     id-lookup-ready view of the whole tree without duplicating the recursive walk.
    /// </remarks>
    /// <param name="section">The section to flatten.</param>
    /// <param name="result">The list to append flattened requirements to.</param>
    private static void FlattenRequirements(Section section, List<Requirement> result)
    {
        result.AddRange(section.Requirements);
        foreach (var childSection in section.Sections)
        {
            FlattenRequirements(childSection, result);
        }
    }

    /// <summary>
    ///     Filters requirements based on tags.
    /// </summary>
    /// <remarks>
    ///     Extracted to encapsulate the tag-filter predicate so both <see cref="ExportSection"/>
    ///     and <see cref="ExportJustificationsSection"/> apply identical filtering logic without
    ///     duplication. A single definition ensures that both export paths diverge only in how
    ///     they render matched requirements, not in how they select them.
    /// </remarks>
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
    /// <remarks>
    ///     Extracted to allow the filter-presence check to recurse through child sections without
    ///     duplicating the predicate. Both <see cref="ExportSection"/> and
    ///     <see cref="ExportJustificationsSection"/> call this method to decide whether an
    ///     otherwise-empty section heading should be suppressed.
    /// </remarks>
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

/// <summary>
///     Represents the result of an orphan-detection scan via <see cref="Requirements.FindOrphans"/>.
/// </summary>
/// <param name="OrphanIds">
///     The ids of every requirement not reachable from any root-tagged requirement, in tree
///     declaration order.
/// </param>
/// <param name="TotalRequirements">The total number of requirements considered by the scan.</param>
public sealed record OrphanResult(IReadOnlyList<string> OrphanIds, int TotalRequirements);
