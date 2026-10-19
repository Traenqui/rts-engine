using Rts.Simulation.Contracts;
using Rts.Simulation.Rules;

namespace Rts.Simulation.Scenarios;

/// <summary>Plays a <see cref="Scenario"/> headless from tick 0 to its end.</summary>
public static class ScenarioRunner
{
    /// <returns>The state after <see cref="Scenario.Ticks"/> ticks.</returns>
    public static WorldSnapshot Run(Scenario scenario, GameRules rules)
    {
        ArgumentNullException.ThrowIfNull(scenario);

        var simulation = new GameSimulation(scenario.Map, rules);

        foreach (var command in scenario.Commands)
        {
            simulation.Enqueue(command);
        }

        for (var tick = 0; tick < scenario.Ticks; tick++)
        {
            simulation.Step();
        }

        return simulation.CreateSnapshot();
    }
}
