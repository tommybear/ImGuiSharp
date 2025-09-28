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

    public FontAtlas(int width, int height, byte[] pixelsRgba, Dictionary<char, Glyph> glyphs, float lineHeight, float ascent)
    {
        Width = width;
        Height = height;
        PixelsRgba = pixelsRgba;
        Glyphs = glyphs;
        LineHeight = lineHeight;
        Ascent = ascent;
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
}

