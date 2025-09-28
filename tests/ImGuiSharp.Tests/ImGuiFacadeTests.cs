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
}
