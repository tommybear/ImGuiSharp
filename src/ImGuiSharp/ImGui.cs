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
    /// Places the next item on the same line as the previous item.
    /// When <paramref name="xOffset"/> is non-zero, sets the X position relative to the previous item's start.
    /// </summary>
    /// <param name="xOffset">Optional X offset from the previous item's MinX. When 0, uses previous MaxX plus spacing.</param>
    /// <param name="spacing">Spacing in pixels between items when <paramref name="xOffset"/> is 0. Default: 8.</param>
    public static void SameLine(float xOffset = 0f, float spacing = 8f)
    {
        var context = GetCurrentContext();
        var last = context.LastItemRect;

        // If no previous item exists, honor xOffset relative to current X and keep Y.
        if (context.LastItemId == 0 && last.MinX == 0f && last.MaxX == 0f && last.MinY == 0f && last.MaxY == 0f)
        {
            if (xOffset != 0f)
            {
                var cur = context.CursorPos;
                context.SetCursorPos(new Vec2(cur.X + xOffset, cur.Y));
            }
            return;
        }

        float newY = last.MinY;
        float newX = (xOffset != 0f) ? (last.MinX + xOffset) : (last.MaxX + spacing);
        context.SetCursorPos(new Vec2(newX, newY));
    }

    /// <summary>
    /// Inserts a vertical gap using the default item spacing.
    /// </summary>
    public static void Spacing()
    {
        GetCurrentContext().AdvanceCursor(new Vec2(0f, 0f));
    }

    /// <summary>
    /// Calculates the on-screen size of the given text without rendering it.
    /// Supports optional wrapping and hiding after "##" to mirror Dear ImGui.
    /// </summary>
    public static Vec2 CalcTextSize(string text, float wrapWidth = -1f, bool hideAfterDoubleHash = false)
    {
        if (string.IsNullOrEmpty(text))
        {
            return Vec2.Zero;
        }
        if (hideAfterDoubleHash)
        {
            var idx = text.IndexOf("##", StringComparison.Ordinal);
            if (idx >= 0) text = text.Substring(0, idx);
        }

        var ctx = GetCurrentContext();
        var lineH = ctx.GetLineHeight();

        if (wrapWidth <= 0f)
        {
            // No wrapping: account for embedded newlines only
            float maxW = 0f;
            int lines = 0;
            int start = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                bool atEnd = i == text.Length;
                if (atEnd || text[i] == '\n')
                {
                    var span = text.AsSpan(start, i - start).ToString();
                    var w = ctx.MeasureTextWidth(span);
                    if (w > maxW) maxW = w;
                    lines++;
                    start = i + 1;
                }
            }
            if (lines == 0) lines = 1;
            return new Vec2(maxW, lines * lineH);
        }
        else
        {
            // Greedy wrap to wrapWidth and measure
            float maxLineW = 0f;
            int totalLines = 0;
            void MeasureLine(ReadOnlySpan<char> s)
            {
                var w = s.Length == 0 ? 0f : ctx.MeasureTextWidth(s.ToString());
                if (w > maxLineW) maxLineW = w;
                totalLines++;
            }

            int start = 0;
            for (int i = 0; i <= text.Length; i++)
            {
                bool atEnd = i == text.Length;
                if (!atEnd && text[i] != '\n') continue;
                var raw = text.AsSpan(start, i - start);

                if (raw.Length > 0 && ctx.MeasureTextWidth(raw.ToString()) <= wrapWidth)
                {
                    MeasureLine(raw);
                }
                else
                {
                    int tokenStart = 0;
                    float curW = 0f;
                    bool firstToken = true;
                    int lineSegStart = 0;
                    float spaceW = ctx.MeasureTextWidth(" ");
                    for (int j = 0; j <= raw.Length; j++)
                    {
                        bool tokenEnd = j == raw.Length || raw[j] == ' ';
                        if (!tokenEnd) continue;
                        var token = raw.Slice(tokenStart, j - tokenStart);
                        float tokenW = token.Length == 0 ? 0f : ctx.MeasureTextWidth(token.ToString());
                        float sep = firstToken ? 0f : spaceW;
                        if (curW + sep + tokenW <= wrapWidth || firstToken)
                        {
                            curW += sep + tokenW;
                            firstToken = false;
                        }
                        else
                        {
                            // Measure accumulated segment
                            var seg = raw.Slice(lineSegStart, tokenStart - lineSegStart);
                            MeasureLine(seg);
                            // New line
                            lineSegStart = tokenStart;
                            curW = tokenW;
                            firstToken = false;

                            if (tokenW > wrapWidth)
                            {
                                int cStart = tokenStart;
                                float w = 0f;
                                for (int c = tokenStart; c < tokenStart + token.Length; c++)
                                {
                                    var chSpan = raw.Slice(c, 1);
                                    float cw = ctx.MeasureTextWidth(chSpan.ToString());
                                    if (w + cw > wrapWidth && w > 0f)
                                    {
                                        var chunk = raw.Slice(cStart, c - cStart);
                                        MeasureLine(chunk);
                                        cStart = c;
                                        w = 0f;
                                    }
                                    w += cw;
                                }
                                maxLineW = System.MathF.Max(maxLineW, w);
                                lineSegStart = cStart;
                            }
                        }

                        if (j < raw.Length && raw[j] == ' ')
                        {
                            j++;
                            tokenStart = j;
                        }
                        else
                        {
                            tokenStart = j;
                        }
                    }

                    if (lineSegStart < raw.Length)
                    {
                        var seg = raw.Slice(lineSegStart);
                        MeasureLine(seg);
                    }
                    else if (raw.Length == 0)
                    {
                        MeasureLine(ReadOnlySpan<char>.Empty);
                    }
                }

                start = i + 1;
            }
            if (totalLines == 0) totalLines = 1;
            return new Vec2(MathF.Min(maxLineW, wrapWidth), totalLines * lineH);
        }
    }

    /// <summary>
    /// Renders text that wraps to the specified width using a greedy word-wrapping algorithm.
    /// Advances the cursor by the total wrapped height.
    /// </summary>
    public static void TextWrapped(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        var ctx = GetCurrentContext();
        var cursor = ctx.CursorPos;
        var lineH = ctx.GetLineHeight();
        var ascent = ctx.GetAscent();

        int totalLines = 0;

        void EmitLine(ReadOnlySpan<char> s, int lineIndex)
        {
            if (s.Length == 0)
            {
                totalLines++;
                return;
            }
            var baseline = new Vec2(cursor.X, cursor.Y + ascent + lineIndex * lineH);
            ctx.AddText(baseline, s.ToString(), new ImGuiSharp.Math.Color(1f, 1f, 1f, 1f));
            totalLines++;
        }

        // Choose wrapping width from wrap stack or window content width
        bool pushed = false;
        if (!ctx.HasTextWrapPos())
        {
            ctx.PushTextWrapPos(0f); // auto wrap at content region max X
            pushed = true;
        }
        float wrapWidth = ctx.ComputeWrapWidthForCurrentLine();

        // Handle embedded newlines line-by-line
        int start = 0;
        int lineIndex = 0;
        for (int i = 0; i <= text.Length; i++)
        {
            bool atEnd = i == text.Length;
            if (!atEnd && text[i] != '\n')
            {
                continue;
            }
            var raw = text.AsSpan(start, i - start);

            // Fast path: fits in one line
            if (raw.Length > 0 && ctx.MeasureTextWidth(raw.ToString()) <= wrapWidth)
            {
                EmitLine(raw, lineIndex);
                lineIndex++;
            }
            else
            {
                // Greedy wrap on spaces; fall back to character wrapping for long tokens
                int tokenStart = 0;
                float curW = 0f;
                bool firstToken = true;
                int lineSegStart = 0;
                float spaceW = ctx.MeasureTextWidth(" ");
                for (int j = 0; j <= raw.Length; j++)
                {
                    bool tokenEnd = j == raw.Length || raw[j] == ' ';
                    if (!tokenEnd) continue;

                    var token = raw.Slice(tokenStart, j - tokenStart);
                    float tokenW = token.Length == 0 ? 0f : ctx.MeasureTextWidth(token.ToString());
                    float sep = firstToken ? 0f : spaceW;
                    if (curW + sep + tokenW <= wrapWidth || firstToken)
                    {
                        curW += sep + tokenW;
                        firstToken = false;
                    }
                    else
                    {
                        // Emit the accumulated segment [lineSegStart..tokenStart)
                        var seg = raw.Slice(lineSegStart, tokenStart - lineSegStart);
                        EmitLine(seg, lineIndex);
                        lineIndex++;
                        // Start new line with current token
                        lineSegStart = tokenStart;
                        curW = tokenW;
                        firstToken = false;

                        // If single token longer than wrapWidth, break it by characters
                        if (tokenW > wrapWidth)
                        {
                            int cStart = tokenStart;
                            float w = 0f;
                            for (int c = tokenStart; c < tokenStart + token.Length; c++)
                            {
                                var chSpan = raw.Slice(c, 1);
                                float cw = ctx.MeasureTextWidth(chSpan.ToString());
                                if (w + cw > wrapWidth && w > 0f)
                                {
                                    var chunk = raw.Slice(cStart, c - cStart);
                                    EmitLine(chunk, lineIndex);
                                    lineIndex++;
                                    cStart = c;
                                    w = 0f;
                                }
                                w += cw;
                            }
                            // Remainder of the long word becomes start of the next line
                            curW = w;
                            lineSegStart = cStart;
                        }
                    }

                    // If this was a space, extend token past it and continue
                    if (j < raw.Length && raw[j] == ' ')
                    {
                        j++; // consume space
                        tokenStart = j;
                    }
                    else
                    {
                        tokenStart = j;
                    }
                }

                // Emit last segment on this raw line
                if (lineSegStart < raw.Length)
                {
                    var seg = raw.Slice(lineSegStart);
                    EmitLine(seg, lineIndex);
                    lineIndex++;
                }
                else if (raw.Length == 0)
                {
                    // preserve empty line
                    EmitLine(ReadOnlySpan<char>.Empty, lineIndex);
                    lineIndex++;
                }
            }

            start = i + 1;
        }

        // Advance by the number of lines drawn
        if (totalLines > 0)
        {
            ctx.AdvanceCursor(new Vec2(0f, totalLines * lineH));
        }

        if (pushed)
        {
            ctx.PopTextWrapPos();
        }
    }

    /// <summary>
    /// Pushes a text wrap position X in pixels. Pass 0 to wrap at the window content width.
    /// </summary>
    public static void PushTextWrapPos(float wrapPosX = 0f) => GetCurrentContext().PushTextWrapPos(wrapPosX);

    /// <summary>
    /// Pops the last text wrap position.
    /// </summary>
    public static void PopTextWrapPos() => GetCurrentContext().PopTextWrapPos();

    /// <summary>
    /// Checkbox with a text label. Returns true if the value changed.
    /// </summary>
    public static bool Checkbox(string label, ref bool value)
    {
        var context = GetCurrentContext();
        var id = context.GetId(label);
        var cursor = context.CursorPos;
        var lineH = context.GetLineHeight();
        float boxSize = MathF.Min(16f, lineH);
        var rect = new ImGuiSharp.Rendering.ImGuiRect(cursor.X, cursor.Y, cursor.X + boxSize, cursor.Y + boxSize);
        context.RegisterItem(id, rect);

        bool hovered = context.IsMouseHoveringRect(new Vec2(rect.MinX, rect.MinY), new Vec2(rect.MaxX, rect.MaxY));
        if (hovered)
        {
            context.SetHoveredId(id);
        }

        bool changed = false;
        if (context.IsMouseJustPressed(ImGuiMouseButton.Left) && hovered)
        {
            context.SetActiveId(id);
        }
        if (context.ActiveId == id)
        {
            if (context.IsMouseJustReleased(ImGuiMouseButton.Left))
            {
                if (hovered)
                {
                    value = !value;
                    changed = true;
                }
                context.ClearActiveId();
            }
        }

        // Draw box
        var normal = new ImGuiSharp.Math.Color(0.20f, 0.22f, 0.27f, 1f);
        var hoveredCol = new ImGuiSharp.Math.Color(0.28f, 0.30f, 0.36f, 1f);
        var activeCol = new ImGuiSharp.Math.Color(0.33f, 0.36f, 0.43f, 1f);
        var col = (context.ActiveId == id) ? activeCol : (hovered ? hoveredCol : normal);
        FillRect(new Vec2(rect.MinX, rect.MinY), new Vec2(boxSize, boxSize), col);
        if (value)
        {
            // simple check: inner smaller filled rect
            FillRect(new Vec2(rect.MinX + 3, rect.MinY + 3), new Vec2(boxSize - 6, boxSize - 6), new ImGuiSharp.Math.Color(0.9f, 0.9f, 0.95f, 1f));
        }

        // Draw label to the right
        if (!string.IsNullOrEmpty(label))
        {
            var baseline = new Vec2(rect.MaxX + 8f, rect.MinY + context.GetAscent());
            context.AddText(baseline, label, new ImGuiSharp.Math.Color(1f, 1f, 1f, 1f));
        }

        context.AdvanceCursor(new Vec2(0f, lineH));
        return changed;
    }

    /// <summary>
    /// Slider for a float value. Returns true if the value changed.
    /// </summary>
    public static bool SliderFloat(string label, ref float value, float min, float max, Vec2? size = null, string? format = null)
    {
        var context = GetCurrentContext();
        var id = context.GetId(label);
        var cursor = context.CursorPos;
        var lineH = context.GetLineHeight();
        var sz = size ?? new Vec2(200f, MathF.Max(18f, lineH));
        var rect = new ImGuiSharp.Rendering.ImGuiRect(cursor.X, cursor.Y, cursor.X + sz.X, cursor.Y + sz.Y);
        context.RegisterItem(id, rect);

        bool hovered = context.IsMouseHoveringRect(new Vec2(rect.MinX, rect.MinY), new Vec2(rect.MaxX, rect.MaxY));
        if (hovered) context.SetHoveredId(id);

        // Begin drag
        if (context.IsMouseJustPressed(ImGuiMouseButton.Left) && hovered)
        {
            context.SetActiveId(id);
        }

        // While dragging, map mouse X to value
        if (context.ActiveId == id && context.IsMouseDown(ImGuiMouseButton.Left))
        {
            var mx = GetCurrentContext().IO.MousePosition.X;
            var left = rect.MinX + 6f; // padding
            var right = rect.MaxX - 6f;
            var t = (mx - left) / MathF.Max(1f, (right - left));
            t = Clamp01(t);
            var newVal = min + t * (max - min);
            newVal = (newVal < min) ? min : (newVal > max ? max : newVal);
            if (newVal != value)
            {
                value = newVal;
            }
        }

        bool released = context.ActiveId == id && context.IsMouseJustReleased(ImGuiMouseButton.Left);
        if (released)
        {
            context.ClearActiveId();
        }

        // Draw track and knob
        var trackCol = new ImGuiSharp.Math.Color(0.20f, 0.22f, 0.27f, 1f);
        var fillCol = new ImGuiSharp.Math.Color(0.33f, 0.36f, 0.43f, 1f);
        FillRect(new Vec2(rect.MinX, rect.MinY + (sz.Y - 8f) * 0.5f), new Vec2(sz.X, 8f), trackCol);
        var tknob = (value - min) / MathF.Max(0.0001f, (max - min));
        var knobX = rect.MinX + 6f + tknob * MathF.Max(1f, (sz.X - 12f));
        FillRect(new Vec2(knobX - 5f, rect.MinY + 2f), new Vec2(10f, sz.Y - 4f), fillCol);

        // Label & value text
        if (!string.IsNullOrEmpty(label))
        {
            var valText = (format == null) ? value.ToString("0.00") : string.Format(format, value);
            var text = string.Concat(label, ": ", valText);
            var baseline = new Vec2(rect.MinX, rect.MinY - context.GetAscent() + sz.Y + context.GetAscent());
            // draw below slider
            context.AddText(new Vec2(rect.MinX, rect.MaxY + context.GetAscent() * 0.1f), text, new ImGuiSharp.Math.Color(1f, 1f, 1f, 1f));
        }

        context.AdvanceCursor(new Vec2(0f, sz.Y + 4f));
        return released; // true when user released (committed) the drag
    }

    private static float Clamp01(float v) => (v < 0f) ? 0f : (v > 1f ? 1f : v);

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
