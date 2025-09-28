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
    public ImGuiDrawCommand(int elementCount)
    {
        ElementCount = elementCount;
    }

    /// <summary>
    /// Gets the number of indices to draw for the command.
    /// </summary>
    public int ElementCount { get; }
}
