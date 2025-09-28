using System;
using System.Collections.Generic;
using ImGuiSharp.Input;
using ImGuiSharp.Math;

namespace ImGuiSharp;

/// <summary>
/// Represents the core runtime state for issuing immediate-mode UI commands.
/// </summary>
public sealed class ImGuiContext
{
    private readonly List<IImGuiInputEvent> _inputEvents = new();
    private readonly bool[] _mouseButtons = new bool[3];
    private readonly Dictionary<ImGuiKey, bool> _keyStates = new();
    private float _time;
    private Vec2 _mousePosition = new(0f, 0f);

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiContext"/> class.
    /// </summary>
    /// <param name="io">The IO configuration for the context. If null, a new instance is created.</param>
    public ImGuiContext(ImGuiIO? io = null)
    {
        IO = io ?? new ImGuiIO();
    }

    /// <summary>
    /// Gets the IO configuration associated with this context.
    /// </summary>
    public ImGuiIO IO { get; }

    /// <summary>
    /// Gets a value indicating whether a frame is currently in progress.
    /// </summary>
    public bool IsFrameStarted { get; private set; }

    /// <summary>
    /// Gets the number of frames that have completed since the context was created.
    /// </summary>
    public uint FrameCount { get; private set; }

    /// <summary>
    /// Begins a new frame, preparing internal state for UI commands.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a frame is already in progress.</exception>
    public void NewFrame()
    {
        if (IsFrameStarted)
        {
            throw new InvalidOperationException("NewFrame called while a frame is already in progress.");
        }

        IsFrameStarted = true;
    }

    /// <summary>
    /// Ends the current frame, finalizing draw command generation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no frame has been started.</exception>
    public void EndFrame()
    {
        if (!IsFrameStarted)
        {
            throw new InvalidOperationException("EndFrame called without a matching NewFrame call.");
        }

        IsFrameStarted = false;
        FrameCount++;
    }

    /// <summary>
    /// Updates the delta time and accumulates total elapsed time.
    /// </summary>
    /// <param name="deltaSeconds">Elapsed time since the previous frame in seconds.</param>
    public void UpdateDeltaTime(float deltaSeconds)
    {
        if (deltaSeconds < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Delta time must be non-negative.");
        }

        IO.DeltaTime = deltaSeconds;
        _time += deltaSeconds;
        IO.Time = _time;
    }

    /// <summary>
    /// Gets the accumulated time since the context was created.
    /// </summary>
    public float GetTime() => _time;

    /// <summary>
    /// Updates the tracked mouse position.
    /// </summary>
    /// <param name="x">The X coordinate in pixels.</param>
    /// <param name="y">The Y coordinate in pixels.</param>
    public void SetMousePosition(float x, float y) => SetMousePosition(new Vec2(x, y));

    /// <summary>
    /// Updates the tracked mouse position.
    /// </summary>
    /// <param name="position">Mouse position in pixels.</param>
    public void SetMousePosition(in Vec2 position)
    {
        _mousePosition = position;
        IO.MousePosition = position;
    }

    /// <summary>
    /// Updates the pressed state for a specific mouse button.
    /// </summary>
    /// <param name="button">The button to update.</param>
    /// <param name="isPressed">True when pressed, false when released.</param>
    public void SetMouseButtonState(ImGuiMouseButton button, bool isPressed)
    {
        var index = (int)button;
        if ((uint)index >= _mouseButtons.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(button));
        }

        _mouseButtons[index] = isPressed;
        IO.SetMouseButton(button, isPressed);
    }

    /// <summary>
    /// Returns a snapshot of the current mouse position and button states.
    /// </summary>
    public ImGuiMouseStateSnapshot GetMouseState() => new(_mousePosition, _mouseButtons);

    /// <summary>
    /// Updates the pressed state for the provided key.
    /// </summary>
    /// <param name="key">The logical key.</param>
    /// <param name="isPressed">True when pressed, false when released.</param>
    public void SetKeyState(ImGuiKey key, bool isPressed)
    {
        if (isPressed)
        {
            _keyStates[key] = true;
        }
        else
        {
            _keyStates.Remove(key);
        }
    }

    /// <summary>
    /// Returns a snapshot of the logical key state map.
    /// </summary>
    public ImGuiKeyStateSnapshot GetKeyState() => new(_keyStates);

    /// <summary>
    /// Enqueues an input event to be processed by the next frame.
    /// </summary>
    /// <param name="inputEvent">The input event to enqueue.</param>
    public void AddInputEvent(IImGuiInputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);
        _inputEvents.Add(inputEvent);
    }

    /// <summary>
    /// Returns all queued input events and clears the internal buffer.
    /// </summary>
    /// <returns>A snapshot of queued input events.</returns>
    public IReadOnlyList<IImGuiInputEvent> DrainInputEvents()
    {
        if (_inputEvents.Count == 0)
        {
            return Array.Empty<IImGuiInputEvent>();
        }

        var snapshot = _inputEvents.ToArray();
        _inputEvents.Clear();
        return snapshot;
    }
}
