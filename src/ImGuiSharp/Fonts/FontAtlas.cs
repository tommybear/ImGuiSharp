using System;
using System.Collections.Generic;

namespace ImGuiSharp.Fonts;

public sealed class FontAtlas
{
    public int Width { get; }
    public int Height { get; }
    public byte[] PixelsRgba { get; }
    public IReadOnlyDictionary<char, Glyph> Glyphs { get; }
    public float LineHeight { get; }
    public float Ascent { get; }
    public IReadOnlyDictionary<int, float> Kerning { get; }

    public FontAtlas(int width, int height, byte[] pixelsRgba, Dictionary<char, Glyph> glyphs, float lineHeight, float ascent, Dictionary<int, float>? kerning = null)
    {
        Width = width;
        Height = height;
        PixelsRgba = pixelsRgba;
        Glyphs = glyphs;
        LineHeight = lineHeight;
        Ascent = ascent;
        Kerning = kerning ?? (IReadOnlyDictionary<int, float>)new Dictionary<int, float>();
    }

    public readonly struct Glyph
    {
        public readonly float Advance;
        public readonly float OffsetX;
        public readonly float OffsetY;
        public readonly float U0, V0, U1, V1;
        public Glyph(float advance, float offsetX, float offsetY, float u0, float v0, float u1, float v1)
        {
            Advance = advance;
            OffsetX = offsetX;
            OffsetY = offsetY;
            U0 = u0; V0 = v0; U1 = u1; V1 = v1;
        }
    }

    public bool TryGetKerning(char left, char right, out float adjust)
    {
        var key = (left << 16) | right;
        if (Kerning is not null && Kerning.Count != 0 && Kerning.TryGetValue(key, out var k))
        {
            adjust = k;
            return true;
        }
        adjust = 0f;
        return false;
    }
}
