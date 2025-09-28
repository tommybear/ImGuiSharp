namespace ImGuiSharp.Input;

/// <summary>
/// Represents a pointer position update.
/// </summary>
/// <param name="X">The X coordinate in pixels.</param>
/// <param name="Y">The Y coordinate in pixels.</param>
public readonly record struct ImGuiMousePositionEvent(float X, float Y) : IImGuiInputEvent;
