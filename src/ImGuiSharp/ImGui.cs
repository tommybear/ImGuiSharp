using System;
using System.Collections.Generic;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;
using SMath = System.Math;

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
        var style = context.Style;
        var framePadding = style.FramePadding;
        var id = context.GetId(label);
        var renderLabel = GetRenderedLabel(label);
        var cursor = context.CursorPos;
        var lineHeight = context.GetLineHeight();
        float textWidth = string.IsNullOrEmpty(renderLabel) ? 0f : context.MeasureTextWidth(renderLabel);
        var defaultSize = new Vec2(textWidth + framePadding.X * 2f, lineHeight + framePadding.Y * 2f);
        defaultSize = new Vec2(
            MathF.Max(defaultSize.X, DefaultButtonSize.X),
            MathF.Max(defaultSize.Y, DefaultButtonSize.Y));
        var actualSize = size ?? defaultSize;
        var rectMax = cursor + actualSize;
        var rect = new ImGuiRect(cursor.X, cursor.Y, rectMax.X, rectMax.Y);
        context.RegisterItem(id, rect);

        var behavior = ButtonBehavior(context, rect, id);
        var pressed = behavior.Pressed;

        var normal = style.GetColor(ImGuiCol.Button);
        var hovered = style.GetColor(ImGuiCol.ButtonHovered);
        var active = style.GetColor(ImGuiCol.ButtonActive);
        bool hasFocus = context.FocusedId == id;

        Color drawColor;
        if (behavior.Held)
        {
            drawColor = active;
        }
        else if (behavior.Hovered)
        {
            drawColor = hovered;
        }
        else if (hasFocus)
        {
            drawColor = hovered;
        }
        else
        {
            drawColor = normal;
        }

        context.AddRectFilled(rect, drawColor);
        // Draw label text if default font is set
        if (!string.IsNullOrEmpty(renderLabel))
        {
            var textMin = new Vec2(cursor.X + framePadding.X, cursor.Y + framePadding.Y);
            var textMax = new Vec2(rectMax.X - framePadding.X, rectMax.Y - framePadding.Y);
            var available = new Vec2(MathF.Max(0f, textMax.X - textMin.X), MathF.Max(0f, textMax.Y - textMin.Y));
            var align = style.ButtonTextAlign;
            var posX = textMin.X + (available.X - textWidth) * align.X;
            posX = MathF.Max(posX, textMin.X);
            var baselineY = textMin.Y + (available.Y - lineHeight) * align.Y + context.GetAscent();
            baselineY = MathF.Max(baselineY, textMin.Y + context.GetAscent());
            context.AddText(new Vec2(posX, baselineY), renderLabel, style.GetColor(ImGuiCol.Text));
        }

        RenderNavHighlight(context, rect, id);
        RenderFrameBorder(context, rect);

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
        context.AddText(baseline, text, context.Style.GetColor(ImGuiCol.Text));
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
        context.AddText(baseline, text, context.Style.GetColor(ImGuiCol.Text));
    }

    /// <summary>
    /// Places the next item on the same line as the previous item.
    /// When <paramref name="xOffset"/> is non-zero, sets the X position relative to the previous item's start.
    /// </summary>
    /// <param name="xOffset">Optional X offset from the previous item's MinX. When 0, uses previous MaxX plus spacing.</param>
    /// <param name="spacing">Spacing in pixels between items when <paramref name="xOffset"/> is 0. Default: 8.</param>
    public static void SameLine(float xOffset = 0f, float spacing = -1f)
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
        float effectiveSpacing = spacing < 0f ? context.Style.ItemSpacing.X : spacing;
        float newX = (xOffset != 0f) ? (last.MinX + xOffset) : (last.MaxX + effectiveSpacing);
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
    /// Moves the cursor to the next line, vertically adding the current line height.
    /// </summary>
    public static void NewLine()
    {
        var ctx = GetCurrentContext();
        ctx.AdvanceCursor(new Vec2(0f, ctx.GetLineHeight()));
    }

    /// <summary>
    /// Adds a horizontal separator spanning the current content width.
    /// </summary>
    public static void Separator()
    {
        var ctx = GetCurrentContext();
        var cursor = ctx.CursorPos;
        var spacing = ctx.Style.ItemSpacing;
        var lineHeight = ctx.GetLineHeight();
        var y = cursor.Y + lineHeight * 0.5f;
        var start = new Vec2(cursor.X, y);
        var end = new Vec2(ctx.GetContentRegionMaxX(), y);

        var thickness = 1f;
        var rect = new ImGuiRect(start.X, y - thickness * 0.5f, end.X, y + thickness * 0.5f);
        ctx.AddRectFilled(rect, ctx.Style.GetColor(ImGuiCol.Separator));
        ctx.RegisterItem(0, rect);
        ctx.AdvanceCursor(new Vec2(0f, thickness + spacing.Y));
    }

    /// <summary>
    /// Draws a separator with a centered text label.
    /// </summary>
    public static void SeparatorText(string label)
    {
        ArgumentNullException.ThrowIfNull(label);

        var renderLabel = GetRenderedLabel(label);
        if (string.IsNullOrEmpty(renderLabel))
        {
            Separator();
            return;
        }

        var ctx = GetCurrentContext();
        var cursor = ctx.CursorPos;
        float startX = cursor.X;
        float maxX = ctx.GetContentRegionMaxX();
        float available = MathF.Max(0f, maxX - startX);
        float lineHeight = ctx.GetLineHeight();
        float ascent = ctx.GetAscent();
        float textWidth = ctx.MeasureTextWidth(renderLabel);
        float padding = 6f;
        float thickness = 1f;
        float lineY = cursor.Y + lineHeight * 0.5f;
        var lineColor = ctx.Style.GetColor(ImGuiCol.Separator);

        float textStart = startX;
        if (textWidth + padding * 2f <= available)
        {
            textStart = startX + (available - textWidth) * 0.5f;
        }

        float leftEnd = textStart - padding;
        if (leftEnd > startX)
        {
            ctx.AddRectFilled(new ImGuiRect(startX, lineY - thickness * 0.5f, leftEnd, lineY + thickness * 0.5f), lineColor);
        }

        var textPos = new Vec2(textStart, cursor.Y + ascent);
        ctx.AddText(textPos, renderLabel, ctx.Style.GetColor(ImGuiCol.Text));

        float rightStart = textStart + textWidth + padding;
        if (rightStart < maxX)
        {
            ctx.AddRectFilled(new ImGuiRect(rightStart, lineY - thickness * 0.5f, maxX, lineY + thickness * 0.5f), lineColor);
        }

        ctx.RegisterItem(0, new ImGuiRect(startX, cursor.Y, maxX, cursor.Y + lineHeight));
        ctx.AdvanceCursor(new Vec2(0f, lineHeight + ctx.Style.ItemSpacing.Y));
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
        var style = context.Style;
        var id = context.GetId(label);
        var renderLabel = GetRenderedLabel(label);
        var cursor = context.CursorPos;
        var lineH = context.GetLineHeight();
        float boxSize = MathF.Min(16f, lineH);
        bool hasLabel = !string.IsNullOrEmpty(renderLabel);
        var labelSize = hasLabel ? CalcTextSize(renderLabel) : Vec2.Zero;
        float labelWidth = hasLabel ? (labelSize.X > 0f ? labelSize.X : lineH) : 0f;
        float labelHeight = hasLabel ? (labelSize.Y > 0f ? labelSize.Y : lineH) : 0f;
        float totalWidth = boxSize + (hasLabel ? style.ItemSpacing.X + labelWidth : 0f);
        float totalHeight = MathF.Max(lineH, MathF.Max(boxSize, labelHeight));
        var totalRect = new ImGuiSharp.Rendering.ImGuiRect(cursor.X, cursor.Y, cursor.X + totalWidth, cursor.Y + totalHeight);
        float boxTop = cursor.Y + (totalHeight - boxSize) * 0.5f;
        var boxRect = new ImGuiSharp.Rendering.ImGuiRect(cursor.X, boxTop, cursor.X + boxSize, boxTop + boxSize);
        context.RegisterItem(id, totalRect);

        var behavior = ButtonBehavior(context, totalRect, id);
        bool hovered = behavior.Hovered;
        bool hasFocus = context.FocusedId == id;

        bool changed = false;

        bool keyboardToggle = hasFocus && (context.IsKeyJustPressed(ImGuiKey.Space) || context.IsKeyJustPressed(ImGuiKey.Enter));
        if (keyboardToggle)
        {
            context.SetActiveId(id);
            context.MarkItemPressed(ImGuiMouseButton.Left);
            value = !value;
            changed = true;
            context.MarkItemReleased();
            context.ClearActiveId();
        }

        if (behavior.Pressed)
        {
            value = !value;
            changed = true;
        }

        // Draw box
        var normal = style.GetColor(ImGuiCol.FrameBg);
        var hoveredCol = style.GetColor(ImGuiCol.FrameBgHovered);
        var activeCol = style.GetColor(ImGuiCol.FrameBgActive);
        var textColor = style.GetColor(ImGuiCol.Text);
        var col = (context.ActiveId == id) ? activeCol : ((hovered || hasFocus) ? hoveredCol : normal);
        FillRect(new Vec2(boxRect.MinX, boxRect.MinY), new Vec2(boxSize, boxSize), col);
        if (value)
        {
            // simple check: inner smaller filled rect
            FillRect(new Vec2(boxRect.MinX + 3f, boxRect.MinY + 3f), new Vec2(boxSize - 6f, boxSize - 6f), textColor);
        }

        // Draw label to the right
        if (hasLabel)
        {
            float textX = boxRect.MaxX + style.ItemSpacing.X;
            float textBaseY = cursor.Y + (totalHeight - lineH) * 0.5f + context.GetAscent();
            context.AddText(new Vec2(textX, textBaseY), renderLabel, textColor);
        }

        RenderNavHighlight(context, totalRect, id);
        RenderFrameBorder(context, boxRect);

        context.AdvanceCursor(new Vec2(0f, totalHeight));
        return changed;
    }

    /// <summary>
    /// Slider for a float value. Returns true if the value changed.
    /// </summary>
    public static bool SliderFloat(string label, ref float value, float min, float max, Vec2? size = null, string? format = null, float? step = null)
    {
        var context = GetCurrentContext();
        var style = context.Style;
        var framePadding = style.FramePadding;
        var id = context.GetId(label);
        var renderLabel = GetRenderedLabel(label);
        var cursor = context.CursorPos;
        var lineH = context.GetLineHeight();
        var sz = size ?? new Vec2(200f, MathF.Max(18f, lineH + framePadding.Y * 2f));
        var rect = new ImGuiSharp.Rendering.ImGuiRect(cursor.X, cursor.Y, cursor.X + sz.X, cursor.Y + sz.Y);
        context.RegisterItem(id, rect);

        var behavior = ButtonBehavior(context, rect, id);
        bool hovered = behavior.Hovered;
        bool hasFocus = context.FocusedId == id;

        bool changed = false;
        // While dragging, map mouse X to value
        if (context.ActiveId == id && context.IsMouseDown(ImGuiMouseButton.Left))
        {
            context.MarkItemActive();
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
                changed = true;
                context.MarkItemEdited();
            }
        }

        bool released = behavior.Released;

        // Keyboard control when active or hovered
        if (context.ActiveId == id || hovered || hasFocus)
        {
            var keys = context.GetKeyState();
            float range = max - min;
            float absRange = MathF.Abs(range);
            float baseStep = step.HasValue ? MathF.Abs(step.Value) : ComputeSliderStep(range, format);
            if (baseStep <= 0f)
            {
                baseStep = absRange > 0f ? absRange / 100f : 0.01f;
            }

            bool ctrl = keys.IsPressed(Input.ImGuiKey.LeftCtrl) || keys.IsPressed(Input.ImGuiKey.RightCtrl);
            bool shift = keys.IsPressed(Input.ImGuiKey.LeftShift) || keys.IsPressed(Input.ImGuiKey.RightShift);

            float stepSmall = baseStep;
            float stepLarge = baseStep * 10f;

            if (ctrl)
            {
                stepSmall /= 10f;
                stepLarge /= 10f;
            }

            if (shift)
            {
                stepSmall *= 10f;
                stepLarge *= 10f;
            }

            float v0 = value;
            if (keys.IsPressed(Input.ImGuiKey.Left)) value -= stepSmall;
            if (keys.IsPressed(Input.ImGuiKey.Right)) value += stepSmall;
            if (keys.IsPressed(Input.ImGuiKey.PageDown)) value -= stepLarge;
            if (keys.IsPressed(Input.ImGuiKey.PageUp)) value += stepLarge;
            if (keys.IsPressed(Input.ImGuiKey.Home)) value = min;
            if (keys.IsPressed(Input.ImGuiKey.End)) value = max;

            if (value < min) value = min;
            if (value > max) value = max;
            if (value != v0) changed = true;
            if (changed)
            {
                context.MarkItemEdited();
            }
        }

        // Draw track and knob
        var trackBase = style.GetColor(ImGuiCol.FrameBg);
        var trackHover = style.GetColor(ImGuiCol.FrameBgHovered);
        var trackActive = style.GetColor(ImGuiCol.FrameBgActive);
        var knobBase = style.GetColor(ImGuiCol.SliderGrab);
        var knobActive = style.GetColor(ImGuiCol.SliderGrabActive);
        var textColor = style.GetColor(ImGuiCol.Text);

        var trackCol = context.ActiveId == id ? trackActive : (hovered ? trackHover : trackBase);
        if (hasFocus && context.ActiveId != id && !hovered)
        {
            trackCol = trackHover;
        }
        FillRect(new Vec2(rect.MinX, rect.MinY + (sz.Y - 8f) * 0.5f), new Vec2(sz.X, 8f), trackCol);
        var tknob = (value - min) / MathF.Max(0.0001f, (max - min));
        var knobX = rect.MinX + 6f + tknob * MathF.Max(1f, (sz.X - 12f));
        var knobCol = (context.ActiveId == id) ? knobActive : ((hovered || hasFocus) ? knobActive : knobBase);
        FillRect(new Vec2(knobX - 5f, rect.MinY + 2f), new Vec2(10f, sz.Y - 4f), knobCol);

        // Label & value text
        if (!string.IsNullOrEmpty(renderLabel))
        {
            var valText = (format == null) ? value.ToString("0.00") : string.Format(format, value);
            var text = string.Concat(renderLabel, ": ", valText);
            var baseline = new Vec2(rect.MinX, rect.MinY - context.GetAscent() + sz.Y + context.GetAscent());
            // draw below slider
            context.AddText(new Vec2(rect.MinX, rect.MaxY + context.GetAscent() * 0.1f), text, textColor);
        }

        RenderNavHighlight(context, rect, id);
        RenderFrameBorder(context, rect);

        context.AdvanceCursor(new Vec2(0f, sz.Y + 4f));
        return changed || released; // true when value changed this frame or on commit
    }

    /// <summary>
    /// Draggable float slider.
    /// </summary>
    public static bool DragFloat(string label, ref float value, float speed = 1f, float min = float.NegativeInfinity, float max = float.PositiveInfinity, string? format = null, Vec2? size = null)
    {
        ArgumentNullException.ThrowIfNull(label);

        var context = GetCurrentContext();
        var style = context.Style;
        var id = context.GetId(label);
        var renderLabel = GetRenderedLabel(label);
        var cursor = context.CursorPos;
        var framePadding = style.FramePadding;
        var lineHeight = context.GetLineHeight();

        float width = size?.X ?? 140f;
        float height = size?.Y ?? (lineHeight + framePadding.Y * 2f);
        var frameSize = new Vec2(width, height);
        var rect = new ImGuiRect(cursor.X, cursor.Y, cursor.X + frameSize.X, cursor.Y + frameSize.Y);
        context.RegisterItem(id, rect);

        var behavior = ButtonBehavior(context, rect, id);
        bool hovered = behavior.Hovered;
        bool held = context.ActiveId == id;
        bool hasFocus = context.FocusedId == id;
        bool changed = false;

        if (behavior.Activated)
        {
            context.BeginDragAccum(id);
        }

        if (held && behavior.Held)
        {
            var mouseDelta = context.GetMouseDelta().X;
            if (mouseDelta != 0f)
            {
                float step = mouseDelta * speed;
                if (context.IsKeyDown(ImGuiKey.LeftShift) || context.IsKeyDown(ImGuiKey.RightShift))
                {
                    step *= 10f;
                }
                if (context.IsKeyDown(ImGuiKey.LeftCtrl) || context.IsKeyDown(ImGuiKey.RightCtrl))
                {
                    step /= 10f;
                }

                if (step != 0f)
                {
                    float newValue = value + step;
                    if (!float.IsNegativeInfinity(min) || !float.IsPositiveInfinity(max))
                    {
                        if (min <= max)
                        {
                            newValue = SMath.Clamp(newValue, min, max);
                        }
                    }

                    if (!float.IsNaN(newValue) && newValue != value)
                    {
                        value = newValue;
                        changed = true;
                        context.MarkItemEdited();
                    }
                }
            }
        }

        if (behavior.Released)
        {
            context.ResetDragAccum(id);
        }

        var frameColor = held ? style.GetColor(ImGuiCol.FrameBgActive) : (hovered ? style.GetColor(ImGuiCol.FrameBgHovered) : style.GetColor(ImGuiCol.FrameBg));
        FillRect(new Vec2(rect.MinX, rect.MinY), new Vec2(rect.MaxX - rect.MinX, rect.MaxY - rect.MinY), frameColor);

        string display = format != null ? string.Format(format, value) : value.ToString("0.000");
        var textPos = new Vec2(rect.MinX + framePadding.X, rect.MinY + framePadding.Y + context.GetAscent());
        context.AddText(textPos, display, style.GetColor(ImGuiCol.Text));

        if (!string.IsNullOrEmpty(renderLabel))
        {
            var labelPos = new Vec2(rect.MaxX + style.ItemSpacing.X, rect.MinY + context.GetAscent());
            context.AddText(labelPos, renderLabel, style.GetColor(ImGuiCol.Text));
        }

        RenderNavHighlight(context, rect, id);
        RenderFrameBorder(context, rect);
        context.UpdateItemStatusFlags(hovered, held, behavior.Pressed, behavior.Released, context.FocusedId == id);

        context.AdvanceCursor(new Vec2(0f, frameSize.Y));
        return changed;
    }

    /// <summary>
    /// Draggable integer slider.
    /// </summary>
    public static bool DragInt(string label, ref int value, float speed = 1f, int min = int.MinValue, int max = int.MaxValue, Vec2? size = null)
    {
        ArgumentNullException.ThrowIfNull(label);

        var context = GetCurrentContext();
        var style = context.Style;
        var id = context.GetId(label);
        var renderLabel = GetRenderedLabel(label);
        var cursor = context.CursorPos;
        var framePadding = style.FramePadding;
        var lineHeight = context.GetLineHeight();

        float width = size?.X ?? 140f;
        float height = size?.Y ?? (lineHeight + framePadding.Y * 2f);
        var frameSize = new Vec2(width, height);
        var rect = new ImGuiRect(cursor.X, cursor.Y, cursor.X + frameSize.X, cursor.Y + frameSize.Y);
        context.RegisterItem(id, rect);

        var behavior = ButtonBehavior(context, rect, id);
        bool hovered = behavior.Hovered;
        bool held = context.ActiveId == id;
        bool hasFocus = context.FocusedId == id;
        bool changed = false;

        if (behavior.Activated)
        {
            context.BeginDragAccum(id);
        }

        if (held && behavior.Held)
        {
            var mouseDelta = context.GetMouseDelta().X;
            if (mouseDelta != 0f)
            {
                float step = mouseDelta * speed;
                if (context.IsKeyDown(ImGuiKey.LeftShift) || context.IsKeyDown(ImGuiKey.RightShift))
                {
                    step *= 10f;
                }
                if (context.IsKeyDown(ImGuiKey.LeftCtrl) || context.IsKeyDown(ImGuiKey.RightCtrl))
                {
                    step /= 10f;
                }

                if (step != 0f)
                {
                    float accum = context.AccumulateDragDelta(id, step);
                    int whole = (int)SMath.Truncate(accum);
                    if (whole != 0)
                    {
                        int newValue = value + whole;
                        if (newValue < min) newValue = min;
                        if (newValue > max) newValue = max;
                        if (newValue != value)
                        {
                            value = newValue;
                            changed = true;
                            context.MarkItemEdited();
                        }
                        context.ConsumeDragDelta(id, whole);
                    }
                }
            }
        }

        if (behavior.Released)
        {
            context.ResetDragAccum(id);
        }

        var frameColor = held ? style.GetColor(ImGuiCol.FrameBgActive) : (hovered ? style.GetColor(ImGuiCol.FrameBgHovered) : style.GetColor(ImGuiCol.FrameBg));
        FillRect(new Vec2(rect.MinX, rect.MinY), new Vec2(rect.MaxX - rect.MinX, rect.MaxY - rect.MinY), frameColor);

        string display = value.ToString();
        var textPos = new Vec2(rect.MinX + framePadding.X, rect.MinY + framePadding.Y + context.GetAscent());
        context.AddText(textPos, display, style.GetColor(ImGuiCol.Text));

        if (!string.IsNullOrEmpty(renderLabel))
        {
            var labelPos = new Vec2(rect.MaxX + style.ItemSpacing.X, rect.MinY + context.GetAscent());
            context.AddText(labelPos, renderLabel, style.GetColor(ImGuiCol.Text));
        }

        RenderNavHighlight(context, rect, id);
        RenderFrameBorder(context, rect);
        context.UpdateItemStatusFlags(hovered, held, behavior.Pressed, behavior.Released, context.FocusedId == id);

        context.AdvanceCursor(new Vec2(0f, frameSize.Y));
        return changed;
    }

    /// <summary>
    /// Text input field. Returns true when the buffer changes or, if specified, when Enter is pressed.
    /// </summary>
    public static bool InputText(string label, ref string text, int maxLength = int.MaxValue, ImGuiInputTextFlags flags = ImGuiInputTextFlags.None, Vec2? size = null)
    {
        ArgumentNullException.ThrowIfNull(label);
        ArgumentNullException.ThrowIfNull(text);

        var context = GetCurrentContext();
        var style = context.Style;
        var id = context.GetId(label);
        var renderLabel = GetRenderedLabel(label);
        var cursor = context.CursorPos;
        var framePadding = style.FramePadding;
        var lineHeight = context.GetLineHeight();

        int actualMaxLength = maxLength <= 0 ? int.MaxValue : maxLength;
        if (text.Length > actualMaxLength)
        {
            text = text.Substring(0, actualMaxLength);
        }

        float width = size?.X ?? MathF.Max(120f, context.MeasureTextWidth(text) + framePadding.X * 2f + 20f);
        float height = size?.Y ?? (lineHeight + framePadding.Y * 2f);
        var frameSize = new Vec2(width, height);
        var rect = new ImGuiRect(cursor.X, cursor.Y, cursor.X + frameSize.X, cursor.Y + frameSize.Y);
        context.RegisterItem(id, rect);

        var state = context.InputTextState;
        bool selectAll = (flags & ImGuiInputTextFlags.AutoSelectAll) != 0;
        bool hovered = context.IsMouseHoveringRect(new Vec2(rect.MinX, rect.MinY), new Vec2(rect.MaxX, rect.MaxY));
        if (hovered && id != 0)
        {
            context.SetHoveredId(id);
            context.MarkItemHovered();
        }

        bool clicked = hovered && context.IsMouseJustPressed(ImGuiMouseButton.Left);
        bool focusRequested = context.FocusedId == id && (!state.IsActive || state.Id != id);

        if ((clicked || focusRequested) && state.Id != id)
        {
            state.Activate(id, text, selectAll);
            if (state.Text.Length > actualMaxLength)
            {
                state.SetText(state.Text.Substring(0, actualMaxLength));
                text = state.Text;
            }
            context.SetActiveId(id);
            context.MarkItemActive();
            context.SetFocusId(id);
        }

        IReadOnlyList<IImGuiInputEvent> events = Array.Empty<IImGuiInputEvent>();
        bool valueChanged = false;
        bool submit = false;
        if (state.Id == id && (context.ActiveId == id || context.ActiveId == 0))
        {
            context.SetActiveId(id);
            context.MarkItemActive();
            context.SetFocusId(id);

            var deactivateReason = InputTextDeactivateReason.None;
            events = context.DrainInputEvents();
            bool changed = ProcessInputText(context, state, flags, actualMaxLength, ref text, events, out deactivateReason, out submit);
            if (changed)
            {
                valueChanged = true;
            }

            if (deactivateReason != InputTextDeactivateReason.None)
            {
                state.Deactivate();
                context.ClearActiveId();
                context.MarkItemReleased();
                if (deactivateReason == InputTextDeactivateReason.Submit || deactivateReason == InputTextDeactivateReason.Escape || deactivateReason == InputTextDeactivateReason.Tab)
                {
                    context.SetFocusId(0);
                }
            }
        }
        else if (state.IsActive && state.Id == id && context.ActiveId != id)
        {
            state.Deactivate();
        }

        bool isActive = state.Id == id;
        var displaySource = isActive ? state.Text : text;
        string displayText = (flags & ImGuiInputTextFlags.Password) != 0
            ? new string('*', displaySource.Length)
            : displaySource;

        float availableWidth = frameSize.X - framePadding.X * 2f;
        float textBaseX = rect.MinX + framePadding.X;
        float textBaseY = rect.MinY + framePadding.Y + context.GetAscent();

        if (!isActive)
        {
            state.ScrollX = 0f;
        }

        int caretIndex = isActive ? SMath.Clamp(state.Cursor, 0, displaySource.Length) : displaySource.Length;
        float cursorOffset = context.MeasureTextWidth(displayText.AsSpan(0, caretIndex));
        if (isActive)
        {
            EnsureCursorVisible(state, MathF.Max(1f, availableWidth), cursorOffset);
        }

        var baseColor = style.GetColor(ImGuiCol.FrameBg);
        var hoverColor = style.GetColor(ImGuiCol.FrameBgHovered);
        var activeColor = style.GetColor(ImGuiCol.FrameBgActive);
        var frameColor = context.ActiveId == id ? activeColor : (hovered ? hoverColor : baseColor);
        FillRect(new Vec2(rect.MinX, rect.MinY), new Vec2(rect.MaxX - rect.MinX, rect.MaxY - rect.MinY), frameColor);

        float textStartX = textBaseX - (isActive ? state.ScrollX : 0f);
        var textColor = style.GetColor(ImGuiCol.Text);

        if (isActive && state.HasSelection)
        {
            int selStart = SMath.Min(state.SelectionStart, state.SelectionEnd);
            int selEnd = SMath.Max(state.SelectionStart, state.SelectionEnd);
            selStart = SMath.Clamp(selStart, 0, displaySource.Length);
            selEnd = SMath.Clamp(selEnd, 0, displaySource.Length);
            float selStartX = textStartX + context.MeasureTextWidth(displayText.AsSpan(0, selStart));
            float selEndX = textStartX + context.MeasureTextWidth(displayText.AsSpan(0, selEnd));
            var selRect = new ImGuiRect(selStartX, rect.MinY + framePadding.Y, selEndX, rect.MaxY - framePadding.Y);
            context.AddRectFilled(selRect, style.GetColor(ImGuiCol.TextSelectedBg));
        }

        if (!string.IsNullOrEmpty(displayText))
        {
            context.AddText(new Vec2(textStartX, textBaseY), displayText, textColor);
        }
        else if (!isActive && renderLabel.Length == 0)
        {
            // keep empty fields visible by drawing a zero-width string (no-op)
        }

        if (isActive)
        {
            double time = context.GetTime();
            bool drawCaret = ((int)(time * 1.5f) & 1) == 0;
            if (drawCaret)
            {
                float caretX = textStartX + cursorOffset;
                var caretRect = new ImGuiRect(caretX, rect.MinY + framePadding.Y, caretX + 1f, rect.MaxY - framePadding.Y);
                context.AddRectFilled(caretRect, textColor);
            }
        }

        if (!string.IsNullOrEmpty(renderLabel))
        {
            var labelPos = new Vec2(rect.MaxX + style.ItemSpacing.X, rect.MinY + context.GetAscent());
            context.AddText(labelPos, renderLabel, style.GetColor(ImGuiCol.Text));
        }

        RenderNavHighlight(context, rect, id);
        RenderFrameBorder(context, rect);

        context.UpdateItemStatusFlags(hovered, context.ActiveId == id, false, false, context.FocusedId == id);

        context.AdvanceCursor(new Vec2(0f, frameSize.Y));

        return valueChanged || submit;
    }

    /// <summary>
    /// Radio button helper. Returns true when the selection changes.
    /// </summary>
    public static bool RadioButton(string label, ref int value, int option)
    {
        var context = GetCurrentContext();
        var style = context.Style;
        var id = context.GetId(label);
        var renderLabel = GetRenderedLabel(label);
        var cursor = context.CursorPos;
        var lineH = context.GetLineHeight();
        float radius = MathF.Min(lineH * 0.5f, 7f);
        float diameter = radius * 2f;
        bool hasLabel = !string.IsNullOrEmpty(renderLabel);
        var labelSize = hasLabel ? CalcTextSize(renderLabel) : Vec2.Zero;
        float labelWidth = hasLabel ? (labelSize.X > 0f ? labelSize.X : lineH) : 0f;
        float labelHeight = hasLabel ? (labelSize.Y > 0f ? labelSize.Y : lineH) : 0f;
        float totalWidth = diameter + (hasLabel ? style.ItemSpacing.X + labelWidth : 0f);
        float totalHeight = MathF.Max(lineH, MathF.Max(diameter, labelHeight));
        var totalRect = new ImGuiRect(cursor.X, cursor.Y, cursor.X + totalWidth, cursor.Y + totalHeight);
        float circleTop = cursor.Y + (totalHeight - diameter) * 0.5f;
        var circleRect = new ImGuiRect(cursor.X, circleTop, cursor.X + diameter, circleTop + diameter);
        context.RegisterItem(id, totalRect);

        var behavior = ButtonBehavior(context, totalRect, id);

        bool hasFocus = context.FocusedId == id;

        bool changed = false;
        bool selected = value == option;
        bool keyboardActivate = hasFocus && (context.IsKeyJustPressed(ImGuiKey.Space) || context.IsKeyJustPressed(ImGuiKey.Enter));
        if (keyboardActivate)
        {
            context.SetActiveId(id);
            context.MarkItemPressed(ImGuiMouseButton.Left);
            if (!selected)
            {
                value = option;
                selected = true;
                changed = true;
                context.MarkItemEdited();
            }
            context.MarkItemReleased();
            context.ClearActiveId();
        }

        if (behavior.Pressed && !selected)
        {
            value = option;
            selected = true;
            changed = true;
            context.MarkItemEdited();
        }

        var baseCol = style.GetColor(ImGuiCol.FrameBg);
        var hoverCol = style.GetColor(ImGuiCol.FrameBgHovered);
        var activeCol = style.GetColor(ImGuiCol.FrameBgActive);
        var markCol = style.GetColor(ImGuiCol.CheckMark);
        var drawColor = context.ActiveId == id ? activeCol : ((behavior.Hovered || hasFocus) ? hoverCol : baseCol);
        var center = new Vec2(circleRect.MinX + radius, circleRect.MinY + radius);
        context.AddCircleFilled(center, radius, drawColor, 16);
        if (selected)
        {
            context.AddCircleFilled(center, MathF.Max(1f, radius * 0.45f), markCol, 12);
        }

        if (hasLabel)
        {
            float textX = circleRect.MaxX + style.ItemSpacing.X;
            float textBaseY = cursor.Y + (totalHeight - lineH) * 0.5f + context.GetAscent();
            context.AddText(new Vec2(textX, textBaseY), renderLabel, style.GetColor(ImGuiCol.Text));
        }

        RenderNavHighlight(context, totalRect, id);

        context.AdvanceCursor(new Vec2(0f, totalHeight));
        return changed;
    }

    private static string GetRenderedLabel(string label)
    {
        if (string.IsNullOrEmpty(label)) return string.Empty;
        var idx = label.IndexOf("##", StringComparison.Ordinal);
        return idx >= 0 ? label.Substring(0, idx) : label;
    }

    private static float Clamp01(float v) => (v < 0f) ? 0f : (v > 1f ? 1f : v);

    private static float ComputeSliderStep(float range, string? format)
    {
        float absRange = MathF.Abs(range);
        int precision = GetDecimalPrecision(format);
        if (precision <= 0)
        {
            if (absRange != 0f && absRange <= 100f)
            {
                return 1f;
            }
            if (absRange > 0f)
            {
                return absRange / 100f;
            }
            return 1f;
        }

        return absRange > 0f ? absRange / 100f : MathF.Pow(10f, -precision);
    }

    private static int GetDecimalPrecision(string? format)
    {
        if (string.IsNullOrEmpty(format))
        {
            return 2;
        }

        int dot = format.LastIndexOf('.');
        if (dot >= 0)
        {
            int count = 0;
            for (int i = dot + 1; i < format.Length; i++)
            {
                char c = format[i];
                if (char.IsDigit(c))
                {
                    count++;
                }
                else
                {
                    break;
                }
            }
            return count;
        }

        int idx = format.IndexOf('F');
        if (idx < 0)
        {
            idx = format.IndexOf('f');
        }
        if (idx >= 0 && idx + 1 < format.Length && char.IsDigit(format[idx + 1]))
        {
            int precision = 0;
            for (int i = idx + 1; i < format.Length && char.IsDigit(format[i]); i++)
            {
                precision = (precision * 10) + (format[i] - '0');
            }
            return precision;
        }

        return 0;
    }

    private enum InputTextDeactivateReason
    {
        None,
        Submit,
        Escape,
        Tab,
        Other,
    }

    private static bool ProcessInputText(ImGuiContext context, ImGuiInputTextState state, ImGuiInputTextFlags flags, int maxLength, ref string text, IReadOnlyList<IImGuiInputEvent> events, out InputTextDeactivateReason deactivateReason, out bool submit)
    {
        deactivateReason = InputTextDeactivateReason.None;
        submit = false;

        bool readOnly = (flags & ImGuiInputTextFlags.ReadOnly) != 0;
        bool changed = false;
        if (!readOnly)
        {
            foreach (var evt in events)
            {
                if (evt is ImGuiTextEvent textEvent)
                {
                    char c = textEvent.Character;
                    if (c == '\r')
                    {
                        continue;
                    }

                    if ((flags & ImGuiInputTextFlags.CharsUppercase) != 0)
                    {
                        c = char.ToUpperInvariant(c);
                    }

                    if ((flags & ImGuiInputTextFlags.CharsNoBlank) != 0 && char.IsWhiteSpace(c))
                    {
                        continue;
                    }

                    if (c == '\t' && (flags & ImGuiInputTextFlags.AllowTabInput) == 0)
                    {
                        continue;
                    }

                    if (c == '\n')
                    {
                        continue;
                    }

                    if (state.InsertText(c.ToString(), maxLength))
                    {
                        changed = true;
                    }
                }
            }
        }

        bool shift = context.IsKeyDown(ImGuiKey.LeftShift) || context.IsKeyDown(ImGuiKey.RightShift);
        bool ctrl = context.IsKeyDown(ImGuiKey.LeftCtrl) || context.IsKeyDown(ImGuiKey.RightCtrl);

        if (context.IsKeyJustPressed(ImGuiKey.Left))
        {
            int target = ctrl ? state.FindPrevWordBoundary(state.Cursor) : state.Cursor - 1;
            state.MoveCursorTo(target, shift);
        }
        else if (context.IsKeyJustPressed(ImGuiKey.Right))
        {
            int target = ctrl ? state.FindNextWordBoundary(state.Cursor) : state.Cursor + 1;
            state.MoveCursorTo(target, shift);
        }
        else if (context.IsKeyJustPressed(ImGuiKey.Home))
        {
            state.MoveCursorTo(0, shift);
        }
        else if (context.IsKeyJustPressed(ImGuiKey.End))
        {
            state.MoveCursorTo(state.Text.Length, shift);
        }

        if (!readOnly)
        {
            if (context.IsKeyJustPressed(ImGuiKey.Backspace))
            {
                if (state.DeleteBackspace(ctrl))
                {
                    changed = true;
                }
            }

            if (context.IsKeyJustPressed(ImGuiKey.Delete))
            {
                if (state.DeleteForward(ctrl))
                {
                    changed = true;
                }
            }
        }

        if (ctrl && context.IsKeyJustPressed(ImGuiKey.A))
        {
            state.SelectAll();
        }

        if (context.IsKeyJustPressed(ImGuiKey.Tab))
        {
            if ((flags & ImGuiInputTextFlags.AllowTabInput) != 0 && !readOnly)
            {
                if (state.InsertText("\t", maxLength))
                {
                    changed = true;
                }
            }
            else
            {
                deactivateReason = InputTextDeactivateReason.Tab;
            }
        }

        bool escapePressed = context.IsKeyJustPressed(ImGuiKey.Escape);
        if (escapePressed)
        {
            if (!readOnly && text != state.InitialText)
            {
                text = state.InitialText;
                state.SetText(text);
                state.MoveCursorTo(text.Length, false);
                changed = false;
            }

            deactivateReason = InputTextDeactivateReason.Escape;
        }

        bool enterPressed = context.IsKeyJustPressed(ImGuiKey.Enter) || context.IsKeyJustPressed(ImGuiKey.KeypadEnter);
        if (enterPressed && (flags & ImGuiInputTextFlags.EnterReturnsTrue) != 0)
        {
            submit = true;
            deactivateReason = InputTextDeactivateReason.Submit;
        }

        if (changed)
        {
            string newText = state.Text;
            if (newText.Length > maxLength)
            {
                newText = newText.Substring(0, maxLength);
                state.SetText(newText);
                state.MoveCursorTo(newText.Length, false);
            }

            if (newText != text)
            {
                text = newText;
                context.MarkItemEdited();
            }
            else
            {
                changed = false;
            }
        }

        return changed;
    }

    private static void EnsureCursorVisible(ImGuiInputTextState state, float availableWidth, float cursorOffset)
    {
        if (cursorOffset - state.ScrollX > availableWidth)
        {
            state.ScrollX = cursorOffset - availableWidth;
        }
        if (cursorOffset - state.ScrollX < 0f)
        {
            state.ScrollX = cursorOffset;
        }
        if (state.ScrollX < 0f)
        {
            state.ScrollX = 0f;
        }
    }

    private readonly struct ButtonBehaviorResult
    {
        public ButtonBehaviorResult(bool hovered, bool held, bool pressed, bool released, bool activated)
        {
            Hovered = hovered;
            Held = held;
            Pressed = pressed;
            Released = released;
            Activated = activated;
        }

        public bool Hovered { get; }
        public bool Held { get; }
        public bool Pressed { get; }
        public bool Released { get; }
        public bool Activated { get; }
    }

    private static ButtonBehaviorResult ButtonBehavior(ImGuiContext context, in ImGuiRect rect, uint id)
    {
        var min = new Vec2(rect.MinX, rect.MinY);
        var max = new Vec2(rect.MaxX, rect.MaxY);

        bool hovered = context.IsMouseHoveringRect(min, max);
        if (hovered && id != 0)
        {
            context.SetHoveredId(id);
            context.MarkItemHovered();
        }

        bool activated = false;
        bool pressed = false;
        bool released = false;

        if (hovered && context.IsMouseJustPressed(ImGuiMouseButton.Left))
        {
            context.SetActiveId(id);
            context.MarkItemPressed(ImGuiMouseButton.Left);
            activated = true;
        }

        bool held = false;
        if (context.ActiveId == id)
        {
            held = context.IsMouseDown(ImGuiMouseButton.Left);
            context.MarkItemActive();

            if (!held)
            {
                released = true;
                if (hovered)
                {
                    pressed = true;
                }

                context.MarkItemReleased();
                context.ClearActiveId();
            }
        }

        context.UpdateItemStatusFlags(hovered, held, pressed, released, context.FocusedId == id);
        return new ButtonBehaviorResult(hovered, held, pressed, released, activated);
    }

    private static void RenderNavHighlight(ImGuiContext context, in ImGuiRect rect, uint id)
    {
        if (id == 0 || context.FocusedId != id)
        {
            return;
        }

        var color = context.Style.GetColor(ImGuiCol.NavHighlight);
        if (color.A <= 0f)
        {
            return;
        }

        var highlightRect = ExpandRect(rect, 2f);
        context.AddRect(highlightRect, color, 2f);
    }

    private static void RenderFrameBorder(ImGuiContext context, in ImGuiRect rect)
    {
        float borderSize = context.Style.FrameBorderSize;
        if (borderSize <= 0f)
        {
            return;
        }

        var color = context.Style.GetColor(ImGuiCol.Border);
        if (color.A <= 0f)
        {
            return;
        }

        context.AddRect(rect, color, borderSize);
    }

    private static ImGuiRect ExpandRect(in ImGuiRect rect, float amount)
    {
        if (amount <= 0f)
        {
            return rect;
        }

        return new ImGuiRect(rect.MinX - amount, rect.MinY - amount, rect.MaxX + amount, rect.MaxY + amount);
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

    /// <summary>Pushes a temporary colour override onto the style stack.</summary>
    public static void PushStyleColor(ImGuiCol idx, Color color) => GetCurrentContext().PushStyleColor(idx, color);

    /// <summary>Pops one or more previously pushed style colours.</summary>
    public static void PopStyleColor(int count = 1) => GetCurrentContext().PopStyleColor(count);

    /// <summary>Pushes a temporary Vec2 style override.</summary>
    public static void PushStyleVar(ImGuiStyleVar idx, Vec2 value) => GetCurrentContext().PushStyleVar(idx, value);

    /// <summary>Pushes a temporary float style override.</summary>
    public static void PushStyleVar(ImGuiStyleVar idx, float value) => GetCurrentContext().PushStyleVar(idx, value);

    /// <summary>Pops one or more previously pushed style variables.</summary>
    public static void PopStyleVar(int count = 1) => GetCurrentContext().PopStyleVar(count);

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

    /// <summary>Returns true when the most recently submitted item is hovered by the mouse.</summary>
    public static bool IsItemHovered()
    {
        var ctx = GetCurrentContext();
        if (ctx.LastItemId == 0)
        {
            return false;
        }
        return (ctx.LastItemStatusFlags & ImGuiItemStatusFlags.Hovered) != 0;
    }

    /// <summary>Returns true when the most recently submitted item is active (held).</summary>
    public static bool IsItemActive()
    {
        var ctx = GetCurrentContext();
        if (ctx.LastItemId == 0)
        {
            return false;
        }
        return (ctx.LastItemStatusFlags & ImGuiItemStatusFlags.Active) != 0;
    }

    /// <summary>Returns true when the most recently submitted item owns keyboard focus.</summary>
    public static bool IsItemFocused()
    {
        var ctx = GetCurrentContext();
        if (ctx.LastItemId == 0)
        {
            return false;
        }
        return ctx.LastItemId == ctx.FocusedId;
    }

    /// <summary>Returns true when the most recently submitted item was clicked with the specified button.</summary>
    public static bool IsItemClicked(ImGuiMouseButton button = ImGuiMouseButton.Left)
    {
        var ctx = GetCurrentContext();
        if (ctx.LastItemId == 0)
        {
            return false;
        }

        if (ctx.LastItemPressedButton != button)
        {
            return false;
        }

        return (ctx.LastItemStatusFlags & ImGuiItemStatusFlags.Released) != 0;
    }

    /// <summary>
    /// Returns true when the last item was deactivated (released or lost focus) this frame.
    /// </summary>
    public static bool IsItemDeactivated()
    {
        var ctx = GetCurrentContext();
        if (ctx.LastItemId == 0)
        {
            return false;
        }

        return (ctx.LastItemStatusFlags & ImGuiItemStatusFlags.Deactivated) != 0;
    }

    /// <summary>
    /// Returns true when the last item was deactivated and modified in the same frame.
    /// Mirrors Dear ImGui's IsItemDeactivatedAfterEdit.
    /// </summary>
    public static bool IsItemDeactivatedAfterEdit()
    {
        var ctx = GetCurrentContext();
        if (!IsItemDeactivated())
        {
            return false;
        }

        return ctx.LastItemEditedFrame == ctx.FrameCount;
    }

}
