using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Widgets;

public sealed class RadioButtonTests
{
    [Fact]
    public void RadioButton_SelectsOption_OnClick()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(200, 100));
        int value = 0;

        ctx.SetMousePosition(new Vec2(5, 5));

        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.RadioButton("Opt", ref value, 1);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.RadioButton("Opt", ref value, 1);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        var changed = ImGui.RadioButton("Opt", ref value, 1);
        ctx.EndFrame();

        Assert.True(changed);
        Assert.Equal(1, value);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void RadioButton_UsesStyleColors()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(200, 100));

        var markColor = new Color(0.8f, 0.1f, 0.2f, 1f);
        ctx.Style.SetColor(ImGuiCol.CheckMark, markColor);

        int value = 0;

        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.RadioButton("Opt", ref value, 0);
        ctx.EndFrame();

        var drawData = ImGui.GetDrawData();
        Assert.Single(drawData.DrawLists);
        var verts = drawData.DrawLists[0].Vertices.Span;
        Assert.True(verts.Length >= 6);
        var found = false;
        foreach (var vtx in verts)
        {
            if (vtx.Color == markColor.PackABGR())
            {
                found = true;
                break;
            }
        }
        Assert.True(found, "Expected to find check mark color in draw list");

        ImGui.SetCurrentContext(null);
    }
}
