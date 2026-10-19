namespace Rts.Simulation.Contracts;

/// <summary>Identifies a player. Present from milestone 1 on so ownership checks exist from day one (ADR-0009).</summary>
public readonly record struct PlayerId(int Value)
{
    /// <summary>Owner of neutral entities such as trees.</summary>
    public static PlayerId Nature => new(0);
}
