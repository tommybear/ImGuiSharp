using System;
using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Widgets;

public sealed class ButtonTests : IDisposable
{
    private readonly ImGuiContext _context;

    public ButtonTests()
    {
        _context = new ImGuiContext();
        ImGui.SetCurrentContext(_context);
        ImGui.SetDisplaySize(new Vec2(400f, 300f));
        ImGui.UpdateDeltaTime(1f / 60f);
    }

    public void Dispose()
    {
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void Button_IsNotPressed_WhenMouseOutside()
    {
        _context.SetMousePosition(new Vec2(500f, 500f));

        _context.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);

        var pressed = ImGui.Button("Click", new Vec2(100f, 30f));

        _context.EndFrame();

        Assert.False(pressed);
        Assert.Equal(0u, _context.HoveredId);
    }

    [Fact]
    public void Button_SetsHoveredId_WhenMouseOver()
    {
        _context.SetMousePosition(new Vec2(10f, 10f));

        _context.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.Button("Hover", new Vec2(100f, 30f));
        _context.EndFrame();

        Assert.NotEqual(0u, _context.HoveredId);
        Assert.Equal(_context.LastItemId, _context.HoveredId);
    }

    [Fact]
    public void Button_ReturnsTrue_OnClickRelease()
    {
        var size = new Vec2(100f, 30f);

        _context.SetMousePosition(new Vec2(10f, 10f));

        _context.SetMouseButtonState(ImGuiMouseButton.Left, false);
        _context.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        var pressed = ImGui.Button("Play", size);
        _context.EndFrame();
        Assert.False(pressed);

        _context.SetMouseButtonState(ImGuiMouseButton.Left, true);
        _context.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        pressed = ImGui.Button("Play", size);
        _context.EndFrame();
        Assert.False(pressed);
        Assert.Equal(_context.LastItemId, _context.ActiveId);

        _context.SetMouseButtonState(ImGuiMouseButton.Left, false);
        _context.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        pressed = ImGui.Button("Play", size);
        _context.EndFrame();

        Assert.True(pressed);
        Assert.Equal(0u, _context.ActiveId);
    }

    [Fact]
    public void Button_UsesIdStack_ForDuplicateLabels()
    {
        var size = new Vec2(80f, 20f);

        _context.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.PushID(1);
        ImGui.Button("Same", size);
        var firstId = _context.LastItemId;
        Assert.NotEqual(0u, firstId);
        ImGui.PopID();

        ImGui.PushID(2);
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.Button("Same", size);
        var secondId = _context.LastItemId;
        Assert.NotEqual(0u, secondId);
        ImGui.PopID();
        _context.EndFrame();

        Assert.NotEqual(firstId, secondId);
    }
}
