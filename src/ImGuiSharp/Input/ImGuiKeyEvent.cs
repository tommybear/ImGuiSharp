namespace ImGuiSharp.Input;

/// <summary>
/// Represents a keyboard key state change.
/// </summary>
/// <param name="Key">The key that changed state.</param>
/// <param name="IsPressed">True when the key is pressed, false when released.</param>
public readonly record struct ImGuiKeyEvent(ImGuiKey Key, bool IsPressed) : IImGuiInputEvent;
