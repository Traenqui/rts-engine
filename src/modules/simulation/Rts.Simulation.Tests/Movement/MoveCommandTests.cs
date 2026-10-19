using System.Collections.Immutable;
using Rts.Navigation.Grid;
using Rts.Primitives;
using Rts.Simulation.Contracts;
using Rts.Simulation.Maps;
using Rts.Simulation.Rules;

namespace Rts.Simulation.Tests.Movement;

public sealed class MoveCommandTests
{
    private const int TicksPerTileAtTestSpeed = 16;

    private static readonly PlayerId Owner = new(1);

    // 1/16 tile per tick is exactly representable, so arrival times are exact multiples of 16 ticks.
    private static readonly GameRules TestRules = new(VillagerSpeedPerTick: Fixed.FromRatio(1, TicksPerTileAtTestSpeed));

    private static readonly MapDefinition Map = AsciiMapParser.Parse(string.Join(
        "\n",
        "V......",
        "...#...",
        "...#...",
        ".......",
        "CCC....",
        "CCC....",
        "CCC...."));

    private static readonly EntityId TownCenter = new(0, 0);
    private static readonly EntityId Villager = new(1, 0);

    [Fact]
    public void Move_StraightLine_ArrivesExactlyAfterDistanceDividedBySpeed()
    {
        const int tiles = 6;
        var simulation = CreateSimulation();
        simulation.Enqueue(Move(Villager, new GridPosition(tiles, 0)));

        StepTimes(simulation, (tiles * TicksPerTileAtTestSpeed) - 1);
        var beforeArrival = PositionOf(simulation, Villager);
        simulation.Step();

        Assert.NotEqual(WorldGrid.TileCenter(new GridPosition(tiles, 0)), beforeArrival);
        Assert.Equal(WorldGrid.TileCenter(new GridPosition(tiles, 0)), PositionOf(simulation, Villager));
    }

    [Fact]
    public void Move_AfterArrival_UnitStaysAtTarget()
    {
        var target = new GridPosition(2, 0);
        var simulation = CreateSimulation();
        simulation.Enqueue(Move(Villager, target));

        StepTimes(simulation, 10 * TicksPerTileAtTestSpeed);

        Assert.Equal(WorldGrid.TileCenter(target), PositionOf(simulation, Villager));
    }

    [Fact]
    public void Move_AroundObstacle_OnlyEntersWalkableTiles()
    {
        var target = new GridPosition(4, 1);
        var grid = Map.BuildNavGrid();
        var simulation = CreateSimulation();
        simulation.Enqueue(Move(Villager, target));

        for (var tick = 0; tick < 20 * TicksPerTileAtTestSpeed; tick++)
        {
            simulation.Step();
            var tile = WorldGrid.TileAt(PositionOf(simulation, Villager));
            Assert.True(grid.IsWalkable(tile), $"tick {tick}: villager entered blocked tile {tile}");
        }

        Assert.Equal(WorldGrid.TileCenter(target), PositionOf(simulation, Villager));
    }

    [Fact]
    public void Move_BlockedTarget_UnitStaysAndCommandIsNotRejected()
    {
        var simulation = CreateSimulation();
        var start = PositionOf(simulation, Villager);
        simulation.Enqueue(Move(Villager, new GridPosition(3, 1)));

        StepTimes(simulation, TicksPerTileAtTestSpeed);

        Assert.Equal(start, PositionOf(simulation, Villager));
        Assert.Equal(0, simulation.CreateSnapshot().RejectedCommandCount);
    }

    [Fact]
    public void Move_NewCommand_ReplacesCurrentPath()
    {
        var simulation = CreateSimulation();
        simulation.Enqueue(Move(Villager, new GridPosition(6, 0)));
        simulation.Enqueue(Move(Villager, new GridPosition(0, 3), targetTick: TicksPerTileAtTestSpeed));

        StepTimes(simulation, 10 * TicksPerTileAtTestSpeed);

        Assert.Equal(WorldGrid.TileCenter(new GridPosition(0, 3)), PositionOf(simulation, Villager));
    }

    [Fact]
    public void Move_TargetIsCurrentTile_WalksToItsCenter()
    {
        const int halfTile = TicksPerTileAtTestSpeed / 2;
        var currentTile = new GridPosition(1, 0);
        var simulation = CreateSimulation();
        simulation.Enqueue(Move(Villager, new GridPosition(6, 0)));
        StepTimes(simulation, halfTile);
        simulation.Enqueue(Move(Villager, currentTile, targetTick: halfTile));

        StepTimes(simulation, halfTile);

        Assert.Equal(WorldGrid.TileCenter(currentTile), PositionOf(simulation, Villager));
    }

