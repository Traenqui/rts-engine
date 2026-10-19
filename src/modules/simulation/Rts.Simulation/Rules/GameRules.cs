using Rts.Primitives;

namespace Rts.Simulation.Rules;

/// <summary>
/// Balancing values injected into the simulation (ADR-0010). All rates are per tick; conversions from
/// per-second values happen once when the rules are created, never inside the simulation.
/// </summary>
/// <param name="VillagerSpeedPerTick">Distance in tiles a villager walks per tick.</param>
public sealed record GameRules(Fixed VillagerSpeedPerTick);
