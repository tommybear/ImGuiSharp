using System;

namespace ImGuiSharp;

[Flags]
public enum ImGuiItemStatusFlags
{
    None = 0,
    Hovered = 1 << 0,
    Held = 1 << 1,
    Pressed = 1 << 2,
    Released = 1 << 3,
    Active = 1 << 4,
    Focused = 1 << 5,
    Deactivated = 1 << 6,
    Edited = 1 << 7,
}
