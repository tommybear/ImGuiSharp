using System;
using ImGuiSharp.Input;
using ImGuiSharp.Math;

namespace ImGuiSharp;

/// <summary>
/// Holds per-frame input configuration consumed by ImGui during frame evaluation.
/// </summary>
public sealed class ImGuiIO
{
    private readonly bool[] _mouseButtons = new bool[3];
    private Vec2 _displaySize = new(1280f, 720f);
    private Vec2 _mousePosition;

    /// <summary>
    /// Gets or sets the elapsed time between frames in seconds.
    /// </summary>
    public float DeltaTime { get; set; } = 1f / 60f;

    /// <summary>
    /// Gets or sets the logical display size in pixels.
    /// </summary>
    public Vec2 DisplaySize
    {
        get => _displaySize;
        set => _displaySize = value;
    }

    /// <summary>
    /// Gets or sets the accumulated time since the context was created.
    /// </summary>
    public float Time { get; internal set; }

    /// <summary>
    /// Gets or sets the current mouse position in pixels.
    /// </summary>
    public Vec2 MousePosition
    {
        get => _mousePosition;
        internal set => _mousePosition = value;
    }

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
        _displaySize = new Vec2(1280f, 720f);
        Time = 0f;
        _mousePosition = Vec2Zero;
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

    private static Vec2 Vec2Zero => new(0f, 0f);
}
