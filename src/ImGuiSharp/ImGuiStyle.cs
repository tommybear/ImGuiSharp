using ImGuiSharp.Math;

namespace ImGuiSharp;

public sealed class ImGuiStyle
{
private readonly Color[] _colors = new Color[(int)ImGuiCol.Count];

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

public Vec2 ItemSpacing { get; set; }

    public Vec2 FramePadding { get; set; }

    public Vec2 ButtonTextAlign { get; set; }

public Color GetColor(ImGuiCol idx) => _colors[(int)idx];

public void SetColor(ImGuiCol idx, Color color)
{
    _colors[(int)idx] = color;
}
}
