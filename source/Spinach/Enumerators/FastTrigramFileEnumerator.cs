namespace Spinach.Enumerators;

public class FastTrigramFileEnumerator : IFastEnumerator<TrigramFileInfo, int>
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public FastTrigramFileEnumerator(
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

    Reset();
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private InternalFileInfoTable InternalFileInfoTable { get; }

  private DiskSortedVarIntListFactory SortedVarIntListFactory { get; }

  private LruCache<int, DiskSortedVarIntList> TrigramPostingsListCache { get; }

  private int TrigramKey { get; }

  private DiskBTree<int, long> TrigramTree { get; }

  private FastTrigramEnumerator FastTrigramEnumerator { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  object IEnumerator.Current => Current;

  public TrigramFileInfo Current => CurrentKey;

  public int CurrentData { get; }

  public TrigramFileInfo CurrentKey { get; private set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void Dispose()
  {
  }

  public bool MoveNext()
  {
    bool hasValue = FastTrigramEnumerator.MoveNext();

    if (hasValue)
    {
      (_, InternalFileInfoTable.InternalFileInfo internalFileInfo) =
        InternalFileInfoTable.FindLastWithOffsetLessThanOrEqual(0L, FastTrigramEnumerator.CurrentKey);

      CurrentKey = new TrigramFileInfo(
        internalFileInfo.InternalId,
        (long)(FastTrigramEnumerator.CurrentKey - internalFileInfo.StartingOffset)
      );
    }

    return hasValue;
  }

  public bool MoveUntilGreaterThanOrEqual(TrigramFileInfo target)
  {
    InternalFileInfoTable.InternalFileInfo internalFileInfo =
      InternalFileInfoTable.FindById((ulong)target.FileId);

    ulong offsetTarget = internalFileInfo.StartingOffset + (ulong)target.Position;

    bool hasValue = FastTrigramEnumerator.MoveUntilGreaterThanOrEqual(offsetTarget);

    if (hasValue)
    {
      (_, internalFileInfo) =
        InternalFileInfoTable.FindLastWithOffsetLessThanOrEqual(0L, FastTrigramEnumerator.CurrentKey);

      CurrentKey = new TrigramFileInfo(
        internalFileInfo.InternalId,
        (long)(FastTrigramEnumerator.CurrentKey - internalFileInfo.StartingOffset)
      );
    }

    return hasValue;
  }

  public void Reset()
  {
    // ReSharper disable once ArrangeMethodOrOperatorBody
    FastTrigramEnumerator = new FastTrigramEnumerator(
      TrigramTree,
      TrigramPostingsListCache,
      SortedVarIntListFactory,
      TrigramKey
    );

    FastTrigramEnumerator.Reset();
  }
}
