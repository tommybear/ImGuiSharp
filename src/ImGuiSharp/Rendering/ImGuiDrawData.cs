using System;
using System.Collections.Generic;

namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents the draw data emitted by the core runtime for a single frame.
/// </summary>
public sealed class ImGuiDrawData
{
    private readonly ImGuiDrawList[] _drawLists;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiDrawData"/> class.
    /// </summary>
    /// <param name="drawLists">The draw lists that should be submitted to the renderer.</param>
    /// <param name="displayRect">The display rectangle covered by this frame.</param>
    public ImGuiDrawData(IReadOnlyList<ImGuiDrawList> drawLists, ImGuiRect displayRect)
    {
        ArgumentNullException.ThrowIfNull(drawLists);

        _drawLists = new ImGuiDrawList[drawLists.Count];
        var totalVertices = 0;
        var totalIndices = 0;

        for (var i = 0; i < drawLists.Count; i++)
        {
            var list = drawLists[i] ?? throw new ArgumentNullException(nameof(drawLists), "Draw list entry cannot be null.");
            _drawLists[i] = list;
            totalVertices += list.Vertices.Length;
            totalIndices += list.Indices.Length;
        }

        DisplayRect = displayRect;
        TotalVtxCount = totalVertices;
        TotalIdxCount = totalIndices;
    }

    /// <summary>
    /// Gets the draw lists composing the frame output.
    /// </summary>
    public IReadOnlyList<ImGuiDrawList> DrawLists => _drawLists;

    /// <summary>
    /// Gets the display rectangle covered by this draw data.
    /// </summary>
    public ImGuiRect DisplayRect { get; }

    /// <summary>
    /// Gets the total vertex count across all draw lists.
    /// </summary>
    public int TotalVtxCount { get; }

    /// <summary>
    /// Gets the total index count across all draw lists.
    /// </summary>
    public int TotalIdxCount { get; }
}
