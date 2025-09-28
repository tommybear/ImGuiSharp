namespace ImGuiSharp.Input;

/// <summary>
/// Represents a mouse wheel delta event.
/// </summary>
/// <param name="WheelX">Horizontal wheel delta.</param>
/// <param name="WheelY">Vertical wheel delta.</param>
public readonly record struct ImGuiMouseWheelEvent(float WheelX, float WheelY) : IImGuiInputEvent;
