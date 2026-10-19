using System.Collections.Immutable;
using Rts.Navigation.Grid;

namespace Rts.Simulation.Movement;

/// <summary>Component of a unit that walks along tile-center waypoints; removed on arrival.</summary>
internal struct PathFollower
{
    public PathFollower(ImmutableArray<GridPosition> waypoints)
    {
        Waypoints = waypoints;
        NextWaypoint = 0;
    }

    public ImmutableArray<GridPosition> Waypoints { get; }

    public int NextWaypoint { get; set; }

    public readonly bool HasArrived => NextWaypoint >= Waypoints.Length;
}
