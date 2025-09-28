using System;
using ImGuiSharp;
using ImGuiSharp.Input;
using Xunit;

namespace ImGuiSharp.Tests;

public sealed class ImGuiFacadeTests : IDisposable
{
    public void Dispose()
    {
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void NewFrame_Throws_WhenContextMissing()
    {
        Assert.Throws<InvalidOperationException>(ImGui.NewFrame);
    }

    [Fact]
    public void EndFrame_Throws_WhenContextMissing()
    {
        Assert.Throws<InvalidOperationException>(ImGui.EndFrame);
    }

    [Fact]
    public void SetCurrentContext_AllowsNullToClear()
    {
        var context = new ImGuiContext();
        ImGui.SetCurrentContext(context);

        Assert.Same(context, ImGui.GetCurrentContext());

        ImGui.SetCurrentContext(null);
        Assert.Throws<InvalidOperationException>(() => ImGui.GetCurrentContext());
    }

    [Fact]
    public void Facade_ForwardsCallsToContext()
    {
        var context = new ImGuiContext();
        ImGui.SetCurrentContext(context);

        ImGui.NewFrame();
        Assert.True(context.IsFrameStarted);
        ImGui.EndFrame();
        Assert.False(context.IsFrameStarted);
    }

    [Fact]
    public void AddInputEvent_RoutesToCurrentContext()
    {
        var context = new ImGuiContext();
        ImGui.SetCurrentContext(context);

        var keyEvent = new ImGuiKeyEvent(ImGuiKey.Enter, true);
        ImGui.AddInputEvent(keyEvent);

        var drained = ImGui.DrainInputEvents();
        Assert.Single(drained);
        Assert.Equal(keyEvent, drained[0]);
    }

    [Fact]
    public void UpdateDeltaTime_RoutesToContext()
    {
        var context = new ImGuiContext();
        ImGui.SetCurrentContext(context);

        ImGui.UpdateDeltaTime(0.02f);

        Assert.Equal(0.02f, context.IO.DeltaTime, 3);
        Assert.Equal(0.02f, ImGui.GetTime(), 3);
    }

    [Fact]
    public void MouseHelpers_ExposeState()
    {
        var context = new ImGuiContext();
        ImGui.SetCurrentContext(context);

        ImGui.SetMousePosition(5f, 6f);
        ImGui.SetMouseButtonState(ImGuiMouseButton.Left, true);

        var mouse = ImGui.GetMouseState();
        Assert.Equal(5f, mouse.PositionX);
        Assert.Equal(6f, mouse.PositionY);
        Assert.True(mouse.IsPressed(ImGuiMouseButton.Left));
    }

    [Fact]
    public void KeyHelpers_ExposeState()
    {
        var context = new ImGuiContext();
        ImGui.SetCurrentContext(context);

        ImGui.SetKeyState(ImGuiKey.Enter, true);
        Assert.True(ImGui.GetKeyState().IsPressed(ImGuiKey.Enter));
    }
}
