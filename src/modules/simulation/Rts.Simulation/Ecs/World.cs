using Rts.Primitives;
using Rts.Simulation.Contracts;

namespace Rts.Simulation.Ecs;

/// <summary>Entity registry plus all component stores. Internal mutable state of the simulation.</summary>
internal sealed class World
{
    public EntityRegistry Entities { get; } = new();

    public ComponentStore<Identity> Identities { get; } = new();

    public ComponentStore<FixedVector2> Positions { get; } = new();

    public EntityId Spawn(EntityKind kind, PlayerId owner, FixedVector2 position)
    {
        var id = Entities.Create();
        Identities.Add(id, new Identity(kind, owner));
        Positions.Add(id, position);
        return id;
    }
}
