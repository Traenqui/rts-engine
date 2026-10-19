using Rts.Navigation.Grid;
using Rts.Primitives;

namespace Rts.Navigation.Tests.Grid;

public sealed class WorldGridTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 7)]
    [InlineData(255, 255)]
    public void TileCenter_ReturnsMiddleOfTile(int x, int y)
    {
        var center = WorldGrid.TileCenter(new GridPosition(x, y));

        Assert.Equal(new FixedVector2(Fixed.FromRatio(2 * x + 1, 2), Fixed.FromRatio(2 * y + 1, 2)), center);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(3, 7)]
    [InlineData(255, 255)]
    public void TileAt_TileCenter_RoundTrips(int x, int y)
    {
        var tile = new GridPosition(x, y);

        Assert.Equal(tile, WorldGrid.TileAt(WorldGrid.TileCenter(tile)));
    }

    [Fact]
    public void TileAt_LowerTileEdge_BelongsToThatTile()
    {
        var edge = new FixedVector2(Fixed.FromInt(1), Fixed.FromInt(2));

        Assert.Equal(new GridPosition(1, 2), WorldGrid.TileAt(edge));
    }

    [Fact]
    public void TileAt_JustBelowUpperEdge_StaysInTile()
    {
        const long justBelowOneRaw = (1L << Fixed.FractionalBits) - 1;
        var position = new FixedVector2(Fixed.FromRaw(justBelowOneRaw), Fixed.FromRaw(justBelowOneRaw));

        Assert.Equal(new GridPosition(0, 0), WorldGrid.TileAt(position));
    }

    [Fact]
    public void TileAt_NegativePosition_ReturnsTileOutsideGrid()
    {
        var position = new FixedVector2(Fixed.FromRatio(-1, 2), Fixed.Zero);

        Assert.Equal(new GridPosition(-1, 0), WorldGrid.TileAt(position));
    }
}
