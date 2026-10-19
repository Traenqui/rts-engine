using Rts.Simulation.Contracts;

namespace Rts.Simulation.Ecs;

/// <summary>Allocates entity handles. Freed slots are reused last-in-first-out, which is deterministic.</summary>
internal sealed class EntityRegistry
{
    private readonly List<int> _generations = [];
    private readonly List<bool> _isAlive = [];
    private readonly Stack<int> _freeIndices = new();

    public EntityId Create()
    {
        if (_freeIndices.TryPop(out var index))
        {
            _isAlive[index] = true;
            return new EntityId(index, _generations[index]);
        }

        _generations.Add(0);
        _isAlive.Add(true);
        return new EntityId(_generations.Count - 1, 0);
    }

    public bool IsAlive(EntityId id)
    {
        return id.Index >= 0
            && id.Index < _generations.Count
            && _isAlive[id.Index]
            && _generations[id.Index] == id.Generation;
    }

    /// <returns><c>false</c> if the entity was already destroyed or never existed.</returns>
    public bool Destroy(EntityId id)
    {
        if (!IsAlive(id))
        {
            return false;
        }

        _isAlive[id.Index] = false;
        _generations[id.Index]++;
        _freeIndices.Push(id.Index);
        return true;
    }
}
