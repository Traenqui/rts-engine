using System.Collections.Immutable;
using System.Numerics;
using Rts.Rendering.Contracts;
using Rts.Simulation.Contracts;

namespace Rts.Client.Picking;

/// <summary>Everything picking needs from one frame. Picks use the latest snapshot, not the interpolated positions.</summary>
public sealed record PickingContext(
    ScreenProjection Projection,
    WorldSnapshot Snapshot,
    PlayerId LocalPlayer,
    TerrainInfo Terrain);

/// <summary>Finds the local player's selectable entities under the pointer or inside a selection box (PRD US-03).</summary>
public static class EntityPicker
{
    /// <summary>
    /// The own villager or town center under <paramref name="screen"/>, null if there is none.
    /// Villagers win over the town center, then the entity closest to the pointer wins.
    /// </summary>
    public static EntityId? PickAt(Vector2 screen, PickingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return SelectableEntities(context)
            .Select(entity => (entity, distance: DistanceToHit(entity, screen, context.Projection)))
            .Where(candidate => candidate.distance is not null)
            .OrderBy(candidate => candidate.entity.Kind == EntityKind.Villager ? 0 : 1)
            .ThenBy(candidate => candidate.distance)
            .Select(candidate => (EntityId?)candidate.entity.Id)
            .FirstOrDefault();
    }

    /// <summary>Own villagers whose body center lies inside <paramref name="rectangle"/>, in ascending id order.</summary>
    public static ImmutableArray<EntityId> PickInRectangle(ScreenRectangle rectangle, PickingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        return [.. SelectableEntities(context)
            .Where(entity => entity.Kind == EntityKind.Villager)
            .Where(entity => context.Projection.ToScreen(BodyCenter(entity)) is { } screen && rectangle.Contains(screen))
            .Select(entity => entity.Id)];
    }

    private static IEnumerable<EntitySnapshot> SelectableEntities(PickingContext context) =>
        context.Snapshot.Entities.Where(entity => entity.Owner == context.LocalPlayer && PickShape.Of(entity.Kind) is not null);

    /// <summary>Distance from the entity center to the pointer ray at the entity's pick height; null on a miss.</summary>
    private static float? DistanceToHit(EntitySnapshot entity, Vector2 screen, ScreenProjection projection)
    {
        var shape = PickShape.Of(entity.Kind)!.Value;

        if (projection.GroundAt(screen, shape.Height) is not { } hit)
        {
            return null;
        }

        var offset = hit - Position(entity);
        var isHit = shape.IsSquare
            ? MathF.Max(MathF.Abs(offset.X), MathF.Abs(offset.Y)) <= shape.HalfExtent
            : offset.Length() <= shape.HalfExtent;

        return isHit ? offset.Length() : null;
    }

    private static Vector3 BodyCenter(EntitySnapshot entity)
    {
        var position = Position(entity);
        return new Vector3(position.X, PickShape.Of(entity.Kind)!.Value.Height, position.Y);
    }

    private static Vector2 Position(EntitySnapshot entity) => new(entity.Position.X.ToSingle(), entity.Position.Y.ToSingle());
}

/// <summary>
/// Clickable volume of a selectable kind, approximated by a horizontal disc or square at half the drawn height,
/// so clicks on the body of the tilted 3D shape hit. Sizes follow the primitives drawn by Rts.Rendering.
/// </summary>
/// <param name="HalfExtent">Radius of the disc or half the edge of the square, in tiles.</param>
internal readonly record struct PickShape(float Height, float HalfExtent, bool IsSquare)
{
    private static readonly PickShape Villager = new(Height: 0.4f, HalfExtent: 0.5f, IsSquare: false);
    private static readonly PickShape TownCenter = new(Height: 0.75f, HalfExtent: 1.5f, IsSquare: true);

    /// <summary>The shape of <paramref name="kind"/>, null if the kind cannot be selected.</summary>
    public static PickShape? Of(EntityKind kind) => kind switch
    {
        EntityKind.Villager => Villager,
        EntityKind.TownCenter => TownCenter,
        _ => null,
    };
}
