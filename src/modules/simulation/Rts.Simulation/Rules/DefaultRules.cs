using Rts.Primitives;
using Rts.Simulation.Contracts;

namespace Rts.Simulation.Rules;

/// <summary>Milestone 1 balancing values, see appendix A of the milestone 1 PRD.</summary>
public static class DefaultRules
{
    // 0.8 tiles per second, close to an AoE2 villager.
    private const int VillagerSpeedTilesNumerator = 4;
    private const int VillagerSpeedTilesDenominator = 5;

    public static GameRules Create()
    {
        return new GameRules(
            VillagerSpeedPerTick: Fixed.FromRatio(
                VillagerSpeedTilesNumerator,
                VillagerSpeedTilesDenominator * SimulationClock.TicksPerSecond));
    }
}
