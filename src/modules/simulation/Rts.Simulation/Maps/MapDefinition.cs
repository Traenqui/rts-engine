using System.Collections.Immutable;
using Rts.Navigation.Grid;

namespace Rts.Simulation.Maps;

/// <summary>Immutable start state of a map, produced by <see cref="AsciiMapParser"/> (ADR-0011).</summary>
/// <param name="TownCenterOrigin">Top-left tile of the square town center footprint.</param>
public sealed record MapDefinition(
    int Width,
    int Height,
    GridPosition TownCenterOrigin,
    ImmutableArray<GridPosition> Trees,
    ImmutableArray<GridPosition> Villagers,
    ImmutableArray<GridPosition> Rocks)
{
    /// <summary>Edge length of the square town center footprint in tiles.</summary>
    public const int TownCenterSize = 3;

    /// <summary>All tiles covered by the town center, row by row.</summary>
    public ImmutableArray<GridPosition> TownCenterFootprint()
    {
        var builder = ImmutableArray.CreateBuilder<GridPosition>(TownCenterSize * TownCenterSize);

        for (var y = 0; y < TownCenterSize; y++)
        {
            for (var x = 0; x < TownCenterSize; x++)
            {
                builder.Add(new GridPosition(TownCenterOrigin.x + x, TownCenterOrigin.y + y));
            }
        }

        return builder.MoveToImmutable();
    }

    /// <summary>Creates a fresh grid in which trees, rocks and the town center are not walkable.</summary>
    public NavGrid BuildNavGrid()
    {
        var grid = new NavGrid(Width, Height);

        foreach (var blocked in Trees.Concat(Rocks).Concat(TownCenterFootprint()))
        {
            grid.SetWalkable(blocked, false);
        }

        return grid;
    }
}
