using System.Collections.Generic;

namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents the draw data emitted by the core runtime for a single frame.
/// </summary>
public sealed class ImGuiDrawData
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiDrawData"/> class.
    /// </summary>
    /// <param name="drawLists">The draw lists that should be submitted to the renderer.</param>
    public ImGuiDrawData(IReadOnlyList<ImGuiDrawList> drawLists)
    {
        DrawLists = drawLists;
    }

    /// <summary>
    /// Gets the draw lists composing the frame output.
    /// </summary>
    public IReadOnlyList<ImGuiDrawList> DrawLists { get; }
}
