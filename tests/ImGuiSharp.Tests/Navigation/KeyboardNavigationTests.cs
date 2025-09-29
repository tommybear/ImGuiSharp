using System;
using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;
using Xunit;

namespace ImGuiSharp.Tests.Navigation;

public sealed class KeyboardNavigationTests : IDisposable
{
    private readonly ImGuiContext _context;
    private bool _checkboxValue;
    private float _sliderValue = 0.5f;
    private int _radioValue;

    private uint _buttonId;
    private uint _checkboxId;
    private uint _sliderId;
    private uint _radioId;

    public KeyboardNavigationTests()
    {
        _context = new ImGuiContext();
        ImGui.SetCurrentContext(_context);
        ImGui.SetDisplaySize(new Vec2(400f, 300f));
        ImGui.UpdateDeltaTime(1f / 60f);
    }

    public void Dispose()
    {
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void TabCyclesFocusForward()
    {
        RunFrame();
        Assert.Equal(0u, _context.FocusedId);

        Assert.Equal(_buttonId, FocusAfterTab());
        Assert.Equal(_checkboxId, FocusAfterTab());
        Assert.Equal(_sliderId, FocusAfterTab());
        Assert.Equal(_radioId, FocusAfterTab());
        Assert.Equal(_buttonId, FocusAfterTab());
    }

    [Fact]
    public void ShiftTabCyclesFocusBackward()
    {
        RunFrame();

        // Move focus to radio using forward tabbing
        FocusAfterTab(); // button
        FocusAfterTab(); // checkbox
        FocusAfterTab(); // slider
        Assert.Equal(_radioId, FocusAfterTab());

        Assert.Equal(_sliderId, FocusAfterTab(reverse: true));
        Assert.Equal(_checkboxId, FocusAfterTab(reverse: true));
        Assert.Equal(_buttonId, FocusAfterTab(reverse: true));
        Assert.Equal(_radioId, FocusAfterTab(reverse: true));
    }

    [Fact]
    public void Checkbox_DrawsNavHighlight_WhenFocused()
    {
        var highlight = new Color(0.9f, 0.2f, 0.1f, 1f);
        _context.Style.SetColor(ImGuiCol.NavHighlight, highlight);

        RunFrame();
        FocusAfterTab(); // button
        FocusAfterTab(); // checkbox

        Assert.Equal(_checkboxId, _context.FocusedId);
        var drawData = ImGui.GetDrawData();
        Assert.Contains(drawData.DrawLists, list => ContainsColor(list.Vertices.Span, highlight));
    }

    [Fact]
    public void Slider_DrawsNavHighlight_WhenFocused()
    {
        var highlight = new Color(0.2f, 0.9f, 0.3f, 1f);
        _context.Style.SetColor(ImGuiCol.NavHighlight, highlight);

        RunFrame();
        FocusAfterTab(); // button
        FocusAfterTab(); // checkbox
        FocusAfterTab(); // slider

        Assert.Equal(_sliderId, _context.FocusedId);
        var drawData = ImGui.GetDrawData();
        Assert.Contains(drawData.DrawLists, list => ContainsColor(list.Vertices.Span, highlight));
    }

    private uint FocusAfterTab(bool reverse = false)
    {
        if (reverse)
        {
            _context.SetKeyState(ImGuiKey.LeftShift, true);
        }

        _context.SetKeyState(ImGuiKey.Tab, true);
        RunFrame();
        var focused = _context.FocusedId;

        _context.SetKeyState(ImGuiKey.Tab, false);
        if (reverse)
        {
            _context.SetKeyState(ImGuiKey.LeftShift, false);
        }

        RunFrame();
        return focused;
    }

    private void RunFrame()
    {
        _context.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);

        ImGui.Button("Button", new Vec2(120f, 30f));
        _buttonId = _context.LastItemId;

        ImGui.Checkbox("Checkbox", ref _checkboxValue);
        _checkboxId = _context.LastItemId;

        ImGui.SliderFloat("Slider", ref _sliderValue, 0f, 1f);
        _sliderId = _context.LastItemId;

        ImGui.RadioButton("Radio", ref _radioValue, 0);
        _radioId = _context.LastItemId;

        _context.EndFrame();
    }

    private static bool ContainsColor(ReadOnlySpan<ImGuiVertex> vertices, Color color)
    {
        uint packed = color.PackABGR();
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
