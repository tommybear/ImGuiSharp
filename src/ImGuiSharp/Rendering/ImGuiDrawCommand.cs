using System;

namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents a single render command with its associated metadata.
/// </summary>
public sealed class ImGuiDrawCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiDrawCommand"/> class.
    /// </summary>
    /// <param name="elementCount">Number of elements to draw.</param>
    /// <param name="clipRect">Clipping rectangle applied to the draw call.</param>
    /// <param name="textureId">Backend texture identifier.</param>
    public ImGuiDrawCommand(int elementCount, ImGuiRect clipRect, IntPtr textureId)
    {
        if (elementCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(elementCount));
        }

        ElementCount = elementCount;
        ClipRect = clipRect;
        TextureId = textureId;
    }

    /// <summary>
    /// Gets the number of indices to draw for the command.
    /// </summary>
    public int ElementCount { get; }

    /// <summary>
    /// Gets the clipping rectangle applied to the command.
    /// </summary>
    public ImGuiRect ClipRect { get; }

    /// <summary>
    /// Gets the renderer-specific texture identifier.
    /// </summary>
    public IntPtr TextureId { get; }
}
