namespace Spinach.Enumerators;

public class FastTrigramFileEnumerable : IFastEnumerable<IFastEnumerator<TrigramFileInfo, int>, TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramFileEnumerable(
    InternalFileInfoTable internalFileInfoTable,
    DiskBTree<int, long> trigramTree,
    LruCache<int, DiskSortedVarIntList> trigramPostingsListCache,
    DiskSortedVarIntListFactory sortedVarIntListFactory,
    int trigramKey
  )
  {
    InternalFileInfoTable = internalFileInfoTable;
    TrigramTree = trigramTree;
    TrigramPostingsListCache = trigramPostingsListCache;
    SortedVarIntListFactory = sortedVarIntListFactory;
    TrigramKey = trigramKey;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private InternalFileInfoTable InternalFileInfoTable { get; set; }

  private DiskSortedVarIntListFactory SortedVarIntListFactory { get; set; }

  private LruCache<int, DiskSortedVarIntList> TrigramPostingsListCache { get; set; }

  private DiskSortedVarIntList PostingsList { get; set; }

  private DiskSortedVarIntListCursor PostingsListCursor { get; set; }

  private int TrigramKey { get; }

  private DiskBTree<int, long> TrigramTree { get; set; }

  private FastTrigramEnumerator FastTrigramEnumerator { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  IEnumerator IEnumerable.GetEnumerator() => GetFastEnumerator();

  public IEnumerator<int> GetEnumerator() => GetFastEnumerator();

  public IFastEnumerator<TrigramFileInfo, int> GetFastEnumerator()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    return new FastTrigramFileEnumerator(
      InternalFileInfoTable,
      TrigramTree,
      TrigramPostingsListCache,
      SortedVarIntListFactory,
      TrigramKey
    );
  }
}
