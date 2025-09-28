using System;
using System.Collections.Generic;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;

namespace ImGuiSharp;

/// <summary>
/// Static entry point mirroring the traditional Dear ImGui API surface.
/// </summary>
public static class ImGui
{
    private static readonly Vec2 DefaultButtonSize = new(120f, 36f);
    private static ImGuiContext? _currentContext;

    /// <summary>
    /// Sets the current context. Pass <c>null</c> to clear the active context.
    /// </summary>
    /// <param name="context">The context to make current.</param>
    public static void SetCurrentContext(ImGuiContext? context) => _currentContext = context;

    /// <summary>
    /// Gets the current context or throws if none has been assigned.
    /// </summary>
    /// <returns>The active <see cref="ImGuiContext"/>.</returns>
    /// <exception cref="InvalidOperationException">Thrown when no context is active.</exception>
    public static ImGuiContext GetCurrentContext()
    {
        return _currentContext ?? throw new InvalidOperationException("ImGui context has not been set.");
    }

    /// <summary>
    /// Provides direct access to the IO configuration of the active context.
    /// </summary>
    /// <returns>The IO object for the active context.</returns>
    public static ImGuiIO GetIO() => GetCurrentContext().IO;

    /// <summary>
    /// Begins a new frame on the active context.
    /// </summary>
    public static void NewFrame() => GetCurrentContext().NewFrame();

    /// <summary>
    /// Ends the current frame on the active context.
    /// </summary>
    public static void EndFrame() => GetCurrentContext().EndFrame();

    /// <summary>
    /// Updates the delta time for the active context.
    /// </summary>
    /// <param name="deltaSeconds">Elapsed time since the previous frame.</param>
    public static void UpdateDeltaTime(float deltaSeconds) => GetCurrentContext().UpdateDeltaTime(deltaSeconds);

    /// <summary>
    /// Gets the accumulated time since the context was created.
    /// </summary>
    public static float GetTime() => GetCurrentContext().GetTime();

    /// <summary>
    /// Sets the display size of the active context.
    /// </summary>
    public static void SetDisplaySize(Vec2 size) => GetCurrentContext().IO.DisplaySize = size;

    /// <summary>
    /// Gets the display size of the active context.
    /// </summary>
    public static Vec2 GetDisplaySize() => GetCurrentContext().IO.DisplaySize;

    /// <summary>
    /// Sets the current mouse cursor position.
    /// </summary>
    public static void SetMousePosition(float x, float y) => SetMousePosition(new Vec2(x, y));

    /// <summary>
    /// Sets the current mouse cursor position.
    /// </summary>
    public static void SetMousePosition(Vec2 position) => GetCurrentContext().SetMousePosition(position);

    /// <summary>
    /// Updates a mouse button state.
    /// </summary>
    public static void SetMouseButtonState(ImGuiMouseButton button, bool isPressed) => GetCurrentContext().SetMouseButtonState(button, isPressed);

    /// <summary>
    /// Gets the snapshot of the mouse state for the active context.
    /// </summary>
    public static ImGuiMouseStateSnapshot GetMouseState() => GetCurrentContext().GetMouseState();

    /// <summary>
    /// Updates the logical key state.
    /// </summary>
    public static void SetKeyState(ImGuiKey key, bool isPressed) => GetCurrentContext().SetKeyState(key, isPressed);

    /// <summary>
    /// Gets a snapshot of the logical key state map.
    /// </summary>
    public static ImGuiKeyStateSnapshot GetKeyState() => GetCurrentContext().GetKeyState();

    /// <summary>
    /// Sets the cursor position for the upcoming item.
    /// </summary>
    public static void SetCursorPos(Vec2 position) => GetCurrentContext().SetCursorPos(position);

    /// <summary>
    /// Gets the current cursor position.
    /// </summary>
    public static Vec2 GetCursorPos() => GetCurrentContext().CursorPos;

    /// <summary>
    /// Pushes an identifier onto the ID stack using a string seed.
    /// </summary>
    public static void PushID(string id)
    {
        ArgumentNullException.ThrowIfNull(id);
        GetCurrentContext().PushId(id);
    }

    /// <summary>
    /// Pushes an identifier onto the ID stack using an integer seed.
    /// </summary>
    public static void PushID(int id) => GetCurrentContext().PushId(unchecked((uint)id));

    /// <summary>
    /// Pops the most recently pushed ID from the stack.
    /// </summary>
    public static void PopID() => GetCurrentContext().PopId();

