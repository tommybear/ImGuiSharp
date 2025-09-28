using System;
using ImGuiSharp;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Widgets;

public sealed class CheckboxSliderTests
{
    [Fact]
    public void Checkbox_Toggles_OnClick()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));

        bool val = false;

        // Frame 1: hover, no click
        ctx.SetMousePosition(new Vec2(5, 5));
        ctx.SetMouseButtonState(Input.ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        var changed = ImGui.Checkbox("Check", ref val);
        ctx.EndFrame();
        Assert.False(changed);
        Assert.False(val);

        // Frame 2: press
        ctx.SetMouseButtonState(Input.ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        changed = ImGui.Checkbox("Check", ref val);
        ctx.EndFrame();
        Assert.False(changed);
        Assert.False(val);

        // Frame 3: release on the box
        ctx.SetMouseButtonState(Input.ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        changed = ImGui.Checkbox("Check", ref val);
        ctx.EndFrame();
        Assert.True(changed);
        Assert.True(val);

        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void Checkbox_UsesIdStack_ForDuplicateLabels()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400, 300));
        bool a = false, b = false;

        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.PushID(1);
        ImGui.Checkbox("Same", ref a);
        var firstId = ctx.LastItemId;
        ImGui.PopID();
        ImGui.PushID(2);
        ImGui.Checkbox("Same", ref b);
        var secondId = ctx.LastItemId;
        ImGui.PopID();
        ctx.EndFrame();

        Assert.NotEqual(firstId, secondId);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void SliderFloat_Maps_MouseX_To_Value()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(600, 200));
        float v = 0.0f;

        // Hover
        ctx.SetMousePosition(new Vec2(10, 10));
        ctx.SetMouseButtonState(Input.ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();
        Assert.Equal(0f, v, 3);

        // Press at ~25% of the track
        ctx.SetMousePosition(new Vec2(50, 10));
        ctx.SetMouseButtonState(Input.ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        // Drag to ~75%
        ctx.SetMousePosition(new Vec2(150, 10));
        ctx.SetMouseButtonState(Input.ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        // Release
        ctx.SetMouseButtonState(Input.ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(new Vec2(0, 0));
        var changed = ImGui.SliderFloat("S", ref v, 0f, 1f, new Vec2(200f, 20f));
        ctx.EndFrame();

        Assert.True(changed);
        Assert.InRange(v, 0.70f, 0.80f);

        ImGui.SetCurrentContext(null);
    }
}

