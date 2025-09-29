using ImGuiSharp;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;
using Xunit;

namespace ImGuiSharp.Tests.Style;

public sealed class StyleStackTests
{
    [Fact]
    public void PushStyleColor_OverridesAndRestores()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        var original = ctx.Style.GetColor(ImGuiCol.Button);
        var overrideColor = new Color(0.3f, 0.7f, 0.2f, 1f);

        ctx.SetMousePosition(new Vec2(1000f, 1000f));
        ctx.NewFrame();
        ImGui.PushStyleColor(ImGuiCol.Button, overrideColor);
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.Button("Styled", new Vec2(100f, 30f));
        ImGui.PopStyleColor();
        ctx.EndFrame();

        Assert.Equal(original.PackABGR(), ctx.Style.GetColor(ImGuiCol.Button).PackABGR());

        var drawData = ImGui.GetDrawData();
        Assert.Contains(drawData.DrawLists, list => ContainsColor(list.Vertices.Span, overrideColor));

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void PushStyleVar_FramePadding_OverridesHeightAndRestores()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        var originalPadding = ctx.Style.FramePadding;
        var overridePadding = new Vec2(10f, 12f);

        ctx.SetMousePosition(new Vec2(1000f, 1000f));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.Button("Baseline");
        var baseline = ctx.LastItemRect;

        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, overridePadding);
        ImGui.SetCursorPos(new Vec2(0f, baseline.MaxY + ctx.Style.ItemSpacing.Y));
        ImGui.Button("Padded");
        var padded = ctx.LastItemRect;
        ImGui.PopStyleVar();
        ctx.EndFrame();

        Assert.Equal(originalPadding, ctx.Style.FramePadding);

        var baselineHeight = baseline.MaxY - baseline.MinY;
        var paddedHeight = padded.MaxY - padded.MinY;
        Assert.True(paddedHeight > baselineHeight);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void PushStyleVar_FrameBorderSize_AppliesBorder()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        var overrideBorder = 2f;
        var borderColor = new Color(0.8f, 0.1f, 0.6f, 1f);
        ctx.Style.SetColor(ImGuiCol.Border, borderColor);

        ctx.SetMousePosition(new Vec2(1000f, 1000f));
        ctx.NewFrame();
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, overrideBorder);
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.Button("Bordered", new Vec2(100f, 30f));
        ImGui.PopStyleVar();
        ctx.EndFrame();

        Assert.Equal(0f, ctx.Style.FrameBorderSize);

        var drawData = ImGui.GetDrawData();
        Assert.Contains(drawData.DrawLists, list => ContainsColor(list.Vertices.Span, borderColor));

        ImGui.SetCurrentContext(null);
    }

    private static bool ContainsColor(ReadOnlySpan<ImGuiVertex> vertices, Color color)
    {
        var packed = color.PackABGR();
        foreach (var vertex in vertices)
        {
            if (vertex.Color == packed)
            {
                return true;
            }
        }

        return false;
    }
}
