using Rts.Simulation.Contracts;

namespace Rts.Simulation.Ecs;

/// <summary>
/// Sparse set of components: O(1) add, remove and lookup, densely packed values.
/// Mutable on purpose; this is the documented exception from immutability in ADR-0006 and never leaves the simulation.
/// </summary>
internal sealed class ComponentStore<T>
    where T : struct
{
    private const int Absent = -1;
    private const int InitialCapacity = 16;

    private int[] _denseIndexByEntity = [];
    private EntityId[] _entities = new EntityId[InitialCapacity];
    private T[] _components = new T[InitialCapacity];

    public int Count { get; private set; }

    public void Add(EntityId id, T component)
    {
        EnsureEntityCapacity(id.Index);

        if (_denseIndexByEntity[id.Index] != Absent)
        {
            throw new InvalidOperationException($"Slot {id.Index} already has a {typeof(T).Name} component.");
        }

        EnsureDenseCapacity();
        _entities[Count] = id;
        _components[Count] = component;
        _denseIndexByEntity[id.Index] = Count;
        Count++;
    }

    public bool Has(EntityId id)
    {
        return id.Index >= 0
            && id.Index < _denseIndexByEntity.Length
            && _denseIndexByEntity[id.Index] != Absent
            && _entities[_denseIndexByEntity[id.Index]] == id;
    }

    /// <exception cref="KeyNotFoundException">The entity has no such component.</exception>
    public ref T Get(EntityId id)
    {
        if (!Has(id))
        {
            throw new KeyNotFoundException($"{id} has no {typeof(T).Name} component.");
        }

        return ref _components[_denseIndexByEntity[id.Index]];
    }

    public bool Remove(EntityId id)
    {
        if (!Has(id))
        {
            return false;
        }

        // Swap-remove keeps the dense arrays packed; iteration order is defined by EntitiesInIdOrder, not by them.
        var removedIndex = _denseIndexByEntity[id.Index];
        var lastIndex = Count - 1;
        _entities[removedIndex] = _entities[lastIndex];
        _components[removedIndex] = _components[lastIndex];
        _denseIndexByEntity[_entities[removedIndex].Index] = removedIndex;
        _denseIndexByEntity[id.Index] = Absent;
        Count--;
        return true;
    }

    /// <summary>All entities with this component in ascending slot order, the deterministic system order (ADR-0006).</summary>
    public IReadOnlyList<EntityId> EntitiesInIdOrder()
    {
        var ids = new List<EntityId>(Count);

        foreach (var denseIndex in _denseIndexByEntity)
        {
            if (denseIndex != Absent)
            {
                ids.Add(_entities[denseIndex]);
            }
        }

        return ids;
    }

    private void EnsureEntityCapacity(int entityIndex)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(entityIndex);

        if (entityIndex < _denseIndexByEntity.Length)
        {
            return;
        }

        var oldLength = _denseIndexByEntity.Length;
        Array.Resize(ref _denseIndexByEntity, Math.Max(entityIndex + 1, oldLength * 2));
        Array.Fill(_denseIndexByEntity, Absent, oldLength, _denseIndexByEntity.Length - oldLength);
    }

    private void EnsureDenseCapacity()
    {
        if (Count < _components.Length)
        {
            return;
        }

        Array.Resize(ref _entities, _entities.Length * 2);
        Array.Resize(ref _components, _components.Length * 2);
    }
}
