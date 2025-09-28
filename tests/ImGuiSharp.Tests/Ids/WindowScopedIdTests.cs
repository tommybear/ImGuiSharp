using ImGuiSharp;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Ids;

public sealed class WindowScopedIdTests
{
    [Fact]
    public void SameLabel_InDifferentWindows_ProducesDistinctIds()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(800, 600));

        ctx.NewFrame();

        ImGui.Begin("WindowA", new Vec2(0, 0), new Vec2(200, 150));
        ImGui.Button("Click");
        var firstId = ctx.LastItemId;
        ImGui.End();

        ImGui.Begin("WindowB", new Vec2(220, 0), new Vec2(200, 150));
        ImGui.Button("Click");
        var secondId = ctx.LastItemId;
        ImGui.End();

        ctx.EndFrame();

        Assert.NotEqual(firstId, secondId);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void PushId_WithWindowScope_StillProducesDistinctIds()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(800, 600));

        ctx.NewFrame();

        ImGui.Begin("Main", new Vec2(0, 0), new Vec2(200, 150));
        ImGui.PushID(1);
        ImGui.Button("Same");
        var first = ctx.LastItemId;
        ImGui.PopID();

        ImGui.PushID(2);
        ImGui.Button("Same");
        var second = ctx.LastItemId;
        ImGui.PopID();
        ImGui.End();

        ctx.EndFrame();

        Assert.NotEqual(first, second);

        ImGui.SetCurrentContext(null);
    }
}

