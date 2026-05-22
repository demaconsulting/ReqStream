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
///     Represents a section containing requirements and/or child sections.
/// </summary>
/// <remarks>
///     <para>
///         <b>YamlDotNet deserialization — two mechanisms:</b>
///         <see cref="Title"/> has a public setter and is populated directly by YamlDotNet
///         during DOM traversal. <see cref="Requirements"/> and <see cref="Sections"/> have
///         no setter; YamlDotNet populates them by calling <c>.Add()</c> on the
///         pre-initialized empty lists, so no setter is required for collection properties.
///     </para>
///     <para>
///         <b>Why merging logic is excluded:</b> Section is a pure data container. All merging
///         logic resides in <see cref="RequirementsLoader"/> to maintain a clean separation of
///         concerns; merging is a loader responsibility, not a data-model responsibility.
///     </para>
///     <para>
///         <b>Thread safety:</b> Not thread-safe. No concurrent access is expected; loading is
///         single-threaded and completes before the model is consumed by callers.
///     </para>
/// </remarks>
public class Section
{
    /// <summary>
    ///     Gets or sets the title of this section.
    /// </summary>
    /// <remarks>
    ///     Serves as the merge identity key used by <see cref="RequirementsLoader"/> to locate
    ///     existing sections when merging content from multiple YAML files.
    /// </remarks>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets the list of requirements in this section.
    /// </summary>
    /// <remarks>
    ///     Pre-initialized to an empty list. YamlDotNet populates this collection during
    ///     deserialization by calling <c>.Add()</c> on the list; no setter is required.
    /// </remarks>
    public List<Requirement> Requirements { get; } = [];

    /// <summary>
    ///     Gets the list of child sections.
    /// </summary>
    /// <remarks>
    ///     Pre-initialized to an empty list. YamlDotNet populates this collection during
    ///     deserialization by calling <c>.Add()</c> on the list; no setter is required.
    /// </remarks>
    public List<Section> Sections { get; } = [];
}
