using ImGuiSharp;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Widgets;

public sealed class HiddenLabelTests
{
    [Fact]
    public void Button_HiddenLabel_IdsDiffer_UsingFullLabel()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.Button("Play##A", new Vec2(100, 30));
        var firstId = ctx.LastItemId;
        ImGui.Button("Play##B", new Vec2(100, 30));
        var secondId = ctx.LastItemId;
        ctx.EndFrame();

        Assert.NotEqual(firstId, secondId);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void Checkbox_HiddenLabel_IdsDiffer_UsingFullLabel()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));
        bool a = false, b = false;

        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.Checkbox("Same##1", ref a);
        var firstId = ctx.LastItemId;
        ImGui.Checkbox("Same##2", ref b);
        var secondId = ctx.LastItemId;
        ctx.EndFrame();

        Assert.NotEqual(firstId, secondId);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void CalcTextSize_HideAfterDoubleHash_TrimsMeasurement()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));
        var full = "Hello##id";
        var trimmed = "Hello";
        var s1 = ImGui.CalcTextSize(full, -1, hideAfterDoubleHash: true);
        var s2 = ImGui.CalcTextSize(trimmed);
        Assert.Equal(s2.X, s1.X, 3);
        Assert.Equal(s2.Y, s1.Y, 3);
        ImGui.SetCurrentContext(null);
    }
}

