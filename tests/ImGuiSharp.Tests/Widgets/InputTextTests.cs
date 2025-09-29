using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Widgets;

public sealed class InputTextTests
{
    [Fact]
    public void InputText_Typing_AppendsCharacters()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        string text = "Hi";

        ctx.SetMousePosition(new Vec2(5f, 5f));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Name", ref text);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Name", ref text);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Name", ref text);
        ctx.EndFrame();

        ImGui.AddInputEvent(new ImGuiTextEvent('a'));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Name", ref text);
        ctx.EndFrame();

        ImGui.AddInputEvent(new ImGuiTextEvent('b'));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Name", ref text);
        ctx.EndFrame();

        Assert.Equal("Hiab", text);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void InputText_Backspace_RemovesCharacter()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        string text = "Test";
        Activate(ctx, ref text, "Edit");

        ctx.SetKeyState(ImGuiKey.Backspace, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Edit", ref text);
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Backspace, false);

        Assert.Equal("Tes", text);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void InputText_EnterReturnsTrue_Submits()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        string text = "Value";
        Activate(ctx, ref text, "Submit", ImGuiInputTextFlags.EnterReturnsTrue);

        ctx.SetKeyState(ImGuiKey.Enter, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        bool submitted = ImGui.InputText("Submit", ref text, flags: ImGuiInputTextFlags.EnterReturnsTrue);
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Enter, false);

        Assert.True(submitted);
        Assert.Equal(0u, ctx.ActiveId);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void InputText_AutoSelectAll_ReplacesText()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        string text = "Hello";
        Activate(ctx, ref text, "Field", ImGuiInputTextFlags.AutoSelectAll);

        ImGui.AddInputEvent(new ImGuiTextEvent('Z'));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Field", ref text, flags: ImGuiInputTextFlags.AutoSelectAll);
        ctx.EndFrame();

        Assert.Equal("Z", text);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void InputText_AllowTabInput_InsertsTab()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        string text = string.Empty;
        Activate(ctx, ref text, "Code", ImGuiInputTextFlags.AllowTabInput);

        ctx.SetKeyState(ImGuiKey.Tab, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Code", ref text, flags: ImGuiInputTextFlags.AllowTabInput);
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Tab, false);

        Assert.Equal("\t", text);
        ImGui.SetCurrentContext(null);
    }

    [Fact]
    public void InputText_Escape_RevertsToInitial()
    {
        var ctx = new ImGuiContext();
        ImGui.SetCurrentContext(ctx);
        ImGui.SetDisplaySize(new Vec2(400f, 200f));

        string text = "Initial";
        Activate(ctx, ref text, "Esc");

        ImGui.AddInputEvent(new ImGuiTextEvent('X'));
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText("Esc", ref text);
        ctx.EndFrame();

        ctx.SetKeyState(ImGuiKey.Escape, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        bool returned = ImGui.InputText("Esc", ref text);
        ctx.EndFrame();
        ctx.SetKeyState(ImGuiKey.Escape, false);

        Assert.False(returned);
        Assert.Equal("Initial", text);
        Assert.Equal(0u, ctx.ActiveId);
        ImGui.SetCurrentContext(null);
    }

    private static void Activate(ImGuiContext ctx, ref string text, string label, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None)
    {
        ctx.SetMousePosition(new Vec2(5f, 5f));
        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText(label, ref text, flags: flags);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, true);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText(label, ref text, flags: flags);
        ctx.EndFrame();

        ctx.SetMouseButtonState(ImGuiMouseButton.Left, false);
        ctx.NewFrame();
        ImGui.SetCursorPos(Vec2.Zero);
        ImGui.InputText(label, ref text, flags: flags);
        ctx.EndFrame();
    }
}
