using Rts.Simulation.Contracts;

namespace Rts.Simulation.Ecs;

/// <summary>Component every entity has: what it is and who owns it.</summary>
internal readonly record struct Identity(EntityKind Kind, PlayerId Owner);
