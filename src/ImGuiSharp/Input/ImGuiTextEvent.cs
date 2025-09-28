namespace ImGuiSharp.Input;

/// <summary>
/// Represents a textual input event (UTF-16 code unit).
/// </summary>
/// <param name="Character">The character entered by the user.</param>
public readonly record struct ImGuiTextEvent(char Character) : IImGuiInputEvent;
