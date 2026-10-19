using Rts.Simulation.Scenarios;

namespace Rts.Simulation.Tests.Scenarios;

public sealed class GoldenHashesTests
{
    [Fact]
    public void FormatThenParse_RoundTripsSortedByName()
    {
        var hashes = new Dictionary<string, ulong> { ["walk.scenario"] = 0xABCDEF, ["around.scenario"] = 1 };

        var text = GoldenHashes.Format(hashes);
        var parsed = GoldenHashes.Parse(text);

        Assert.Equal("around.scenario 0000000000000001\nwalk.scenario 0000000000abcdef\n", text);
        Assert.Equal(hashes.OrderBy(pair => pair.Key, StringComparer.Ordinal), parsed);
    }

    [Fact]
    public void Parse_IgnoresCommentsAndBlankLines()
    {
        var parsed = GoldenHashes.Parse("# generated\n\nwalk.scenario 00000000000000ff\r\n");

        Assert.Equal(0xFFUL, parsed["walk.scenario"]);
    }

    [Theory]
    [InlineData("walk.scenario", 1)]
    [InlineData("walk.scenario xyz", 1)]
    [InlineData("a.scenario 01\na.scenario 02", 2)]
    public void Parse_InvalidLine_ThrowsWithLine(string text, int line)
    {
        var error = Assert.Throws<FormatException>(() => GoldenHashes.Parse(text));

        Assert.Contains($"line {line}", error.Message);
    }

    [Fact]
    public void Verify_ReportsEveryScenarioByStatus()
    {
        var expected = new Dictionary<string, ulong> { ["same"] = 1, ["changed"] = 2, ["deleted"] = 3 };
        var actual = new Dictionary<string, ulong> { ["same"] = 1, ["changed"] = 9, ["new"] = 4 };

        var results = GoldenHashes.Verify(expected, actual);

        Assert.Equal<VerificationResult>(
            [
                new VerificationResult("changed", VerificationStatus.Mismatch, 2, 9),
                new VerificationResult("deleted", VerificationStatus.MissingScenario, 3, null),
                new VerificationResult("new", VerificationStatus.MissingGolden, null, 4),
                new VerificationResult("same", VerificationStatus.Match, 1, 1),
            ],
            results);
    }
}
