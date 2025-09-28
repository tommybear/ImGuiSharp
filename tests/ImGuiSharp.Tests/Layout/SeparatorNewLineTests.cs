using ImGuiSharp;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;
using Xunit;

namespace ImGuiSharp.Tests.Layout;

public sealed class SeparatorNewLineTests
{
    [Fact]
    public void NewLine_AdvancesCursor_ByLineHeightAndSpacing()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        ctx.NewFrame();
        ImGui.Begin("Window", new Vec2(0, 0), new Vec2(200, 200));
        var start = ImGui.GetCursorPos();
        var expectedDelta = ctx.GetLineHeight() + ctx.Style.ItemSpacing.Y;

        ImGui.NewLine();

        var after = ImGui.GetCursorPos();
        ImGui.End();
        ctx.EndFrame();

        Assert.Equal(start.Y + expectedDelta, after.Y, 3);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void Separator_DrawsFullWidthLine_AndAdvancesCursor()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        ctx.NewFrame();
        ImGui.Begin("Window", new Vec2(10, 10), new Vec2(180, 120));
        ImGui.SetCursorPos(new Vec2(20, 30));

        var before = ImGui.GetCursorPos();
        ImGui.Separator();
        var after = ImGui.GetCursorPos();

        ImGui.End();
        ctx.EndFrame();

        var drawData = ImGui.GetDrawData();
        Assert.NotNull(drawData);
        Assert.Single(drawData.DrawLists);
        var list = drawData.DrawLists[0];
        Assert.True(list.Vertices.Length > 0);
        Assert.True(list.Commands.Count > 0);

        var expectedDelta = 1f + ctx.Style.ItemSpacing.Y * 2f;
        Assert.Equal(before.Y + expectedDelta, after.Y, 3);

        // Separator should cover from cursor.X to content max X
        var rect = ctx.LastItemRect;
        Assert.Equal(before.X, rect.MinX, 3);
        Assert.True(rect.MaxX > rect.MinX);

        ImGui.SetCurrentContext(null);
    }
}
