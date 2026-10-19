using Rts.Navigation.Grid;
using Rts.Primitives;
using Rts.Simulation.Contracts;
using Rts.Simulation.Maps;
using Rts.Simulation.Rules;

namespace Rts.Simulation.Tests;

public sealed class GameSimulationTests
{
    private static readonly PlayerId Player = new(1);

    private static readonly MapDefinition Map = AsciiMapParser.Parse(string.Join(
        "\n",
        "T....",
        ".CCC.",
        ".CCC.",
        ".CCCV",
        "V..T."));

    [Fact]
    public void NewSimulation_StartsAtTickZero()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());

        Assert.Equal(0, simulation.CreateSnapshot().Tick);
    }

    [Fact]
    public void NewSimulation_SpawnsTownCenterThenVillagersThenTrees()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());

        var entities = simulation.CreateSnapshot().Entities;

        Assert.Equal(
            [EntityKind.TownCenter, EntityKind.Villager, EntityKind.Villager, EntityKind.Tree, EntityKind.Tree],
            entities.Select(entity => entity.Kind));
        Assert.Equal(Enumerable.Range(0, entities.Length), entities.Select(entity => entity.Id.Index));
    }

    [Fact]
    public void NewSimulation_AssignsOwnersAndTileCenterPositions()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());

        var entities = simulation.CreateSnapshot().Entities;

        var townCenter = entities[0];
        Assert.Equal(Player, townCenter.Owner);
        Assert.Equal(new FixedVector2(Fixed.FromRatio(5, 2), Fixed.FromRatio(5, 2)), townCenter.Position);

        var firstVillager = entities[1];
        Assert.Equal(Player, firstVillager.Owner);
        Assert.Equal(WorldGrid.TileCenter(new GridPosition(4, 3)), firstVillager.Position);

        var firstTree = entities[3];
        Assert.Equal(PlayerId.Nature, firstTree.Owner);
        Assert.Equal(WorldGrid.TileCenter(new GridPosition(0, 0)), firstTree.Position);
    }

    [Fact]
    public void Terrain_ReflectsMapSizeAndRocks()
    {
        var map = AsciiMapParser.Parse(string.Join("\n", "CCC.#", "CCC..", "CCC#."));

        var terrain = new GameSimulation(map, DefaultRules.Create()).Terrain;

        Assert.Equal(5, terrain.Width);
        Assert.Equal(3, terrain.Height);
        Assert.Equal<GridPosition>([new GridPosition(4, 0), new GridPosition(3, 2)], terrain.Rocks);
    }

    [Fact]
    public void Step_AdvancesTickByOne()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());

        simulation.Step();
        simulation.Step();

        Assert.Equal(2, simulation.CreateSnapshot().Tick);
    }

    [Fact]
    public void Step_UnknownCommandType_IsRejectedAndCounted()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());
        simulation.Enqueue(new UnknownCommand(Player, TargetTick: 0));

        simulation.Step();

        Assert.Equal(1, simulation.CreateSnapshot().RejectedCommandCount);
    }

    [Fact]
    public void Step_CommandForLaterTick_IsNotProcessedEarly()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());
        simulation.Enqueue(new UnknownCommand(Player, TargetTick: 1));

        simulation.Step();
        var afterFirstTick = simulation.CreateSnapshot().RejectedCommandCount;
        simulation.Step();

        Assert.Equal(0, afterFirstTick);
        Assert.Equal(1, simulation.CreateSnapshot().RejectedCommandCount);
    }

    [Fact]
    public void Enqueue_CommandForPastTick_IsRejectedAndCounted()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());
        simulation.Step();

        simulation.Enqueue(new UnknownCommand(Player, TargetTick: 0));

        Assert.Equal(1, simulation.CreateSnapshot().RejectedCommandCount);
    }

    [Fact]
    public void Enqueue_Null_Throws()
    {
        var simulation = new GameSimulation(Map, DefaultRules.Create());

        Assert.Throws<ArgumentNullException>(() => simulation.Enqueue(null!));
    }

    [Fact]
    public void StateHash_SameMapAndCommands_IsIdentical()
    {
        var first = RunScenario(Map);
        var second = RunScenario(Map);

        Assert.Equal(first.StateHash, second.StateHash);
    }

    [Fact]
    public void StateHash_DiffersWhenStateDiffers()
    {
        var otherMap = AsciiMapParser.Parse(string.Join("\n", "CCC.", "CCC.", "CCCV"));
        var simulation = new GameSimulation(Map, DefaultRules.Create());
        var initialHash = simulation.CreateSnapshot().StateHash;
        simulation.Step();

        Assert.NotEqual(initialHash, simulation.CreateSnapshot().StateHash);
        Assert.NotEqual(initialHash, new GameSimulation(otherMap, DefaultRules.Create()).CreateSnapshot().StateHash);
    }

    private static WorldSnapshot RunScenario(MapDefinition map)
    {
        const int scenarioTicks = 10;
        var simulation = new GameSimulation(map, DefaultRules.Create());
        simulation.Enqueue(new UnknownCommand(Player, TargetTick: 3));

        for (var tick = 0; tick < scenarioTicks; tick++)
        {
            simulation.Step();
        }

        return simulation.CreateSnapshot();
    }

    private sealed record UnknownCommand(PlayerId Issuer, long TargetTick) : Command(Issuer, TargetTick);
}
