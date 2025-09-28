namespace ImGuiSharp.Input;

/// <summary>
/// Represents keys recognised by the immediate-mode input system.
/// </summary>
public enum ImGuiKey
{
    /// <summary>
    /// Unknown or unhandled key.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The Enter/Return key.
    /// </summary>
    Enter,

    /// <summary>
    /// The Escape key.
    /// </summary>
    Escape,

    /// <summary>
    /// The Space bar.
    /// </summary>
    Space,

    /// <summary>
    /// The left Control key.
    /// </summary>
    LeftCtrl,

    /// <summary>
    /// The right Control key.
    /// </summary>
    RightCtrl,

    /// <summary>
    /// The left Shift key.
    /// </summary>
    LeftShift,

    /// <summary>
    /// The right Shift key.
    /// </summary>
    RightShift,

    /// <summary>
    /// The left Alt key.
    /// </summary>
    LeftAlt,

    /// <summary>
    /// The right Alt key.
    /// </summary>
    RightAlt,

    /// <summary>
    /// A generic key slot for letter keys.
    /// </summary>
    A,

    /// <summary>
    /// A generic key slot for letter keys.
    /// </summary>
    B,

    /// <summary>
    /// A generic key slot for letter keys.
    /// </summary>
    C,

    // Navigation keys
    Tab,
    Backspace,
    Left,
    Right,
    Up,
    Down,

    // Navigation extended
    Home,
    End,
    PageUp,
    PageDown,

    // Number row
    D0,
    D1,
    D2,
    D3,
    D4,
    D5,
    D6,
    D7,
    D8,
    D9
}
