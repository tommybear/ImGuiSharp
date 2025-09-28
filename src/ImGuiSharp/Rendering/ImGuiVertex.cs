using ImGuiSharp.Math;
using System.Runtime.InteropServices;

namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents a single vertex emitted by the UI renderer.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct ImGuiVertex(float PositionX, float PositionY, float TexU, float TexV, uint Color)
{
    /// <summary>
    /// Gets the size in bytes of the vertex structure.
    /// </summary>
    public static readonly int SizeInBytes = System.Runtime.CompilerServices.Unsafe.SizeOf<ImGuiVertex>();

    /// <summary>
    /// Creates a vertex from vector position/uv and colour components.
    /// </summary>
    public static ImGuiVertex From(Vec2 position, Vec2 uv, Color color)
    {
        return new ImGuiVertex(position.X, position.Y, uv.X, uv.Y, color.PackABGR());
    }
}
