using System;

namespace ImGuiSharp;

/// <summary>
/// Bit flags describing the interaction state of the most recently submitted item.
/// </summary>
[Flags]
public enum ImGuiItemStatusFlags
{
    /// <summary>No flags set.</summary>
    None = 0,
    /// <summary>Item is currently hovered by the mouse.</summary>
    Hovered = 1 << 0,
    /// <summary>Item is being held/dragged.</summary>
    Held = 1 << 1,
    /// <summary>Item was pressed this frame.</summary>
    Pressed = 1 << 2,
    /// <summary>Item was released this frame.</summary>
    Released = 1 << 3,
    /// <summary>Item is the active (held) widget.</summary>
    Active = 1 << 4,
    /// <summary>Item currently has keyboard focus.</summary>
    Focused = 1 << 5,
    /// <summary>Item was deactivated (released or focus lost) this frame.</summary>
    Deactivated = 1 << 6,
    /// <summary>Item value was edited this frame.</summary>
    Edited = 1 << 7,
}
