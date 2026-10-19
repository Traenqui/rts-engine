using Rts.Navigation.Grid;
using Rts.Simulation.Rules;
using Rts.Simulation.Scenarios;

namespace Rts.Simulation.Tests.Scenarios;

public sealed class ScenarioRunnerTests
{
    private static readonly Scenario Walk = ScenarioParser.Parse(string.Join(
        "\n",
        "ticks 500",
        "map",
        "V......",
        "CCC....",
        "CCC....",
        "CCC....",
        "end",
        "at 0 move 1 to 6,0",
        "at 10 move 0 to 6,0"));

    [Fact]
    public void Run_SimulatesAllTicksAndCommands()
    {
        var snapshot = ScenarioRunner.Run(Walk, DefaultRules.Create());

        Assert.Equal(Walk.Ticks, snapshot.Tick);
        Assert.Equal(WorldGrid.TileCenter(new GridPosition(6, 0)), snapshot.Entities[1].Position);
        Assert.Equal(1, snapshot.RejectedCommandCount);
    }

    [Fact]
    public void Run_Twice_ProducesIdenticalHash()
    {
        var first = ScenarioRunner.Run(Walk, DefaultRules.Create());
        var second = ScenarioRunner.Run(Walk, DefaultRules.Create());

        Assert.Equal(first.StateHash, second.StateHash);
    }
}
