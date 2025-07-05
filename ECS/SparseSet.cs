namespace SparxECS;

interface ISparseSet
{
    void Delete(EntityID id);
    void Clear();
    int Size();
    int UnsafeGetDenseToIdDirect(int index);
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
    public void Set(EntityID id, T item)
    {
        int index = GetDenseIndex(id);
        if (index != -1)
        {
            dense[index] = item;
            denseToId[index] = id;
            return;
        }
        SetDenseIndex(id, Size());
        dense.Add(item);
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
    /// Returns whether or not a given entity exists in this sparse set
    /// </summary>
    /// <returns>True if entity is in this sparse set</returns>
    public bool Has(EntityID id)
    {
        return GetDenseIndex(id) != -1;
    }

    /// <summary>
    /// Delete a given entity from the dense list from their id
    /// </summary>
    public void Delete(EntityID id)
    {
        int deletedIndex = GetDenseIndex(id);

        if (IsEmpty || deletedIndex == -1 || !Has(id))
        {
            return;
        }

        int lastDenseIndex = Size() - 1;

        var tempItem = dense[lastDenseIndex];
        dense[lastDenseIndex] = dense[deletedIndex];
        dense[deletedIndex] = tempItem;

        SetDenseIndex(denseToId[lastDenseIndex], deletedIndex);
        SetDenseIndex(id, -1);

        int tempId = denseToId[lastDenseIndex];
        denseToId[lastDenseIndex] = denseToId[deletedIndex];
        denseToId[deletedIndex] = tempId;

        denseToId.RemoveAt(Size() - 1);
        dense.RemoveAt(Size() - 1);
    }

    /// <summary>
    /// Resets the sparse set
    /// </summary>
    public void Clear()
    {
        denseToId.RemoveRange(0, Size());
        dense.RemoveRange(0, Size());
        sparsePages = new List<Sparse>();
    }

    /// <summary>
    /// DON'T USE UNLESS YOU KNOW WHAT YOU'RE DOING
    /// </summary>
    /// <param name="index">Index of dense object wanted</param>
    /// <returns>The dense object at the given index</return>
    public T UnsafeGetDenseDirect(int index)
    {
        return dense[index];
    }

    /// <summary>
    /// DON'T USE UNLESS YOU KNOW WHAT YOU'RE DOING
    /// </summary>
    /// <param name="index">Index of dense object wanted</param>
    /// <returns>The dense object at the given index</return>
    public int UnsafeGetDenseToIdDirect(int index)
    {
        return denseToId[index];
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

        while (page >= sparsePages.Count)
        {
            sparsePages.Add(new Sparse());
        }

        Sparse sparse = sparsePages[page];
        sparse[sparseIndex] = index;
        sparsePages[page] = sparse;
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

    /// <summary>
    /// Returns the length of the dense list
    /// </summary>
    /// <returns>The length of the dense list</returns>
    public int Size()
    {
        return dense.Count;
    }

    public bool IsEmpty => Size() == 0;
}
