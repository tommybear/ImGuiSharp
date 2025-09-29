using System;
using System.Collections.Generic;
using ImGuiSharp.Input;
using ImGuiSharp.Math;
using ImGuiSharp.Rendering;
using ImGuiSharp.Fonts;

namespace ImGuiSharp;

/// <summary>
/// Represents the core runtime state for issuing immediate-mode UI commands.
/// </summary>
public sealed class ImGuiContext
{
    private const float DefaultItemSpacingY = 4f; // legacy default; replaced by Style.ItemSpacing.Y

    private readonly List<IImGuiInputEvent> _inputEvents = new();
    private readonly bool[] _mouseButtons = new bool[3];
    private readonly bool[] _mouseButtonsPrev = new bool[3];
    private readonly Dictionary<ImGuiKey, bool> _keyStates = new();
    private readonly Dictionary<ImGuiKey, bool> _keyStatesPrev = new();
    private readonly Stack<uint> _idStack = new();
    private readonly ImGuiDrawListBuilder _drawListBuilder = new();
    private readonly List<uint> _focusableItems = new();

    private float _time;
    private Vec2 _mousePosition = Vec2.Zero;
    private Vec2 _cursorPos = Vec2.Zero;
    private float _pendingMouseWheelX;
    private float _pendingMouseWheelY;
    private FontAtlas? _fontAtlas;
    private IntPtr _fontTexture;
    private readonly Dictionary<string, float> _textWidthCache = new();
    private readonly Stack<WindowState> _windowStack = new();
    private readonly System.Collections.Generic.Dictionary<string, float> _windowScrollY = new();
    private readonly System.Collections.Generic.Dictionary<string, ScrollbarState> _windowScrollbar = new();
    private readonly Stack<float> _wrapPosStack = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiContext"/> class.
    /// </summary>
    /// <param name="io">The IO configuration for the context. If null, a new instance is created.</param>
    public ImGuiContext(ImGuiIO? io = null)
    {
        IO = io ?? new ImGuiIO();
        Style = new ImGuiStyle();
    }

    /// <summary>
    /// Gets the IO configuration associated with this context.
    /// </summary>
    public ImGuiIO IO { get; }

    /// <summary>
    /// Global style configuration (spacing, padding, etc.).
    /// </summary>
    public ImGuiStyle Style { get; }

    /// <summary>
    /// Gets a value indicating whether a frame is currently in progress.
    /// </summary>
    public bool IsFrameStarted { get; private set; }

    /// <summary>
    /// Gets the number of frames that have completed since the context was created.
    /// </summary>
    public uint FrameCount { get; private set; }

    /// <summary>
    /// Gets the ID of the item currently active (held by mouse button).
    /// </summary>
    public uint ActiveId { get; private set; }

    /// <summary>
    /// Gets the ID of the item most recently hovered this frame.
    /// </summary>
    public uint HoveredId { get; private set; }

    /// <summary>
    /// Gets the ID of the most recently submitted item.
    /// </summary>
    public uint LastItemId { get; private set; }

    /// <summary>
    /// Gets the bounding rectangle of the most recently submitted item.
    /// </summary>
    public ImGuiRect LastItemRect { get; private set; }

    /// <summary>
    /// Gets flags describing the state of the most recently submitted item.
    /// </summary>
    public ImGuiItemStatusFlags LastItemStatusFlags { get; private set; }

    /// <summary>Gets the mouse button that most recently pressed the last item.</summary>
    public ImGuiMouseButton LastItemPressedButton { get; private set; }

    /// <summary>Gets the ID of the item considered focused for navigation.</summary>
    public uint FocusedId { get; private set; }

    /// <summary>Gets the frame index when the last item was edited.</summary>
    public uint LastItemEditedFrame { get; private set; }

    /// <summary>
    /// Gets the current cursor position used for automatic layout.
    /// </summary>
    public Vec2 CursorPos => _cursorPos;

    /// <summary>
    /// Begins a new frame, preparing internal state for UI commands.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a frame is already in progress.</exception>
    public void NewFrame()
    {
        if (IsFrameStarted)
        {
            throw new InvalidOperationException("NewFrame called while a frame is already in progress.");
        }

        HoveredId = 0;
        LastItemId = 0;
        LastItemRect = default;
        LastItemStatusFlags = ImGuiItemStatusFlags.None;
        LastItemPressedButton = ImGuiMouseButton.Left;
        LastItemEditedFrame = 0;
        IO.MouseWheel = _pendingMouseWheelY;
        IO.MouseWheelH = _pendingMouseWheelX;
        _pendingMouseWheelX = 0f;
        _pendingMouseWheelY = 0f;
        _drawListBuilder.Reset();
        _focusableItems.Clear();
        IsFrameStarted = true;
    }

