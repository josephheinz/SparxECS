namespace SparxEcs;

public class SparseSet<T>
{
    private readonly List<T> dense;
    private readonly List<Sparse> sparsePages;
    private readonly List<int> denseToId;

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
    public void Set(int id, T item)
    {
        int index = GetDenseIndex(id);
        if (index != -1)
        {
            dense[index] = item;
            denseToId[index] = id;
            return;
        }
        SetDenseIndex(id, Size);
        dense.Add(item);
        denseToId.Add(id);
    }

    /// <summary>
    /// Attempts to get a value by given id
    /// </summary>
    /// <param name="id">The id of the entity being searched for</param>
    /// <param name="value">Output value of the given id if it exists</param>
    /// <returns>True if entity exists</param>
    public bool TryGet(int id, out T value)
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
    /// Sets a pointer from a sparse id to a dense item
    /// </summary>
    /// <param name="id">Id of the entity whose being pointed to a dense value</param>
    /// <param name="index">Index of the item in the dense list</param>
    public void SetDenseIndex(int id, int index)
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
    public int GetDenseIndex(int id)
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
