using Rts.Navigation.Grid;
using Rts.Navigation.Pathfinding;

namespace Rts.Navigation.Tests.Pathfinding;

public sealed class BreadthFirstSearchTests
{
    private const int GridSize = 5;

    [Fact]
    public void FindPath_StartEqualsGoal_ReturnsOnlyStart()
    {
        var grid = new NavGrid(GridSize, GridSize);
        var start = new GridPosition(2, 2);

        var path = BreadthFirstSearch.FindPath(grid, start, start);

        Assert.Equal([start], path);
    }

    [Fact]
    public void FindPath_AdjacentGoal_ReturnsStartAndGoal()
    {
        var grid = new NavGrid(GridSize, GridSize);
        var start = new GridPosition(0, 0);
        var goal = new GridPosition(1, 0);

        var path = BreadthFirstSearch.FindPath(grid, start, goal);

        Assert.Equal([start, goal], path);
    }

    [Theory]
    [InlineData(0, 0, 4, 0)]
    [InlineData(0, 0, 0, 4)]
    [InlineData(0, 0, 4, 4)]
    [InlineData(4, 4, 0, 0)]
    [InlineData(1, 3, 3, 1)]
    public void FindPath_OpenGrid_ReturnsShortestPath(int startX, int startY, int goalX, int goalY)
    {
        var grid = new NavGrid(GridSize, GridSize);
        var start = new GridPosition(startX, startY);
        var goal = new GridPosition(goalX, goalY);

        var path = BreadthFirstSearch.FindPath(grid, start, goal);

        var manhattanDistance = Math.Abs(goalX - startX) + Math.Abs(goalY - startY);
        Assert.Equal(manhattanDistance + 1, path.Count);
        AssertValidPath(grid, path, (start, goal));
    }

    [Fact]
    public void FindPath_WallBetween_PathGoesAroundWall()
    {
        var grid = new NavGrid(GridSize, GridSize);
        BlockColumn(grid, x: 2, fromY: 0, toY: 3);
        var start = new GridPosition(0, 0);
        var goal = new GridPosition(4, 0);

        var path = BreadthFirstSearch.FindPath(grid, start, goal);

        AssertValidPath(grid, path, (start, goal));
        Assert.Contains(new GridPosition(2, 4), path);
    }

    [Fact]
    public void FindPath_GoalBlocked_ReturnsEmptyPath()
    {
        var grid = new NavGrid(GridSize, GridSize);
        var goal = new GridPosition(3, 3);
        grid.SetWalkable(goal, false);

        var path = BreadthFirstSearch.FindPath(grid, new GridPosition(0, 0), goal);

        Assert.Empty(path);
    }

    [Fact]
    public void FindPath_GoalEnclosed_ReturnsEmptyPath()
    {
        var grid = new NavGrid(GridSize, GridSize);
        BlockColumn(grid, x: 2, fromY: 0, toY: GridSize - 1);

        var path = BreadthFirstSearch.FindPath(grid, new GridPosition(0, 0), new GridPosition(4, 4));

        Assert.Empty(path);
    }

    [Fact]
    public void FindPathToAdjacent_SingleBlockedTarget_EndsOnWalkableNeighbor()
    {
        var grid = new NavGrid(GridSize, GridSize);
        var tree = new GridPosition(4, 2);
        grid.SetWalkable(tree, false);
        var start = new GridPosition(0, 2);

        var path = BreadthFirstSearch.FindPathToAdjacent(grid, start, [tree]);

        Assert.Equal(new GridPosition(3, 2), path[^1]);
        Assert.Equal(4, path.Count);
        AssertValidPath(grid, path, (start, path[^1]));
    }

    [Fact]
    public void FindPathToAdjacent_StartAlreadyAdjacent_ReturnsOnlyStart()
    {
        var grid = new NavGrid(GridSize, GridSize);
        var tree = new GridPosition(2, 2);
        grid.SetWalkable(tree, false);
        var start = new GridPosition(2, 1);

        var path = BreadthFirstSearch.FindPathToAdjacent(grid, start, [tree]);

        Assert.Equal([start], path);
    }

    [Fact]
    public void FindPathToAdjacent_MultiCellFootprint_EndsNextToNearestCell()
    {
        const int largeGridSize = 9;
        var grid = new NavGrid(largeGridSize, largeGridSize);
        var townCenter = BlockSquare(grid, origin: new GridPosition(3, 3), size: 3);
        var start = new GridPosition(4, 0);

        var path = BreadthFirstSearch.FindPathToAdjacent(grid, start, townCenter);

        Assert.Equal(new GridPosition(4, 2), path[^1]);
        AssertValidPath(grid, path, (start, path[^1]));
    }

    [Fact]
    public void FindPathToAdjacent_AllNeighborsBlocked_ReturnsEmptyPath()
    {
        var grid = new NavGrid(GridSize, GridSize);
        var target = BlockSquare(grid, origin: new GridPosition(1, 1), size: 3);
        var innerTree = new GridPosition(2, 2);

        var path = BreadthFirstSearch.FindPathToAdjacent(grid, new GridPosition(0, 0), [innerTree]);

        Assert.Contains(innerTree, target);
        Assert.Empty(path);
    }

    [Fact]
    public void FindPathToAdjacent_EmptyTarget_Throws()
    {
        var grid = new NavGrid(GridSize, GridSize);

        Assert.Throws<ArgumentException>(
            () => BreadthFirstSearch.FindPathToAdjacent(grid, new GridPosition(0, 0), []));
    }

    private static void BlockColumn(NavGrid grid, int x, int fromY, int toY)
    {
        for (var y = fromY; y <= toY; y++)
        {
            grid.SetWalkable(new GridPosition(x, y), false);
        }
    }

    private static GridPosition[] BlockSquare(NavGrid grid, GridPosition origin, int size)
    {
        var cells = new List<GridPosition>();

        for (var y = origin.y; y < origin.y + size; y++)
        {
            for (var x = origin.x; x < origin.x + size; x++)
            {
                var cell = new GridPosition(x, y);
                grid.SetWalkable(cell, false);
                cells.Add(cell);
            }
        }

        return [.. cells];
    }

    private static void AssertValidPath(
        NavGrid grid,
        IReadOnlyList<GridPosition> path,
        (GridPosition Start, GridPosition Goal) endpoints)
    {
        Assert.Equal(endpoints.Start, path[0]);
        Assert.Equal(endpoints.Goal, path[^1]);
        Assert.All(path, cell => Assert.True(grid.IsInside(cell), $"{cell} is outside the grid"));

        for (var i = 1; i < path.Count; i++)
        {
            var step = Math.Abs(path[i].x - path[i - 1].x) + Math.Abs(path[i].y - path[i - 1].y);
            Assert.True(step == 1, $"{path[i - 1]} -> {path[i]} is not a single orthogonal step");
        }
    }
}
