namespace SparxEcs;

public readonly struct EntityID
{
    public readonly int Value;
    public EntityID(int value) => Value = value;

    public static implicit operator int(EntityID id) => id.Value;
    public static implicit operator EntityID(int value) => new EntityID(value);

    public override string ToString() => Value.ToString();
}

public class ECS
{
    public int entityCount = 0;
    public readonly int maxComponents = 256; // Probably more than ever necessary but change this to a multiple of 64

    private int highestEntityId = 0;

    private List<ISparseSet> componentPools;
    private Dictionary<Type, int> typeToId;
    private List<Type> idToType;

    private List<ComponentMask> entityMasks;

    public ECS()
    {
        componentPools = new List<ISparseSet>();
        typeToId = new Dictionary<Type, int>();
        idToType = new List<Type>();
        entityMasks = new List<ComponentMask>();
    }

    public void RegisterComponent<T>()
    {
        SparseSet<T> newSparseSet = new SparseSet<T>();
        componentPools.Add(newSparseSet);
        typeToId.Add(typeof(T), typeToId.Count);
        idToType.Add(typeof(T));
    }

    public EntityID AddEntity()
    {
        entityCount++;
        highestEntityId++;

        entityMasks.Add(new ComponentMask(maxComponents));

        EntityID id = highestEntityId;

        return id;
    }

    public SparseSet<T> GetComponentPool<T>()
    {
        var type = typeof(T);
        if (!typeToId.TryGetValue(type, out var id))
        {
            throw new KeyNotFoundException($"No Sparse Set found for component type {type.Name}");
        }

        if (componentPools[id] is SparseSet<T> typedSet)
        {
            return typedSet;
        }

        throw new InvalidOperationException($"Component type {type.Name} not registered.");
    }

    public void Add<T>(EntityID id, T component = default!)
    {
        if (component == null)
        {
            throw new ArgumentNullException($"{nameof(component)} : Cannot add a null component");
        }

        SparseSet<T> pool = GetComponentPool<T>();

        if (pool.TryGet(id, out T value))
        {
            return;
        }

        if (TryGetEntityMask(id, out ComponentMask mask))
        {
            SetComponentMask<T>(mask, 1);
        }

        pool.Set(id, component);

    }

    public T? Get<T>(EntityID id)
    {
        SparseSet<T> pool = GetComponentPool<T>();
        if (pool.TryGet(id, out T component)) return component;
        return default!;
    }

    public bool TryGetEntityMask(EntityID id, out ComponentMask mask)
    {
        mask = new ComponentMask();
        if (id >= 0 && id < entityMasks.Count)
        {
            mask = entityMasks[id];
            return true;
        }
        return false;
    }

    public void SetComponentMask<Component>(ComponentMask mask, byte value)
    {
        int bitPos = GetComponentIndex<Component>();
        mask.Set(bitPos, value);
    }

    public int GetComponentIndex<Component>()
    {
        if (typeToId.TryGetValue(typeof(Component), out int index)) return index;
        return -1;
    }
}
