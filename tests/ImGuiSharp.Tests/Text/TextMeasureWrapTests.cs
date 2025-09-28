using ImGuiSharp;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Text;

public sealed class TextMeasureWrapTests
{
    [Fact]
    public void CalcTextSize_NoWrap_MeasuresSingleLine()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(800, 600));

        var s = "Hello World";
        var size = ImGui.CalcTextSize(s);
        var w = ctx.MeasureTextWidth(s);
        Assert.Equal(w, size.X, 3);
        Assert.Equal(ctx.GetLineHeight(), size.Y, 3);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void CalcTextSize_Wraps_ToWidth()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(800, 600));

        var s = "wrap this text nicely";
        var wrapW = 50f; // small width to force wrapping
        var size = ImGui.CalcTextSize(s, wrapW);

        Assert.True(size.X <= wrapW + 0.5f);
        Assert.True(size.Y >= ctx.GetLineHeight());

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void TextWrapped_AdvancesCursor_ByWrappedHeight()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(300, 200));

        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(10, 10));
        var before = ImGui.GetCursorPos();

        // Force a narrow wrap region: wrap at current X + 60
        ImGui.PushTextWrapPos(before.X + 60f);
        ImGui.TextWrapped("This should span multiple lines due to narrow width");
        ImGui.PopTextWrapPos();

        var after = ImGui.GetCursorPos();
        ctx.EndFrame();

        Assert.True(after.Y > before.Y + ctx.GetLineHeight() - 0.5f);

        ImGui.SetCurrentContext(null);
    }
}

