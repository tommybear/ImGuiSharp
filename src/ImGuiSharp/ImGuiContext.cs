using System;
using System.Collections.Generic;
using ImGuiSharp.Input;

namespace ImGuiSharp;

/// <summary>
/// Represents the core runtime state for issuing immediate-mode UI commands.
/// </summary>
public sealed class ImGuiContext
{
    private readonly List<IImGuiInputEvent> _inputEvents = new();

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
