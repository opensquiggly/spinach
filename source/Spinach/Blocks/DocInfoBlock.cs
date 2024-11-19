namespace Spinach.Blocks;

public struct DocInfoBlock
{
  public long NameAddress { get; set; }
  public long ExternalIdOrPathAddress { get; set; }
  public DocStatus Status { get; set; }
  public bool IsIndexed { get; set; }
  public long OriginalLength { get; set; }
  public long CurrentLength { get; set; }
  public ulong StartingOffset { get; set; }
}
