using System;
using ImGuiSharp;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;
using ImGuiSharp.Rendering.SilkNet;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

var options = WindowOptions.Default with
{
    Title = "ImGuiSharp Triangle",
    Size = new Silk.NET.Maths.Vector2D<int>(960, 640),
    API = WindowOptions.Default.API with
    {
        API = ContextAPI.OpenGL,
        Profile = ContextProfile.Core,
        Version = new APIVersion(3, 3)
    }
};

using var window = Window.Create(options);
SilkNetRenderPipeline? pipeline = null;
GL? gl = null;
ImGuiContext? context = null;

window.Load += () =>
{
    gl = GL.GetApi(window);
    pipeline = new SilkNetRenderPipeline(gl);

    context = new ImGuiContext();
    ImGui.SetCurrentContext(context);
};

window.Render += deltaTime =>
{
    if (gl is null || pipeline is null || context is null)
    {
        return;
    }

    context.UpdateDeltaTime((float)deltaTime);

    var framebufferSize = window.FramebufferSize;
    var displaySize = new Vec2(framebufferSize.X, framebufferSize.Y);
    ImGui.SetDisplaySize(displaySize);

    pipeline.BeginFrame();

    gl.Viewport(0, 0, (uint)framebufferSize.X, (uint)framebufferSize.Y);
    gl.ClearColor(0.08f, 0.09f, 0.12f, 1.0f);
    gl.Clear(ClearBufferMask.ColorBufferBit);

    // Simple test triangle filling part of the window.
    var vertices = new[]
    {
        ImGuiVertex.From(new Vec2(100f, 540f), new Vec2(0f, 1f), new Color(1f, 0.2f, 0.2f)),
        ImGuiVertex.From(new Vec2(480f, 100f), new Vec2(0.5f, 0f), new Color(0.2f, 0.9f, 0.3f)),
        ImGuiVertex.From(new Vec2(860f, 540f), new Vec2(1f, 1f), new Color(0.2f, 0.4f, 1f))
    };

    var indices = new ushort[] { 0, 1, 2 };
    var command = new ImGuiDrawCommand(
        elementCount: 3,
        clipRect: new ImGuiRect(0f, 0f, displaySize.X, displaySize.Y),
        textureId: IntPtr.Zero);

    var drawList = new ImGuiDrawList(vertices, indices, new[] { command });
    var drawData = new ImGuiDrawData(new[] { drawList }, new ImGuiRect(0f, 0f, displaySize.X, displaySize.Y));

    pipeline.Render(drawData);
    pipeline.EndFrame();
};

window.Update += deltaTime =>
{
    if (window.IsClosing)
    {
        return;
    }

    if (ImGui.GetCurrentContext() is { } ctx)
    {
        ctx.NewFrame();
        ctx.EndFrame();
    }
};

window.Closing += () =>
{
    pipeline?.Dispose();
    ImGui.SetCurrentContext(null);
};

window.Run();
