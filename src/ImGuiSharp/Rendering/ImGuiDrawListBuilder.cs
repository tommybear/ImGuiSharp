using System;
using System.Collections.Generic;
using ImGuiSharp.Math;

namespace ImGuiSharp.Rendering;

/// <summary>
/// Utility to construct draw lists from immediate-mode primitives.
/// </summary>
internal sealed class ImGuiDrawListBuilder
{
    private readonly List<ImGuiVertex> _vertices = new();
    private readonly List<ushort> _indices = new();
    private readonly List<ImGuiDrawCommand> _commands = new();

    /// <summary>
    /// Resets the accumulated geometry.
    /// </summary>
    public void Reset()
    {
        _vertices.Clear();
        _indices.Clear();
        _commands.Clear();
    }

    /// <summary>
    /// Adds a filled rectangle made of two triangles.
    /// </summary>
    /// <param name="rect">Rectangle bounds.</param>
    /// <param name="color">Fill colour.</param>
    public void AddRectFilled(in ImGuiRect rect, Color color)
    {
        if (_vertices.Count > ushort.MaxValue - 4)
        {
            throw new InvalidOperationException("Draw list exceeds maximum vertex count.");
        }

        var baseIndex = (ushort)_vertices.Count;

        var topLeft = new Vec2(rect.MinX, rect.MinY);
        var topRight = new Vec2(rect.MaxX, rect.MinY);
        var bottomRight = new Vec2(rect.MaxX, rect.MaxY);
        var bottomLeft = new Vec2(rect.MinX, rect.MaxY);

        _vertices.Add(ImGuiVertex.From(topLeft, new Vec2(0f, 0f), color));
        _vertices.Add(ImGuiVertex.From(topRight, new Vec2(1f, 0f), color));
        _vertices.Add(ImGuiVertex.From(bottomRight, new Vec2(1f, 1f), color));
        _vertices.Add(ImGuiVertex.From(bottomLeft, new Vec2(0f, 1f), color));

        _indices.Add(baseIndex);
        _indices.Add((ushort)(baseIndex + 1));
        _indices.Add((ushort)(baseIndex + 2));
        _indices.Add(baseIndex);
        _indices.Add((ushort)(baseIndex + 2));
        _indices.Add((ushort)(baseIndex + 3));

        _commands.Add(new ImGuiDrawCommand(6, rect, IntPtr.Zero));
    }

    /// <summary>
    /// Builds an immutable draw list snapshot.
    /// </summary>
    public ImGuiDrawList Build()
    {
        var vertices = _vertices.Count == 0 ? Array.Empty<ImGuiVertex>() : _vertices.ToArray();
        var indices = _indices.Count == 0 ? Array.Empty<ushort>() : _indices.ToArray();
        var commands = _commands.Count == 0 ? Array.Empty<ImGuiDrawCommand>() : _commands.ToArray();
        return new ImGuiDrawList(vertices, indices, commands);
    }
}
