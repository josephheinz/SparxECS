using SparxEcs;
SparseSet<string> ss = new SparseSet<string>();
ss.Set(0, "asdf");
if (ss.TryGet(0, out string val))
{
    Console.WriteLine(val);
}
