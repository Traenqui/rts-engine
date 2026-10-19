using System.Numerics;
using Rts.Rendering.Contracts;

namespace Rts.Rendering.Tests.Contracts;

public sealed class ScreenRectangleTests
{
    [Fact]
    public void FromCorners_InAnyOrder_NormalizesMinAndMax()
    {
        var rectangle = ScreenRectangle.FromCorners(new Vector2(100, 20), new Vector2(40, 80));

        Assert.Equal(new Vector2(40, 20), rectangle.Min);
        Assert.Equal(new Vector2(100, 80), rectangle.Max);
        Assert.Equal(new Vector2(60, 60), rectangle.Size);
    }

    [Theory]
    [InlineData(40, 20, true)]
    [InlineData(100, 80, true)]
    [InlineData(70, 50, true)]
    [InlineData(39, 50, false)]
    [InlineData(70, 81, false)]
    public void Contains_IncludesEdges(float x, float y, bool expected)
    {
        var rectangle = ScreenRectangle.FromCorners(new Vector2(40, 20), new Vector2(100, 80));

        Assert.Equal(expected, rectangle.Contains(new Vector2(x, y)));
    }
}
