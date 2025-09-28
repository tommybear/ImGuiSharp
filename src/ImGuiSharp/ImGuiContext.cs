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
    private const float DefaultItemSpacingY = 4f;

    private readonly List<IImGuiInputEvent> _inputEvents = new();
    private readonly bool[] _mouseButtons = new bool[3];
    private readonly bool[] _mouseButtonsPrev = new bool[3];
    private readonly Dictionary<ImGuiKey, bool> _keyStates = new();
    private readonly Stack<uint> _idStack = new();
    private readonly ImGuiDrawListBuilder _drawListBuilder = new();

    private float _time;
    private Vec2 _mousePosition = Vec2.Zero;
    private Vec2 _cursorPos = Vec2.Zero;
    private float _pendingMouseWheelX;
    private float _pendingMouseWheelY;
    private FontAtlas? _fontAtlas;
    private IntPtr _fontTexture;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImGuiContext"/> class.
    /// </summary>
    /// <param name="io">The IO configuration for the context. If null, a new instance is created.</param>
    public ImGuiContext(ImGuiIO? io = null)
    {
        IO = io ?? new ImGuiIO();
    }

    /// <summary>
    /// Gets the IO configuration associated with this context.
    /// </summary>
    public ImGuiIO IO { get; }

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
        IO.MouseWheel = _pendingMouseWheelY;
        IO.MouseWheelH = _pendingMouseWheelX;
        _pendingMouseWheelX = 0f;
        _pendingMouseWheelY = 0f;
        _drawListBuilder.Reset();
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

        IsFrameStarted = false;
        FrameCount++;
        Array.Copy(_mouseButtons, _mouseButtonsPrev, _mouseButtons.Length);
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
        _cursorPos = new Vec2(_cursorPos.X, _cursorPos.Y + itemSize.Y + DefaultItemSpacingY);
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
        var seed = ImGuiId.FnvOffset;
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

    internal void SetCurrentTexture(IntPtr textureId)
    {
        _drawListBuilder.SetTexture(textureId);
    }

    public void SetDefaultFont(FontAtlas atlas, IntPtr textureId)
    {
        _fontAtlas = atlas;
        _fontTexture = textureId;
    }

    public float MeasureTextWidth(string text)
    {
        if (_fontAtlas is null || string.IsNullOrEmpty(text))
        {
            return 0f;
        }
        float w = 0f;
        foreach (var ch in text)
        {
            if (_fontAtlas.Glyphs.TryGetValue(ch, out var g))
            {
                w += g.Advance;
            }
        }
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
        foreach (var ch in text)
        {
            if (!_fontAtlas.Glyphs.TryGetValue(ch, out var g))
            {
                continue;
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
}
