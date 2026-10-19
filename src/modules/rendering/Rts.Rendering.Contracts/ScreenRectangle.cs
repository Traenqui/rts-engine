using System.Numerics;

namespace Rts.Rendering.Contracts;

/// <summary>Axis-aligned rectangle in screen pixels, origin top left; used for box selection.</summary>
/// <param name="Min">Top left corner.</param>
/// <param name="Max">Bottom right corner.</param>
public readonly record struct ScreenRectangle(Vector2 Min, Vector2 Max)
{
    public Vector2 Size => Max - Min;

    /// <summary>The rectangle spanned by two opposite corners given in any order, e.g. drag start and pointer.</summary>
    public static ScreenRectangle FromCorners(Vector2 first, Vector2 second) =>
        new(Vector2.Min(first, second), Vector2.Max(first, second));

    /// <summary>True if <paramref name="point"/> lies inside or on the edge.</summary>
    public bool Contains(Vector2 point) =>
        point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y;
}
