using System.Collections.Immutable;
using System.Globalization;
using Rts.Navigation.Grid;
using Rts.Simulation.Contracts;
using Rts.Simulation.Ecs;
using Rts.Simulation.Maps;

namespace Rts.Simulation.Scenarios;

/// <summary>
/// Parses scenario files. Format, one directive per line:
/// <code>
/// # comment (outside the map block, where '#' is a rock)
/// ticks 400
/// map
/// ...ASCII map (ADR-0011)...
/// end
/// at 0 move 1,2 to 4,1
/// </code>
/// Entities are referenced by their index, which is deterministic per map. All commands are issued by player 1.
/// </summary>
public static class ScenarioParser
{
    /// <summary>Upper bound for <c>ticks</c>, about 28 hours of game time, so a typo cannot hang CI.</summary>
    public const int MaxTicks = 2_000_000;

    private const string CommandSyntax = "expected 'at <tick> move <entity>[,<entity>...] to <x>,<y>'";
    private const int CommandTokenCount = 6;

    /// <exception cref="ArgumentNullException"><paramref name="text"/> is null.</exception>
    /// <exception cref="ScenarioFormatException">The scenario violates the format.</exception>
    public static Scenario Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var state = new ParseState();
        var lines = text.Replace("\r\n", "\n").Split('\n');

        for (var index = 0; index < lines.Length; index++)
        {
            ParseLine(state, lines[index], lineNumber: index + 1);
        }

        return Finish(state);
    }

    private static void ParseLine(ParseState state, string line, int lineNumber)
    {
        if (state.MapStartLine is not null)
        {
            ParseMapLine(state, line, lineNumber);
            return;
        }

        var tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (tokens.Length == 0 || tokens[0].StartsWith('#'))
        {
            return;
        }

        switch (tokens[0])
        {
            case "ticks":
                state.Ticks = ParseTicks(state, tokens, lineNumber);
                break;
            case "map":
                StartMap(state, lineNumber);
                break;
            case "at":
                state.Commands.Add((ParseCommand(tokens, lineNumber), lineNumber));
                break;
            default:
                throw new ScenarioFormatException(lineNumber, $"unknown directive '{tokens[0]}'");
        }
    }

    private static void StartMap(ParseState state, int lineNumber)
    {
        if (state.Map is not null)
        {
            throw new ScenarioFormatException(lineNumber, "scenario contains more than one 'map' block");
        }

        state.MapStartLine = lineNumber;
    }

    private static void ParseMapLine(ParseState state, string line, int lineNumber)
    {
        if (line.Trim() != "end")
        {
            state.MapRows.Add(line);
            return;
        }

        var firstRowLine = state.MapStartLine!.Value + 1;
        state.MapStartLine = null;

        try
        {
            state.Map = AsciiMapParser.Parse(string.Join("\n", state.MapRows));
        }
        catch (MapFormatException error)
        {
            throw new ScenarioFormatException(firstRowLine + error.Line - 1, error.Message, error);
        }
    }

    private static int ParseTicks(ParseState state, string[] tokens, int lineNumber)
    {
        if (state.Ticks is not null)
        {
            throw new ScenarioFormatException(lineNumber, "duplicate 'ticks'");
        }

        var isValid = tokens.Length == 2
            && int.TryParse(tokens[1], NumberStyles.None, CultureInfo.InvariantCulture, out var ticks)
            && ticks is > 0 and <= MaxTicks;

        return isValid
            ? int.Parse(tokens[1], CultureInfo.InvariantCulture)
            : throw new ScenarioFormatException(lineNumber, $"expected 'ticks <1..{MaxTicks}>'");
    }

    private static MoveCommand ParseCommand(string[] tokens, int lineNumber)
    {
        if (tokens.Length != CommandTokenCount || tokens[4] != "to")
        {
            throw new ScenarioFormatException(lineNumber, CommandSyntax);
        }

        if (!long.TryParse(tokens[1], NumberStyles.None, CultureInfo.InvariantCulture, out var tick))
        {
            throw new ScenarioFormatException(lineNumber, $"tick must be a non-negative integer, got '{tokens[1]}'");
        }

        if (tokens[2] != "move")
        {
            throw new ScenarioFormatException(lineNumber, $"unknown command '{tokens[2]}'");
        }

        return new MoveCommand(WorldFactory.MapPlayer, tick, ParseEntities(tokens[3], lineNumber), ParsePosition(tokens[5], lineNumber));
    }

    private static ImmutableArray<EntityId> ParseEntities(string token, int lineNumber)
    {
        var builder = ImmutableArray.CreateBuilder<EntityId>();

        foreach (var part in token.Split(','))
        {
            if (!int.TryParse(part, NumberStyles.None, CultureInfo.InvariantCulture, out var index))
            {
                throw new ScenarioFormatException(lineNumber, $"entity must be a non-negative index, got '{part}'");
            }

            builder.Add(new EntityId(index, 0));
        }

        return builder.ToImmutable();
    }

    private static GridPosition ParsePosition(string token, int lineNumber)
    {
        var parts = token.Split(',');
        var isValid = parts.Length == 2
            && int.TryParse(parts[0], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _)
            && int.TryParse(parts[1], NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out _);

        return isValid
            ? new GridPosition(int.Parse(parts[0], CultureInfo.InvariantCulture), int.Parse(parts[1], CultureInfo.InvariantCulture))
            : throw new ScenarioFormatException(lineNumber, $"position must be '<x>,<y>', got '{token}'");
    }

    private static Scenario Finish(ParseState state)
    {
        if (state.MapStartLine is { } mapStartLine)
        {
            throw new ScenarioFormatException(mapStartLine, "'map' block is not closed with 'end'");
        }

        var ticks = state.Ticks ?? throw new ScenarioFormatException(1, "missing 'ticks'");
        var map = state.Map ?? throw new ScenarioFormatException(1, "missing 'map' block");

        foreach (var (command, lineNumber) in state.Commands)
        {
            if (command.TargetTick >= ticks)
            {
                throw new ScenarioFormatException(lineNumber, $"tick {command.TargetTick} is not before the end tick {ticks}");
            }
        }

        return new Scenario(ticks, map, [.. state.Commands.Select(entry => (Command)entry.Command)]);
    }

    private sealed class ParseState
    {
        public int? Ticks { get; set; }

        public MapDefinition? Map { get; set; }

        public int? MapStartLine { get; set; }

        public List<string> MapRows { get; } = [];

        public List<(MoveCommand Command, int Line)> Commands { get; } = [];
    }
}
