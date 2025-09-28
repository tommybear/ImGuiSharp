using System;
using System.Linq;
using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;
using ImGuiSharp.Rendering.SilkNet;
using Silk.NET.Input;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

var options = WindowOptions.Default with
{
    Title = "ImGuiSharp Sandbox",
    Size = new Silk.NET.Maths.Vector2D<int>(960, 640),
    API = WindowOptions.Default.API with
    {
        API = ContextAPI.OpenGL,
        Profile = ContextProfile.Core,
        Version = new APIVersion(3, 3)
    }
};

using var window = Window.Create(options);
SilkNetRenderPipeline? pipeline = null;
GL? gl = null;
ImGuiContext? context = null;
IInputContext? inputContext = null;
IMouse? mouse = null;
IKeyboard? keyboard = null;
var toggle = false;
var mousePosition = new Vec2(-100f, -100f);
var mouseScroll = new Vec2(0f, 0f);
var smoothedMousePosition = new Vec2(-100f, -100f);
var smoothedButtons = new float[3];

window.Load += () =>
{
    gl = GL.GetApi(window);
    pipeline = new SilkNetRenderPipeline(gl);

    context = new ImGuiContext();
    ImGui.SetCurrentContext(context);

    inputContext = window.CreateInput();
    mouse = inputContext.Mice.FirstOrDefault();
    keyboard = inputContext.Keyboards.FirstOrDefault();

    // Reduce tearing/flicker on some platforms
    window.VSync = true;

    if (keyboard is not null)
    {
        keyboard.KeyDown += (_, key, _) => HandleKey(key, true);
        keyboard.KeyUp += (_, key, _) => HandleKey(key, false);
        keyboard.KeyChar += (_, character) => ImGui.AddInputEvent(new ImGuiTextEvent(character));
    }
};

window.Update += deltaTime =>
{
    if (context is null)
    {
        return;
    }

    if (mouse is not null)
    {
        var pos = mouse.Position;
        mousePosition = new Vec2((float)pos.X, (float)pos.Y);
        var wheel = mouse.ScrollWheels.FirstOrDefault();
        mouseScroll = new Vec2((float)wheel.X, (float)wheel.Y);
    }
    else
    {
        mouseScroll = Vec2.Zero;
    }

    const float smoothing = 0.25f;
    smoothedMousePosition = Vec2.Lerp(smoothedMousePosition, mousePosition, smoothing);

    if (mouse is not null)
    {
        smoothedButtons[(int)ImGuiMouseButton.Left] = SmoothButton(smoothedButtons[(int)ImGuiMouseButton.Left], mouse.IsButtonPressed(MouseButton.Left), smoothing);
        smoothedButtons[(int)ImGuiMouseButton.Right] = SmoothButton(smoothedButtons[(int)ImGuiMouseButton.Right], mouse.IsButtonPressed(MouseButton.Right), smoothing);
    }

    context.SetMousePosition(smoothedMousePosition);
    context.SetMouseButtonState(ImGuiMouseButton.Left, smoothedButtons[(int)ImGuiMouseButton.Left] > 0.5f);
    context.SetMouseButtonState(ImGuiMouseButton.Right, smoothedButtons[(int)ImGuiMouseButton.Right] > 0.5f);
    ImGui.AddMouseWheel(mouseScroll.X, mouseScroll.Y);

    context.NewFrame();

    ImGui.SetCursorPos(new Vec2(40f, 40f));
    var pressed = ImGui.Button("Primary", new Vec2(200f, 50f));
    if (pressed)
    {
        toggle = !toggle;
    }

    ImGui.SetCursorPos(new Vec2(40f, 110f));
    ImGui.Button(toggle ? "Toggle On" : "Toggle Off", new Vec2(200f, 45f));

    context.EndFrame();
};

window.Render += deltaTime =>
{
    if (gl is null || pipeline is null || context is null)
    {
        return;
    }

    context.UpdateDeltaTime((float)deltaTime);

    var framebufferSize = window.FramebufferSize;
    var displaySize = new Vec2(framebufferSize.X, framebufferSize.Y);
    ImGui.SetDisplaySize(displaySize);

    pipeline.BeginFrame();

    gl.Viewport(0, 0, (uint)framebufferSize.X, (uint)framebufferSize.Y);
    gl.ClearColor(0.08f, 0.09f, 0.12f, 1.0f);
    // Clear must not be scissored; temporarily disable scissor to avoid flicker
    gl.Disable(GLEnum.ScissorTest);
    gl.Clear(ClearBufferMask.ColorBufferBit);
    gl.Enable(GLEnum.ScissorTest);

    var drawData = ImGui.GetDrawData();
    if (drawData.TotalVtxCount > 0)
    {
        pipeline.Render(drawData);
    }

    pipeline.EndFrame();
};

window.Closing += () =>
{
    pipeline?.Dispose();
    inputContext?.Dispose();
    ImGui.SetCurrentContext(null);
};

window.Run();

void HandleKey(Key key, bool isPressed)
{
    if (context is null)
    {
        return;
    }

    var mapped = TryMapKey(key);
    if (mapped.HasValue)
    {
        context.SetKeyState(mapped.Value, isPressed);
    }
}

ImGuiKey? TryMapKey(Key key) => key switch
{
    Key.Enter => ImGuiKey.Enter,
    Key.Space => ImGuiKey.Space,
    Key.Escape => ImGuiKey.Escape,
    Key.Tab => ImGuiKey.Tab,
    Key.Backspace => ImGuiKey.Backspace,
    Key.ShiftLeft => ImGuiKey.LeftShift,
    Key.ShiftRight => ImGuiKey.RightShift,
    Key.ControlLeft => ImGuiKey.LeftCtrl,
    Key.ControlRight => ImGuiKey.RightCtrl,
    Key.AltLeft => ImGuiKey.LeftAlt,
    Key.AltRight => ImGuiKey.RightAlt,
    Key.Left => ImGuiKey.Left,
    Key.Right => ImGuiKey.Right,
    Key.Up => ImGuiKey.Up,
    Key.Down => ImGuiKey.Down,
    Key.Number0 => ImGuiKey.D0,
    Key.Number1 => ImGuiKey.D1,
    Key.Number2 => ImGuiKey.D2,
    Key.Number3 => ImGuiKey.D3,
    Key.Number4 => ImGuiKey.D4,
    Key.Number5 => ImGuiKey.D5,
    Key.Number6 => ImGuiKey.D6,
    Key.Number7 => ImGuiKey.D7,
    Key.Number8 => ImGuiKey.D8,
    Key.Number9 => ImGuiKey.D9,
    Key.A => ImGuiKey.A,
    Key.B => ImGuiKey.B,
    Key.C => ImGuiKey.C,
    _ => null
};


float SmoothButton(float current, bool target, float smoothing)
{
    var desired = target ? 1f : 0f;
    return current + (desired - current) * smoothing;
}
