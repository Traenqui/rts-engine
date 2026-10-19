using Rts.Simulation.Contracts;
using Rts.Simulation.Ecs;

namespace Rts.Simulation.Tests.Ecs;

public sealed class ComponentStoreTests
{
    private static readonly EntityId First = new(0, 0);
    private static readonly EntityId Second = new(1, 0);
    private static readonly EntityId Third = new(2, 0);

    [Fact]
    public void Add_ThenGet_ReturnsValue()
    {
        var store = new ComponentStore<int>();

        store.Add(Second, 42);

        Assert.True(store.Has(Second));
        Assert.Equal(42, store.Get(Second));
        Assert.Equal(1, store.Count);
    }

    [Fact]
    public void Get_ReturnsReferenceThatCanBeUpdatedInPlace()
    {
        var store = new ComponentStore<int>();
        store.Add(First, 1);

        store.Get(First) = 7;

        Assert.Equal(7, store.Get(First));
    }

    [Fact]
    public void Add_SameEntityTwice_Throws()
    {
        var store = new ComponentStore<int>();
        store.Add(First, 1);

        Assert.Throws<InvalidOperationException>(() => store.Add(First, 2));
    }

    [Fact]
    public void Get_MissingEntity_Throws()
    {
        var store = new ComponentStore<int>();

        Assert.Throws<KeyNotFoundException>(() => store.Get(First));
    }

    [Fact]
    public void Has_StaleGeneration_ReturnsFalse()
    {
        var store = new ComponentStore<int>();
        store.Add(First, 1);

        Assert.False(store.Has(First with { Generation = 1 }));
        Assert.False(store.Has(new EntityId(99, 0)));
    }

    [Fact]
    public void Remove_KeepsOtherComponentsAccessible()
    {
        var store = new ComponentStore<int>();
        store.Add(First, 1);
        store.Add(Second, 2);
        store.Add(Third, 3);

        Assert.True(store.Remove(First));

        Assert.False(store.Has(First));
        Assert.Equal(2, store.Get(Second));
        Assert.Equal(3, store.Get(Third));
        Assert.False(store.Remove(First));
    }

    [Fact]
    public void Add_BeyondInitialCapacity_KeepsAllComponents()
    {
        const int entityCount = 100;
        var store = new ComponentStore<int>();

        for (var index = 0; index < entityCount; index++)
        {
            store.Add(new EntityId(index, 0), index);
        }

        Assert.Equal(entityCount, store.Count);
        Assert.All(Enumerable.Range(0, entityCount), index => Assert.Equal(index, store.Get(new EntityId(index, 0))));
    }

    [Fact]
    public void EntitiesInIdOrder_IgnoresInsertionAndRemovalOrder()
    {
        var store = new ComponentStore<int>();
        store.Add(Third, 3);
        store.Add(First, 1);
        store.Add(Second, 2);
        store.Remove(First);
        store.Add(First with { Generation = 1 }, 10);

        var ids = store.EntitiesInIdOrder();

        Assert.Equal([First with { Generation = 1 }, Second, Third], ids);
    }
}