    [Fact]
    public void Move_WithDefaultRules_UsesEightTenthsTilePerSecond()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());
        var start = PositionOf(simulation, Villager);
        simulation.Enqueue(Move(Villager, new GridPosition(6, 0)));

        StepTimes(simulation, SimulationClock.TicksPerSecond);

        var travelled = PositionOf(simulation, Villager).X - start.X;
        Assert.InRange(travelled.Raw, Fixed.FromRatio(79, 100).Raw, Fixed.FromRatio(4, 5).Raw);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_NonPositiveSpeed_Throws(int speedRaw)
    {
        var rules = new GameRules(VillagerSpeedPerTick: Fixed.FromRaw(speedRaw));

        Assert.Throws<ArgumentOutOfRangeException>(() => new GameSimulation(Map, rules));
    }

    [Fact]
    public void Constructor_NullRules_Throws()
    {
        Assert.Throws<ArgumentNullException>(() => new GameSimulation(Map, null!));
    }

    [Theory]
    [MemberData(nameof(InvalidMoves))]
    public void Move_InvalidCommand_IsRejectedAndNothingMoves(string reason, MoveCommand command)
    {
        var simulation = CreateSimulation();
        var start = PositionOf(simulation, Villager);
        simulation.Enqueue(command);

        StepTimes(simulation, TicksPerTileAtTestSpeed);

        Assert.True(simulation.CreateSnapshot().RejectedCommandCount == 1, reason);
        Assert.Equal(start, PositionOf(simulation, Villager));
    }

    public static TheoryData<string, MoveCommand> InvalidMoves() => new()
    {
        { "no units", new MoveCommand(Owner, 0, [], new GridPosition(1, 0)) },
        { "uninitialized unit list", new MoveCommand(Owner, 0, default, new GridPosition(1, 0)) },
        { "target outside map", Move(Villager, new GridPosition(-1, 0)) },
        { "foreign unit", new MoveCommand(new PlayerId(2), 0, [Villager], new GridPosition(1, 0)) },
        { "unknown entity", Move(new EntityId(99, 0), new GridPosition(1, 0)) },
        { "stale generation", Move(Villager with { Generation = 1 }, new GridPosition(1, 0)) },
        { "building cannot move", new MoveCommand(Owner, 0, [TownCenter, Villager], new GridPosition(1, 0)) },
    };

    [Fact]
    public void Snapshot_MovingUnit_ExposesRemainingWaypoints()
    {
        var simulation = CreateSimulation();
        simulation.Enqueue(Move(Villager, new GridPosition(3, 0)));

        simulation.Step();
        var whileMoving = EntityOf(simulation, Villager).RemainingPath;
        StepTimes(simulation, TicksPerTileAtTestSpeed);
        var afterFirstTile = EntityOf(simulation, Villager).RemainingPath;
        StepTimes(simulation, 3 * TicksPerTileAtTestSpeed);

        Assert.Equal<GridPosition>([new GridPosition(1, 0), new GridPosition(2, 0), new GridPosition(3, 0)], whileMoving);
        Assert.Equal<GridPosition>([new GridPosition(2, 0), new GridPosition(3, 0)], afterFirstTile);
        Assert.Empty(EntityOf(simulation, Villager).RemainingPath);
    }

    [Fact]
    public void Snapshot_IdleUnit_HasNoRemainingPath()
    {
        var simulation = CreateSimulation();

        Assert.Empty(EntityOf(simulation, Villager).RemainingPath);
    }

    [Fact]
    public void StateHash_IncludesPathState()
    {
        var towardsFarTile = CreateSimulation();
        var towardsNearTile = CreateSimulation();
        towardsFarTile.Enqueue(Move(Villager, new GridPosition(6, 0)));
        towardsNearTile.Enqueue(Move(Villager, new GridPosition(5, 0)));

        towardsFarTile.Step();
        towardsNearTile.Step();

        Assert.Equal(PositionOf(towardsFarTile, Villager), PositionOf(towardsNearTile, Villager));
        Assert.NotEqual(towardsFarTile.CreateSnapshot().StateHash, towardsNearTile.CreateSnapshot().StateHash);
    }

    [Fact]
    public void StateHash_SameMovesInTwoRuns_IsIdentical()
    {
        var first = CreateSimulation();
        var second = CreateSimulation();

        foreach (var simulation in new[] { first, second })
        {
            simulation.Enqueue(Move(Villager, new GridPosition(4, 1)));
            StepTimes(simulation, 50);
        }

        Assert.Equal(first.CreateSnapshot().StateHash, second.CreateSnapshot().StateHash);
    }

    private static GameSimulation CreateSimulation() => new(Map, TestRules);

    private static MoveCommand Move(EntityId unit, GridPosition target, long targetTick = 0) =>
        new(Owner, targetTick, ImmutableArray.Create(unit), target);

    private static EntitySnapshot EntityOf(GameSimulation simulation, EntityId id) =>
        simulation.CreateSnapshot().Entities.Single(entity => entity.Id == id);

    private static FixedVector2 PositionOf(GameSimulation simulation, EntityId id) => EntityOf(simulation, id).Position;

    private static void StepTimes(GameSimulation simulation, int ticks)
    {
        for (var tick = 0; tick < ticks; tick++)
        {
            simulation.Step();
        }
    }
}
