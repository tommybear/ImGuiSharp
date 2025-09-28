namespace ImGuiSharp.Input;

/// <summary>
/// Represents a mouse button state change.
/// </summary>
/// <param name="Button">The mouse button that changed state.</param>
/// <param name="IsPressed">True when the button is pressed, false when released.</param>
public readonly record struct ImGuiMouseButtonEvent(ImGuiMouseButton Button, bool IsPressed) : IImGuiInputEvent;
