using Rts.Primitives;

namespace Rts.Navigation.Grid;

/// <summary>
/// Conversion between tiles and world coordinates (1 tile = 1.0 world unit, see ADR-0005).
/// Tile (x, y) covers the world area [x, x + 1) × [y, y + 1).
/// </summary>
public static class WorldGrid
{
    private static readonly Fixed Half = Fixed.FromRatio(1, 2);

    /// <summary>World position of the tile center, used as movement waypoint (ADR-0008).</summary>
    public static FixedVector2 TileCenter(GridPosition tile)
    {
        return new FixedVector2(Fixed.FromInt(tile.x) + Half, Fixed.FromInt(tile.y) + Half);
    }

    /// <summary>
    /// Tile containing <paramref name="position"/>. Positions left of or above the map yield negative
    /// indices; callers check <see cref="NavGrid.IsInside"/>.
    /// </summary>
    public static GridPosition TileAt(FixedVector2 position)
    {
        return new GridPosition(position.X.FloorToInt(), position.Y.FloorToInt());
    }
}
