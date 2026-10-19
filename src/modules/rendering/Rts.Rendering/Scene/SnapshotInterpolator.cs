using System.Collections.Immutable;
using System.Numerics;
using Rts.Navigation.Grid;
using Rts.Primitives;
using Rts.Simulation.Contracts;

namespace Rts.Rendering.Scene;

/// <summary>An entity as drawn in one frame, in float render units. Never fed back into the simulation.</summary>
public sealed record RenderEntity(
    EntityId Id,
    EntityKind Kind,
    PlayerId Owner,
    Vector2 Position,
    ImmutableArray<GridPosition> RemainingPath);

/// <summary>Blends two consecutive snapshots so movement looks smooth between 20 Hz ticks (ADR-0007).</summary>
public static class SnapshotInterpolator
{
    /// <param name="previous">Snapshot of the tick before <paramref name="current"/>; null before the first tick.</param>
    /// <param name="alpha">0 shows <paramref name="previous"/>, 1 shows <paramref name="current"/>.</param>
    /// <remarks>New entities appear and removed entities vanish without blending.</remarks>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="alpha"/> is not in [0, 1].</exception>
    public static ImmutableArray<RenderEntity> Interpolate(WorldSnapshot? previous, WorldSnapshot current, float alpha)
    {
        ArgumentNullException.ThrowIfNull(current);

        if (!(alpha is >= 0f and <= 1f))
        {
            throw new ArgumentOutOfRangeException(nameof(alpha), alpha, "Alpha must be within [0, 1].");
        }

        var previousPositions = previous?.Entities.ToDictionary(entity => entity.Id, entity => entity.Position)
            ?? [];

        return [.. current.Entities.Select(entity => ToRenderEntity(entity, previousPositions, alpha))];
    }

    private static RenderEntity ToRenderEntity(
        EntitySnapshot entity,
        Dictionary<EntityId, FixedVector2> previousPositions,
        float alpha)
    {
        var currentPosition = ToRender(entity.Position);
        var position = previousPositions.TryGetValue(entity.Id, out var previousPosition)
            ? Vector2.Lerp(ToRender(previousPosition), currentPosition, alpha)
            : currentPosition;

        return new RenderEntity(entity.Id, entity.Kind, entity.Owner, position, entity.RemainingPath);
    }

    private static Vector2 ToRender(FixedVector2 position) => new(position.X.ToSingle(), position.Y.ToSingle());
}
