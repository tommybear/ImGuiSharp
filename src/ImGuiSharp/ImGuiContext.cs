using System;

namespace ImGuiSharp;

/// <summary>
/// Represents the core runtime state for issuing immediate-mode UI commands.
/// </summary>
public sealed class ImGuiContext
{
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
    }
}
