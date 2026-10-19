using System.Collections.Immutable;
using Rts.Navigation.Grid;

namespace Rts.Simulation.Contracts;

/// <summary>Static terrain of the running map. It never changes, so it is read once instead of per snapshot.</summary>
/// <param name="Rocks">Impassable tiles that are not entities.</param>
public sealed record TerrainInfo(int Width, int Height, ImmutableArray<GridPosition> Rocks);
