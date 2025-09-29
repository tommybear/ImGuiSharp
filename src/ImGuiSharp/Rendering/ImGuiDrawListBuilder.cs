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
    private IntPtr _currentTexture = IntPtr.Zero;
    private bool _hasClip;
    private ImGuiRect _clipRect;

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

        // UVs are irrelevant for solid fills; renderer binds a 1x1 white texture when TextureId == 0
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

        var clip = _hasClip ? _clipRect : rect;
        _commands.Add(new ImGuiDrawCommand(6, clip, IntPtr.Zero));
    }

    public void AddRect(in ImGuiRect rect, Color color, float thickness)
    {
        if (thickness <= 0f)
        {
            return;
        }

        float t = thickness;
        float minX = rect.MinX;
        float minY = rect.MinY;
        float maxX = rect.MaxX;
        float maxY = rect.MaxY;

        if (minX >= maxX || minY >= maxY)
        {
            return;
        }

        float innerMinX = minX + t;
        float innerMinY = minY + t;
        float innerMaxX = maxX - t;
        float innerMaxY = maxY - t;

        if (innerMinX > innerMaxX) innerMinX = innerMaxX = (minX + maxX) * 0.5f;
        if (innerMinY > innerMaxY) innerMinY = innerMaxY = (minY + maxY) * 0.5f;

        AddRectFilled(new ImGuiRect(minX, minY, maxX, minY + t), color);
        AddRectFilled(new ImGuiRect(minX, maxY - t, maxX, maxY), color);
        if (innerMinY < innerMaxY)
        {
            AddRectFilled(new ImGuiRect(minX, innerMinY, minX + t, innerMaxY), color);
            AddRectFilled(new ImGuiRect(maxX - t, innerMinY, maxX, innerMaxY), color);
        }
    }

    public void AddQuad(float x0, float y0, float x1, float y1, float u0, float v0, float u1, float v1, Color color)
    {
        if (_vertices.Count > ushort.MaxValue - 4)
        {
            throw new InvalidOperationException("Draw list exceeds maximum vertex count.");
        }

        var baseIndex = (ushort)_vertices.Count;

        _vertices.Add(new ImGuiVertex(x0, y0, u0, v0, color.PackABGR()));
        _vertices.Add(new ImGuiVertex(x1, y0, u1, v0, color.PackABGR()));
        _vertices.Add(new ImGuiVertex(x1, y1, u1, v1, color.PackABGR()));
        _vertices.Add(new ImGuiVertex(x0, y1, u0, v1, color.PackABGR()));

        _indices.Add(baseIndex);
        _indices.Add((ushort)(baseIndex + 1));
        _indices.Add((ushort)(baseIndex + 2));
        _indices.Add(baseIndex);
        _indices.Add((ushort)(baseIndex + 2));
        _indices.Add((ushort)(baseIndex + 3));

        var clip = _hasClip ? _clipRect : new ImGuiRect(x0, y0, x1, y1);
        _commands.Add(new ImGuiDrawCommand(6, clip, _currentTexture));
    }

    public void AddCircleFilled(Vec2 center, float radius, Color color, int segments = 12)
    {
        if (radius <= 0f)
        {
            return;
        }

        segments = System.Math.Clamp(segments, 3, 64);

        if (_vertices.Count > ushort.MaxValue - (segments + 2))
        {
            throw new InvalidOperationException("Draw list exceeds maximum vertex count.");
        }

        var centerIndex = (ushort)_vertices.Count;
        _vertices.Add(ImGuiVertex.From(center, Vec2.Zero, color));

        var angleStep = (float)(System.Math.PI * 2.0 / segments);
        for (int i = 0; i <= segments; i++)
        {
            var angle = angleStep * i;
            var x = center.X + System.MathF.Cos(angle) * radius;
            var y = center.Y + System.MathF.Sin(angle) * radius;
            _vertices.Add(ImGuiVertex.From(new Vec2(x, y), Vec2.Zero, color));
        }

        for (int i = 0; i < segments; i++)
        {
            _indices.Add(centerIndex);
            _indices.Add((ushort)(centerIndex + i + 1));
            _indices.Add((ushort)(centerIndex + i + 2));
        }

        var clipRect = _hasClip
            ? _clipRect
            : new ImGuiRect(center.X - radius, center.Y - radius, center.X + radius, center.Y + radius);
        _commands.Add(new ImGuiDrawCommand(segments * 3, clipRect, IntPtr.Zero));
    }

    public void SetTexture(IntPtr textureId)
    {
        _currentTexture = textureId;
    }

    public void PushClipRect(ImGuiRect rect)
    {
        _hasClip = true;
        _clipRect = rect;
    }

    public void PopClipRect()
    {
        _hasClip = false;
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
