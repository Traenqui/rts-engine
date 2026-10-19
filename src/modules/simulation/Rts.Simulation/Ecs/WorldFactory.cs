using Rts.Navigation.Grid;
using Rts.Primitives;
using Rts.Simulation.Contracts;
using Rts.Simulation.Maps;

namespace Rts.Simulation.Ecs;

/// <summary>Creates the start world of a map. The spawn order fixes the entity IDs and therefore the state hash.</summary>
internal static class WorldFactory
{
    /// <summary>Milestone 1 maps have exactly one player.</summary>
    public static PlayerId MapPlayer => new(1);

    public static World FromMap(MapDefinition map)
    {
        var world = new World(map.BuildNavGrid());

        world.Spawn(EntityKind.TownCenter, MapPlayer, TownCenterPosition(map));

        foreach (var villager in map.Villagers)
        {
            world.Spawn(EntityKind.Villager, MapPlayer, WorldGrid.TileCenter(villager));
        }

        foreach (var tree in map.Trees)
        {
            world.Spawn(EntityKind.Tree, PlayerId.Nature, WorldGrid.TileCenter(tree));
        }

        return world;
    }

    private static FixedVector2 TownCenterPosition(MapDefinition map)
    {
        var halfSize = Fixed.FromRatio(MapDefinition.TownCenterSize, 2);
        var origin = map.TownCenterOrigin;
        return new FixedVector2(Fixed.FromInt(origin.X) + halfSize, Fixed.FromInt(origin.Y) + halfSize);
    }
}
