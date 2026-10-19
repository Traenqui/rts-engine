namespace Rts.Simulation.Maps;

/// <summary>An ASCII map is malformed. <see cref="Line"/> and <see cref="Column"/> are 1-based.</summary>
public sealed class MapFormatException : FormatException
{
    public MapFormatException(int line, int column, string reason)
        : base($"Invalid map at line {line}, column {column}: {reason}")
    {
        Line = line;
        Column = column;
    }

    public int Line { get; }

    public int Column { get; }
}
