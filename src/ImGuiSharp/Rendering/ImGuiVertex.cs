using ImGuiSharp.Math;

namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents a single vertex emitted by the UI renderer.
/// </summary>
public readonly record struct ImGuiVertex(float PositionX, float PositionY, float TexU, float TexV, uint Color)
{
    /// <summary>
    /// Creates a vertex from vector position/uv and colour components.
    /// </summary>
    public static ImGuiVertex From(Vec2 position, Vec2 uv, Color color)
    {
        return new ImGuiVertex(position.X, position.Y, uv.X, uv.Y, color.PackABGR());
    }
}
