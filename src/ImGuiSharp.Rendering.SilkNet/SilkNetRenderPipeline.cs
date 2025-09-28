using System;
using ImGuiSharp.Rendering;
using Silk.NET.OpenGL;

namespace ImGuiSharp.Rendering.SilkNet;

/// <summary>
/// Silk.NET-backed implementation of the ImGui render pipeline.
/// </summary>
public sealed class SilkNetRenderPipeline : IRenderPipeline
{
    private readonly GL _gl;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="SilkNetRenderPipeline"/> class.
    /// </summary>
    /// <param name="gl">The OpenGL bindings used to issue GPU commands.</param>
    public SilkNetRenderPipeline(GL gl)
    {
        _gl = gl;
    }

    /// <inheritdoc />
    public void BeginFrame()
    {
        ThrowIfDisposed();
        // Additional state setup will be added as the pipeline evolves.
    }

    /// <inheritdoc />
    public void Render(ImGuiDrawData drawData)
    {
        ThrowIfDisposed();
        // Rendering logic to be implemented in follow-up iterations.
        _ = drawData;
    }

    /// <inheritdoc />
    public void EndFrame()
    {
        ThrowIfDisposed();
        // Present or flush operations will be introduced once window management hooks exist.
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(SilkNetRenderPipeline));
        }
    }
}
