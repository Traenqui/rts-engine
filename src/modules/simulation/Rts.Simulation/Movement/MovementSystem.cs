using Rts.Navigation.Grid;
using Rts.Primitives;
using Rts.Simulation.Ecs;

namespace Rts.Simulation.Movement;

/// <summary>Moves every unit with a path by its speed per tick, in ascending entity order.</summary>
internal static class MovementSystem
{
    public static void Update(World world, Fixed speedPerTick)
    {
        foreach (var unit in world.Paths.EntitiesInIdOrder())
        {
            ref var follower = ref world.Paths.Get(unit);
            ref var position = ref world.Positions.Get(unit);

            position = Advance(position, ref follower, speedPerTick);

            if (follower.HasArrived)
            {
                world.Paths.Remove(unit);
            }
        }
    }

    private static FixedVector2 Advance(FixedVector2 position, ref PathFollower follower, Fixed budget)
    {
        // Distance left over after reaching a waypoint carries on to the next one, so arrival times are exact.
        while (budget > Fixed.Zero && !follower.HasArrived)
        {
            var waypoint = WorldGrid.TileCenter(follower.Waypoints[follower.NextWaypoint]);
            var offset = waypoint - position;
            var distance = offset.Length();

            if (distance <= budget)
            {
                position = waypoint;
                budget -= distance;
                follower.NextWaypoint++;
                continue;
            }

            return position + (offset * budget / distance);
        }

        return position;
    }
}
