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

    private int ToIndex(GridPosition position)
    {
        return position.y * Width + position.x;
    }
}
