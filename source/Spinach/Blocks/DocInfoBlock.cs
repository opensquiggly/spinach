namespace Spinach.Blocks;

public struct DocInfoBlock
{
  public long NameAddress { get; set; }
  public long ExternalIdOrPathAddress { get; set; }
  public long Length { get; set; }
  public ulong StartingOffset { get; set; }
}
