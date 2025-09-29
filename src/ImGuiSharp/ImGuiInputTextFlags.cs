using System;

namespace ImGuiSharp;

/// <summary>
/// Flags controlling InputText behaviour, mirroring a subset of Dear ImGui.
/// </summary>
[Flags]
public enum ImGuiInputTextFlags
{
    /// <summary>No additional behaviour.</summary>
    None = 0,
    /// <summary>Turn entered characters to uppercase.</summary>
    CharsUppercase = 1 << 0,
    /// <summary>Disable inserting blank characters.</summary>
    CharsNoBlank = 1 << 1,
    /// <summary>Render text as password (display '*').</summary>
    Password = 1 << 2,
    /// <summary>Make field read-only.</summary>
    ReadOnly = 1 << 3,
    /// <summary>Return true when Enter is pressed (and deactivate).</summary>
    EnterReturnsTrue = 1 << 4,
    /// <summary>Select all text when the widget is activated.</summary>
    AutoSelectAll = 1 << 5,
    /// <summary>Allow Tab key to insert a tab character instead of moving focus.</summary>
    AllowTabInput = 1 << 6,
}
