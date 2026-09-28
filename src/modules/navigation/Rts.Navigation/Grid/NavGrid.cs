using Rts.Navigation.Contracts;

namespace Rts.Navigation.Grid;

public sealed class NavGrid
{
    private readonly bool[] _walkable;

    public int Width { get; }
    public int Height { get; }

    public NavGrid(int width, int height)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);

        Width = width;
        Height = height;

        _walkable = new bool[width * height];

        Array.Fill(_walkable, true);
    }

    public bool IsInside(GridPosition position)
    {
        return position.x >= 0 && position.x < Width && position.y >= 0 && position.y < Height;
    }

    public bool IsWalkable(GridPosition position)
    {
        if (!IsInside(position))
        {
            return false;
        }
        return _walkable[ToIndex(position)];
    }

    public void SetWalkable(GridPosition position, bool walkable)
    {
        if (!IsInside(position))
        {
            throw new ArgumentOutOfRangeException(nameof(position));
        }

        _walkable[ToIndex(position)] = walkable;
    }

    // Array instead of ReadOnlySpan: spans cannot live across yield boundaries (CS4007)
    private static readonly GridPosition[] NeighborDirections =
    [
        new GridPosition { x = 0, y = -1 }, // Up
        new GridPosition { x = 1, y = 0 },  // Right
        new GridPosition { x = 0, y = 1 },  // Down
        new GridPosition { x = -1, y = 0 }  // Left
    ];

    public IEnumerable<GridPosition> GetNeighbors(GridPosition position)
    {
        foreach (var direction in NeighborDirections)
        {
            var neighbor = new GridPosition { x = position.x + direction.x, y = position.y + direction.y };

            if (IsWalkable(neighbor))
            {
                yield return neighbor;
            }
        }
    }

    private int ToIndex(GridPosition position)
    {
        return position.y * Width + position.x;
    }
}
