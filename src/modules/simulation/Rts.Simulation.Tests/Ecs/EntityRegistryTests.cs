using Rts.Simulation.Contracts;
using Rts.Simulation.Ecs;

namespace Rts.Simulation.Tests.Ecs;

public sealed class EntityRegistryTests
{
    [Fact]
    public void Create_AssignsAscendingIndicesStartingAtZero()
    {
        var registry = new EntityRegistry();

        var first = registry.Create();
        var second = registry.Create();

        Assert.Equal(new EntityId(0, 0), first);
        Assert.Equal(new EntityId(1, 0), second);
        Assert.True(registry.IsAlive(first));
        Assert.True(registry.IsAlive(second));
    }

    [Fact]
    public void Destroy_ThenCreate_ReusesSlotWithNextGeneration()
    {
        var registry = new EntityRegistry();
        var original = registry.Create();
        registry.Create();

        registry.Destroy(original);
        var reused = registry.Create();

        Assert.Equal(new EntityId(0, 1), reused);
    }

    [Fact]
    public void IsAlive_StaleIdAfterReuse_ReturnsFalse()
    {
        var registry = new EntityRegistry();
        var stale = registry.Create();
        registry.Destroy(stale);
        registry.Create();

        Assert.False(registry.IsAlive(stale));
    }

    [Fact]
    public void Destroy_StaleOrUnknownId_ReturnsFalse()
    {
        var registry = new EntityRegistry();
        var id = registry.Create();

        Assert.True(registry.Destroy(id));
        Assert.False(registry.Destroy(id));
        Assert.False(registry.Destroy(new EntityId(42, 0)));
        Assert.False(registry.IsAlive(new EntityId(-1, 0)));
    }

    [Fact]
    public void Create_AfterSeveralDestroys_ReusesMostRecentlyFreedSlotFirst()
    {
        var registry = new EntityRegistry();
        var a = registry.Create();
        var b = registry.Create();
        registry.Create();

        registry.Destroy(a);
        registry.Destroy(b);

        Assert.Equal(new EntityId(1, 1), registry.Create());
        Assert.Equal(new EntityId(0, 1), registry.Create());
        Assert.Equal(new EntityId(3, 0), registry.Create());
    }
}
