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
}
