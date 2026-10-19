using System.Collections.Immutable;
using System.Numerics;
using Rts.Primitives;
using Rts.Rendering.Scene;
using Rts.Simulation.Contracts;

namespace Rts.Rendering.Tests.Scene;

public sealed class SnapshotInterpolatorTests
{
    private const float Tolerance = 1e-5f;

    private static readonly EntityId Walker = new(0, 0);
    private static readonly EntityId Newcomer = new(1, 0);
    private static readonly EntityId Leaver = new(2, 0);

    [Fact]
    public void Interpolate_BlendsPositionsOfEntitiesInBothSnapshots()
    {
        var previous = Snapshot(Entity(Walker, x: 1, y: 2));
        var current = Snapshot(Entity(Walker, x: 3, y: 6));

        var entities = SnapshotInterpolator.Interpolate(previous, current, alpha: 0.25f);

        Assert.Equal(new Vector2(1.5f, 3f), Assert.Single(entities).Position);
    }

    [Theory]
    [InlineData(0f, 1f)]
    [InlineData(1f, 3f)]
    public void Interpolate_AlphaBounds_MatchSnapshots(float alpha, float expectedX)
    {
        var previous = Snapshot(Entity(Walker, x: 1, y: 0));
        var current = Snapshot(Entity(Walker, x: 3, y: 0));

        var entity = Assert.Single(SnapshotInterpolator.Interpolate(previous, current, alpha));

        Assert.Equal(expectedX, entity.Position.X, Tolerance);
    }

    [Fact]
    public void Interpolate_NewEntity_AppearsAtCurrentPosition()
    {
        var previous = Snapshot();
        var current = Snapshot(Entity(Newcomer, x: 4, y: 4));

        var entity = Assert.Single(SnapshotInterpolator.Interpolate(previous, current, alpha: 0.5f));

        Assert.Equal(new Vector2(4, 4), entity.Position);
    }

    [Fact]
    public void Interpolate_RemovedEntity_DisappearsImmediately()
    {
        var previous = Snapshot(Entity(Walker, 0, 0), Entity(Leaver, 1, 1));
        var current = Snapshot(Entity(Walker, 0, 0));

        var entities = SnapshotInterpolator.Interpolate(previous, current, alpha: 0.5f);

        Assert.Equal([Walker], entities.Select(entity => entity.Id));
    }

    [Fact]
    public void Interpolate_WithoutPrevious_UsesCurrentAndKeepsKindOwnerAndPath()
    {
        var current = Snapshot(Entity(Walker, x: 2, y: 3));

        var entity = Assert.Single(SnapshotInterpolator.Interpolate(previous: null, current, alpha: 0.5f));

        Assert.Equal(new Vector2(2, 3), entity.Position);
        Assert.Equal(EntityKind.Villager, entity.Kind);
        Assert.Equal(new PlayerId(1), entity.Owner);
        Assert.Equal<Navigation.Grid.GridPosition>([new(5, 5)], entity.RemainingPath);
    }

    [Theory]
    [InlineData(-0.1f)]
    [InlineData(1.1f)]
    [InlineData(float.NaN)]
    public void Interpolate_AlphaOutsideUnitRange_Throws(float alpha)
    {
        var current = Snapshot();

        Assert.Throws<ArgumentOutOfRangeException>(() => SnapshotInterpolator.Interpolate(null, current, alpha));
    }

    private static WorldSnapshot Snapshot(params EntitySnapshot[] entities) =>
        new(Tick: 0, StateHash: 0, RejectedCommandCount: 0, [.. entities]);

    private static EntitySnapshot Entity(EntityId id, int x, int y) =>
        new(id, EntityKind.Villager, new PlayerId(1), new FixedVector2(Fixed.FromInt(x), Fixed.FromInt(y)), [new(5, 5)]);
}
