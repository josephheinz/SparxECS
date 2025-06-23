namespace SparxEcs;

interface ISparseSet
{
    void Delete(EntityID id);
    void Clear();
};

public class SparseSet<T> : ISparseSet
{
    private List<T> dense;
    private List<Sparse> sparsePages;
    private List<int> denseToId;

    public SparseSet()
    {
        sparsePages = new List<Sparse>();
        dense = new List<T>();
        denseToId = new List<int>();
    }

    /// <summary>
    /// Sets or adds a value to the dense list for the given id
    /// </summary>
    /// <param name="id">The id of the entity being set</param>
    /// <param name="item">The object being set or added to the dense list</param>
    public void Set(EntityID id, object item)
    {
        T typedItem = (T)item;
        int index = GetDenseIndex(id);
        if (index != -1)
        {
            dense[index] = typedItem;
            denseToId[index] = id;
            return;
        }
        SetDenseIndex(id, Size);
        dense.Add(typedItem);
        denseToId.Add(id);
    }

    /// <summary>
    /// Attempts to get a value by given id
    /// </summary>
    /// <param name="id">The id of the entity being searched for</param>
    /// <param name="value">Output value of the given id if it exists</param>
    /// <returns>True if entity exists</param>
    public bool TryGet(EntityID id, out T value)
    {
        int index = GetDenseIndex(id);
        if (index != -1)
        {
            value = dense[index];
            return true;
        }
        value = default!;
        return false;
    }

    /// <summary>
    /// Delete a given entity from the dense list from their id
    /// </summary>
    public void Delete(EntityID id)
    {
        int deletedIndex = GetDenseIndex(id);

        if (IsEmpty || deletedIndex == -1)
        {
            return;
        }

        int lastDenseIndex = Size - 1;

        SetDenseIndex(denseToId[lastDenseIndex], deletedIndex);
        SetDenseIndex(id, -1);

        var tempItem = dense[lastDenseIndex];
        dense[lastDenseIndex] = dense[deletedIndex];
        dense[deletedIndex] = tempItem;

        int tempId = denseToId[lastDenseIndex];
        denseToId[lastDenseIndex] = denseToId[deletedIndex];
        denseToId[deletedIndex] = tempId;

        dense.RemoveAt(Size - 1);
        denseToId.RemoveAt(Size - 1);
    }

    /// <summary>
    /// Resets the sparse set
    /// </summary>
    public void Clear()
    {
        denseToId.RemoveRange(0, Size);
        dense.RemoveRange(0, Size);
        sparsePages = new List<Sparse>();
    }

    /// <summary>
    /// Sets a pointer from a sparse id to a dense item
    /// </summary>
    /// <param name="id">Id of the entity whose being pointed to a dense value</param>
    /// <param name="index">Index of the item in the dense list</param>
    private void SetDenseIndex(EntityID id, int index)
    {
        int page = id / Sparse.SPARSE_MAX_SIZE;
        int sparseIndex = id % Sparse.SPARSE_MAX_SIZE;

        if (page >= sparsePages.Count)
        {
            sparsePages.Add(new Sparse());
        }

        Sparse sparse = sparsePages[page];
        sparse[sparseIndex] = index;
    }

    /// <summary>
    /// Gets the dense index of a given entity id
    /// </summary>
    /// <param name="id">Entity id being searched for</param>
    /// <returns>Dense id of the entity or -1 if it doesn't exist on the dense list</returns>
    private int GetDenseIndex(EntityID id)
    {
        int page = id / Sparse.SPARSE_MAX_SIZE;
        int sparseIndex = id % Sparse.SPARSE_MAX_SIZE;

        if (page < sparsePages.Count)
        {
            Sparse sparse = sparsePages[page];
            return sparse[sparseIndex];
        }

        return -1;
    }

    public int Size => dense.Count;
    public bool IsEmpty => Size > 0;
}
