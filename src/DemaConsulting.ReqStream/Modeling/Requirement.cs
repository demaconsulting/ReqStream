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
///     Represents a single requirement with its metadata.
/// </summary>
/// <remarks>
///     <para>
///         <b>Why mutable DTO:</b> YAML deserialization via <see cref="RequirementsLoader"/>
///         requires mutable properties; <c>RequirementsLoader</c> builds objects incrementally
///         during DOM traversal, setting fields as they are encountered in the YAML node tree.
///     </para>
///     <para>
///         <b>Validation delegation:</b> All validation (blank <c>Id</c>, blank <c>Title</c>,
///         duplicate <c>Id</c>, non-scalar list entries) is delegated entirely to
///         <see cref="RequirementsLoader"/>. <c>Requirement</c> itself never throws.
///     </para>
///     <para>
///         <b>Thread safety:</b> Not thread-safe. No concurrent access is expected; loading is
///         single-threaded and completes before the model is consumed by callers.
///     </para>
///     <para>
///         <b>List initialization:</b> All list properties (<see cref="Tests"/>,
///         <see cref="Children"/>, <see cref="Tags"/>) are always initialized to empty
///         <see cref="List{T}"/> instances; callers can iterate without null checks.
///     </para>
/// </remarks>
public class Requirement
{
    /// <summary>
    ///     Uniquely identifies this requirement across all loaded files, enabling traceability,
    ///     deduplication, and child reference resolution.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    ///     Human-readable description surfaced in reports so stakeholders can understand
    ///     requirements without consulting source YAML.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    ///     Gets or sets the optional justification explaining why this requirement exists.
    /// </summary>
    public string? Justification { get; set; }

    /// <summary>
    ///     Gets the list of test identifiers associated with this requirement.
    ///     Always initialized; never <c>null</c>.
    /// </summary>
    public List<string> Tests { get; } = [];

    /// <summary>
    ///     Gets the list of child requirement identifiers.
    ///     Always initialized; never <c>null</c>.
    /// </summary>
    public List<string> Children { get; } = [];

    /// <summary>
    ///     Gets the list of tags associated with this requirement.
    ///     Always initialized; never <c>null</c>.
    /// </summary>
    public List<string> Tags { get; } = [];

    /// <summary>
    ///     Gets or sets the source location where this requirement is defined (e.g. "path(line,col)").
    /// </summary>
    public string? Location { get; set; }
}
