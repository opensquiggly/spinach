namespace Spinach.Blocks;

public struct RepoInfoBlock
{
  public uint InternalId { get; set; }
  public long ExternalIdAddress { get; set; }
  public long NameAddress { get; set; }
  public long RootFolderPathAddress { get; set; }
  public uint LastDocId { get; set; }
}
