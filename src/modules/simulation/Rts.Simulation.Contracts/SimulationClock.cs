namespace Rts.Simulation.Contracts;

/// <summary>Fixed simulation rate, independent of the frame rate (ADR-0004).</summary>
public static class SimulationClock
{
    /// <summary>Simulation ticks per second of game time.</summary>
    public const int TicksPerSecond = 20;
}
