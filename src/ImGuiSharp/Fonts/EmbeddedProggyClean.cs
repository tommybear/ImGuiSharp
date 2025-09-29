using System;
using System.IO;
using System.Reflection;

namespace ImGuiSharp.Fonts;

/// <summary>
/// Provides access to the embedded Proggy Clean TTF font used as the default style font.
/// </summary>
public static class EmbeddedProggyClean
{
    /// <summary>
    /// Retrieves the raw font bytes for the embedded Proggy Clean font.
    /// </summary>
    /// <returns>Byte array containing the TTF data.</returns>
    public static byte[] GetBytes()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var s = asm.GetManifestResourceStream("ImGuiSharp.Fonts.ProggyClean.ttf")
                    ?? throw new InvalidOperationException("Embedded font resource not found.");
        using var ms = new MemoryStream();
        s.CopyTo(ms);
        return ms.ToArray();
    }
}
