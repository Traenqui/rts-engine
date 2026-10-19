using System.Collections.Immutable;
using Rts.Simulation.Contracts;
using Rts.Simulation.Maps;

namespace Rts.Simulation.Scenarios;

/// <summary>
/// A reproducible game: start map, timed commands and duration. Same scenario, same rules, same hash on every
/// platform; this is also the shape of a future replay (ADR-0004).
/// </summary>
/// <param name="Commands">Commands in file order, which is also their execution order within a tick.</param>
public sealed record Scenario(int Ticks, MapDefinition Map, ImmutableArray<Command> Commands);
