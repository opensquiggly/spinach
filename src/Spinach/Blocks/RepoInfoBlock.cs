namespace Spinach.Blocks;

public struct RepoInfoBlock
{
  public RepoInfoBlock()
  {
    InternalId = 0;
    ExternalIdAddress = 0;
    NameAddress = 0;
    RootFolderPathAddress = 0;
    LastDocId = 0;
    LastDocLength = 0;
    LastDocStartingOffset = 0;
  }

  public uint InternalId { get; set; }
  public long ExternalIdAddress { get; set; }
  public long NameAddress { get; set; }
  public long RootFolderPathAddress { get; set; }
  public uint LastDocId { get; set; }
  public long LastDocLength { get; set; }
  public ulong LastDocStartingOffset { get; set; }

  // Track file indexing state
  public int LastFileEnumerationIndex { get; set; } = -1;
  public bool HasFilesToIndex { get; set; } = true;
  public bool IsIndexing { get; set; } = false;
}
