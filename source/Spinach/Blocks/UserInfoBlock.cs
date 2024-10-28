namespace Spinach.Blocks;

public struct UserInfoBlock
{
  public UserInfoBlock()
  {
  }

  public ushort UserType { get; set; }

  public uint UserId { get; set; }

  public long NameAddress { get; set; }

  public long ExternalIdAddress { get; set; }

  public uint LastRepoId { get; set; }
}
