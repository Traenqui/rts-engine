using System.Collections.Immutable;
using Rts.Navigation.Grid;
using Rts.Primitives;
using Rts.Simulation.Contracts;
using Rts.Simulation.Ecs;
using Rts.Simulation.Maps;
using Rts.Simulation.Movement;
using Rts.Simulation.Rules;

namespace Rts.Simulation;

/// <summary>
/// Deterministic fixed-tick simulation (ADR-0004). Hosts enqueue commands, call <see cref="Step"/> once per tick
/// and read the result through <see cref="CreateSnapshot"/>; nothing else crosses this boundary.
/// </summary>
public sealed class GameSimulation
{
    private readonly World _world;
    private readonly GameRules _rules;
    private readonly SortedDictionary<long, List<Command>> _pendingByTick = [];
    private long _tick;
    private int _rejectedCommandCount;

    /// <summary>Creates the start state of <paramref name="map"/> at tick 0, played with <paramref name="rules"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The villager speed is not positive.</exception>
    public GameSimulation(MapDefinition map, GameRules rules)
    {
        ArgumentNullException.ThrowIfNull(map);
        ArgumentNullException.ThrowIfNull(rules);

        if (rules.VillagerSpeedPerTick <= Fixed.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(rules), "Villager speed must be positive.");
        }

        _world = WorldFactory.FromMap(map);
        _rules = rules;
        Terrain = new TerrainInfo(map.Width, map.Height, map.Rocks);
    }

    /// <summary>Static terrain of the map, for rendering and clients.</summary>
    public TerrainInfo Terrain { get; }

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

    /// <summary>Executes the commands of the current tick, runs all systems and advances by one tick.</summary>
    public void Step()
    {
        if (_pendingByTick.Remove(_tick, out var commands))
        {
            foreach (var command in commands)
            {
                Execute(command);
            }
        }

        MovementSystem.Update(_world, _rules.VillagerSpeedPerTick);
        _tick++;
    }

    /// <summary>Immutable view of the current state, including its deterministic hash.</summary>
    public WorldSnapshot CreateSnapshot()
    {
        var entities = _world.Identities.EntitiesInIdOrder()
            .Select(CreateEntitySnapshot)
            .ToImmutableArray();

        return new WorldSnapshot(_tick, ComputeStateHash(entities), _rejectedCommandCount, entities);
    }

    private EntitySnapshot CreateEntitySnapshot(EntityId id)
    {
        var identity = _world.Identities.Get(id);
        return new EntitySnapshot(id, identity.Kind, identity.Owner, _world.Positions.Get(id), RemainingPath(id));
    }

    private ImmutableArray<GridPosition> RemainingPath(EntityId id)
    {
        if (!_world.Paths.Has(id))
        {
            return [];
        }

        var follower = _world.Paths.Get(id);
        return follower.Waypoints[follower.NextWaypoint..];
    }

    private void Execute(Command command)
    {
        var isExecuted = command switch
        {
            MoveCommand move => MoveCommandHandler.TryExecute(_world, move),
            _ => false,
        };

        if (!isExecuted)
        {
            Reject();
        }
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

        foreach (var unit in _world.Paths.EntitiesInIdOrder())
        {
            var follower = _world.Paths.Get(unit);
            hasher.Add(unit.Index);
            hasher.Add(follower.NextWaypoint);

            foreach (var waypoint in follower.Waypoints)
            {
                hasher.Add(waypoint.X);
                hasher.Add(waypoint.Y);
            }
        }

        return hasher.Value;
    }
}
