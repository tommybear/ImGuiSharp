using ImGuiSharp;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Layout;

public sealed class SameLineSpacingTests
{
    [Fact]
    public void SameLine_PlacesNextItem_OnSameRow_WithDefaultSpacing()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(800, 600));

        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0f, 10f));

        // First item
        ImGui.Button("A", new Vec2(120f, 36f));
        var first = ctx.LastItemRect;

        // Same line + second item
        ImGui.SameLine();
        ImGui.Button("B", new Vec2(120f, 36f));
        var second = ctx.LastItemRect;

        ctx.EndFrame();

        // Y should match; X should be first.MaxX + 8 spacing
        Assert.Equal(first.MinY, second.MinY, 3);
        Assert.Equal(first.MaxX + 8f, second.MinX, 3);
        // No overlap
        Assert.True(second.MinX >= first.MaxX);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void SameLine_Honors_XOffset_RelativeToFirstItemStart()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(800, 600));

        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(20f, 50f));

        ImGui.Button("A", new Vec2(120f, 36f));
        var first = ctx.LastItemRect;

        ImGui.SameLine(xOffset: 200f);
        ImGui.Button("B", new Vec2(120f, 36f));
        var second = ctx.LastItemRect;

        ctx.EndFrame();

        Assert.Equal(first.MinY, second.MinY, 3);
        Assert.Equal(first.MinX + 200f, second.MinX, 3);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void Spacing_AdvancesDownward_WithoutChangingX()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(5f, 10f));
        var before = ImGui.GetCursorPos();
        ImGui.Spacing();
        var after = ImGui.GetCursorPos();
        ctx.EndFrame();

        Assert.Equal(before.X, after.X, 3);
        Assert.True(after.Y > before.Y);

        ImGui.SetCurrentContext(null);
    }
}

