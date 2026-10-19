using System.Collections.Immutable;
using Rts.Simulation.Contracts;

namespace Rts.Rendering.Scene;

/// <summary>Everything the world renderer draws in one frame.</summary>
/// <param name="Entities">Interpolated entities, see <see cref="SnapshotInterpolator"/>.</param>
/// <param name="Selected">Entities that get a selection ring.</param>
public sealed record SceneFrame(
    TerrainInfo Terrain,
    ImmutableArray<RenderEntity> Entities,
    ImmutableHashSet<EntityId> Selected);
