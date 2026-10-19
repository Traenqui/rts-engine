using System.Collections.Immutable;
using Rts.Navigation.Grid;

namespace Rts.Simulation.Maps;

/// <summary>
/// Parses the ASCII map format of ADR-0011. This is a system boundary: every violation is reported
/// as <see cref="MapFormatException"/> with the offending line and column.
/// </summary>
public static class AsciiMapParser
{
    /// <summary>Maximum width and height in tiles; the fixed-point range in ADR-0005 is sized for it.</summary>
    public const int MaxMapSize = 256;

    private const char Ground = '.';
    private const char Tree = 'T';
    private const char TownCenter = 'C';
    private const char Villager = 'V';
    private const char Rock = '#';

    /// <summary>Parses <paramref name="text"/>; one line per row, the top row is y = 0.</summary>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
    /// <exception cref="MapFormatException">The map violates the format.</exception>
    public static MapDefinition Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var rows = SplitRows(text);
        var width = ValidateDimensions(rows);
        var cells = ReadCells(rows);
        var townCenterOrigin = ValidateTownCenter(cells[TownCenter]);

        return new MapDefinition(
            width,
            rows.Length,
            townCenterOrigin,
            cells[Tree].ToImmutable(),
            cells[Villager].ToImmutable(),
            cells[Rock].ToImmutable());
    }

    private static string[] SplitRows(string text)
    {
        var rows = text.Replace("\r\n", "\n").Split('\n');
        var rowCount = rows.Length;

        // Editors commonly add trailing newlines; they are not part of the map.
        while (rowCount > 0 && rows[rowCount - 1].Length == 0)
        {
            rowCount--;
        }

        return rows[..rowCount];
    }

    private static int ValidateDimensions(string[] rows)
    {
        if (rows.Length == 0)
        {
            throw new MapFormatException(1, 1, "map is empty");
        }

        var width = rows[0].Length;

        if (width > MaxMapSize || rows.Length > MaxMapSize)
        {
            throw new MapFormatException(1, 1, $"map exceeds {MaxMapSize}x{MaxMapSize} tiles");
        }

        for (var y = 1; y < rows.Length; y++)
        {
            if (rows[y].Length != width)
            {
                throw new MapFormatException(y + 1, 1, $"row has {rows[y].Length} tiles, expected {width}");
            }
        }

        return width;
    }

    private static Dictionary<char, ImmutableArray<GridPosition>.Builder> ReadCells(string[] rows)
    {
        var cells = new Dictionary<char, ImmutableArray<GridPosition>.Builder>
        {
            [Tree] = ImmutableArray.CreateBuilder<GridPosition>(),
            [TownCenter] = ImmutableArray.CreateBuilder<GridPosition>(),
            [Villager] = ImmutableArray.CreateBuilder<GridPosition>(),
            [Rock] = ImmutableArray.CreateBuilder<GridPosition>(),
        };

        // Row-major order keeps entity lists, and therefore entity IDs later on, deterministic.
        for (var y = 0; y < rows.Length; y++)
        {
            for (var x = 0; x < rows[y].Length; x++)
            {
                var symbol = rows[y][x];

                if (cells.TryGetValue(symbol, out var positions))
                {
                    positions.Add(new GridPosition(x, y));
                }
                else if (symbol != Ground)
                {
                    throw new MapFormatException(y + 1, x + 1, $"unknown symbol '{symbol}'");
                }
            }
        }

        return cells;
    }

    private static GridPosition ValidateTownCenter(IReadOnlyList<GridPosition> cells)
    {
        const int expectedCellCount = MapDefinition.TownCenterSize * MapDefinition.TownCenterSize;
        const int expectedSpan = MapDefinition.TownCenterSize - 1;

        if (cells.Count == 0)
        {
            throw new MapFormatException(1, 1, $"map contains no town center ('{TownCenter}')");
        }

        // Cells arrive in row-major order, so the first cell is the top-left one of a valid block.
        var origin = cells[0];
        var isSingleFullBlock = cells.Count == expectedCellCount
            && cells.Min(cell => cell.x) == origin.x
            && cells.Max(cell => cell.x) - origin.x == expectedSpan
            && cells[^1].y - origin.y == expectedSpan;

        if (!isSingleFullBlock)
        {
            throw new MapFormatException(
                origin.y + 1,
                origin.x + 1,
                $"town center must be exactly one complete {MapDefinition.TownCenterSize}x{MapDefinition.TownCenterSize} block of '{TownCenter}'");
        }

        return origin;
    }
}
