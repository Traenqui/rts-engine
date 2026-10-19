using Rts.Navigation.Grid;
using Rts.Primitives;
using Rts.Simulation.Contracts;
using Rts.Simulation.Movement;

namespace Rts.Simulation.Ecs;

/// <summary>Entity registry, navigation grid and all component stores. Internal mutable state of the simulation.</summary>
internal sealed class World(NavGrid grid)
{
    public NavGrid Grid { get; } = grid;

    public EntityRegistry Entities { get; } = new();

    public ComponentStore<Identity> Identities { get; } = new();

    public ComponentStore<FixedVector2> Positions { get; } = new();

    public ComponentStore<PathFollower> Paths { get; } = new();

    public EntityId Spawn(EntityKind kind, PlayerId owner, FixedVector2 position)
    {
        var id = Entities.Create();
        Identities.Add(id, new Identity(kind, owner));
        Positions.Add(id, position);
        return id;
    }
}
