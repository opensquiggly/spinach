namespace Spinach.Collections;

public class TrigramTree : DiskBTree<int, long>
{
  public TrigramTree(DiskBTreeFactory<int, long> factory, long address, short tempNodeSize = 0) : base(factory, address, tempNodeSize)
  {
  }
}
