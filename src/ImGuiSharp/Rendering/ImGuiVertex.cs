namespace ImGuiSharp.Rendering;

/// <summary>
/// Represents a single vertex emitted by the UI renderer.
/// </summary>
/// <param name="PositionX">The X coordinate in pixels.</param>
/// <param name="PositionY">The Y coordinate in pixels.</param>
/// <param name="TexU">The texture U coordinate.</param>
/// <param name="TexV">The texture V coordinate.</param>
/// <param name="Color">Packed RGBA colour (ABGR, same as Dear ImGui).</param>
public readonly record struct ImGuiVertex(float PositionX, float PositionY, float TexU, float TexV, uint Color);
