using System;
using System.Collections.Generic;
using StbTrueTypeSharp;

namespace ImGuiSharp.Fonts;

public static class FontAtlasBuilder
{
    public static FontAtlas Build(byte[] ttfBytes, float pixelHeight = 18f, int atlasWidth = 512, int atlasHeight = 512)
    {
        const int firstChar = 32;   // space
        const int charCount = 95;   // ASCII 32..126

        var alpha = new byte[atlasWidth * atlasHeight];
        var baked = new StbTrueType.stbtt_bakedchar[charCount];
        var ok = StbTrueType.stbtt_BakeFontBitmap(ttfBytes, 0, pixelHeight, alpha, atlasWidth, atlasHeight, firstChar, charCount, baked);
        if (!ok)
        {
            throw new InvalidOperationException("stbtt_BakeFontBitmap failed; atlas likely too small.");
        }

        var glyphs = new Dictionary<char, FontAtlas.Glyph>(charCount);
        for (int i = 0; i < charCount; i++)
        {
            var c = (char)(firstChar + i);
            var bc = baked[i];
            float u0 = (float)bc.x0 / atlasWidth;
            float v0 = (float)bc.y0 / atlasHeight;
            float u1 = (float)bc.x1 / atlasWidth;
            float v1 = (float)bc.y1 / atlasHeight;
            glyphs[c] = new FontAtlas.Glyph(
                advance: bc.xadvance,
                offsetX: bc.xoff,
                offsetY: bc.yoff,
                u0: u0, v0: v0, u1: u1, v1: v1);
        }

        // Expand alpha to RGBA (white)
        var rgba = new byte[atlasWidth * atlasHeight * 4];
        for (int i = 0; i < alpha.Length; i++)
        {
            var a = alpha[i];
            var p = i * 4;
            rgba[p + 0] = 0xFF;
            rgba[p + 1] = 0xFF;
            rgba[p + 2] = 0xFF;
            rgba[p + 3] = a;
        }

        // Derive vertical metrics and kerning using stbtt font info
        float ascentPx = pixelHeight * 0.8f;
        float lineHeightPx = pixelHeight;
        var kerning = new Dictionary<int, float>();

        try
        {
            // Initialize font and compute proper v-metrics and kerning
            var font = new StbTrueType.stbtt_fontinfo();
            unsafe
            {
                fixed (byte* dataPtr = ttfBytes)
                {
                    if (StbTrueType.stbtt_InitFont(font, dataPtr, 0) != 0)
                    {
                        float scale = StbTrueType.stbtt_ScaleForPixelHeight(font, pixelHeight);
                        int ascent, descent, lineGap;
                        StbTrueType.stbtt_GetFontVMetrics(font, &ascent, &descent, &lineGap);
                        ascentPx = ascent * scale;
                        lineHeightPx = (ascent - descent + lineGap) * scale;

                        // Precompute kerning for ASCII range
                        for (int i = 0; i < charCount; i++)
                        {
                            int c0 = firstChar + i;
                            for (int j = 0; j < charCount; j++)
                            {
                                int c1 = firstChar + j;
                                int kern = StbTrueType.stbtt_GetCodepointKernAdvance(font, c0, c1);
                                if (kern != 0)
                                {
                                    float adj = kern * scale;
                                    kerning[(c0 << 16) | c1] = adj;
                                }
                            }
                        }
                    }
                }
            }
        }
        catch
        {
            // Fallback to baked approximations if stb v-metrics/kerning fail
        }

        return new FontAtlas(atlasWidth, atlasHeight, rgba, glyphs, lineHeight: lineHeightPx, ascent: ascentPx, kerning: kerning);
    }
}
