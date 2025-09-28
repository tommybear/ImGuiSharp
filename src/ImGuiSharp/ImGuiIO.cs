using System;
using ImGuiSharp.Input;

namespace ImGuiSharp;

/// <summary>
/// Holds per-frame input configuration consumed by ImGui during frame evaluation.
/// </summary>
public sealed class ImGuiIO
{
    private readonly bool[] _mouseButtons = new bool[3];

    /// <summary>
    /// Gets or sets the elapsed time between frames in seconds.
    /// </summary>
    public float DeltaTime { get; set; } = 1f / 60f;

    /// <summary>
    /// Gets or sets the logical width of the display surface in pixels.
    /// </summary>
    public float DisplayWidth { get; set; } = 1280f;

    /// <summary>
    /// Gets or sets the logical height of the display surface in pixels.
    /// </summary>
    public float DisplayHeight { get; set; } = 720f;

    /// <summary>
    /// Gets the accumulated time since the context was created.
    /// </summary>
    public float Time { get; internal set; }

    /// <summary>
    /// Gets the current mouse X position in pixels.
    /// </summary>
    public float MousePositionX { get; internal set; }

    /// <summary>
    /// Gets the current mouse Y position in pixels.
    /// </summary>
    public float MousePositionY { get; internal set; }

    /// <summary>
    /// Gets a read-only view over the mouse button state array.
    /// </summary>
    public ReadOnlySpan<bool> MouseButtons => _mouseButtons;

    /// <summary>
    /// Resets the per-frame input to defaults typically used for the first frame.
    /// </summary>
    public void Reset()
    {
        DeltaTime = 1f / 60f;
        DisplayWidth = 1280f;
        DisplayHeight = 720f;
        Time = 0f;
        MousePositionX = 0f;
        MousePositionY = 0f;
        Array.Clear(_mouseButtons, 0, _mouseButtons.Length);
    }

    internal void SetMouseButton(ImGuiMouseButton button, bool isPressed)
    {
        var index = (int)button;
        if ((uint)index >= _mouseButtons.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(button));
        }

        _mouseButtons[index] = isPressed;
    }
}