    /// <summary>
    /// Ends the current frame, finalizing draw command generation.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when no frame has been started.</exception>
    public void EndFrame()
    {
        if (!IsFrameStarted)
        {
            throw new InvalidOperationException("EndFrame called without a matching NewFrame call.");
        }

        ProcessNavigation();

        IsFrameStarted = false;
        FrameCount++;
        Array.Copy(_mouseButtons, _mouseButtonsPrev, _mouseButtons.Length);
        SyncKeyStates();
    }

    /// <summary>
    /// Updates the delta time and accumulates total elapsed time.
    /// </summary>
    /// <param name="deltaSeconds">Elapsed time since the previous frame in seconds.</param>
    public void UpdateDeltaTime(float deltaSeconds)
    {
        if (deltaSeconds < 0f)
        {
            throw new ArgumentOutOfRangeException(nameof(deltaSeconds), "Delta time must be non-negative.");
        }

        IO.DeltaTime = deltaSeconds;
        _time += deltaSeconds;
        IO.Time = _time;
    }

    /// <summary>
    /// Gets the accumulated time since the context was created.
    /// </summary>
    public float GetTime() => _time;

    /// <summary>
    /// Updates the tracked mouse position.
    /// </summary>
    /// <param name="x">The X coordinate in pixels.</param>
    /// <param name="y">The Y coordinate in pixels.</param>
    public void SetMousePosition(float x, float y) => SetMousePosition(new Vec2(x, y));

    /// <summary>
    /// Updates the tracked mouse position.
    /// </summary>
    /// <param name="position">Mouse position in pixels.</param>
    public void SetMousePosition(in Vec2 position)
    {
        _mousePosition = position;
        IO.MousePosition = position;
    }

    /// <summary>
    /// Updates the pressed state for a specific mouse button.
    /// </summary>
    /// <param name="button">The button to update.</param>
    /// <param name="isPressed">True when pressed, false when released.</param>
    public void SetMouseButtonState(ImGuiMouseButton button, bool isPressed)
    {
        var index = (int)button;
        if ((uint)index >= _mouseButtons.Length)
        {
            throw new ArgumentOutOfRangeException(nameof(button));
        }

        _mouseButtons[index] = isPressed;
        IO.SetMouseButton(button, isPressed);
    }

    /// <summary>
    /// Returns a snapshot of the current mouse position and button states.
    /// </summary>
    public ImGuiMouseStateSnapshot GetMouseState() => new(_mousePosition, _mouseButtons);

    /// <summary>
    /// Updates the pressed state for the provided key.
    /// </summary>
    /// <param name="key">The logical key.</param>
    /// <param name="isPressed">True when pressed, false when released.</param>
    public void SetKeyState(ImGuiKey key, bool isPressed)
    {
        if (isPressed)
        {
            _keyStates[key] = true;
        }
        else
        {
            _keyStates.Remove(key);
        }
    }

    /// <summary>
    /// Returns a snapshot of the logical key state map.
    /// </summary>
    public ImGuiKeyStateSnapshot GetKeyState() => new(_keyStates);

    /// <summary>
    /// Sets the cursor position explicitly.
    /// </summary>
    public void SetCursorPos(in Vec2 position)
    {
        _cursorPos = position;
    }

    /// <summary>
    /// Advances the cursor position after an item of the given size.
    /// </summary>
    public void AdvanceCursor(in Vec2 itemSize)
    {
        var spacingY = Style.ItemSpacing.Y;
        _cursorPos = new Vec2(_cursorPos.X, _cursorPos.Y + itemSize.Y + spacingY);
    }

