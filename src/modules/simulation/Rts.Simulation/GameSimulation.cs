using System.Collections.Immutable;
using Rts.Simulation.Contracts;
using Rts.Simulation.Ecs;
using Rts.Simulation.Maps;

namespace Rts.Simulation;

/// <summary>
/// Deterministic fixed-tick simulation (ADR-0004). Hosts enqueue commands, call <see cref="Step"/> once per tick
/// and read the result through <see cref="CreateSnapshot"/>; nothing else crosses this boundary.
/// </summary>
public sealed class GameSimulation
{
    private readonly World _world;
    private readonly SortedDictionary<long, List<Command>> _pendingByTick = [];
    private long _tick;
    private int _rejectedCommandCount;

    /// <summary>Creates the start state of <paramref name="map"/> at tick 0.</summary>
    public GameSimulation(MapDefinition map)
    {
        ArgumentNullException.ThrowIfNull(map);
        _world = WorldFactory.FromMap(map);
    }

    /// <summary>
    /// Schedules <paramref name="command"/> for its target tick. Commands for already simulated ticks are rejected,
    /// commands for the same tick run in the order they were enqueued.
    /// </summary>
    public void Enqueue(Command command)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.TargetTick < _tick)
        {
            Reject();
            return;
        }

        if (!_pendingByTick.TryGetValue(command.TargetTick, out var commands))
        {
            commands = [];
            _pendingByTick.Add(command.TargetTick, commands);
        }

        commands.Add(command);
    }

    /// <summary>Executes the commands of the current tick and advances the simulation by one tick.</summary>
    public void Step()
    {
        if (_pendingByTick.Remove(_tick, out var commands))
        {
            // No command type is executable yet (Move follows in step 5), so every command is invalid input.
            foreach (var _ in commands)
            {
                Reject();
            }
        }

        _tick++;
    }

    /// <summary>Immutable view of the current state, including its deterministic hash.</summary>
    public WorldSnapshot CreateSnapshot()
    {
        var entities = _world.Identities.EntitiesInIdOrder()
            .Select(id => new EntitySnapshot(id, _world.Identities.Get(id).Kind, _world.Identities.Get(id).Owner, _world.Positions.Get(id)))
            .ToImmutableArray();

        return new WorldSnapshot(_tick, ComputeStateHash(entities), _rejectedCommandCount, entities);
    }

    private void Reject()
    {
        _rejectedCommandCount++;
    }

    private ulong ComputeStateHash(ImmutableArray<EntitySnapshot> entities)
    {
        var hasher = new StateHasher();
        hasher.Add(_tick);
        hasher.Add(_rejectedCommandCount);

        foreach (var entity in entities)
        {
            hasher.Add(entity.Id.Index);
            hasher.Add(entity.Id.Generation);
            hasher.Add((long)entity.Kind);
            hasher.Add(entity.Owner.Value);
            hasher.Add(entity.Position.X.Raw);
            hasher.Add(entity.Position.Y.Raw);
        }

        return hasher.Value;
    }
}
