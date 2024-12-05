namespace Spinach.Blocks;

public struct RepoIdCompoundKeyBlock : IComparable<RepoIdCompoundKeyBlock>, IComparable
{

  public RepoIdCompoundKeyBlock()
  {
  }

  public RepoIdCompoundKeyBlock(ushort userType, uint userId, ushort repoType, uint repoId)
  {
    UserType = userType;
    UserId = userId;
    RepoType = repoType;
    RepoId = repoId;
  }

  public ushort UserType { get; set; }
  public uint UserId { get; set; }
  public ushort RepoType { get; set; }
  public uint RepoId { get; set; }
  public ulong FileId { get; set; }

  public int CompareTo(RepoIdCompoundKeyBlock other)
  {
    if (UserType < other.UserType) return -1;
    if (UserType > other.UserType) return 1;
    if (UserId < other.UserId) return -1;
    if (UserId > other.UserId) return 1;
    if (RepoType < other.RepoType) return -1;
    if (RepoType > other.RepoType) return 1;
    if (RepoId < other.RepoId) return -1;
    if (RepoId > other.RepoId) return 1;
    if (FileId < other.FileId) return -1;
    if (FileId > other.FileId) return 1;

    return 0;
  }

  public int CompareTo(object obj) => CompareTo((RepoIdCompoundKeyBlock)obj);
}
