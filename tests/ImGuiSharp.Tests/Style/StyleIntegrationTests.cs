using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Style;

public sealed class StyleIntegrationTests
{
    [Fact]
    public void Button_UsesStyleColors()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        var baseCol = new Color(0.1f, 0.2f, 0.3f, 1f);
        var hoverCol = new Color(0.2f, 0.3f, 0.4f, 1f);
        var activeCol = new Color(0.3f, 0.4f, 0.5f, 1f);
        ctx.Style.SetColor(ImGuiCol.Button, baseCol);
        ctx.Style.SetColor(ImGuiCol.ButtonHovered, hoverCol);
        ctx.Style.SetColor(ImGuiCol.ButtonActive, activeCol);

        ctx.SetMousePosition(new Vec2(1000f, 1000f));
        ctx.SetMousePosition(new Vec2(1000f, 1000f));
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.Button("Styled");
        ctx.EndFrame();

        var drawData = ImGui.GetDrawData();
        Assert.Single(drawData.DrawLists);
        var vertices = drawData.DrawLists[0].Vertices.Span;
        Assert.True(vertices.Length >= 4);
        Assert.Equal(baseCol.PackABGR(), vertices[0].Color);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void Separator_UsesStyleColor()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        var sepColor = new Color(0.5f, 0.1f, 0.2f, 1f);
        ctx.Style.SetColor(ImGuiCol.Separator, sepColor);

        ctx.SetMousePosition(new Vec2(1000f, 1000f));
        ctx.SetMousePosition(new Vec2(1000f, 1000f));
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(10, 10));
        ImGui.Separator();
        ctx.EndFrame();

        var drawData = ImGui.GetDrawData();
        Assert.Single(drawData.DrawLists);
        var first = drawData.DrawLists[0].Vertices.Span[0];
        Assert.Equal(sepColor.PackABGR(), first.Color);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void FramePadding_AffectsButtonAndSliderHeight()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        ctx.Style.FramePadding = new Vec2(6f, 12f);

        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.Button("Pad");
        var buttonRect = ctx.LastItemRect;

        float sliderValue = 0f;
        ImGui.SetCursorPos(new Vec2(0, buttonRect.MaxY + ctx.Style.ItemSpacing.Y));
        ImGui.SliderFloat("S", ref sliderValue, 0f, 1f);
        var sliderRect = ctx.LastItemRect;
        ctx.EndFrame();

        var expectedButtonHeight = ctx.GetLineHeight() + ctx.Style.FramePadding.Y * 2f;
        Assert.Equal(expectedButtonHeight, buttonRect.MaxY - buttonRect.MinY, 1);

        var expectedSliderHeight = MathF.Max(18f, ctx.GetLineHeight() + ctx.Style.FramePadding.Y * 2f);
        Assert.Equal(expectedSliderHeight, sliderRect.MaxY - sliderRect.MinY, 1);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void Slider_UsesGrabColors()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 200));

        var grab = new Color(0.2f, 0.6f, 0.3f, 1f);
        var grabActive = new Color(0.8f, 0.2f, 0.4f, 1f);
        ctx.Style.SetColor(ImGuiCol.SliderGrab, grab);
        ctx.Style.SetColor(ImGuiCol.SliderGrabActive, grabActive);

        float value = 0.5f;
        ctx.SetMousePosition(new Vec2(1000f, 1000f));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.SliderFloat("S", ref value, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        var drawData = ImGui.GetDrawData();
        var list = drawData.DrawLists[0];
        var verts = list.Vertices.Span;
        Assert.True(verts.Length >= 8);
        Assert.Equal(grab.PackABGR(), verts[4].Color);

        // Activate slider with click to ensure active color used
        ctx.SetMousePosition(new Vec2(5f, 5f));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.SliderFloat("S", ref value, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.SliderFloat("S", ref value, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        drawData = ImGui.GetDrawData();
        verts = drawData.DrawLists[0].Vertices.Span;
        Assert.Equal(grabActive.PackABGR(), verts[4].Color);

        ImGui.SetCurrentContext(null);
    }
}
