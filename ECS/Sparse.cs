namespace SparxECS;

public class Sparse
{
    public const int SPARSE_MAX_SIZE = 2048; // Max size for each sparse page
    private readonly int[] _data;

    public Sparse()
    {
        _data = new int[SPARSE_MAX_SIZE];

        Array.Fill(_data, -1);
    }

    public int this[int index]
    {
        get => _data[index];
        set => _data[index] = value;
    }

    public int Length => _data.Length;

    public int[] Raw => _data;
}
