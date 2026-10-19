using System.Collections.Immutable;
using Rts.Primitives;

namespace Rts.Simulation.Contracts;

/// <summary>
/// Immutable state after a tick, the only view of the simulation that rendering and clients get (ADR-0007).
/// Equality of <see cref="ImmutableArray{T}"/> is by reference; compare <see cref="StateHash"/> or entities element-wise.
/// </summary>
/// <param name="Tick">Number of ticks simulated so far.</param>
/// <param name="StateHash">Deterministic hash of the full simulation state; identical on every platform.</param>
/// <param name="RejectedCommandCount">Commands rejected by validation since the start.</param>
/// <param name="Entities">All living entities in ascending <see cref="EntityId"/> order.</param>
public sealed record WorldSnapshot(
    long Tick,
    ulong StateHash,
    int RejectedCommandCount,
    ImmutableArray<EntitySnapshot> Entities);

/// <summary>Render-relevant state of one entity.</summary>
public sealed record EntitySnapshot(EntityId Id, EntityKind Kind, PlayerId Owner, FixedVector2 Position);
