using Rts.Navigation.Contracts;
using Rts.Navigation.Grid;

namespace Rts.Navigation.Tests.Grid;

public sealed class NavGridTests
{
    [Fact]
    public void NewGrid_IsWalkable()
    {
        var grid = new NavGrid(10, 10);

        Assert.True(grid.IsWalkable(new GridPosition(5, 5)));
    }

    [Fact]
    public void BlockedCell_IsNotWalkable()
    {
        var grid = new NavGrid(10, 10);
        grid.SetWalkable(new GridPosition(5, 5), false);
        Assert.False(grid.IsWalkable(new GridPosition(5, 5)));
    }

    [Fact]
    public void GetNeighbors_ReturnsCorrectNeighbors()
    {
        var grid = new NavGrid(10, 10);
        var position = new GridPosition(5, 5);
        var neighbors = grid.GetNeighbors(position).ToList();
        Assert.Contains(new GridPosition(5, 4), neighbors); // Up
        Assert.Contains(new GridPosition(6, 5), neighbors); // Right
        Assert.Contains(new GridPosition(5, 6), neighbors); // Down
        Assert.Contains(new GridPosition(4, 5), neighbors); // Left
    }

    [Fact]
    public void GetNeighbors_AtEdge_ReturnsOnlyValidNeighbors()
    {
        var grid = new NavGrid(10, 10);
        var position = new GridPosition(0, 0);
        var neighbors = grid.GetNeighbors(position).ToList();
        Assert.DoesNotContain(new GridPosition(-1, 0), neighbors); // Left (invalid)
        Assert.DoesNotContain(new GridPosition(0, -1), neighbors); // Up (invalid)
        Assert.Contains(new GridPosition(1, 0), neighbors); // Right
        Assert.Contains(new GridPosition(0, 1), neighbors); // Down
    }
}
