using Rts.Navigation.Grid;
using Rts.Simulation.Maps;

namespace Rts.Simulation.Tests.Maps;

public sealed class AsciiMapParserTests
{
    private static readonly string ValidMap = Lines(
        "T...#",
        ".CCC.",
        ".CCC.",
        ".CCCV",
        "V..TT");

    [Fact]
    public void Parse_ValidMap_ReadsDimensionsAndEntities()
    {
        var map = AsciiMapParser.Parse(ValidMap);

        Assert.Equal(5, map.Width);
        Assert.Equal(5, map.Height);
        Assert.Equal(new GridPosition(1, 1), map.TownCenterOrigin);
        Assert.Equal<GridPosition>([new GridPosition(0, 0), new GridPosition(3, 4), new GridPosition(4, 4)], map.Trees);
        Assert.Equal<GridPosition>([new GridPosition(4, 3), new GridPosition(0, 4)], map.Villagers);
        Assert.Equal<GridPosition>([new GridPosition(4, 0)], map.Rocks);
    }

    [Fact]
    public void Parse_WindowsLineEndingsAndTrailingNewline_AreAccepted()
    {
        var map = AsciiMapParser.Parse(ValidMap.Replace("\n", "\r\n") + "\r\n");

        Assert.Equal(5, map.Height);
    }

    [Fact]
    public void BuildNavGrid_BlocksTreesRocksAndTownCenter()
    {
        var map = AsciiMapParser.Parse(ValidMap);

        var grid = map.BuildNavGrid();

        Assert.False(grid.IsWalkable(new GridPosition(0, 0)), "tree");
        Assert.False(grid.IsWalkable(new GridPosition(4, 0)), "rock");
        Assert.False(grid.IsWalkable(new GridPosition(2, 2)), "town center");
        Assert.True(grid.IsWalkable(new GridPosition(1, 0)), "ground");
        Assert.True(grid.IsWalkable(new GridPosition(4, 3)), "villager start");
    }

    [Fact]
    public void Parse_Null_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => AsciiMapParser.Parse(null!));
    }

    [Theory]
    [InlineData("")]
    [InlineData("\n")]
    [InlineData("\r\n\r\n")]
    public void Parse_EmptyInput_Throws(string text)
    {
        var error = Assert.Throws<MapFormatException>(() => AsciiMapParser.Parse(text));

        Assert.Equal(1, error.Line);
        Assert.Equal(1, error.Column);
    }

    [Fact]
    public void Parse_UnknownCharacter_ReportsLineAndColumn()
    {
        var text = ValidMap.Replace("V..TT", "V.XTT");

        var error = Assert.Throws<MapFormatException>(() => AsciiMapParser.Parse(text));

        Assert.Equal(5, error.Line);
        Assert.Equal(3, error.Column);
        Assert.Contains("'X'", error.Message);
    }

    [Fact]
    public void Parse_RowsOfDifferentLength_ReportsFirstDeviatingLine()
    {
        var text = ValidMap.Replace(".CCC.\n.CCC.", ".CCC.\n.CCC");

        var error = Assert.Throws<MapFormatException>(() => AsciiMapParser.Parse(text));

        Assert.Equal(3, error.Line);
    }

    [Fact]
    public void Parse_NoTownCenter_Throws()
    {
        var error = Assert.Throws<MapFormatException>(() => AsciiMapParser.Parse(Lines("...", ".V.")));

        Assert.Contains("town center", error.Message);
    }

    [Theory]
    [InlineData(".CC.", ".CC.", ".CC.")]
    [InlineData("CCC.", "CCC.", "CC..")]
    [InlineData("CCC..", "CCC..", "CCC.C")]
    [InlineData("CCCCCC", "CCCCCC", "CCCCCC")]
    public void Parse_TownCenterNotSingleFullBlock_Throws(string row1, string row2, string row3)
    {
        var error = Assert.Throws<MapFormatException>(() => AsciiMapParser.Parse(Lines(row1, row2, row3)));

        Assert.Contains("3x3", error.Message);
    }

    [Fact]
    public void Parse_MapWiderThanLimit_Throws()
    {
        var tooWide = new string('.', AsciiMapParser.MaxMapSize + 1);
        var text = Lines("CCC" + tooWide[3..], "CCC" + tooWide[3..], "CCC" + tooWide[3..]);

        var error = Assert.Throws<MapFormatException>(() => AsciiMapParser.Parse(text));

        Assert.Contains(AsciiMapParser.MaxMapSize.ToString(), error.Message);
    }

    [Fact]
    public void Parse_MapTallerThanLimit_Throws()
    {
        var rows = Enumerable.Repeat("CCC", 3).Concat(Enumerable.Repeat("...", AsciiMapParser.MaxMapSize - 2));

        var error = Assert.Throws<MapFormatException>(() => AsciiMapParser.Parse(Lines([.. rows])));

        Assert.Contains(AsciiMapParser.MaxMapSize.ToString(), error.Message);
    }

    private static string Lines(params string[] rows) => string.Join("\n", rows);
}
