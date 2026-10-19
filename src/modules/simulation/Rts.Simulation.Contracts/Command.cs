namespace Rts.Simulation.Contracts;

/// <summary>
/// The only way into the simulation (ADR-0009). Commands are untrusted: the simulation validates them
/// when <see cref="TargetTick"/> is processed and deterministically rejects invalid ones.
/// </summary>
/// <param name="Issuer">Player who sent the command.</param>
/// <param name="TargetTick">Tick in which the command is executed.</param>
public abstract record Command(PlayerId Issuer, long TargetTick);
