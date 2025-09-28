namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents an axis-aligned rectangle in screen space.
/// </summary>
/// <param name="MinX">Minimum X coordinate.</param>
/// <param name="MinY">Minimum Y coordinate.</param>
/// <param name="MaxX">Maximum X coordinate.</param>
/// <param name="MaxY">Maximum Y coordinate.</param>
public readonly record struct ImGuiRect(float MinX, float MinY, float MaxX, float MaxY);
