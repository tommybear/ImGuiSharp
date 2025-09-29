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

        // Press Right for small step while hovered (default: range/100 = 0.01)
        ctx.SetKeyState(ImGuiKey.Right, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        var changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Right, false);
        Assert.True(changed);
        var afterRight = v;
        Assert.Equal(0.51f, afterRight, 3);

        // PageUp for large step
        ctx.SetKeyState(ImGuiKey.PageUp, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.PageUp, false);
        Assert.True(changed);
        Assert.Equal(afterRight + 0.10f, v, 3);

        // End to clamp to max
        ctx.SetKeyState(ImGuiKey.End, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.End, false);
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

    [Fact]
    public void SliderFloat_Modifiers_AdjustStep()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 120));
        float v = 0.5f;

        ctx.SetMousePosition(new Vec2(100, 10));
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        // Shift => coarse (x10)
        ctx.SetKeyState(ImGuiKey.LeftShift, true);
        ctx.SetKeyState(ImGuiKey.Right, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Right, false);
        ctx.SetKeyState(ImGuiKey.LeftShift, false);
        Assert.Equal(0.6f, v, 3); // 0.5 + 0.1

        // Ctrl => fine (/10)
        ctx.SetKeyState(ImGuiKey.LeftCtrl, true);
        ctx.SetKeyState(ImGuiKey.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Left, false);
        ctx.SetKeyState(ImGuiKey.LeftCtrl, false);
        Assert.Equal(0.599f, v, 3); // 0.6 - 0.001

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void SliderFloat_CustomStep_UsesExplicitValue()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 120));
        float v = 0f;

        ctx.SetMousePosition(new Vec2(100, 10));
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f), step: 0.05f);
        ctx.EndFrame();

        ctx.SetKeyState(ImGuiKey.Right, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f), step: 0.05f);
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Right, false);
        Assert.Equal(0.05f, v, 3);

        // Shift multiplies explicit step
        ctx.SetKeyState(ImGuiKey.LeftShift, true);
        ctx.SetKeyState(ImGuiKey.Right, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f), step: 0.05f);
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Right, false);
        ctx.SetKeyState(ImGuiKey.LeftShift, false);
        Assert.Equal(0.55f, v, 3);

        // Ctrl divides explicit step
        ctx.SetKeyState(ImGuiKey.LeftCtrl, true);
        ctx.SetKeyState(ImGuiKey.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f), step: 0.05f);
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Left, false);
        ctx.SetKeyState(ImGuiKey.LeftCtrl, false);
        Assert.Equal(0.545f, v, 3);

        ImGui.SetCurrentContext(null);
    }
}
