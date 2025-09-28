using System.Collections.Generic;

namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents a single draw list containing GPU-ready primitives.
/// </summary>
public sealed class ImGuiDrawList
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiDrawList"/> class.
    /// </summary>
    /// <param name="commands">The commands describing how to render the list.</param>
    public ImGuiDrawList(IReadOnlyList<ImGuiDrawCommand> commands)
    {
        Commands = commands;
    }

    /// <summary>
    /// Gets the commands associated with this draw list.
    /// </summary>
    public IReadOnlyList<ImGuiDrawCommand> Commands { get; }
}
