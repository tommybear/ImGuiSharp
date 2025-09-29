using System;
using System.Linq;
using ImGuiSharp;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;
using ImGuiSharp.Rendering.SilkNet;
using Silk.NET.Input;
using ImGuiSharp.Fonts;
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
float scrollY = 0f;
bool[] demoChecks = Enumerable.Repeat(false, 32).ToArray();
float[] demoValues = Enumerable.Repeat(0.5f, 32).ToArray();
int mode = 0;

static float Clamp(float v, float min, float max) => (v < min) ? min : (v > max ? max : v);
// Use raw input for hit-testing to avoid input-lag induced mis-clicks

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

    // Build font atlas from embedded Proggy Clean and upload to GL; set as default font
    var fontBytes = ImGuiSharp.Fonts.EmbeddedProggyClean.GetBytes();
    var atlas = ImGuiSharp.Fonts.FontAtlasBuilder.Build(fontBytes, 18f, 512, 512);
    var handle = pipeline!.RegisterTexture(atlas.PixelsRgba, atlas.Width, atlas.Height);
    context!.SetDefaultFont(atlas, (nint)handle);
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

    context.SetMousePosition(mousePosition);
    context.SetMouseButtonState(ImGuiMouseButton.Left, mouse?.IsButtonPressed(MouseButton.Left) == true);
    context.SetMouseButtonState(ImGuiMouseButton.Right, mouse?.IsButtonPressed(MouseButton.Right) == true);
    ImGui.AddMouseWheel(mouseScroll.X, mouseScroll.Y);

    context.NewFrame();

    ImGui.SetCursorPos(new Vec2(40f, 30f));
    ImGui.SeparatorText("Primary Controls");

    // Primary button: no toggle side-effect; use a stable ID to avoid collisions
    ImGui.SetCursorPos(new Vec2(40f, 70f));
    ImGui.PushID("primary");
    var primaryPressed = ImGui.Button("Primary", new Vec2(200f, 50f));
    ImGui.PopID();
    if (primaryPressed)
    {
        // Demonstrate Text helper by printing a transient message below
        ImGui.SetCursorPos(new Vec2(260f, 40f));
        ImGui.Text("Primary clicked!");
    }

    // Toggle button: clicking this flips the state; also give it a stable ID
    ImGui.SetCursorPos(new Vec2(40f, 130f));
    ImGui.PushID("toggle");
    var togglePressed = ImGui.Button(toggle ? "Toggle On" : "Toggle Off", new Vec2(200f, 45f));
    ImGui.PopID();
    if (togglePressed)
    {
        toggle = !toggle;
    }

    ImGui.SetCursorPos(new Vec2(40f, 190f));
    ImGui.SeparatorText("Mode");
    ImGui.SetCursorPos(new Vec2(40f, 220f));
    ImGui.RadioButton("Mode A", ref mode, 0);
    ImGui.RadioButton("Mode B", ref mode, 1);

    ImGui.SetCursorPos(new Vec2(40f, 280f));
    ImGui.Separator();

    // Demonstrate Label helper (absolute position, not affecting cursor)
    ImGui.Label("Hello, ImGuiSharp!", new Vec2(40f, 300f));

    // Scrollable region demo
    // Scrollable region using BeginChild/EndChild
    ImGui.SetCursorPos(new Vec2(260f, 80f));
    ImGui.SeparatorText("Scrollable Items");
    var regionPos = new Vec2(260f, 110f);
    var regionSize = new Vec2(280f, 160f);
    ImGui.FillRect(regionPos, regionSize, new ImGuiSharp.Math.Color(0.12f, 0.14f, 0.18f, 1f));
    ImGui.SetCursorPos(regionPos);
    ImGui.BeginChild("scrolling-list", regionSize, new Vec2(8f, 8f));
    {
        ImGui.SeparatorText("Items");
        ImGui.Spacing();
        const int itemCount = 20;
        for (int i = 0; i < itemCount; i++)
        {
            ImGui.PushID(i);
            ImGui.Checkbox($"Item {i}", ref demoChecks[i]);
            ImGui.SliderFloat($"Value {i}", ref demoValues[i], 0f, 1f, new ImGuiSharp.Math.Vec2(180f, 18f));
            ImGui.PopID();
        }
    }
    ImGui.EndChild();

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


// (smoothing helper removed; raw input is used for accuracy)
