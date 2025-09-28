using System;

namespace ImGuiSharp.Input;

/// <summary>
/// Immutable snapshot of mouse position and button states for the immediate-mode runtime.
/// </summary>
public readonly struct ImGuiMouseStateSnapshot
{
    private readonly bool[]? _buttons;

    internal ImGuiMouseStateSnapshot(float positionX, float positionY, bool[] buttonStates)
    {
        PositionX = positionX;
        PositionY = positionY;
        _buttons = (bool[])buttonStates.Clone();
    }

    /// <summary>
    /// Gets the X coordinate of the mouse cursor in pixels.
    /// </summary>
    public float PositionX { get; }

    /// <summary>
    /// Gets the Y coordinate of the mouse cursor in pixels.
    /// </summary>
    public float PositionY { get; }

    /// <summary>
    /// Gets a read-only view over the mouse button states.
    /// </summary>
    public ReadOnlySpan<bool> Buttons => _buttons ?? Array.Empty<bool>();

    /// <summary>
    /// Returns whether the specified button is currently pressed.
    /// </summary>
    /// <param name="button">The mouse button to inspect.</param>
    public bool IsPressed(ImGuiMouseButton button)
    {
        var buttons = _buttons;
        var index = (int)button;
        return buttons is not null && index >= 0 && index < buttons.Length && buttons[index];
    }
}
