using ImGuiSharp.Math;

namespace ImGuiSharp;

/// <summary>
/// Encapsulates layout spacing, padding, text alignment, and colour palette used by widgets.
/// </summary>
public sealed class ImGuiStyle
{
    private readonly Color[] _colors = new Color[(int)ImGuiCol.Count];

    /// <summary>
    /// Initializes default style values matching Dear ImGui.
    /// </summary>
    public ImGuiStyle()
    {
        ItemSpacing = new Vec2(8f, 4f);
        FramePadding = new Vec2(4f, 3f);
        ButtonTextAlign = new Vec2(0.5f, 0.5f);

        SetColor(ImGuiCol.Text, new Color(1f, 1f, 1f, 1f));
        SetColor(ImGuiCol.TextDisabled, new Color(0.50f, 0.50f, 0.50f, 1f));
        SetColor(ImGuiCol.Button, new Color(0.20f, 0.22f, 0.27f, 1f));
        SetColor(ImGuiCol.ButtonHovered, new Color(0.28f, 0.30f, 0.36f, 1f));
        SetColor(ImGuiCol.ButtonActive, new Color(0.33f, 0.36f, 0.43f, 1f));
        SetColor(ImGuiCol.FrameBg, new Color(0.20f, 0.22f, 0.27f, 1f));
        SetColor(ImGuiCol.FrameBgHovered, new Color(0.25f, 0.27f, 0.32f, 1f));
        SetColor(ImGuiCol.FrameBgActive, new Color(0.33f, 0.36f, 0.43f, 1f));
        SetColor(ImGuiCol.Separator, new Color(0.43f, 0.43f, 0.50f, 1f));
        SetColor(ImGuiCol.CheckMark, new Color(0.90f, 0.90f, 0.90f, 1f));
        SetColor(ImGuiCol.SliderGrab, new Color(0.34f, 0.36f, 0.43f, 1f));
        SetColor(ImGuiCol.SliderGrabActive, new Color(0.41f, 0.43f, 0.51f, 1f));
    }

    /// <summary>Gets or sets the spacing between consecutive items.</summary>
    public Vec2 ItemSpacing { get; set; }

    /// <summary>Gets or sets padding applied inside frame widgets (buttons, sliders).</summary>
    public Vec2 FramePadding { get; set; }

    /// <summary>Gets or sets button text alignment within the button rectangle.</summary>
    public Vec2 ButtonTextAlign { get; set; }

    /// <summary>Gets the colour associated with the specified slot.</summary>
    public Color GetColor(ImGuiCol idx) => _colors[(int)idx];

    /// <summary>Sets the colour associated with the specified slot.</summary>
    public void SetColor(ImGuiCol idx, Color color)
    {
        _colors[(int)idx] = color;
    }
}
