namespace SparxEcs;

public class ComponentMask
{
    private ulong[] masks;

    public ComponentMask(int size = 64)
    {
        masks = new ulong[size / 64];
    }

    public void Add(int bit)
    {
        int idx = bit / 64;
        int offset = bit % 64;
        masks[idx] |= 1UL << offset;
    }

    public void Set(int bit, byte value)
    {
      int idx = bit / 64;
      int offset = bit % 64;
      masks[idx] = 1UL << offset;
    }

    public void Remove(int bit)
    {
        int idx = bit / 64;
        int offset = bit % 64;
        masks[idx] &= ~(1UL << offset);
    }

    public bool Has(int bit)
    {
        int idx = bit / 64;
        int offset = bit % 64;
        return (masks[idx] & (1UL << offset)) != 0;
    }
}
