namespace Spinach.Enumerators;

public class FastTrigramEnumerable : IFastEnumerable<IFastEnumerator<ulong, long>, ulong, long>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramEnumerable(
    DiskBTree<int, long> trigramTree,
    LruCache<int, DiskSortedVarIntList> trigramPostingsListCache,
    DiskSortedVarIntListFactory sortedVarIntListFactory,
    int trigramKey
  )
  {
    TrigramTree = trigramTree;
    TrigramPostingsListCache = trigramPostingsListCache;
    SortedVarIntListFactory = sortedVarIntListFactory;
    TrigramKey = trigramKey;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private LruCache<int, DiskSortedVarIntList> TrigramPostingsListCache { get; set; }

  private DiskSortedVarIntListFactory SortedVarIntListFactory { get; set; }

  private int TrigramKey { get; set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<ulong> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<ulong, long> GetFastEnumerator()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return new FastTrigramEnumerator(
      TrigramTree,
      TrigramPostingsListCache,
      SortedVarIntListFactory,
      TrigramKey
    );
  }
}
