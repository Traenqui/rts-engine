namespace Rts.Navigation.Grid;

public sealed class NavGrid
{
    private readonly bool[] _walkable;

    // Array instead of ReadOnlySpan: spans cannot live across yield boundaries (CS4007)
    private static readonly GridPosition[] NeighborDirections =
    [
        new GridPosition { X = 0, Y = -1 }, // Up
        new GridPosition { X = 1, Y = 0 },  // Right
        new GridPosition { X = 0, Y = 1 },  // Down
        new GridPosition { X = -1, Y = 0 }  // Left
    ];

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
        return position.X >= 0 && position.X < Width && position.Y >= 0 && position.Y < Height;
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

    public IEnumerable<GridPosition> GetNeighbors(GridPosition position)
    {
        foreach (var direction in NeighborDirections)
        {
            var neighbor = new GridPosition { X = position.X + direction.X, Y = position.Y + direction.Y };

            if (IsWalkable(neighbor))
            {
                yield return neighbor;
            }
        }
    }

    private int ToIndex(GridPosition position)
    {
        return position.Y * Width + position.X;
    }
}
