namespace ImGuiSharp.Math;

/// <summary>
/// Represents an RGBA colour using 32-bit floats.
/// </summary>
public readonly struct Color
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Color"/> struct.
    /// </summary>
    public Color(float r, float g, float b, float a = 1f)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    /// <summary>
    /// Gets the red component.
    /// </summary>
    public float R { get; }

    /// <summary>
    /// Gets the green component.
    /// </summary>
    public float G { get; }

    /// <summary>
    /// Gets the blue component.
    /// </summary>
    public float B { get; }

    /// <summary>
    /// Gets the alpha component.
    /// </summary>
    public float A { get; }

    /// <summary>
    /// Packs the colour into ABGR format (matching Dear ImGui).
    /// </summary>
    public uint PackABGR()
    {
        static uint Clamp(float value) => (uint)(System.Math.Clamp(value, 0f, 1f) * 255f + 0.5f);
        var a = Clamp(A);
        var b = Clamp(B);
        var g = Clamp(G);
        var r = Clamp(R);
        return (a << 24) | (b << 16) | (g << 8) | r;
    }

    /// <summary>
    /// Creates a colour from an ABGR-packed integer.
    /// </summary>
    public static Color FromABGR(uint abgr)
    {
        var a = ((abgr >> 24) & 0xFF) / 255f;
        var b = ((abgr >> 16) & 0xFF) / 255f;
        var g = ((abgr >> 8) & 0xFF) / 255f;
        var r = (abgr & 0xFF) / 255f;
        return new Color(r, g, b, a);
    }

    /// <summary>
    /// Returns a <see cref="Vec4"/> view of this colour.
    /// </summary>
    public Vec4 ToVec4() => new(R, G, B, A);
}
