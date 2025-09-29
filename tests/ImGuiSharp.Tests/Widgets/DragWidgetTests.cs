using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Widgets;

public sealed class DragWidgetTests
{
    [Fact]
    public void DragFloat_RespondsToMouseDelta()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        float value = 0f;

        // Initial frame (no interaction)
        ctx.SetMousePosition(new Vec2(-40f, 10f));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragFloat("Drag", ref value);
        ctx.EndFrame();

        // Press to activate
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.SetMousePosition(new Vec2(10f, 10f));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragFloat("Drag", ref value, speed: 0.1f);
        ctx.EndFrame();

        // Drag to the right
        ctx.SetMousePosition(new Vec2(30f, 10f));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        bool changed = ImGui.DragFloat("Drag", ref value, speed: 0.1f);
        ctx.EndFrame();

        // Release
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragFloat("Drag", ref value, speed: 0.1f);
        ctx.EndFrame();

        Assert.True(changed);
        Assert.True(value > 0f);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void DragFloat_ClampsToRange()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        float value = 0.5f;

        ctx.SetMousePosition(new Vec2(10f, 10f));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragFloat("Clamp", ref value, speed: 0.5f, min: 0f, max: 1f);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.SetMousePosition(new Vec2(10f, 10f));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragFloat("Clamp", ref value, speed: 0.5f, min: 0f, max: 1f);
        ctx.EndFrame();

        ctx.SetMousePosition(new Vec2(-50f, 10f));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragFloat("Clamp", ref value, speed: 0.5f, min: 0f, max: 1f);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragFloat("Clamp", ref value, speed: 0.5f, min: 0f, max: 1f);
        ctx.EndFrame();

        Assert.InRange(value, 0f, 1f);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void DragInt_UsesModifierSteps()
    {
        int baseValue = DragIntScenario(deltaX: 20f, speed: 1f);
        int shiftValue = DragIntScenario(deltaX: 20f, speed: 1f, modifier: ImGuiKey.LeftShift);
        int ctrlValue = DragIntScenario(deltaX: 20f, speed: 1f, modifier: ImGuiKey.LeftCtrl);

        Assert.True(shiftValue > baseValue);
        Assert.True(ctrlValue < baseValue);
    }

    private static int DragIntScenario(float deltaX, float speed, ImGuiKey? modifier = null)
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        int value = 0;

        ctx.SetMousePosition(new Vec2(10f, 10f));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragInt("DragInt", ref value, speed: speed);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.SetMousePosition(new Vec2(10f, 10f));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragInt("DragInt", ref value, speed: speed);
        ctx.EndFrame();

        if (modifier.HasValue)
        {
            ctx.SetKeyState(modifier.Value, true);
        }

        ctx.SetMousePosition(new Vec2(10f + deltaX, 10f));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragInt("DragInt", ref value, speed: speed);
        ctx.EndFrame();

        if (modifier.HasValue)
        {
            ctx.SetKeyState(modifier.Value, false);
        }

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.DragInt("DragInt", ref value, speed: speed);
        ctx.EndFrame();

        ImGui.SetCurrentContext(null);
        return value;
    }
}
