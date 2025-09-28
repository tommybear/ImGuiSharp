using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Widgets;

public sealed class SliderFloatParityTests
{
    [Fact]
    public void SliderFloat_ReturnsTrue_OnDragChange()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(300, 100));
        float v = 0.0f;

        // Press to activate
        ctx.SetMousePosition(new Vec2(10, 10));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        // Drag to change value
        ctx.SetMousePosition(new Vec2(150, 10));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        var changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        Assert.True(changed);
        Assert.True(v > 0.0f);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void SliderFloat_Keyboard_Steps_And_Clamps()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 120));
        float v = 0.5f;

        // Hover over the slider; use keyboard without dragging
        ctx.SetMousePosition(new Vec2(100, 10));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        // Press Right for small step while hovered
        ctx.SetKeyState(ImGuiKey.Right, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        var changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        Assert.True(changed);
        var afterRight = v;
        Assert.True(afterRight > 0.5f);

        // PageUp for large step
        ctx.SetKeyState(ImGuiKey.Right, false);
        ctx.SetKeyState(ImGuiKey.PageUp, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        Assert.True(changed);
        Assert.True(v > afterRight);

        // End to clamp to max
        ctx.SetKeyState(ImGuiKey.PageUp, false);
        ctx.SetKeyState(ImGuiKey.End, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        Assert.True(changed);
        Assert.Equal(1f, v, 3);

        // Try increasing beyond max: should clamp and not change further
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        Assert.False(changed);

        ImGui.SetCurrentContext(null);
    }
}
