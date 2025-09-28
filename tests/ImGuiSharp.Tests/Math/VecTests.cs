using ImGuiSharp.Math;
using Xunit;

namespace ImGuiSharp.Tests.Math;

public sealed class VecTests
{
    [Fact]
    public void Vec2_Addition_Works()
    {
        var a = new Vec2(1f, 2f);
        var b = new Vec2(-3f, 4f);

        var result = a + b;

        Assert.Equal(-2f, result.X);
        Assert.Equal(6f, result.Y);
    }

    [Fact]
    public void Vec2_LengthSquared_Computes()
    {
        var vec = new Vec2(3f, 4f);
        Assert.Equal(25f, vec.LengthSquared);
    }

    [Fact]
    public void Vec4_Lerp_Blends()
    {
        var start = new Vec4(0f, 0f, 0f, 1f);
        var end = new Vec4(1f, 2f, 3f, 1f);

        var result = Vec4.Lerp(start, end, 0.5f);

        Assert.Equal(0.5f, result.X);
        Assert.Equal(1f, result.Y);
        Assert.Equal(1.5f, result.Z);
        Assert.Equal(1f, result.W);
    }
}
