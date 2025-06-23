namespace SparxEcs;

public readonly struct EntityID
{
    public readonly UInt64 Value;
    public EntityID(UInt64 value) => Value = value;

    public static implicit operator UInt64(EntityID id) => id.Value;
    public static implicit operator EntityID(UInt64 value) => new EntityID(value);

    public override string ToString() => Value.ToString();
}

public class ECS
{
    public UInt64 entityCount = 0;
    public readonly int maxComponents = 256; // Probably more than ever necessary but change this to a multiple of 64

    private UInt64 highestEntityId = 0;

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

    public void Add<T>(EntityID id)
    {
        SparseSet<T> pool = GetComponentPool<T>();

        if (pool.TryGet(id, out T value))
        {
          return;
        }


    }
}
