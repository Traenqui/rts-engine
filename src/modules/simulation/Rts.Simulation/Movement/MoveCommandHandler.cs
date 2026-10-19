using System.Collections.Immutable;
using Rts.Navigation.Grid;
using Rts.Navigation.Pathfinding;
using Rts.Simulation.Contracts;
using Rts.Simulation.Ecs;

namespace Rts.Simulation.Movement;

/// <summary>Validates <see cref="MoveCommand"/>s and gives every unit a fresh path (ADR-0008, ADR-0009).</summary>
internal static class MoveCommandHandler
{
    /// <returns><c>false</c> if the command is invalid; then no unit is changed.</returns>
    public static bool TryExecute(World world, MoveCommand command)
    {
        if (!IsValid(world, command))
        {
            return false;
        }

        foreach (var unit in command.Units)
        {
            AssignPath(world, unit, command.Target);
        }

        return true;
    }

    private static bool IsValid(World world, MoveCommand command)
    {
        if (command.Units.IsDefaultOrEmpty || !world.Grid.IsInside(command.Target))
        {
            return false;
        }

        return command.Units.All(unit =>
            world.Entities.IsAlive(unit)
            && world.Identities.Get(unit).Owner == command.Issuer
            && world.Identities.Get(unit).Kind == EntityKind.Villager);
    }

    private static void AssignPath(World world, EntityId unit, GridPosition target)
    {
        world.Paths.Remove(unit);

        var start = WorldGrid.TileAt(world.Positions.Get(unit));
        var path = BreadthFirstSearch.FindPath(world.Grid, start, target);

        // An unreachable target is not an error: the unit simply stays idle (PRD edge case E4).
        if (path.Count == 0)
        {
            return;
        }

        // The start tile is skipped so a unit standing off-center does not walk back first. Moving straight to the
        // next tile center stays inside the walkable start and next tiles, which form a convex rectangle.
        var waypoints = path.Count == 1 ? [.. path] : path.Skip(1).ToImmutableArray();
        world.Paths.Add(unit, new PathFollower(waypoints));
    }
}
