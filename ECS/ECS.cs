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

    private List<ComponentMask> entityMasks; // Every EntityID maps 1:1 to their ComponentMask

    public ECS()
    {
        componentPools = new List<ISparseSet>();
        typeToId = new Dictionary<Type, int>();
        idToType = new List<Type>();
        entityMasks = new List<ComponentMask>();
    }

    /// <summary>
    /// Adds a component T to the component registry
    /// </summary>
    public void RegisterComponent<T>()
    {
        SparseSet<T> newSparseSet = new SparseSet<T>();
        componentPools.Add(newSparseSet);
        typeToId.Add(typeof(T), typeToId.Count);
        idToType.Add(typeof(T));
    }

    /// <summary>
    /// Adds a new entity to the ECS
    /// </summary>
    /// <returns>The id of the newly created entity</returns>
    public EntityID AddEntity()
    {
        entityCount++;
        highestEntityId++;

        entityMasks.Add(new ComponentMask(maxComponents));

        EntityID id = highestEntityId;

        return id;
    }

    /// <summary>
    /// Gets the sparse set for a given component T
    /// </summary>
    /// <returns>The sparse set for component T or throws an exception</returns>
    public SparseSet<T> GetComponentPool<T>()
    {
        var type = typeof(T);
        if (!typeToId.TryGetValue(type, out var id))
        {
            throw new KeyNotFoundException($"Component {type} not registered");
        }

        if (componentPools[id] is SparseSet<T> typedSet)
        {
            return typedSet;
        }

        throw new InvalidOperationException($"Component type {type.Name} not registered.");
    }

    /// <summary>
    /// Adds a component of type T to an ECS entity
    /// </summary>
    /// <param name="id">Id of the entity being targeted</param>
    /// <param name="component">Value of the component being added</param>
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

    /// <summary>
    /// Sets a component of type T to an ECS entity
    /// </summary>
    /// <param name="id">Id of the entity being targeted</param>
    /// <param name="component">Value of the component being set</param>
    public void Set<T>(EntityID id, T component = default!)
    {
        if (component == null)
        {
            throw new ArgumentNullException($"{nameof(component)} : Cannot add a null component");
        }

        SparseSet<T> pool = GetComponentPool<T>();

        if (!pool.TryGet(id, out T value))
        {
            Add<T>(id, component);
            return;
        }

        if (TryGetEntityMask(id, out ComponentMask mask))
        {
            SetComponentMask<T>(mask, 1);
        }
        pool.Set(id, component);
    }

    /// <summary>
    /// Gets the component T if the entity has it
    /// </summary>
    /// <param name="id">Id of an entity to search for</param>
    /// <returns>Value of entity's component or null if entity doesn't have component</param>
    public T? Get<T>(EntityID id)
    {
        SparseSet<T> pool = GetComponentPool<T>();
        if (pool.TryGet(id, out T component)) return component;
        return default!;
    }

    /// <summary>
    /// Tries to get a component mask of a given entity
    /// </summary>
    /// <param name="id">Id of entity whose mask is being searched for</param>
    /// <param name="mask">An out variable to reference the entity's mask if its found, empty mask if not.</param>
    /// <returns>True if entity has a valid component mask</returns>
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

    /// <summary>
    /// Sets the bit of a component on a component to a given byte
    /// </summary>
    /// <param name="mask">The mask being set</param>
    /// <param name="value">The value being set</param>
    public void SetComponentMask<Component>(ComponentMask mask, byte value)
    {
        int bitPos = GetComponentIndex<Component>();
        mask.Set(bitPos, value);
    }

    /// <summary>
    /// Gets the index of a component in the ECS component registry
    /// </summary>
    /// <returns>The id of the component in the registry</returns>
    public int GetComponentIndex<Component>()
    {
        if (typeToId.TryGetValue(typeof(Component), out int index)) return index;
        return -1;
    }

    /// <summary>
    /// Queries entities that have the T component,
    /// </summary>
    /// <typeparam name="T">The component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing component of type T from entities that have it.
    /// </returns>
    public IEnumerable<T> Query<T>()
    {
        SparseSet<T> pool = GetComponentPool<T>();
        for (int i = 0; i < pool.Size(); i++)
        {
            yield return pool.UnsafeGetDenseDirect(i);
        }
    }

    /// <summary>
    /// Queries entities that have both T1 and T2 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1 and T2 from entities that have both.
    /// </returns>
    public IEnumerable<(T1, T2)> Query<T1, T2>()
    {
        int[] componentIds = { GetComponentIndex<T1>(), GetComponentIndex<T2>() };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2))
            {
                if (poolT1.TryGet(id, out var compT1) && poolT2.TryGet(id, out var compT2))
                {
                    yield return (compT1, compT2);
                }
            }
        }

    }

    /// <summary>
    /// Queries entities that have all T1, T2, and T3 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <typeparam name="T3">The third component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1, T2 and T3 from entities that have all.
    /// </returns>
    public IEnumerable<(T1, T2, T3)> Query<T1, T2, T3>()
    {
        int[] componentIds = { GetComponentIndex<T1>(), GetComponentIndex<T2>(), GetComponentIndex<T3>() };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();
        var poolT3 = GetComponentPool<T3>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];
        int idT3 = componentIds[2];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2) && mask.Has(idT3))
            {
                if (poolT1.TryGet(id, out var compT1) && poolT2.TryGet(id, out var compT2) && poolT3.TryGet(id, out var compT3))
                {
                    yield return (compT1, compT2, compT3);
                }
            }
        }

    }

    /// <summary>
    /// Queries entities that have all T1, T2, T3 and T4 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <typeparam name="T3">The third component type.</typeparam>
    /// <typeparam name="T4">The fourth component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1, T2, T3 and T4 from entities that have all.
    /// </returns>
    public IEnumerable<(T1, T2, T3, T4)> Query<T1, T2, T3, T4>()
    {
        int[] componentIds = { GetComponentIndex<T1>(), GetComponentIndex<T2>(), GetComponentIndex<T3>(), GetComponentIndex<T4>() };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();
        var poolT3 = GetComponentPool<T3>();
        var poolT4 = GetComponentPool<T4>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];
        int idT3 = componentIds[2];
        int idT4 = componentIds[3];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2) && mask.Has(idT3) && mask.Has(idT4))
            {
                if (poolT1.TryGet(id, out var compT1) &&
                    poolT2.TryGet(id, out var compT2) &&
                    poolT3.TryGet(id, out var compT3) &&
                    poolT4.TryGet(id, out var compT4))

                {
                    yield return (compT1, compT2, compT3, compT4);
                }
            }
        }

    }

    /// <summary>
    /// Queries entities that have all T1, T2, T3, T4, and T5 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <typeparam name="T3">The third component type.</typeparam>
    /// <typeparam name="T4">The fourth component type.</typeparam>
    /// <typeparam name="T5">The fifth component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1, T2, T3, T4, and T5 from entities that have all.
    /// </returns>
    public IEnumerable<(T1, T2, T3, T4, T5)> Query<T1, T2, T3, T4, T5>()
    {
        int[] componentIds = { GetComponentIndex<T1>(), GetComponentIndex<T2>(), GetComponentIndex<T3>(), GetComponentIndex<T4>(), GetComponentIndex<T5>() };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();
        var poolT3 = GetComponentPool<T3>();
        var poolT4 = GetComponentPool<T4>();
        var poolT5 = GetComponentPool<T5>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];
        int idT3 = componentIds[2];
        int idT4 = componentIds[3];
        int idT5 = componentIds[4];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2) && mask.Has(idT3) && mask.Has(idT4) && mask.Has(idT5))
            {
                if (poolT1.TryGet(id, out var compT1) &&
                    poolT2.TryGet(id, out var compT2) &&
                    poolT3.TryGet(id, out var compT3) &&
                    poolT4.TryGet(id, out var compT4) &&
                    poolT5.TryGet(id, out var compT5))

                {
                    yield return (compT1, compT2, compT3, compT4, compT5);
                }
            }
        }

    }

    /// <summary>
    /// Queries entities that have all T1, T2, T3, T4, T5 and T6 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <typeparam name="T3">The third component type.</typeparam>
    /// <typeparam name="T4">The fourth component type.</typeparam>
    /// <typeparam name="T5">The fifth component type.</typeparam>
    /// <typeparam name="T6">The sixth component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1, T2, T3, T4, T5 and T6 from entities that have all.
    /// </returns>
    public IEnumerable<(T1, T2, T3, T4, T5, T6)> Query<T1, T2, T3, T4, T5, T6>()
    {
        int[] componentIds = {
          GetComponentIndex<T1>(),
          GetComponentIndex<T2>(),
          GetComponentIndex<T3>(),
          GetComponentIndex<T4>(),
          GetComponentIndex<T5>(),
          GetComponentIndex<T6>()
        };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();
        var poolT3 = GetComponentPool<T3>();
        var poolT4 = GetComponentPool<T4>();
        var poolT5 = GetComponentPool<T5>();
        var poolT6 = GetComponentPool<T6>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];
        int idT3 = componentIds[2];
        int idT4 = componentIds[3];
        int idT5 = componentIds[4];
        int idT6 = componentIds[5];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2) && mask.Has(idT3) && mask.Has(idT4) && mask.Has(idT5) && mask.Has(idT6))
            {
                if (poolT1.TryGet(id, out var compT1) &&
                    poolT2.TryGet(id, out var compT2) &&
                    poolT3.TryGet(id, out var compT3) &&
                    poolT4.TryGet(id, out var compT4) &&
                    poolT5.TryGet(id, out var compT5) &&
                    poolT6.TryGet(id, out var compT6))

                {
                    yield return (compT1, compT2, compT3, compT4, compT5, compT6);
                }
            }
        }

    }

    /// <summary>
    /// Queries entities that have all T1 through T7 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <typeparam name="T3">The third component type.</typeparam>
    /// <typeparam name="T4">The fourth component type.</typeparam>
    /// <typeparam name="T5">The fifth component type.</typeparam>
    /// <typeparam name="T6">The sixth component type.</typeparam>
    /// <typeparam name="T7">The seventh component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1 through T7 from entities that have all.
    /// </returns>
    public IEnumerable<(T1, T2, T3, T4, T5, T6, T7)> Query<T1, T2, T3, T4, T5, T6, T7>()
    {
        int[] componentIds = {
        GetComponentIndex<T1>(), GetComponentIndex<T2>(), GetComponentIndex<T3>(),
        GetComponentIndex<T4>(), GetComponentIndex<T5>(), GetComponentIndex<T6>(), GetComponentIndex<T7>()
    };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();
        var poolT3 = GetComponentPool<T3>();
        var poolT4 = GetComponentPool<T4>();
        var poolT5 = GetComponentPool<T5>();
        var poolT6 = GetComponentPool<T6>();
        var poolT7 = GetComponentPool<T7>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];
        int idT3 = componentIds[2];
        int idT4 = componentIds[3];
        int idT5 = componentIds[4];
        int idT6 = componentIds[5];
        int idT7 = componentIds[6];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2) && mask.Has(idT3) && mask.Has(idT4) &&
                mask.Has(idT5) && mask.Has(idT6) && mask.Has(idT7))
            {
                if (poolT1.TryGet(id, out var compT1) && poolT2.TryGet(id, out var compT2) &&
                    poolT3.TryGet(id, out var compT3) && poolT4.TryGet(id, out var compT4) &&
                    poolT5.TryGet(id, out var compT5) && poolT6.TryGet(id, out var compT6) &&
                    poolT7.TryGet(id, out var compT7))
                {
                    yield return (compT1, compT2, compT3, compT4, compT5, compT6, compT7);
                }
            }
        }
    }

    /// <summary>
    /// Queries entities that have all T1 through T8 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <typeparam name="T3">The third component type.</typeparam>
    /// <typeparam name="T4">The fourth component type.</typeparam>
    /// <typeparam name="T5">The fifth component type.</typeparam>
    /// <typeparam name="T6">The sixth component type.</typeparam>
    /// <typeparam name="T7">The seventh component type.</typeparam>
    /// <typeparam name="T8">The eighth component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1 through T8 from entities that have all.
    /// </returns>
    public IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8)> Query<T1, T2, T3, T4, T5, T6, T7, T8>()
    {
        int[] componentIds = {
        GetComponentIndex<T1>(), GetComponentIndex<T2>(), GetComponentIndex<T3>(),
        GetComponentIndex<T4>(), GetComponentIndex<T5>(), GetComponentIndex<T6>(),
        GetComponentIndex<T7>(), GetComponentIndex<T8>()
    };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();
        var poolT3 = GetComponentPool<T3>();
        var poolT4 = GetComponentPool<T4>();
        var poolT5 = GetComponentPool<T5>();
        var poolT6 = GetComponentPool<T6>();
        var poolT7 = GetComponentPool<T7>();
        var poolT8 = GetComponentPool<T8>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];
        int idT3 = componentIds[2];
        int idT4 = componentIds[3];
        int idT5 = componentIds[4];
        int idT6 = componentIds[5];
        int idT7 = componentIds[6];
        int idT8 = componentIds[7];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2) && mask.Has(idT3) && mask.Has(idT4) &&
                mask.Has(idT5) && mask.Has(idT6) && mask.Has(idT7) && mask.Has(idT8))
            {
                if (poolT1.TryGet(id, out var compT1) && poolT2.TryGet(id, out var compT2) &&
                    poolT3.TryGet(id, out var compT3) && poolT4.TryGet(id, out var compT4) &&
                    poolT5.TryGet(id, out var compT5) && poolT6.TryGet(id, out var compT6) &&
                    poolT7.TryGet(id, out var compT7) && poolT8.TryGet(id, out var compT8))
                {
                    yield return (compT1, compT2, compT3, compT4, compT5, compT6, compT7, compT8);
                }
            }
        }
    }

    /// <summary>
    /// Queries entities that have all T1 through T9 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <typeparam name="T3">The third component type.</typeparam>
    /// <typeparam name="T4">The fourth component type.</typeparam>
    /// <typeparam name="T5">The fifth component type.</typeparam>
    /// <typeparam name="T6">The sixth component type.</typeparam>
    /// <typeparam name="T7">The seventh component type.</typeparam>
    /// <typeparam name="T8">The eighth component type.</typeparam>
    /// <typeparam name="T9">The ninth component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1 through T9 from entities that have all.
    /// </returns>
    public IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9)> Query<T1, T2, T3, T4, T5, T6, T7, T8, T9>()
    {
        int[] componentIds = {
        GetComponentIndex<T1>(), GetComponentIndex<T2>(), GetComponentIndex<T3>(),
        GetComponentIndex<T4>(), GetComponentIndex<T5>(), GetComponentIndex<T6>(),
        GetComponentIndex<T7>(), GetComponentIndex<T8>(), GetComponentIndex<T9>()
    };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();
        var poolT3 = GetComponentPool<T3>();
        var poolT4 = GetComponentPool<T4>();
        var poolT5 = GetComponentPool<T5>();
        var poolT6 = GetComponentPool<T6>();
        var poolT7 = GetComponentPool<T7>();
        var poolT8 = GetComponentPool<T8>();
        var poolT9 = GetComponentPool<T9>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];
        int idT3 = componentIds[2];
        int idT4 = componentIds[3];
        int idT5 = componentIds[4];
        int idT6 = componentIds[5];
        int idT7 = componentIds[6];
        int idT8 = componentIds[7];
        int idT9 = componentIds[8];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2) && mask.Has(idT3) && mask.Has(idT4) &&
                mask.Has(idT5) && mask.Has(idT6) && mask.Has(idT7) && mask.Has(idT8) &&
                mask.Has(idT9))
            {
                if (poolT1.TryGet(id, out var compT1) && poolT2.TryGet(id, out var compT2) &&
                    poolT3.TryGet(id, out var compT3) && poolT4.TryGet(id, out var compT4) &&
                    poolT5.TryGet(id, out var compT5) && poolT6.TryGet(id, out var compT6) &&
                    poolT7.TryGet(id, out var compT7) && poolT8.TryGet(id, out var compT8) &&
                    poolT9.TryGet(id, out var compT9))
                {
                    yield return (compT1, compT2, compT3, compT4, compT5, compT6, compT7, compT8, compT9);
                }
            }
        }
    }

    /// <summary>
    /// Queries entities that have all T1 through T10 components.
    /// </summary>
    /// <typeparam name="T1">The first component type.</typeparam>
    /// <typeparam name="T2">The second component type.</typeparam>
    /// <typeparam name="T3">The third component type.</typeparam>
    /// <typeparam name="T4">The fourth component type.</typeparam>
    /// <typeparam name="T5">The fifth component type.</typeparam>
    /// <typeparam name="T6">The sixth component type.</typeparam>
    /// <typeparam name="T7">The seventh component type.</typeparam>
    /// <typeparam name="T8">The eighth component type.</typeparam>
    /// <typeparam name="T9">The ninth component type.</typeparam>
    /// <typeparam name="T10">The tenth component type.</typeparam>
    /// <returns>
    /// An enumerable of tuples containing components of type T1 through T10 from entities that have all.
    /// </returns>
    public IEnumerable<(T1, T2, T3, T4, T5, T6, T7, T8, T9, T10)> Query<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>()
    {
        int[] componentIds = {
        GetComponentIndex<T1>(), GetComponentIndex<T2>(), GetComponentIndex<T3>(),
        GetComponentIndex<T4>(), GetComponentIndex<T5>(), GetComponentIndex<T6>(),
        GetComponentIndex<T7>(), GetComponentIndex<T8>(), GetComponentIndex<T9>(),
        GetComponentIndex<T10>()
    };
        int shortestPoolId = componentIds[0];
        foreach (int id in componentIds)
        {
            var pool = componentPools[id];
            if (pool.Size() == 0) yield break;
            if (pool.Size() < componentPools[shortestPoolId].Size())
            {
                shortestPoolId = id;
            }
        }

        var shortestPool = componentPools[shortestPoolId];

        var poolT1 = GetComponentPool<T1>();
        var poolT2 = GetComponentPool<T2>();
        var poolT3 = GetComponentPool<T3>();
        var poolT4 = GetComponentPool<T4>();
        var poolT5 = GetComponentPool<T5>();
        var poolT6 = GetComponentPool<T6>();
        var poolT7 = GetComponentPool<T7>();
        var poolT8 = GetComponentPool<T8>();
        var poolT9 = GetComponentPool<T9>();
        var poolT10 = GetComponentPool<T10>();

        int idT1 = componentIds[0];
        int idT2 = componentIds[1];
        int idT3 = componentIds[2];
        int idT4 = componentIds[3];
        int idT5 = componentIds[4];
        int idT6 = componentIds[5];
        int idT7 = componentIds[6];
        int idT8 = componentIds[7];
        int idT9 = componentIds[8];
        int idT10 = componentIds[9];

        for (int i = 0; i < shortestPool.Size(); i++)
        {
            EntityID id = shortestPool.UnsafeGetDenseToIdDirect(i);
            if (!TryGetEntityMask(id, out ComponentMask mask)) continue;

            if (mask.Has(idT1) && mask.Has(idT2) && mask.Has(idT3) && mask.Has(idT4) &&
                mask.Has(idT5) && mask.Has(idT6) && mask.Has(idT7) && mask.Has(idT8) &&
                mask.Has(idT9) && mask.Has(idT10))
            {
                if (poolT1.TryGet(id, out var compT1) && poolT2.TryGet(id, out var compT2) &&
                    poolT3.TryGet(id, out var compT3) && poolT4.TryGet(id, out var compT4) &&
                    poolT5.TryGet(id, out var compT5) && poolT6.TryGet(id, out var compT6) &&
                    poolT7.TryGet(id, out var compT7) && poolT8.TryGet(id, out var compT8) &&
                    poolT9.TryGet(id, out var compT9) && poolT10.TryGet(id, out var compT10))
                {
                    yield return (compT1, compT2, compT3, compT4, compT5, compT6, compT7, compT8, compT9, compT10);
                }
            }
        }
    }

}
