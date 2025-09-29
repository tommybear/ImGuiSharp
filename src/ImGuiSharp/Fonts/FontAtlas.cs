using System;
using System.Collections.Generic;

namespace ImGuiSharp.Fonts;

/// <summary>
/// Represents a baked font atlas containing glyph metrics and texture data.
/// </summary>
public sealed class FontAtlas
{
    /// <summary>Gets the atlas width in pixels.</summary>
    public int Width { get; }

    /// <summary>Gets the atlas height in pixels.</summary>
    public int Height { get; }

    /// <summary>Gets the RGBA pixel data representing the atlas texture.</summary>
    public byte[] PixelsRgba { get; }

    /// <summary>Gets glyph metadata keyed by character.</summary>
    public IReadOnlyDictionary<char, Glyph> Glyphs { get; }

    /// <summary>Gets the line height of the baked font.</summary>
    public float LineHeight { get; }

    /// <summary>Gets the ascent (baseline offset) of the baked font.</summary>
    public float Ascent { get; }

    /// <summary>Gets the optional kerning table hashed by character pairs.</summary>
    public IReadOnlyDictionary<int, float> Kerning { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="FontAtlas"/> class.
    /// </summary>
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

    /// <summary>
    /// Describes per-glyph metrics and UV coordinates in the atlas.
    /// </summary>
    public readonly struct Glyph
    {
        /// <summary>Advance value in pixels.</summary>
        public readonly float Advance;

        /// <summary>Horizontal offset relative to the baseline.</summary>
        public readonly float OffsetX;

        /// <summary>Vertical offset relative to the baseline.</summary>
        public readonly float OffsetY;

        /// <summary>Texture UV coordinates.</summary>
        public readonly float U0, V0, U1, V1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Glyph"/> struct.
        /// </summary>
        public Glyph(float advance, float offsetX, float offsetY, float u0, float v0, float u1, float v1)
        {
            Advance = advance;
            OffsetX = offsetX;
            OffsetY = offsetY;
            U0 = u0; V0 = v0; U1 = u1; V1 = v1;
        }
    }

    /// <summary>
    /// Attempts to retrieve precomputed kerning adjustment between two characters.
    /// </summary>
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
