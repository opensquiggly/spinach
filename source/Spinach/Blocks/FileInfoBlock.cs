namespace Spinach.Blocks;

public struct FileInfoBlock
{
  public ulong InternalId { get; set; }
  public long NameAddress { get; set; }
  public long Length { get; set; }
}
