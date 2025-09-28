using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Math;

public sealed class ColorTests
{
    [Fact]
    public void Pack_Unpack_RoundTrips()
    {
        var color = new Color(0.1f, 0.2f, 0.3f, 0.4f);
        var packed = color.PackABGR();
        var unpacked = Color.FromABGR(packed);

        Assert.Equal(color.R, unpacked.R, 2);
        Assert.Equal(color.G, unpacked.G, 2);
        Assert.Equal(color.B, unpacked.B, 2);
        Assert.Equal(color.A, unpacked.A, 2);
    }

    [Fact]
    public void ToVec4_ReturnsVectorView()
    {
        var color = new Color(0.5f, 0.25f, 1f, 1f);
        var vec = color.ToVec4();

        Assert.Equal(color.R, vec.X);
        Assert.Equal(color.G, vec.Y);
        Assert.Equal(color.B, vec.Z);
        Assert.Equal(color.A, vec.W);
    }
}
