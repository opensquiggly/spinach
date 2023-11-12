namespace Spinach.Enumerators;

public class FastTrigramEnumerator : IFastEnumerator<ulong, long>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramEnumerator(
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

    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private DiskSortedVarIntListFactory SortedVarIntListFactory { get; set; }

  private LruCache<int, DiskSortedVarIntList> TrigramPostingsListCache { get; set; }

  private DiskSortedVarIntList PostingsList { get; set; }

  private DiskSortedVarIntListCursor PostingsListCursor { get; set; }

  private int TrigramKey { get; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public ulong Current => CurrentKey;

  public long CurrentData => PostingsListCursor.CurrentData;

  public ulong CurrentKey => PostingsListCursor.CurrentKey;

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return PostingsListCursor.MoveNext();
  }

  public bool MoveUntilGreaterThanOrEqual(ulong target)
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return PostingsListCursor.MoveUntilGreaterThanOrEqual(target);
  }

  public void Reset()
  {
    if (!TrigramPostingsListCache.TryGetValue(TrigramKey, out DiskSortedVarIntList postingsList))
    {
      if (TrigramTree.TryFind(TrigramKey, out long postingsListAddress))
      {
        postingsList = SortedVarIntListFactory.LoadExisting(postingsListAddress);
        TrigramPostingsListCache.Add(TrigramKey, postingsList);
      }
    }

    PostingsList = postingsList;
    PostingsListCursor = new DiskSortedVarIntListCursor(PostingsList);
    PostingsListCursor.Reset();
  }
}
