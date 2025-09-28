using System;
using ImGuiSharp;
using ImGuiSharp.Input;
using Xunit;

namespace ImGuiSharp.Tests;

public sealed class ImGuiContextTests
{
    [Fact]
    public void DrainInputEvents_ReturnsSnapshotAndClearsQueue()
    {
        var context = new ImGuiContext();
        context.AddInputEvent(new ImGuiKeyEvent(ImGuiKey.Enter, true));
        context.AddInputEvent(new ImGuiTextEvent('A'));

        var snapshot = context.DrainInputEvents();

        Assert.Equal(2, snapshot.Count);
        Assert.IsType<ImGuiKeyEvent>(snapshot[0]);
        Assert.IsType<ImGuiTextEvent>(snapshot[1]);
        Assert.Empty(context.DrainInputEvents());
    }

    [Fact]
    public void AddInputEvent_ThrowsWhenNull()
    {
        var context = new ImGuiContext();
        Assert.Throws<ArgumentNullException>(() => context.AddInputEvent(null!));
    }

    [Fact]
    public void UpdateDeltaTime_AccumulatesElapsedTime()
    {
        var context = new ImGuiContext();

        context.UpdateDeltaTime(0.016f);

        Assert.Equal(0.016f, context.IO.DeltaTime, 3);
        Assert.Equal(0.016f, context.GetTime(), 3);
    }

    [Fact]
    public void MouseState_IsTracked()
    {
        var context = new ImGuiContext();
        context.SetMousePosition(10f, 20f);
        context.SetMouseButtonState(ImGuiMouseButton.Left, true);

        var mouse = context.GetMouseState();
        Assert.Equal(10f, mouse.PositionX);
        Assert.Equal(20f, mouse.PositionY);
        Assert.True(mouse.IsPressed(ImGuiMouseButton.Left));
        Assert.False(mouse.IsPressed(ImGuiMouseButton.Right));
    }

    [Fact]
    public void KeyState_IsTracked()
    {
        var context = new ImGuiContext();
        context.SetKeyState(ImGuiKey.Enter, true);

        var snapshot = context.GetKeyState();
        Assert.True(snapshot.IsPressed(ImGuiKey.Enter));

        context.SetKeyState(ImGuiKey.Enter, false);
        snapshot = context.GetKeyState();
        Assert.False(snapshot.IsPressed(ImGuiKey.Enter));
    }
}
