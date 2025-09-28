using System;
using ImGuiSharp;
using Xunit;

namespace ImGuiSharp.Tests;

public sealed class FrameLifecycleTests
{
    [Fact]
    public void NewFrame_SetsFlagUntilEndFrame()
    {
        var context = new ImGuiContext();

        Assert.False(context.IsFrameStarted);
        context.NewFrame();
        Assert.True(context.IsFrameStarted);
        context.EndFrame();
        Assert.False(context.IsFrameStarted);
    }

    [Fact]
    public void NewFrame_Throws_WhenCalledTwice()
    {
        var context = new ImGuiContext();

        context.NewFrame();
        var exception = Assert.Throws<InvalidOperationException>(() => context.NewFrame());
        Assert.Contains("already", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EndFrame_Throws_WhenNoFrameActive()
    {
        var context = new ImGuiContext();

        var exception = Assert.Throws<InvalidOperationException>(() => context.EndFrame());
        Assert.Contains("NewFrame", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void EndFrame_IncrementsFrameCount()
    {
        var context = new ImGuiContext();

        context.NewFrame();
        context.EndFrame();

        Assert.Equal(1u, context.FrameCount);
    }
}
