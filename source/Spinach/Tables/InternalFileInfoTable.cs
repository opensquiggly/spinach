namespace Spinach.Tables;

public class InternalFileInfoTable
{
  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Constructors
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public InternalFileInfoTable(DiskBlockManager diskBlockManager, DiskBTree<FileInfoKey, FileInfoBlock> fileInfoTree)
  {
    DiskBlockManager = diskBlockManager;
    FileInfoTree = fileInfoTree;
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Private Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  private bool IsBuilt { get; set; } = false;

  private DiskBlockManager DiskBlockManager { get; }

  DiskBTree<FileInfoKey, FileInfoBlock> FileInfoTree { get; }

  private SortedDictionary<ulong, InternalFileInfo> ByIdDictionary { get; set; }

  private List<Tuple<ulong, InternalFileInfo>> ByOffsetList { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Properties
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public ulong FileCount { get; set; }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Methods
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public void EnsureBuilt()
  {
    if (IsBuilt)
    {
      return;
    }

    // Doesn't the b-tree keep track of its own count???
    FileCount = (ulong)FileInfoTree.Count();
    ByIdDictionary = new SortedDictionary<ulong, InternalFileInfo>();
    ByOffsetList = new List<Tuple<ulong, InternalFileInfo>>((int)FileCount);

    ulong currentOffset = 0;
    var cursor = new DiskBTreeCursor<FileInfoKey, FileInfoBlock>(FileInfoTree);

    while (cursor.MoveNext())
    {
      FileInfoBlock fileInfoBlock = cursor.CurrentData;
      DiskImmutableString nameString = DiskBlockManager.ImmutableStringFactory.LoadExisting(fileInfoBlock.NameAddress);

      // InternalFileInfo is similar to FileInfoBlock except it contains
      // the StartingOffset which we can't store on disk, we have to compute
      // it at runtime
      var internalFileInfo = new InternalFileInfo
      {
        InternalId = fileInfoBlock.InternalId,
        Name = nameString.GetValue(),
        Length = fileInfoBlock.Length,
        StartingOffset = currentOffset
      };

      if (internalFileInfo.Length != 0)
      {
        ByIdDictionary.Add(internalFileInfo.InternalId, internalFileInfo);
        ByOffsetList.Add(new Tuple<ulong, InternalFileInfo>(currentOffset, internalFileInfo));
      }

      currentOffset += (ulong)internalFileInfo.Length;
    }

    IsBuilt = true;
  }

  public InternalFileInfo FindById(ulong internalId)
  {
    EnsureBuilt();

    return ByIdDictionary[internalId];
  }

  public (ulong nextStartingIndex, InternalFileInfo internalFileInfo) FindLastWithOffsetLessThanOrEqual(ulong startingIndex, ulong offset)
  {
    EnsureBuilt();

    ulong end = (ulong)(ByOffsetList.Count - 1);
    ulong nextStartingIndex = startingIndex;
    InternalFileInfo resultFileInfo = null;

    while (nextStartingIndex <= end)
    {
      ulong mid = (nextStartingIndex + end) / 2;
      if (ByOffsetList[(int)mid].Item1 <= offset)
      {
        resultFileInfo = ByOffsetList[(int)mid].Item2;
        nextStartingIndex = mid + 1;
      }
      else
      {
        end = mid - 1;
      }
    }

    return (nextStartingIndex, resultFileInfo);
  }

  public (ulong nextStartingIndex, InternalFileInfo internalFileInfo) FindFirstWithOffsetGreaterThanOrEqual(ulong startingIndex, ulong offset)
  {
    EnsureBuilt();

    ulong end = (ulong)(ByOffsetList.Count - 1);
    ulong nextStartingIndex = startingIndex;
    InternalFileInfo resultFileInfo = null;

    while (nextStartingIndex <= end)
    {
      ulong mid = (nextStartingIndex + end) / 2;
      if (ByOffsetList[(int)mid].Item1 >= offset)
      {
        resultFileInfo = ByOffsetList[(int)mid].Item2;
        end = mid - 1;
      }
      else
      {
        nextStartingIndex = mid + 1;
      }
    }

    return (nextStartingIndex, resultFileInfo);
  }

  // /////////////////////////////////////////////////////////////////////////////////////////////
  // Public Inner Classes
  // /////////////////////////////////////////////////////////////////////////////////////////////

  public class InternalFileInfo
  {
    public ulong InternalId { get; set; }
    public string Name { get; set; }
    public long Length { get; set; }
    public ulong StartingOffset { get; set; }
  }
}