    /// <summary>
    /// Queues an input event for the active context.
    /// </summary>
    /// <param name="inputEvent">The input event to enqueue.</param>
    public static void AddInputEvent(IImGuiInputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);
        GetCurrentContext().AddInputEvent(inputEvent);
    }

    /// <summary>
    /// Drains the queued input events from the active context.
    /// </summary>
    /// <returns>A snapshot of input events queued since the last drain.</returns>
    public static IReadOnlyList<IImGuiInputEvent> DrainInputEvents()
    {
        return GetCurrentContext().DrainInputEvents();
    }

    /// <summary>
    /// Queues a mouse wheel delta for the active context.
    /// </summary>
    /// <param name="wheelX">Horizontal wheel delta.</param>
    /// <param name="wheelY">Vertical wheel delta.</param>
    public static void AddMouseWheel(float wheelX, float wheelY) => GetCurrentContext().AddMouseWheel(wheelX, wheelY);

    /// <summary>
    /// Retrieves the draw data generated during the last frame.
    /// </summary>
    public static ImGuiDrawData GetDrawData() => GetCurrentContext().GetDrawData();

    /// <summary>
    /// Renders an interactive button and returns true when the button is clicked.
    /// </summary>
    /// <param name="label">Display label for the button.</param>
    /// <param name="size">Optional size. When omitted a default size is used.</param>
    public static bool Button(string label, Vec2? size = null)
    {
        ArgumentNullException.ThrowIfNull(label);

        var context = GetCurrentContext();
        var id = context.GetId(label);
        var cursor = context.CursorPos;
        var actualSize = size ?? DefaultButtonSize;
        var rectMax = cursor + actualSize;
        var rect = new ImGuiRect(cursor.X, cursor.Y, rectMax.X, rectMax.Y);
        context.RegisterItem(id, rect);

        var isHovered = context.IsMouseHoveringRect(cursor, rectMax);
        if (isHovered)
        {
            context.SetHoveredId(id);
        }

        var pressed = false;
        if (context.IsMouseJustPressed(ImGuiMouseButton.Left) && isHovered)
        {
            context.SetActiveId(id);
        }

        if (context.ActiveId == id)
        {
            if (context.IsMouseJustReleased(ImGuiMouseButton.Left))
            {
                if (isHovered)
                {
                    pressed = true;
                }

                context.ClearActiveId();
            }
        }

        var normal = new Color(0.20f, 0.22f, 0.27f);
        var hovered = new Color(0.28f, 0.30f, 0.36f);
        var active = new Color(0.33f, 0.36f, 0.43f);

        Color drawColor;
        if (context.ActiveId == id && context.IsMouseDown(ImGuiMouseButton.Left))
        {
            drawColor = active;
        }
        else if (isHovered)
        {
            drawColor = hovered;
        }
        else
        {
            drawColor = normal;
        }

        context.AddRectFilled(rect, drawColor);
        // Draw label text if default font is set
        if (!string.IsNullOrEmpty(label))
        {
            var textWidth = context.MeasureTextWidth(label);
            var fontLine = 16f; // fallback if no font
            var atlasField = typeof(ImGuiContext).GetField("_fontAtlas", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (atlasField?.GetValue(context) is ImGuiSharp.Fonts.FontAtlas atlas)
            {
                fontLine = atlas.LineHeight;
            }
            var textPos = new Vec2(
                cursor.X + (actualSize.X - textWidth) * 0.5f,
                cursor.Y + (actualSize.Y - fontLine) * 0.5f + (atlasField?.GetValue(context) is ImGuiSharp.Fonts.FontAtlas a ? a.Ascent : 0f));
            context.AddText(textPos, label, new ImGuiSharp.Math.Color(1f, 1f, 1f, 1f));
        }

        context.AdvanceCursor(actualSize);
        return pressed;
    }

    /// <summary>
    /// Renders a line of text at the current cursor, advancing the cursor vertically.
    /// </summary>
    public static void Text(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var context = GetCurrentContext();
        var baseline = new Vec2(context.CursorPos.X, context.CursorPos.Y + context.GetAscent());
        context.AddText(baseline, text, new ImGuiSharp.Math.Color(1f, 1f, 1f, 1f));
        context.AdvanceCursor(new Vec2(0f, context.GetLineHeight()));
    }

    /// <summary>
    /// Renders a non-interactive label at an absolute position. Does not change cursor.
    /// </summary>
    public static void Label(string text, Vec2 position)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var context = GetCurrentContext();
        var baseline = new Vec2(position.X, position.Y + context.GetAscent());
        context.AddText(baseline, text, new ImGuiSharp.Math.Color(1f, 1f, 1f, 1f));
    }

    /// <summary>
    /// Sets a clip rectangle for subsequent draw calls. Use PopClipRect to restore.
    /// </summary>
    public static void PushClipRect(Vec2 min, Vec2 max)
    {
        GetCurrentContext().PushClipRect(new ImGuiSharp.Rendering.ImGuiRect(min.X, min.Y, max.X, max.Y));
    }

    /// <summary>
    /// Pops the current clip rectangle.
    /// </summary>
    public static void PopClipRect() => GetCurrentContext().PopClipRect();

    /// <summary>
    /// Fills a rectangle with a solid color.
    /// </summary>
    public static void FillRect(Vec2 pos, Vec2 size, ImGuiSharp.Math.Color color)
    {
        var context = GetCurrentContext();
        var rect = new ImGuiSharp.Rendering.ImGuiRect(pos.X, pos.Y, pos.X + size.X, pos.Y + size.Y);
        context.AddRectFilled(rect, color);
    }

    /// <summary>
    /// Begins a window at an absolute position and size. Call End() to close.
    /// </summary>
    public static void Begin(string name, Vec2 pos, Vec2 size, Vec2? padding = null)
    {
        GetCurrentContext().BeginWindow(name, pos, size, padding ?? new Vec2(8f, 8f), isChild: false);
    }

    /// <summary>
    /// Ends the current window.
    /// </summary>
    public static void End()
    {
        GetCurrentContext().EndWindow();
    }

    /// <summary>
    /// Begins a child region at the current cursor with the given size. Call EndChild() to close.
    /// </summary>
    public static void BeginChild(string id, Vec2 size, Vec2? padding = null)
    {
        var ctx = GetCurrentContext();
        ctx.BeginWindow(id, ctx.CursorPos, size, padding ?? new Vec2(8f, 8f), isChild: true);
    }

    /// <summary>
    /// Ends the current child region.
    /// </summary>
    public static void EndChild()
    {
        GetCurrentContext().EndWindow();
    }

}
