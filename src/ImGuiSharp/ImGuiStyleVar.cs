namespace ImGuiSharp;

/// <summary>
/// Enumerates style variables that can be temporarily overridden via PushStyleVar.
/// Matches the subset used by current widgets.
/// </summary>
public enum ImGuiStyleVar
{
    /// <summary>Horizontal/vertical spacing between items.</summary>
    ItemSpacing,
    /// <summary>Padding applied inside framed widgets.</summary>
    FramePadding,
    /// <summary>Border thickness around framed widgets.</summary>
    FrameBorderSize,
    /// <summary>Alignment of text within buttons.</summary>
    ButtonTextAlign,
}
