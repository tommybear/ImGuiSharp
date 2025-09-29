namespace ImGuiSharp;

/// <summary>
/// Enumerates style colour slots mirroring Dear ImGui defaults.
/// </summary>
public enum ImGuiCol
{
    /// <summary>Default text colour.</summary>
    Text = 0,
    /// <summary>Colour used for disabled text.</summary>
    TextDisabled,
    /// <summary>Default button background colour.</summary>
    Button,
    /// <summary>Button colour while hovered.</summary>
    ButtonHovered,
    /// <summary>Button colour while active.</summary>
    ButtonActive,
    /// <summary>Background colour for frame widgets (sliders, checkboxes).</summary>
    FrameBg,
    /// <summary>Frame background colour while hovered.</summary>
    FrameBgHovered,
    /// <summary>Frame background colour while active.</summary>
    FrameBgActive,
    /// <summary>Colour for separators.</summary>
    Separator,
    /// <summary>Colour for check marks and radio dots.</summary>
    CheckMark,
    /// <summary>Colour of the slider grab handle.</summary>
    SliderGrab,
    /// <summary>Colour of the slider grab handle when active.</summary>
    SliderGrabActive,
    /// <summary>Frame/window border colour.</summary>
    Border,
    /// <summary>Navigation/focus highlight colour.</summary>
    NavHighlight,
    /// <summary>Total number of style colour slots.</summary>
    Count
}
