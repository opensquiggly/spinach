namespace Spinach.Blocks;

public struct DocOffsetCompoundKeyBlock : IComparable<DocOffsetCompoundKeyBlock>, IComparable
{
  public DocOffsetCompoundKeyBlock()
  {
  }

  public DocOffsetCompoundKeyBlock(ushort userType, uint userId, ushort repoType, uint repoId, ulong startingOffset)
  {
    UserType = userType;
    UserId = userId;
    RepoType = repoType;
    RepoId = repoId;
    StartingOffset = startingOffset;
  }

  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public ushort RepoType { get; set; }
  public uint RepoId { get; set; }
  public ulong StartingOffset { get; set; }

  public int CompareTo(DocOffsetCompoundKeyBlock other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoType < other.RepoType) return -1;
    if (RepoType > other.RepoType) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;
    if (StartingOffset < other.StartingOffset) return -1;
    if (StartingOffset > other.StartingOffset) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((DocOffsetCompoundKeyBlock) obj);
}
