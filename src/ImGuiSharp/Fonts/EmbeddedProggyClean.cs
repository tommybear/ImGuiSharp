using System;
using System.IO;
using System.Reflection;

namespace ImGuiSharp.Fonts;

public static class EmbeddedProggyClean
{
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

