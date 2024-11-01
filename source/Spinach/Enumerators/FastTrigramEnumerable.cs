namespace Spinach.Enumerators;

public class FastTrigramEnumerable : IFastEnumerable<IFastEnumerator<TrigramMatchPositionKey, ulong>, TrigramMatchPositionKey, ulong>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramEnumerable(
    DiskBTree<int, long> trigramTree,
    LruCache<TrigramMatchCacheKey, DiskSortedVarIntList> postingsListCache,
    DiskSortedVarIntListFactory sortedVarIntListFactory,
    DiskBTreeFactory<TrigramMatchKey, long> trigramMatchesFactory,
    int trigramKey
  )
  {
    TrigramTree = trigramTree;
    PostingsListCache = postingsListCache;
    SortedVarIntListFactory = sortedVarIntListFactory;
    TrigramMatchesFactory = trigramMatchesFactory;
    TrigramKey = trigramKey;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private LruCache<TrigramMatchCacheKey, DiskSortedVarIntList> PostingsListCache { get; set; }

  private DiskSortedVarIntListFactory SortedVarIntListFactory { get; set; }

  DiskBTreeFactory<TrigramMatchKey, long> TrigramMatchesFactory { get; set; }

  private int TrigramKey { get; set; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<ulong> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<TrigramMatchPositionKey, ulong> GetFastEnumerator()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return new FastTrigramEnumerator(
      TrigramTree,
      PostingsListCache,
      SortedVarIntListFactory,
      TrigramMatchesFactory,
      TrigramKey
    );
  }
}
