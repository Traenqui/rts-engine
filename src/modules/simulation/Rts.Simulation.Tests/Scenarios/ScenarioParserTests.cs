using Rts.Navigation.Grid;
using Rts.Simulation.Contracts;
using Rts.Simulation.Maps;
using Rts.Simulation.Scenarios;

namespace Rts.Simulation.Tests.Scenarios;

public sealed class ScenarioParserTests
{
    private static readonly string[] ValidLines =
    [
        "# villagers walk around the rocks",
        "ticks 400",
        "",
        "map",
        "V..#...",
        "...#...",
        "CCC....",
        "CCC....",
        "CCC...V",
        "end",
        "at 0 move 1,2 to 4,1",
        "at 120 move 1 to 0,0",
    ];

    [Fact]
    public void Parse_ValidScenario_ReadsTicksMapAndCommands()
    {
        var scenario = ScenarioParser.Parse(Text(ValidLines));

        Assert.Equal(400, scenario.Ticks);
        Assert.Equal(7, scenario.Map.Width);
        Assert.Equal(2, scenario.Commands.Length);

        var first = Assert.IsType<MoveCommand>(scenario.Commands[0]);
        Assert.Equal(0, first.TargetTick);
        Assert.Equal<EntityId>([new EntityId(1, 0), new EntityId(2, 0)], first.Units);
        Assert.Equal(new GridPosition(4, 1), first.Target);
        Assert.Equal(new PlayerId(1), first.Issuer);

        var second = Assert.IsType<MoveCommand>(scenario.Commands[1]);
        Assert.Equal(120, second.TargetTick);
    }

    [Fact]
    public void Parse_HashInsideMapBlock_IsRockNotComment()
    {
        var scenario = ScenarioParser.Parse(Text(ValidLines));

        Assert.Equal<GridPosition>([new GridPosition(3, 0), new GridPosition(3, 1)], scenario.Map.Rocks);
    }

    [Fact]
    public void Parse_WindowsLineEndings_AreAccepted()
    {
        var scenario = ScenarioParser.Parse(string.Join("\r\n", ValidLines));

        Assert.Equal(2, scenario.Commands.Length);
    }

    [Fact]
    public void Parse_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => ScenarioParser.Parse(null!));
    }

    [Theory]
    [InlineData("ticks 400", "ticks 500", 3, "ticks")]
    [InlineData("ticks 400", "ticks 0", 2, "ticks")]
    [InlineData("ticks 400", "ticks many", 2, "ticks")]
    [InlineData("ticks 400", "ticks 2000001", 2, "ticks")]
    [InlineData("at 120 move 1 to 0,0", "jump 1", 12, "unknown directive")]
    [InlineData("at 120 move 1 to 0,0", "at 400 move 1 to 0,0", 12, "tick")]
    [InlineData("at 120 move 1 to 0,0", "at -1 move 1 to 0,0", 12, "tick")]
    [InlineData("at 120 move 1 to 0,0", "at 120 move 1 0,0", 12, "expected")]
    [InlineData("at 120 move 1 to 0,0", "at 120 move x to 0,0", 12, "entity")]
    [InlineData("at 120 move 1 to 0,0", "at 120 move 1 to 0;0", 12, "position")]
    [InlineData("at 120 move 1 to 0,0", "at 120 build 1 to 0,0", 12, "command")]
    public void Parse_InvalidLine_ReportsLineAndReason(string original, string replacement, int expectedLine, string reason)
    {
        var lines = ValidLines.Select(line => line == original ? replacement : line).ToList();
        if (original == "ticks 400" && replacement == "ticks 500")
        {
            lines = [.. ValidLines.Take(2), replacement, .. ValidLines.Skip(2)];
        }

        var error = Assert.Throws<ScenarioFormatException>(() => ScenarioParser.Parse(Text(lines)));

        Assert.Equal(expectedLine, error.Line);
        Assert.Contains(reason, error.Message);
    }

    [Fact]
    public void Parse_MissingTicks_Throws()
    {
        var error = Assert.Throws<ScenarioFormatException>(
            () => ScenarioParser.Parse(Text(ValidLines.Where(line => !line.StartsWith("ticks", StringComparison.Ordinal)))));

        Assert.Contains("ticks", error.Message);
    }

    [Fact]
    public void Parse_MissingMap_Throws()
    {
        var error = Assert.Throws<ScenarioFormatException>(() => ScenarioParser.Parse(Text(["ticks 10"])));

        Assert.Contains("map", error.Message);
    }

    [Fact]
    public void Parse_SecondMapBlock_Throws()
    {
        var lines = ValidLines.Concat(["map", "CCC", "CCC", "CCC", "end"]);

        var error = Assert.Throws<ScenarioFormatException>(() => ScenarioParser.Parse(Text(lines)));

        Assert.Equal(13, error.Line);
    }

    [Fact]
    public void Parse_UnterminatedMap_ReportsMapStart()
    {
        var lines = ValidLines.Take(9);

        var error = Assert.Throws<ScenarioFormatException>(() => ScenarioParser.Parse(Text(lines)));

        Assert.Equal(4, error.Line);
        Assert.Contains("end", error.Message);
    }

    [Fact]
    public void Parse_InvalidMap_ReportsLineInScenarioFile()
    {
        var lines = ValidLines.Select(line => line == "...#..." ? "...X..." : line);

        var error = Assert.Throws<ScenarioFormatException>(() => ScenarioParser.Parse(Text(lines)));

        Assert.Equal(6, error.Line);
        Assert.IsType<MapFormatException>(error.InnerException);
    }

    private static string Text(IEnumerable<string> lines) => string.Join("\n", lines);
}