    /// <summary>
    /// Enqueues an input event to be processed by the next frame.
    /// </summary>
    /// <param name="inputEvent">The input event to enqueue.</param>
    public void AddInputEvent(IImGuiInputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);
        _inputEvents.Add(inputEvent);
    }

    /// <summary>
    /// Returns all queued input events and clears the internal buffer.
    /// </summary>
    /// <returns>A snapshot of queued input events.</returns>
    public IReadOnlyList<IImGuiInputEvent> DrainInputEvents()
    {
        if (_inputEvents.Count == 0)
        {
            return Array.Empty<IImGuiInputEvent>();
        }

        var snapshot = _inputEvents.ToArray();
        _inputEvents.Clear();
        return snapshot;
    }

    /// <summary>
    /// Computes an ID for the specified label, taking the ID stack into account.
    /// </summary>
    internal uint GetId(string label) => GetId(label.AsSpan());

    internal uint GetId(ReadOnlySpan<char> label)
    {
        uint seed = _windowStack.Count > 0 ? _windowStack.Peek().Id : ImGuiId.FnvOffset;
        foreach (var stackId in _idStack)
        {
            seed = ImGuiId.Combine(seed, stackId);
        }

        return ImGuiId.Hash(label, seed);
    }

    internal void PushId(uint id)
    {
        _idStack.Push(id);
    }

    internal void PushId(string label)
    {
        _idStack.Push(GetId(label));
    }

    internal void PopId()
    {
        if (_idStack.Count == 0)
        {
            throw new InvalidOperationException("PopID called with an empty stack.");
        }

        _idStack.Pop();
    }

    internal void SetHoveredId(uint id)
    {
        HoveredId = id;
    }

    internal void SetActiveId(uint id)
    {
        ActiveId = id;
    }

    internal void ClearActiveId()
    {
        ActiveId = 0;
    }

    internal void AddRectFilled(in ImGuiRect rect, Color color)
    {
        _drawListBuilder.AddRectFilled(rect, color);
    }

    internal void AddRect(in ImGuiRect rect, Color color, float thickness = 1f)
    {
        _drawListBuilder.AddRect(rect, color, thickness);
    }

    internal void AddCircleFilled(Vec2 center, float radius, Color color, int segments = 12)
    {
        _drawListBuilder.AddCircleFilled(center, radius, color, segments);
    }

    internal void SetCurrentTexture(IntPtr textureId)
    {
        _drawListBuilder.SetTexture(textureId);
    }

    internal void PushClipRect(in ImGuiRect rect)
    {
        _drawListBuilder.PushClipRect(rect);
    }

    internal void PopClipRect()
    {
        _drawListBuilder.PopClipRect();
    }

    /// <summary>Sets the default font atlas and associated texture ID.</summary>
    public void SetDefaultFont(FontAtlas atlas, IntPtr textureId)
    {
        _fontAtlas = atlas;
        _fontTexture = textureId;
        _textWidthCache.Clear();
    }

    /// <summary>Measures the width of the provided text string using the default font.</summary>
    public float MeasureTextWidth(string text)
    {
        if (_fontAtlas is null || string.IsNullOrEmpty(text))
        {
            return 0f;
        }
        if (_textWidthCache.TryGetValue(text, out var cached))
        {
            return cached;
        }

        float w = 0f;
        char prev = '\0';
        foreach (var ch in text)
        {
            if (_fontAtlas.Glyphs.TryGetValue(ch, out var g))
            {
                if (prev != '\0' && _fontAtlas.TryGetKerning(prev, ch, out var k))
                {
                    w += k;
                }
                w += g.Advance;
                prev = ch;
            }
        }
        _textWidthCache[text] = w;
        return w;
    }

    internal void AddText(in Vec2 baselinePos, string text, Color color)
    {
        if (_fontAtlas is null || string.IsNullOrEmpty(text))
        {
            return;
        }
        SetCurrentTexture(_fontTexture);
        float x = baselinePos.X;
        float y = baselinePos.Y;
        char prev = '\0';
        foreach (var ch in text)
        {
            if (!_fontAtlas.Glyphs.TryGetValue(ch, out var g))
            {
                continue;
            }

            if (prev != '\0' && _fontAtlas.TryGetKerning(prev, ch, out var k))
            {
                x += k;
            }

            // Compute glyph rectangle in pixels from atlas UVs
            // Width/height in pixels
            float gw = (g.U1 - g.U0) * _fontAtlas.Width;
            float gh = (g.V1 - g.V0) * _fontAtlas.Height;

            float x0 = x + g.OffsetX;
            float y0 = y + g.OffsetY;
            float x1 = x0 + gw;
            float y1 = y0 + gh;

            _drawListBuilder.AddQuad(x0, y0, x1, y1, g.U0, g.V0, g.U1, g.V1, color);

            x += g.Advance;
            prev = ch;
        }
    }

    internal void AddMouseWheel(float wheelX, float wheelY)
    {
        _pendingMouseWheelX += wheelX;
        _pendingMouseWheelY += wheelY;
    }

    internal void RegisterItem(uint id, in ImGuiRect rect)
    {
        LastItemId = id;
        LastItemRect = rect;
        LastItemStatusFlags = ImGuiItemStatusFlags.None;
        LastItemPressedButton = ImGuiMouseButton.Left;
        LastItemEditedFrame = 0;
        if (id != 0)
        {
            _focusableItems.Add(id);
        }
        if (FocusedId == id)
        {
            LastItemStatusFlags |= ImGuiItemStatusFlags.Focused;
        }
    }

    internal void UpdateItemStatusFlags(bool hovered, bool held, bool pressed, bool released, bool focused)
    {
        var flags = ImGuiItemStatusFlags.None;
        if (hovered) flags |= ImGuiItemStatusFlags.Hovered;
        if (held) flags |= ImGuiItemStatusFlags.Held;
        if (pressed) flags |= ImGuiItemStatusFlags.Pressed;
        if (released) flags |= ImGuiItemStatusFlags.Released;
        if (ActiveId == LastItemId) flags |= ImGuiItemStatusFlags.Active;
        if (focused) flags |= ImGuiItemStatusFlags.Focused;
        LastItemStatusFlags |= flags;
    }

    internal void MarkItemActive()
    {
        LastItemStatusFlags |= ImGuiItemStatusFlags.Active | ImGuiItemStatusFlags.Held;
    }

    internal void MarkItemHovered()
    {
        LastItemStatusFlags |= ImGuiItemStatusFlags.Hovered;
    }

    internal void MarkItemPressed(ImGuiMouseButton button)
    {
        LastItemStatusFlags |= ImGuiItemStatusFlags.Pressed | ImGuiItemStatusFlags.Held | ImGuiItemStatusFlags.Active;
        LastItemPressedButton = button;
    }

    internal void MarkItemReleased()
    {
        LastItemStatusFlags |= ImGuiItemStatusFlags.Released | ImGuiItemStatusFlags.Deactivated;
        LastItemStatusFlags &= ~ImGuiItemStatusFlags.Held;
        LastItemStatusFlags &= ~ImGuiItemStatusFlags.Active;
        FocusedId = LastItemId;
    }

    internal void MarkItemEdited()
    {
        LastItemStatusFlags |= ImGuiItemStatusFlags.Edited;
        LastItemEditedFrame = FrameCount;
    }

    internal bool IsMouseHoveringRect(Vec2 min, Vec2 max)
    {
        var pos = _mousePosition;
        return pos.X >= min.X && pos.X <= max.X && pos.Y >= min.Y && pos.Y <= max.Y;
    }

    internal bool IsMouseDown(ImGuiMouseButton button)
    {
        var index = (int)button;
        return (uint)index < _mouseButtons.Length && _mouseButtons[index];
    }

    internal bool IsKeyDown(ImGuiKey key)
    {
        return _keyStates.TryGetValue(key, out var value) && value;
    }

    internal bool IsKeyJustPressed(ImGuiKey key)
    {
        var down = _keyStates.TryGetValue(key, out var value) && value;
        var prev = _keyStatesPrev.TryGetValue(key, out var prevValue) && prevValue;
        return down && !prev;
    }

    internal bool IsMouseJustPressed(ImGuiMouseButton button)
    {
        var index = (int)button;
        if ((uint)index >= _mouseButtons.Length)
        {
            return false;
        }

        return _mouseButtons[index] && !_mouseButtonsPrev[index];
    }

    internal bool IsMouseJustReleased(ImGuiMouseButton button)
    {
        var index = (int)button;
        if ((uint)index >= _mouseButtons.Length)
        {
            return false;
        }

        return !_mouseButtons[index] && _mouseButtonsPrev[index];
    }

    /// <summary>Builds draw data for the current frame.</summary>
    public ImGuiDrawData GetDrawData()
    {
        var displaySize = IO.DisplaySize;
        var displayRect = new ImGuiRect(0f, 0f, displaySize.X, displaySize.Y);
        var drawList = _drawListBuilder.Build();
        if (drawList.Vertices.Length == 0 || drawList.Indices.Length == 0)
        {
            return new ImGuiDrawData(Array.Empty<ImGuiDrawList>(), displayRect);
        }

        return new ImGuiDrawData(new[] { drawList }, displayRect);
    }

    /// <summary>Gets the current font line height.</summary>
    public float GetLineHeight() => _fontAtlas?.LineHeight ?? 16f;

    /// <summary>Gets the current font ascent (baseline offset).</summary>
    public float GetAscent() => _fontAtlas?.Ascent ?? 12f;

    internal void PushTextWrapPos(float wrapPosX)
    {
        _wrapPosStack.Push(wrapPosX);
    }

    internal void PopTextWrapPos()
    {
        if (_wrapPosStack.Count > 0)
        {
            _wrapPosStack.Pop();
        }
    }

    internal bool HasTextWrapPos() => _wrapPosStack.Count > 0;

    internal float GetContentRegionMaxX()
    {
        if (_windowStack.Count > 0)
        {
            var ws = _windowStack.Peek();
            return ws.Pos.X + ws.Size.X - ws.Padding.X;
        }
        return IO.DisplaySize.X;
    }

    internal float ComputeWrapWidthForCurrentLine()
    {
        float wrapPosX = _wrapPosStack.Count > 0 ? _wrapPosStack.Peek() : 0f;
        float targetX = wrapPosX <= 0f ? GetContentRegionMaxX() : wrapPosX;
        float width = MathF.Max(1f, targetX - _cursorPos.X);
        return width;
    }

    private sealed class WindowState
    {
        public string Name = string.Empty;
        public Vec2 Pos;
        public Vec2 Size;
        public Vec2 Padding;
        public float ScrollY;
        public Vec2 PrevCursorPos;
        public float ContentStartY;
        public bool IsChild;
        public uint Id;
    }

    internal void BeginWindow(string name, in Vec2 pos, in Vec2 size, in Vec2 padding, bool isChild)
    {
        var ws = new WindowState
        {
            Name = name,
            Pos = pos,
            Size = size,
            Padding = padding,
            IsChild = isChild,
            PrevCursorPos = _cursorPos,
        };

        var parentId = _windowStack.Count > 0 ? _windowStack.Peek().Id : ImGuiId.FnvOffset;
        ws.Id = ImGuiId.Hash(name.AsSpan(), parentId);

        // Restore persistent scroll
        if (_windowScrollY.TryGetValue(name, out var persistedScroll))
        {
            ws.ScrollY = persistedScroll;
        }

        // Apply mouse wheel to scroll if hovered
        var rect = new ImGuiRect(pos.X, pos.Y, pos.X + size.X, pos.Y + size.Y);
        var hovered = IsMouseHoveringRect(pos, new Vec2(pos.X + size.X, pos.Y + size.Y));
        if (hovered)
        {
            var dy = IO.MouseWheel;
            if (MathF.Abs(dy) > 0f)
            {
                ws.ScrollY = MathF.Max(0f, ws.ScrollY - dy * 40f);
            }
        }

        // Set cursor to content origin (account for padding and scroll)
        _cursorPos = new Vec2(pos.X + padding.X, pos.Y + padding.Y - ws.ScrollY);
        ws.ContentStartY = _cursorPos.Y;

        // Clip to window rect
        PushClipRect(rect);
        _windowStack.Push(ws);
    }

    internal void EndWindow()
    {
        if (_windowStack.Count == 0)
        {
            throw new InvalidOperationException("End/EndChild called with no matching Begin.");
        }

        var ws = _windowStack.Pop();
        // Compute content height and clamp scroll to content height
        var innerH = ws.Size.Y - (ws.Padding.Y * 2f);
        var contentH = (_cursorPos.Y - ws.ContentStartY) + ws.Padding.Y;
        if (contentH > 0 && innerH > 0)
        {
            var maxScroll = MathF.Max(0f, contentH - innerH);
            ws.ScrollY = Clamp(ws.ScrollY, 0f, maxScroll);
        }

        // Draw and handle vertical scrollbar if needed
        if (contentH > innerH + 0.5f)
        {
            float barW = 10f;
            float trackX = ws.Pos.X + ws.Size.X - barW;
            float trackTop = ws.Pos.Y + ws.Padding.Y;
            float trackBottom = ws.Pos.Y + ws.Size.Y - ws.Padding.Y;
            float trackH = MathF.Max(1f, trackBottom - trackTop);
            float scrollRange = MathF.Max(1f, contentH - innerH);
            float thumbH = MathF.Max(20f, (innerH * trackH) / contentH);
            float thumbTravel = MathF.Max(1f, trackH - thumbH);
            float thumbTop = trackTop + (ws.ScrollY / scrollRange) * thumbTravel;
            float thumbBottom = thumbTop + thumbH;

            // Track
            AddRectFilled(new ImGuiRect(trackX, trackTop, trackX + barW, trackBottom), new Color(0.2f, 0.2f, 0.25f, 0.6f));

            // Interaction
            var mp = _mousePosition;
            bool thumbHovered = mp.X >= trackX && mp.X <= trackX + barW && mp.Y >= thumbTop && mp.Y <= thumbBottom;
            var id = GetId(ws.Name + "##scrollbar");
            if (thumbHovered && IsMouseJustPressed(ImGuiMouseButton.Left))
            {
                SetActiveId(id);
                _windowScrollbar[ws.Name] = new ScrollbarState { Dragging = true, StartMouseY = mp.Y, StartScrollY = ws.ScrollY, Id = id };
            }

            if (IsMouseDown(ImGuiMouseButton.Left) && ActiveId == id && _windowScrollbar.TryGetValue(ws.Name, out var drag))
            {
                float delta = mp.Y - drag.StartMouseY;
                float newScroll = drag.StartScrollY + (delta * (scrollRange / thumbTravel));
                ws.ScrollY = Clamp(newScroll, 0f, scrollRange);
                // Recompute thumb after scroll
                thumbTop = trackTop + (ws.ScrollY / scrollRange) * thumbTravel;
                thumbBottom = thumbTop + thumbH;
            }

            if (IsMouseJustReleased(ImGuiMouseButton.Left) && ActiveId == id)
            {
                ClearActiveId();
                if (_windowScrollbar.TryGetValue(ws.Name, out var s))
                {
                    s.Dragging = false;
                    _windowScrollbar[ws.Name] = s;
                }
            }

            var thumbColor = (thumbHovered || ActiveId == id) ? new Color(0.85f, 0.85f, 0.90f, 0.95f) : new Color(0.75f, 0.75f, 0.80f, 0.85f);
            AddRectFilled(new ImGuiRect(trackX + 1f, thumbTop, trackX + barW - 1f, thumbBottom), thumbColor);
        }

        _cursorPos = ws.PrevCursorPos;
        PopClipRect();

        // Persist scroll for this window name
        _windowScrollY[ws.Name] = ws.ScrollY;

        // Advance cursor by window size if it was a child positioned at current cursor
        if (ws.IsChild)
        {
            AdvanceCursor(new Vec2(0f, ws.Size.Y + DefaultItemSpacingY));
        }
    }

    private void ProcessNavigation()
    {
        if (_focusableItems.Count == 0)
        {
            return;
        }

        bool shiftDown = IsKeyDown(ImGuiKey.LeftShift) || IsKeyDown(ImGuiKey.RightShift);
        bool moveForward = IsKeyJustPressed(ImGuiKey.Tab) && !shiftDown;
        bool moveBackward = IsKeyJustPressed(ImGuiKey.Tab) && shiftDown;

        if (!moveForward && !moveBackward)
        {
            if (IsKeyJustPressed(ImGuiKey.Right) || IsKeyJustPressed(ImGuiKey.Down))
            {
                moveForward = true;
            }
            else if (IsKeyJustPressed(ImGuiKey.Left) || IsKeyJustPressed(ImGuiKey.Up))
            {
                moveBackward = true;
            }
        }

        if (moveForward)
        {
            FocusNextItem(reverse: false);
        }
        else if (moveBackward)
        {
            FocusNextItem(reverse: true);
        }
    }

    private void FocusNextItem(bool reverse)
    {
        if (_focusableItems.Count == 0)
        {
            return;
        }

        int index;
        if (FocusedId == 0)
        {
            index = reverse ? _focusableItems.Count - 1 : 0;
        }
        else
        {
            index = _focusableItems.IndexOf(FocusedId);
            if (index == -1)
            {
                index = 0;
            }

            index = reverse
                ? (index - 1 + _focusableItems.Count) % _focusableItems.Count
                : (index + 1) % _focusableItems.Count;
        }

        FocusedId = _focusableItems[index];
    }

    private void SyncKeyStates()
    {
        _keyStatesPrev.Clear();
        foreach (var kvp in _keyStates)
        {
            _keyStatesPrev[kvp.Key] = kvp.Value;
        }
    }

    private static float Clamp(float v, float min, float max) => (v < min) ? min : (v > max ? max : v);

    private struct ScrollbarState
    {
        public bool Dragging;
        public float StartMouseY;
        public float StartScrollY;
        public uint Id;
    }
}
