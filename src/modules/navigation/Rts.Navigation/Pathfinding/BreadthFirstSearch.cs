using System.Collections.ObjectModel;
using Rts.Navigation.Contracts;
using Rts.Navigation.Grid;

namespace Rts.Navigation.Pathfinding;

/// <summary>
/// Shortest paths on an unweighted 4-neighbour <see cref="NavGrid"/>.
/// Deterministic: ties are broken by the fixed neighbour order of the grid.
/// </summary>
internal static class BreadthFirstSearch
{
    /// <summary>
    /// Finds the shortest path from <paramref name="start"/> to <paramref name="goal"/>.
    /// </summary>
    /// <returns>Path including start and goal, or an empty list if the goal is unreachable.</returns>
    public static IReadOnlyList<GridPosition> FindPath(NavGrid grid, GridPosition start, GridPosition goal)
    {
        return Search(grid, start, cell => cell == goal);
    }

    /// <summary>
    /// Finds the shortest path to any walkable tile orthogonally adjacent to <paramref name="targetCells"/>,
    /// e.g. the footprint of a tree or a building that units cannot enter themselves.
    /// </summary>
    /// <returns>Path ending next to the nearest target cell, or an empty list if no such tile is reachable.</returns>
    /// <exception cref="ArgumentException"><paramref name="targetCells"/> is empty.</exception>
    public static IReadOnlyList<GridPosition> FindPathToAdjacent(
        NavGrid grid,
        GridPosition start,
        IReadOnlyCollection<GridPosition> targetCells)
    {
        if (targetCells.Count == 0)
        {
            throw new ArgumentException("Target must contain at least one cell.", nameof(targetCells));
        }

        var goals = targetCells
            .SelectMany(grid.GetNeighbors)
            .Except(targetCells)
            .ToHashSet();

        return Search(grid, start, goals.Contains);
    }

    private static IReadOnlyList<GridPosition> Search(NavGrid grid, GridPosition start, Func<GridPosition, bool> isGoal)
    {
        // Lookup-only collections: iteration order never affects the result, which keeps the search deterministic.
        HashSet<GridPosition> visited = [start];
        Dictionary<GridPosition, GridPosition> cameFrom = [];
        Queue<GridPosition> frontier = new([start]);

        while (frontier.Count != 0)
        {
            var cell = frontier.Dequeue();

            if (isGoal(cell))
            {
                return ReconstructPath(cameFrom, start, cell);
            }

            foreach (var neighbor in grid.GetNeighbors(cell))
            {
                if (!visited.Add(neighbor))
                {
                    continue;
                }

                cameFrom[neighbor] = cell;
                frontier.Enqueue(neighbor);
            }
        }

        return [];
    }

    private static ReadOnlyCollection<GridPosition> ReconstructPath(
        Dictionary<GridPosition, GridPosition> cameFrom,
        GridPosition start,
        GridPosition goal)
    {
        List<GridPosition> path = [goal];
        var current = goal;

        while (current != start)
        {
            current = cameFrom[current];
            path.Add(current);
        }

        path.Reverse();
        return path.AsReadOnly();
    }
}
