namespace SparxEcs;

public class ComponentMask
{
    private ulong[] masks;

    public ComponentMask(int size = 64)
    {
        masks = new ulong[size / 64];
    }

    /// <summary>
    /// Adds a component to a bit mask
    /// </summary>
    /// <param name="bit">The index of the component being added</param>
    public void Add(int bit)
    {
        int idx = bit / 64;
        int offset = bit % 64;
        masks[idx] |= 1UL << offset;
    }

    /// <summary>
    /// Sets a bit in a mask to a given value
    /// </summary>
    /// <param name="bit">Index being set in the mask</param>
    /// <param name="value">0 or 1, new value being set</param>
    public void Set(int bit, byte value)
    {
        int idx = bit / 64;
        int offset = bit % 64;
        if (value == 1)
            masks[idx] |= 1UL << offset;
        else
            masks[idx] &= ~(1UL << offset);

    }

    /// <summary>
    /// Removes a component from a mask
    /// </summary>
    /// <param name="bit">Index of the component being removed</param>
    public void Remove(int bit)
    {
        int idx = bit / 64;
        int offset = bit % 64;
        masks[idx] &= ~(1UL << offset);
    }

    /// <summary>
    /// Returns whether or not a mask contains a component
    /// </summary>
    /// <param name="bit">Index of component being searched for</param>
    /// <returns>True if mask contains component</returns>
    public bool Has(int bit)
    {
        int idx = bit / 64;
        int offset = bit % 64;
        return (masks[idx] & (1UL << offset)) != 0;
    }

    /// <summary>
    /// Returns a string of the mask
    /// </summary>
    /// <returns>The string version of the mask in bits</returns>
    public string ToBitString()
    {
        return string.Join("_", masks.Select(b => Convert.ToString((long)b, 2).PadLeft(64, '0')));
    }


    public int Length => masks.Length * 64;
}
