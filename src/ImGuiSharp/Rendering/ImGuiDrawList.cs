using System;
using System.Collections.Generic;

namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents a single draw list containing GPU-ready primitives.
/// </summary>
public sealed class ImGuiDrawList
{
    private readonly ImGuiVertex[] _vertices;
    private readonly ushort[] _indices;
    private readonly ImGuiDrawCommand[] _commands;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiDrawList"/> class.
    /// </summary>
    /// <param name="vertices">The vertex buffer for the list.</param>
    /// <param name="indices">The index buffer for the list.</param>
    /// <param name="commands">The commands describing how to render the list.</param>
    public ImGuiDrawList(ImGuiVertex[] vertices, ushort[] indices, IReadOnlyList<ImGuiDrawCommand> commands)
    {
        ArgumentNullException.ThrowIfNull(vertices);
        ArgumentNullException.ThrowIfNull(indices);
        ArgumentNullException.ThrowIfNull(commands);

        _vertices = vertices.Length == 0 ? Array.Empty<ImGuiVertex>() : (ImGuiVertex[])vertices.Clone();
        _indices = indices.Length == 0 ? Array.Empty<ushort>() : (ushort[])indices.Clone();

        var copiedCommands = new ImGuiDrawCommand[commands.Count];
        for (var i = 0; i < commands.Count; i++)
        {
            copiedCommands[i] = commands[i] ?? throw new ArgumentNullException(nameof(commands), "Command entry cannot be null.");
        }

        _commands = copiedCommands;
    }

    /// <summary>
    /// Gets the vertex data for the draw list.
    /// </summary>
    public ReadOnlyMemory<ImGuiVertex> Vertices => _vertices;

    /// <summary>
    /// Gets the index data for the draw list.
    /// </summary>
    public ReadOnlyMemory<ushort> Indices => _indices;

    /// <summary>
    /// Gets the commands associated with this draw list.
    /// </summary>
    public IReadOnlyList<ImGuiDrawCommand> Commands => _commands;
}
