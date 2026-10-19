namespace Rts.Simulation.Contracts;

/// <summary>
/// Handle to a simulation entity. <see cref="Generation"/> increases whenever a slot is reused,
/// so handles to destroyed entities are detected instead of silently pointing at a new one (ADR-0006).
/// </summary>
public readonly record struct EntityId(int Index, int Generation);
