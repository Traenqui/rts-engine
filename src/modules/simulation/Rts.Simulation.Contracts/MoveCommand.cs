using System.Collections.Immutable;
using Rts.Navigation.Grid;

namespace Rts.Simulation.Contracts;

/// <summary>
/// Sends <paramref name="Units"/> to the center of <paramref name="Target"/>. The target is a tile because clients
/// quantize picking before creating commands, so no floating-point value ever enters the simulation (ADR-0007).
/// </summary>
public sealed record MoveCommand(
    PlayerId Issuer,
    long TargetTick,
    ImmutableArray<EntityId> Units,
    GridPosition Target) : Command(Issuer, TargetTick);
